using System;
using System.Buffers;
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
	private readonly static Guid MF_SOURCE_READER_READ_ANY_STREAM = new(0x49306d83, 0x4b12, 0x4a17, 0x85, 0x10, 0x55, 0x43, 0x14, 0xb2, 0x09, 0x95);

	public bool HasAudio { get; private set; }
	public bool IsReady { get; private set; }
	public int Stride { get; private set; }

	public VideoPixelFormat PixelFormat => VideoPixelFormat.NV12;

	private IMFSourceReader? _sourceReader;
	private IMFByteStream? _mfByteStream;

	private uint _width;
	private uint _height;

	public void Initialize(VideoClip clip, VideoRenderColorType colorType = VideoRenderColorType.FollowSystem)
	{
		MediaFactory.MFStartup().CheckError();

		_mfByteStream = new MFByteStream(clip.Stream);

		using var attributes = MediaFactory.MFCreateAttributes(3);
		attributes.Set(SourceReaderAttributeKeys.EnableVideoProcessing, true).CheckError();
		attributes.Set(MF_SOURCE_READER_READ_ANY_STREAM, true).CheckError();

		_sourceReader = MediaFactory.MFCreateSourceReaderFromByteStream(_mfByteStream, attributes);

		using var nativeType = _sourceReader.GetNativeMediaType((int) SourceReaderIndex.FirstVideoStream, 0);

		(_width, _height, Stride) = CalculateLogicalVideoSize(_sourceReader, nativeType, colorType);

		clip.Width = (int) _width;
		clip.Height = (int) _height;

		var variant = _sourceReader.GetPresentationAttribute((int) SourceReaderIndex.MediaSource, PresentationDescriptionAttributeKeys.Duration);
		if (variant.Value != null)
		{
			clip.Duration = (ulong) variant.Value / 10_000_000.0;
		}

		HasAudio = false;

		try
		{
			using var audioType = MediaFactory.MFCreateMediaType();
			audioType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Audio).CheckError();
			audioType.Set(MediaTypeAttributeKeys.Subtype, AudioFormatGuids.Pcm).CheckError();

			_sourceReader.SetCurrentMediaType((int) SourceReaderIndex.FirstAudioStream, audioType);
			HasAudio = true;
		}
		catch
		{
#if DEBUG
			Console.WriteLine("Audio stream not found or unsupported.");
#endif
			HasAudio = false;
		}

		IsReady = true;
	}

	public bool TryReadNextVideoFrame(byte[] destination, out int stride, out double timestamp)
	{
		stride = 0;
		timestamp = 0;

		var reader = _sourceReader;
		if (reader == null || !IsReady) return false;

		try
		{
			var sample = reader.ReadSample((int) SourceReaderIndex.FirstVideoStream, 0, out int _, out var flags, out long timestamp100ns);

			if (flags.HasFlag(SourceReaderFlag.EndOfStream) || sample == null)
			{
				sample?.Dispose();
				return false;
			}

			try
			{
				timestamp = timestamp100ns / 10_000_000.0;
				stride = Stride;

				using var buffer = sample.ConvertToContiguousBuffer();
				buffer.Lock(out nint ptr, out _, out int currentLength);

				try
				{
					int alignedHeight = currentLength * 2 / (stride * 3);

					unsafe
					{
						fixed (byte* pDest = destination)
						{
							byte* pSrc = (byte*) ptr;
							byte* pUVSrc = pSrc + (long) stride * alignedHeight;
							byte* pUVDest = pDest + stride * _height;

							// Copy Y Plane
							for (int y = 0; y < _height; y++)
							{
								Buffer.MemoryCopy(pSrc + (long) y * stride, pDest + (long) y * stride, destination.Length - (long) y * stride, _width);
							}

							// Copy UV Plane
							int uvHeight = (int) _height / 2;
							for (int y = 0; y < uvHeight; y++)
							{
								Buffer.MemoryCopy(pUVSrc + (long) y * stride, pUVDest + (long) y * stride, destination.Length - stride * _height - (long) y * stride, _width);
							}
						}
					}
				}
				finally
				{
					buffer.Unlock();
				}

				return true;
			}
			finally
			{
				sample.Dispose();
			}
		}
		catch
		{
			return false;
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
			buffer.Lock(out nint ptr, out _, out int currentLength);

			byte[] rentedData = ArrayPool<byte>.Shared.Rent(currentLength);

			try
			{
				unsafe
				{
					fixed (byte* dest = rentedData)
					{
						Buffer.MemoryCopy((void*) ptr, dest, rentedData.Length, currentLength);
					}
				}
			}
			finally
			{
				buffer.Unlock();
			}

			using var mt = _sourceReader!.GetCurrentMediaType((int) SourceReaderIndex.FirstAudioStream);

			audioData = new AudioData
			{
				Samples = rentedData,
				SampleLength = currentLength,
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
		uint w = 0, h = 0;

		try
		{
			byte[] blob = nativeType.GetBlob(MediaTypeAttributeKeys.MinimumDisplayAperture);
			if (blob is {Length: >= 16})
			{
				fixed (byte* p = blob)
				{
					var aperture = *(MFVideoArea*) p;
					w = (uint) aperture.Area.cx;
					h = (uint) aperture.Area.cy;
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
		videoType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.NV12).CheckError();
		if (colorType == VideoRenderColorType.Full)
		{
			videoType.Set(MediaTypeAttributeKeys.VideoNominalRange, (uint) eAVEncVideoColorNominalRange.eAVEncVideoColorNominalRange_0_255).CheckError();
		}
		else if (colorType == VideoRenderColorType.Limited)
		{
			videoType.Set(MediaTypeAttributeKeys.VideoNominalRange, (uint) eAVEncVideoColorNominalRange.eAVEncVideoColorNominalRange_16_235).CheckError();
		}

		MediaFactory.MFSetAttributeSize(videoType, MediaTypeAttributeKeys.FrameSize, w, h).CheckError();
		sourceReader.SetCurrentMediaType((int) SourceReaderIndex.FirstVideoStream, videoType);

		using var currentVideoType = sourceReader.GetCurrentMediaType((int) SourceReaderIndex.FirstVideoStream);

		int s;
		if (currentVideoType.GetUInt32(MediaTypeAttributeKeys.DefaultStride, out uint realStride).Success)
		{
			s = (int) realStride;
		}
		else
		{
			s = (int) w;
		}

		s = Math.Abs(s);
		return (w, h, s);
	}
}