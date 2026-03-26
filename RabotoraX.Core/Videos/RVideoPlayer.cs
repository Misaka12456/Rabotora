using System.Buffers;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using RabotoraX.Core.Audios;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.UI;
using Silk.NET.OpenAL;

namespace RabotoraX.Core.Videos;

public class RVideoPlayer : Component2D
{
	private const int BufferCount = 12;
	private const int VideoQueueMaxCacheCount = 20;

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
	private readonly ConcurrentQueue<VideoFrame> _videoQueue = new();
	private readonly ConcurrentQueue<AudioFrame> _audioQueue = new();
	private readonly ArrayPool<byte> _pixelPool = ArrayPool<byte>.Shared;
	private readonly AutoResetEvent _frameNeededSignal = new(false);

	private INativeShader? _videoShader;

	private RawImage? _rawImage;
	private Thread? _decodeThread;
	private volatile bool _running;
	private double _audioClock;
	private double _videoClock;
	private bool _isPaused = true;

	public override void OnAwake()
	{
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

		_decoder.Initialize(Clip, ColorType);

		Texture = new Texture2D();
		Texture.CreateEmpty2DForVideo(Clip.Width, Clip.Height);
		
		using var sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("RabotoraX.Core.Assets.Shaders.VideoDefault.hlsl")!, new UTF8Encoding(false));
		string fragSource = sr.ReadToEnd();
		sr.Close();
		_rawImage = GetComponent<RawImage>();
		if (GraphicsService.API.ApiName == "Direct3D 12")
		{
			_videoShader = GraphicsService.API.CreateNativeShader(ShaderType.FragmentShader, fragSource, "PSMain");
			_rawImage?.CustomShader = _videoShader;
		}
		StartDecodeThread();
	}

	public void Play()
	{
		if (_decoder == null) return;

		if (_isPaused)
		{
			_isPaused = false;

			PrerollAudio();

			AudioService.AL.SourcePlay(_alSource);
		}
	}

	[UsedImplicitly]
	public void Pause()
	{
		if (_decoder == null) return;

		if (!_isPaused)
		{
			_isPaused = true;
			AudioService.AL.SourcePause(_alSource);
		}
	}

	[UsedImplicitly]
	public void Stop()
	{
		_isPaused = true;

		_running = false;
		_decodeThread?.Join();

		AudioService.AL.SourceStop(_alSource);

		ClearQueues();

		_decoder?.Dispose();
		_decoder = IVideoDecoder.PlatformCreate();
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
			if (_decoder == null) continue;

			bool videoFull = _videoQueue.Count >= VideoQueueMaxCacheCount;
			bool audioFull = _audioQueue.Count >= 60;

			// Only when both queues are full that we wait
			if (videoFull && audioFull)
			{
				_frameNeededSignal.WaitOne(5);
				continue;
			}

			bool worked = false;

			// Video Queue
			if (!videoFull)
			{
				// int frameSize = Clip!.Height * _decoder.Stride;
				int frameSize = (int) (Clip!.Height * _decoder.Stride * 1.5f); // NV12 format may require up to 1.5x the size of the Y plane for the full frame
				byte[] buffer = _pixelPool.Rent(frameSize);
            
				if (_decoder.TryReadNextVideoFrame(buffer, out int stride, out double vPts))
				{
					_videoQueue.Enqueue(new VideoFrame {Pixels = buffer, Stride = stride, Pts = vPts});
					worked = true;
				}
				else
				{
					_pixelPool.Return(buffer);
				}
			}

			// Audio Queue
			if (!audioFull && _decoder.TryReadNextAudioBlock(out var a))
			{
				byte[] rentedBuffer = ArrayPool<byte>.Shared.Rent(a.Samples.Length);
				a.Samples.CopyTo(rentedBuffer);
				_audioQueue.Enqueue(new AudioFrame
				{
					// Samples = a.Samples.ToArray(),
					Samples = rentedBuffer,
					SampleLength = a.Samples.Length,
					SampleRate = a.SampleRate,
					Channels = a.Channels,
					BitDepth = a.BitDepth,
					Pts = a.Pts
				});
				worked = true;
			}

			if (!worked)
			{
				Thread.Sleep(1);
			}
		}
	}

	private unsafe void UpdateAudio()
	{
		if (_decoder is not {HasAudio: true}) return;

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

			// Dequeue the corresponding PTS for the processed buffers after buffers are played
			for (int i = 0; i < processed; i++)
			{
				_alBufferPtsQueue.TryDequeue(out _);
			}
		}

		while (_alBuffers.Count > 0 && _audioQueue.TryDequeue(out var frame))
		{
			uint buf = _alBuffers.Dequeue();

			fixed (byte* p = frame.Samples)
			{
				// Here we used a pooled Samples array, so we must use SampleLength instead of Samples.Length
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

		// Fixed the audio clock to the PTS of the front buffer in queue
		if (_alBufferPtsQueue.TryPeek(out double frontPts))
		{
			_audioClock = frontPts;
		}

		AudioService.AL.GetSourceProperty(_alSource, GetSourceInteger.SourceState, out int state);
		if (state != (int) SourceState.Playing)
		{
			AudioService.AL.SourcePlay(_alSource);
		}
	}

	private void PrerollAudio()
	{
		int count = 0;

		while (count < 5 && _audioQueue.TryPeek(out _))
		{
			UpdateAudio();
			count++;
		}
	}

	private void UpdateVideo(double masterTime)
	{
		VideoFrame frameToRender = default;
		bool hasFrame = false;

		while (_videoQueue.TryPeek(out var frame))
		{
			if (frame.Pts > masterTime + 0.005) break;

			if (hasFrame && frameToRender.Pixels != null)
			{
				_pixelPool.Return(frameToRender.Pixels);
			}

			if (_videoQueue.Count < VideoQueueMaxCacheCount / 2)
			{
				_frameNeededSignal.Set();
			}

			_videoQueue.TryDequeue(out frameToRender);
			hasFrame = true;
		}

		if (hasFrame && frameToRender.Pixels != null)
		{
			try 
			{
				UpdateRenderTexturePixels(frameToRender);
			}
			finally 
			{
				_pixelPool.Return(frameToRender.Pixels);
			}
		}
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
			AudioService.AL.GetSourceProperty(_alSource, SourceFloat.SecOffset, out float offset);
			return _audioClock + offset;
		}
		else
		{
			_videoClock += deltaTime;
			return _videoClock;
		}
	}

	private void ClearQueues()
	{
		while (_videoQueue.TryDequeue(out var frame))
		{
			if (frame.Pixels != null)
			{
				_pixelPool.Return(frame.Pixels);
			}
		}

		while (_audioQueue.TryDequeue(out var frame))
		{
			if (frame.Samples != null)
			{
				ArrayPool<byte>.Shared.Return(frame.Samples);
			}
		}
		
		_alBufferPtsQueue.Clear();
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
			_running = false;
			_decodeThread?.Join();
			if (GraphicsService.API.ApiName == "Direct3D 12")
			{
				_rawImage?.CustomShader = null;
				_videoShader?.Dispose();
				_videoShader = null;
			}

			AudioService.AL.SourceStop(_alSource);
			AudioService.AL.DeleteSource(_alSource);

			_decoder?.Dispose();
		}

		base.Dispose(disposing);
	}
}