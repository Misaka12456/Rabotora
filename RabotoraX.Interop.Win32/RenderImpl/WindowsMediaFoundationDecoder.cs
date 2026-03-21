using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using JetBrains.Annotations;
using RabotoraX.Core.Videos;
using TerraFX.Interop.Windows;
using Vortice.MediaFoundation;
using IMFByteStream = Vortice.MediaFoundation.IMFByteStream;
using IMFMediaType = Vortice.MediaFoundation.IMFMediaType;
using IMFSourceReader = Vortice.MediaFoundation.IMFSourceReader;

namespace RabotoraX.Interop.Win32.RenderImpl;

[SupportedOSPlatform("windows"), UsedImplicitly]
public class WindowsMediaFoundationDecoder : IVideoDecoder
{
	private readonly static Guid MF_SOURCE_READER_LOW_LATENCY = new Guid(0x9c27891a, 0xed7a, 0x40e1, 0x88, 0xe8, 0xb2, 0x27, 0x27, 0xa0, 0x24, 0xee);
	private readonly static Guid MF_SOURCE_READER_READ_ANY_STREAM = new Guid(0x49306d83, 0x4b12, 0x4a17, 0x85, 0x10, 0x55, 0x43, 0x14, 0xb2, 0x09, 0x95);
	
	public bool HasAudio { get; private set; }
	public bool IsReady { get; private set; }
	public int Stride { get; private set; }

	private IMFSourceReader? _sourceReader;
	private IMFByteStream? _mfByteStream;

	private uint _width;
	private uint _height;

	public void Initialize(VideoClip clip, VideoRenderColorType colorType = VideoRenderColorType.FollowSystem)
	{
		MediaFactory.MFStartup().CheckError();
		_mfByteStream = new MFByteStream(clip.Stream);

		using var attributes = MediaFactory.MFCreateAttributes(1);
		attributes.Set(SourceReaderAttributeKeys.EnableVideoProcessing, true).CheckError();
		attributes.Set(MF_SOURCE_READER_LOW_LATENCY, true).CheckError();
		attributes.Set(MF_SOURCE_READER_READ_ANY_STREAM, true).CheckError();

		_sourceReader = MediaFactory.MFCreateSourceReaderFromByteStream(_mfByteStream, attributes);

	    using var nativeType = _sourceReader.GetNativeMediaType((int)SourceReaderIndex.FirstVideoStream, 0);

	    (_width, _height, Stride) = CalculateLogicalVideoSize(_sourceReader, nativeType, colorType);

	    clip.Width = (int)_width;
	    clip.Height = (int)_height;

	    var variant = _sourceReader.GetPresentationAttribute((int) SourceReaderIndex.MediaSource, PresentationDescriptionAttributeKeys.Duration);
		if (variant.Value != null)
		{
			clip.Duration = (ulong)variant.Value / 10_000_000.0f;
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

	public bool TryReadNextVideoFrame(byte[] destination, out int stride, out double timestamp)
	{
		stride = 0;
		timestamp = 0;

		var sample = _sourceReader!.ReadSample((int) SourceReaderIndex.FirstVideoStream, 0,
			out int _, out var flags, out long timestamp100ns);

		if (flags.HasFlag(SourceReaderFlag.EndOfStream) || sample == null)
		{
			sample?.Dispose();
			return false;
		}

		try
		{
			timestamp = timestamp100ns / 10_000_000.0f;
			stride = Stride;
			
			using var buffer = sample.ConvertToContiguousBuffer();
			buffer.Lock(out nint ptr, out _, out int currentLength);
			int validDataLength = (int)_height * Stride;
			try
			{
				unsafe
				{
					fixed (byte* pDest = destination)
					{
						long sourceBytesToCopy = Math.Min(currentLength, validDataLength);
						long finalCopySize = Math.Min(sourceBytesToCopy, destination.Length);
						Buffer.MemoryCopy((void*)ptr, pDest, destination.Length, finalCopySize);
					}
				}
				return true;
			}
			finally
			{
				buffer.Unlock();
			}
		}
		finally
		{
			sample.Dispose();
		}
	}

	public bool TryReadNextAudioBlock(out AudioData audioData)
	{
		audioData = default;
		if (!HasAudio) return false;

		var sample = _sourceReader!.ReadSample((int) SourceReaderIndex.FirstAudioStream, 0, out _, out _, out long timestamp100ns);

		if (sample == null) return false;

		try
		{
			using var buffer = sample.ConvertToContiguousBuffer();
			buffer.Lock(out nint ptr, out _, out var currentLength);
			byte[] data = new byte[currentLength];
			Marshal.Copy(ptr, data, 0, currentLength);
			buffer.Unlock();

			using var mt = _sourceReader!.GetCurrentMediaType((int) SourceReaderIndex.FirstAudioStream);
			audioData = new AudioData
			{
				Samples = data,
				Channels = (int) mt.GetUInt32(MediaTypeAttributeKeys.AudioNumChannels),
				SampleRate = (int) mt.GetUInt32(MediaTypeAttributeKeys.AudioSamplesPerSecond),
				BitDepth = (int) mt.GetUInt32(MediaTypeAttributeKeys.AudioBitsPerSample),
				Pts = timestamp100ns / 10_000_000.0
			};
			return true;
		}
		finally
		{
			sample.Dispose();
		}
	}

	public void Seek(double timeInSeconds)
	{
		if (_sourceReader == null) return;
		long time100ns = (long) (timeInSeconds * 10_000_000.0);
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
	
	private static unsafe (uint width, uint height, int stride) CalculateLogicalVideoSize(IMFSourceReader sourceReader, IMFMediaType nativeType, VideoRenderColorType colorType)
	{
		bool foundAperture = false;
		uint w = 0;
		uint h = 0;
		int s;
		try
		{
			byte[] blob = nativeType.GetBlob(MediaTypeAttributeKeys.MinimumDisplayAperture);
			if (blob is {Length: >= 16})
			{
				fixed (byte* p = blob)
				{
					var aperture = *(MFVideoArea*)p;
					w = (uint)aperture.Area.cx;
					h = (uint)aperture.Area.cy;
					foundAperture = true;
				}
			}
		}
		catch
		{
			// ignored
		}

		if (!foundAperture)
		{
			MediaFactory.MFGetAttributeSize(nativeType, MediaTypeAttributeKeys.FrameSize, out w, out h).CheckError();
		}
		using var videoType = MediaFactory.MFCreateMediaType();
		videoType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video).CheckError();
		videoType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.Rgb32).CheckError();
		if (colorType == VideoRenderColorType.Full)
		{
			videoType.Set(MediaTypeAttributeKeys.VideoNominalRange, (uint)eAVEncVideoColorNominalRange.eAVEncVideoColorNominalRange_0_255).CheckError();
		}
		else if (colorType == VideoRenderColorType.Limited)
		{
			videoType.Set(MediaTypeAttributeKeys.VideoNominalRange, (uint)eAVEncVideoColorNominalRange.eAVEncVideoColorNominalRange_16_235).CheckError();
		}
	    
		MediaFactory.MFSetAttributeSize(videoType, MediaTypeAttributeKeys.FrameSize, w, h).CheckError();
		sourceReader.SetCurrentMediaType((int)SourceReaderIndex.FirstVideoStream, videoType);

		using var currentVideoType = sourceReader.GetCurrentMediaType((int)SourceReaderIndex.FirstVideoStream);
		s = (int)currentVideoType.GetUInt32(MediaTypeAttributeKeys.DefaultStride);
		if (s == 0) s = (int)w * 4;
		s = Math.Abs(s);
		return (w, h, s);
	}
}