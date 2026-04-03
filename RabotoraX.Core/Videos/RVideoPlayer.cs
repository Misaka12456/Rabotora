using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RabotoraX.Core.Audios;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.UI;
using Silk.NET.OpenAL;

namespace RabotoraX.Core.Videos;

public class RVideoPlayer : Component2D
{
	private const int BufferCount = 12;
	private const int VideoQueueMaxCacheCount = 60;
	private const int AudioQueueMaxCacheCount = 120;

	public VideoClip? Clip { get; set; }
	public Texture2D? Texture { get; private set; }
	public bool IsPlaying => !_isPaused;
	public TimeSpan Time => TimeSpan.FromSeconds(_audioClock);
	public TimeSpan Length => Clip != null ? TimeSpan.FromSeconds(Clip.Duration) : TimeSpan.Zero;
	public VideoRenderColorType ColorType { get; set; } = VideoRenderColorType.FollowSystem;

	private IVideoDecoder? _decoder;
	private uint _alSource;
	private readonly Queue<uint> _alBuffers = new();
	private readonly Queue<double> _alBufferPtsQueue = new();

	private readonly Queue<VideoFrame> _videoQueue = new();
	private readonly Queue<AudioFrame> _audioQueue = new();
	private readonly Lock _videoQueueLock = new();
	private readonly Lock _audioQueueLock = new();

	private SemaphoreSlim _videoQueueSlots = new(VideoQueueMaxCacheCount);
	private SemaphoreSlim _audioQueueSlots = new(AudioQueueMaxCacheCount);

	private readonly ArrayPool<byte> _pixelPool = ArrayPool<byte>.Shared;
	private readonly AutoResetEvent _frameNeededSignal = new(false);

	private Thread? _decodeThread;
	private volatile bool _running;
	private double _audioClock;
	private double _videoClock;
	private bool _isPaused = true;

	public override void OnAwake()
	{
		base.OnAwake();
		_decoder = IVideoDecoder.PlatformCreate();

		_alSource = AudioService.AL.GenSource();

		for (int i = 0; i < BufferCount; i++)
		{
			_alBuffers.Enqueue(AudioService.AL.GenBuffer());
		}
	}

	public void Prepare()
	{
		if (Clip == null || _decoder == null) return;

		ShutdownPlayback(recreateDecoder: false);

		_decoder.Initialize(Clip, ColorType);

		Texture = new Texture2D();
		Texture.CreateEmpty2DForVideo(Clip.Width, Clip.Height, VideoPixelFormat.NV12);
		
		GetComponent<RawImage>();

		_audioClock = 0;
		_videoClock = 0;
		_isPaused = true;

		StartDecodeThread();
	}

	public void Play()
	{
		if (_decoder == null) return;

		if (_isPaused)
		{
			_isPaused = false;

			if (_decoder.HasAudio)
			{
				PrerollAudio();
				AudioService.AL.SourcePlay(_alSource);
			}
		}
	}

	[UsedImplicitly]
	public void Pause()
	{
		if (_decoder == null) return;

		if (!_isPaused)
		{
			_isPaused = true;

			if (_decoder.HasAudio)
			{
				AudioService.AL.SourcePause(_alSource);
			}
		}
	}

	[UsedImplicitly]
	public void Stop()
	{
		ShutdownPlayback(recreateDecoder: true);

		_audioClock = 0;
		_videoClock = 0;
		_isPaused = true;
	}

	public override void OnUpdate(float deltaTime)
	{
		if (_isPaused || _decoder == null) return;

		UpdateAudio();
		double masterTime = GetMasterTime(deltaTime);
		UpdateVideo(masterTime);
	}

	private void StartDecodeThread()
	{
		_running = true;

		_decodeThread = new Thread(DecodeThreadLoop)
		{
			Name = "RabotoraX RUI VideoPlayer Decode Thread",
			Priority = ThreadPriority.Highest,
			IsBackground = true
		};

		_decodeThread.Start();
	}

	private void DecodeThreadLoop()
	{
		while (_running)
		{
			var decoder = _decoder;
			if (decoder == null)
			{
				_frameNeededSignal.WaitOne(10);
				continue;
			}

			bool worked = false;

			if (_videoQueueSlots.Wait(0))
			{
				if (TryDecodeVideoFrame(decoder, out var videoFrame))
				{
					lock (_videoQueueLock)
					{
						_videoQueue.Enqueue(videoFrame);
					}
					worked = true;
				}
				else
				{
					_videoQueueSlots.Release();
				}
			}

			if (_audioQueueSlots.Wait(0))
			{
				if (TryDecodeAudioFrame(decoder, out var audioFrame))
				{
					lock (_audioQueueLock)
					{
						_audioQueue.Enqueue(audioFrame);
					}
					worked = true;
				}
				else
				{
					_audioQueueSlots.Release();
				}
			}

			if (!worked)
			{
				_frameNeededSignal.WaitOne(10);
			}
		}
	}

	private bool TryDecodeVideoFrame(IVideoDecoder decoder, out VideoFrame frame)
	{
		frame = default;

		if (Clip == null) return false;

		// int frameSize = checked(decoder.Stride * Clip.Height);
		int frameSize = decoder.PixelFormat switch
		{
			VideoPixelFormat.NV12 => checked(decoder.Stride * Clip.Height * 3 / 2),
			_ => checked(decoder.Stride * Clip.Height) // for VideoPixelFormat.Bgra32, Stride should already account for 4 bytes per pixel
		};
		byte[] buffer = _pixelPool.Rent(frameSize);

		if (decoder.TryReadNextVideoFrame(buffer, out int stride, out double vPts))
		{
			frame = new VideoFrame
			{
				Pixels = buffer,
				Stride = stride,
				Pts = vPts
			};
			return true;
		}

		_pixelPool.Return(buffer);
		return false;
	}

	[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
	[SuppressMessage("Roslyn", "CA1822")]
	private bool TryDecodeAudioFrame(IVideoDecoder decoder, out AudioFrame frame)
	{
		frame = default;

		if (!decoder.TryReadNextAudioBlock(out var a))
		{
			return false;
		}

		byte[] rentedBuffer = ArrayPool<byte>.Shared.Rent(a.SampleLength);
		a.Samples.CopyTo(rentedBuffer);

		frame = new AudioFrame()
		{
			Samples = rentedBuffer,
			SampleLength = a.SampleLength,
			SampleRate = a.SampleRate,
			Channels = a.Channels,
			BitDepth = a.BitDepth,
			Pts = a.Pts
		};

		return true;
	}

	private unsafe void UpdateAudio()
	{
		if (_decoder is not { HasAudio: true }) return;

		AudioService.AL.GetSourceProperty(_alSource, GetSourceInteger.BuffersProcessed, out int processed);

		if (processed > 0)
		{
			uint[] arr = new uint[processed];

			fixed (uint* p = arr)
			{
				AudioService.AL.SourceUnqueueBuffers(_alSource, processed, p);
			}

			foreach (uint b in arr)
			{
				_alBuffers.Enqueue(b);
			}

			for (int i = 0; i < processed; i++)
			{
				_alBufferPtsQueue.TryDequeue(out _);
			}
		}
		
		while (_alBuffers.Count > 0 && TryDequeueAudioFrame(out var frame))
		{
			uint buf = _alBuffers.Dequeue();

			fixed (byte* p = frame.Samples)
			{
				AudioService.AL.BufferData(buf, GetFormat(frame), p, frame.SampleLength, frame.SampleRate);
			}

			uint[] tmp = [buf];
			fixed (uint* p = tmp)
			{
				AudioService.AL.SourceQueueBuffers(_alSource, 1, p);
			}

			_alBufferPtsQueue.Enqueue(frame.Pts);

			if (frame.Samples != null)
			{
				ArrayPool<byte>.Shared.Return(frame.Samples);
			}
		}

		if (_alBufferPtsQueue.TryPeek(out double frontPts))
		{
			_audioClock = frontPts;
		}

		AudioService.AL.GetSourceProperty(_alSource, GetSourceInteger.SourceState, out int state);
		AudioService.AL.GetSourceProperty(_alSource, GetSourceInteger.BuffersQueued, out int queued);

		if (queued > 0 && state != (int)SourceState.Playing)
		{
			AudioService.AL.SourcePlay(_alSource);
		}
	}

	private void PrerollAudio()
	{
		for (int i = 0; i < 5; i++)
		{
			UpdateAudio();
		}
	}

	private void UpdateVideo(double masterTime)
	{
		VideoFrame? frameToRender = null;

		while (TryPeekAndDequeueReadyVideoFrame(masterTime, out var frame))
		{
			if (frameToRender is {Pixels: not null})
			{
				_pixelPool.Return(frameToRender.Value.Pixels!);
			}

			frameToRender = frame;
		}

		if (frameToRender.HasValue)
		{
			try
			{
				UpdateRenderTexturePixels(frameToRender.Value);
			}
			finally
			{
				if (frameToRender.Value.Pixels != null)
				{
					_pixelPool.Return(frameToRender.Value.Pixels);
				}
			}
		}
	}

	private bool TryPeekAndDequeueReadyVideoFrame(double masterTime, out VideoFrame frame)
	{
		frame = default;

		lock (_videoQueueLock)
		{
			if (_videoQueue.Count == 0) return false;

			var head = _videoQueue.Peek();
			if (head.Pts > masterTime + 0.005)
			{
				return false;
			}

			frame = _videoQueue.Dequeue();
		}

		_videoQueueSlots.Release();
		_frameNeededSignal.Set();
		return true;
	}

	private bool TryDequeueAudioFrame(out AudioFrame frame)
	{
		frame = default;

		lock (_audioQueueLock)
		{
			if (_audioQueue.Count == 0) return false;
			frame = _audioQueue.Dequeue();
		}

		_audioQueueSlots.Release();
		_frameNeededSignal.Set();
		return true;
	}

	private void UpdateRenderTexturePixels(VideoFrame frame)
	{
		if (Texture == null) return;
		GraphicsService.API.UpdateTexture2D(Texture.NativeTexture, frame.Pixels, frame.Stride);
	}

	private double GetMasterTime(float deltaTime)
	{
		if (_decoder!.HasAudio)
		{
			AudioService.AL.GetSourceProperty(_alSource, GetSourceInteger.SourceState, out int state);
			if (state != (int)SourceState.Playing)
				return _videoClock;

			AudioService.AL.GetSourceProperty(_alSource, SourceFloat.SecOffset, out float offset);
			_videoClock = _audioClock + offset;
			return _videoClock;
		}

		_videoClock += deltaTime;
		return _videoClock;
	}

	private void ClearQueues()
	{
		lock (_videoQueueLock)
		{
			while (_videoQueue.Count > 0)
			{
				var frame = _videoQueue.Dequeue();
				if (frame.Pixels != null)
				{
					_pixelPool.Return(frame.Pixels);
				}
			}
		}

		lock (_audioQueueLock)
		{
			while (_audioQueue.Count > 0)
			{
				var frame = _audioQueue.Dequeue();
				if (frame.Samples != null)
				{
					ArrayPool<byte>.Shared.Return(frame.Samples);
				}
			}
		}

		// 重新创建 semaphore（最安全）
		_videoQueueSlots.Dispose();
		_audioQueueSlots.Dispose();

		_videoQueueSlots = new SemaphoreSlim(VideoQueueMaxCacheCount);
		_audioQueueSlots = new SemaphoreSlim(AudioQueueMaxCacheCount);

		_alBufferPtsQueue.Clear();
		_frameNeededSignal.Set();
	}

	private unsafe void ReturnAllQueuedOpenAlBuffers()
	{
		try
		{
			AudioService.AL.GetSourceProperty(_alSource, GetSourceInteger.BuffersQueued, out int queued);
			if (queued <= 0) return;

			uint[] arr = new uint[queued];
			fixed (uint* p = arr)
			{
				AudioService.AL.SourceUnqueueBuffers(_alSource, queued, p);
			}

			foreach (uint b in arr)
			{
				_alBuffers.Enqueue(b);
			}

			_alBufferPtsQueue.Clear();
		}
		catch
		{
			// ignore
		}
	}

	private void ShutdownPlayback(bool recreateDecoder)
	{
		_isPaused = true;
		_running = false;
		_frameNeededSignal.Set();

		_decodeThread?.Join();
		_decodeThread = null;

		try
		{
			AudioService.AL.SourceStop(_alSource);
		}
		catch
		{
			// ignore
		}

		ReturnAllQueuedOpenAlBuffers();
		ClearQueues();

		if (recreateDecoder)
		{
			_decoder?.Dispose();
			_decoder = IVideoDecoder.PlatformCreate();
		}
	}

	private static BufferFormat GetFormat(AudioFrame d)
	{
		return d.Channels == 1
			? (d.BitDepth == 8 ? BufferFormat.Mono8 : BufferFormat.Mono16)
			: (d.BitDepth == 8 ? BufferFormat.Stereo8 : BufferFormat.Stereo16);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			ShutdownPlayback(recreateDecoder: false);
			
			try
			{
				AudioService.AL.DeleteSource(_alSource);
			}
			catch
			{
				// ignore
			}

			_decoder?.Dispose();
			_decoder = null;

			_videoQueueSlots.Dispose();
			_audioQueueSlots.Dispose();
			_frameNeededSignal.Dispose();
		}

		base.Dispose(disposing);
	}
}