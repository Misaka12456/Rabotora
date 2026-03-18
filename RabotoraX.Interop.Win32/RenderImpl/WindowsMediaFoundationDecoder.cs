using System;
using System.Runtime.InteropServices;
using RabotoraX.Core.Videos;
using SharpGen.Runtime.Win32;
using Vortice.MediaFoundation;

namespace RabotoraX.Interop.Win32.RenderImpl;

public class WindowsMediaFoundationDecoder : IVideoDecoder
{
	public bool HasAudio { get; private set; }
	public bool IsReady { get; private set; }

	private IMFSourceReader? _sourceReader;
	private IMFByteStream? _mfByteStream;
	private byte[]? _frameBuffer;
	
	private uint _width;
	private uint _height;
	private int _stride;

	public void Initialize(VideoClip clip)
	{
		MediaFactory.MFStartup().CheckError();
		_mfByteStream = new MFByteStream(clip.Stream, false);

		var attributes = MediaFactory.MFCreateAttributes(1);
		_sourceReader = MediaFactory.MFCreateSourceReaderFromByteStream(_mfByteStream, attributes);
		
		using var videoType = MediaFactory.MFCreateMediaType();
		videoType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video).CheckError();
		videoType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.Rgb32).CheckError();
		_sourceReader.SetCurrentMediaType((int) SourceReaderIndex.FirstVideoStream, videoType);

		using var currentVideoType = _sourceReader.GetCurrentMediaType((int) SourceReaderIndex.FirstVideoStream);
		MediaFactory.MFGetAttributeSize(currentVideoType, MediaTypeAttributeKeys.FrameSize, out _width, out _height).CheckError();
		_stride = (int)currentVideoType.GetUInt32(MediaTypeAttributeKeys.DefaultStride);
		if (_stride < 0) _stride = -_stride; // Handle negative stride for bottom-up bitmaps
		
		_frameBuffer = new byte[_stride * (int)_height];
		clip.Width = (int)_width;
		clip.Height = (int)_height;

		var variant = _sourceReader.GetPresentationAttribute((int)SourceReaderIndex.MediaSource, PresentationDescriptionAttributeKeys.Duration);
		if (variant.Value != null)
		{
			clip.Duration = (long)variant.Value / 10_000_000.0f;
		}
		
		try
		{
			using var audioType = MediaFactory.MFCreateMediaType();
			audioType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Audio).CheckError();
			audioType.Set(MediaTypeAttributeKeys.Subtype, AudioFormatGuids.Pcm).CheckError();
			_sourceReader.SetCurrentMediaType((int) SourceReaderIndex.FirstAudioStream, audioType);
			HasAudio = true;
		}
		catch (Exception ex)
		{
#if DEBUG
			Console.WriteLine($"Audio stream not found or unsupported: {ex.Message}");
#endif
			HasAudio = false;
		}
		IsReady = true;
	}
	
	public bool TryReadNextVideoFrame(out ReadOnlySpan<byte> pixelData, out double timestamp)
	{
		pixelData = ReadOnlySpan<byte>.Empty;
		timestamp = 0;

		var sample = _sourceReader!.ReadSample((int) SourceReaderIndex.FirstVideoStream, 0,
			out _stride, out var flags, out long timestamp100ns);
		if (flags.HasFlag(SourceReaderFlag.EndOfStream) || sample == null) return false;
		
		timestamp = timestamp100ns / 10_000_000.0f;
		using var buffer = sample.ConvertToContiguousBuffer();
		buffer.Lock(out nint ptr, out _, out int currentLength);
		try
		{
			unsafe
			{
				fixed (byte* dest = _frameBuffer)
				{
					Buffer.MemoryCopy((void*) ptr, dest, _frameBuffer!.Length, currentLength);
				}
			}

			pixelData = _frameBuffer!.AsSpan(0, currentLength);
			return true;
		}
		finally
		{
			buffer.Unlock();
		}
	}
	
	public bool TryReadNextAudioBlock(out AudioData audioData)
	{
		audioData = default;
		if (!HasAudio) return false;

		var sample = _sourceReader!.ReadSample((int) SourceReaderIndex.FirstAudioStream, 0, out _, out var flags, out long timestamp100ns);

		if (sample == null) return false;

		using var buffer = sample.ConvertToContiguousBuffer();
		buffer.Lock(out nint ptr, out _, out var currentLength);
		byte[] data = new byte[currentLength];
		Marshal.Copy(ptr, data, 0, (int)currentLength);
		buffer.Unlock();

		var mt = _sourceReader!.GetCurrentMediaType((int)SourceReaderIndex.FirstAudioStream);
		audioData = new AudioData
		{
			Samples = data,
			Channels = (int)mt.GetUInt32(MediaTypeAttributeKeys.AudioNumChannels),
			SampleRate = (int)mt.GetUInt32(MediaTypeAttributeKeys.AudioSamplesPerSecond),
			BitDepth = (int)mt.GetUInt32(MediaTypeAttributeKeys.AudioBitsPerSample),
			Pts = timestamp100ns / 10_000_000.0
		};
		sample.Dispose();
		return true;
	}

	public void Seek(double timeInSeconds)
	{
		if (_sourceReader == null) return;
		long time100ns = (long)(timeInSeconds * 10_000_000.0);
		_sourceReader.SetCurrentPosition(time100ns);
	}

	public void Dispose()
	{
		_sourceReader?.Dispose();
		_mfByteStream?.Dispose();
		MediaFactory.MFShutdown();
		IsReady = false;
		GC.SuppressFinalize(this);
	}
}