using System.Collections.Concurrent;
using RabotoraX.Core.Audios;
using RabotoraX.Core.Graphics;
using Silk.NET.OpenAL;

namespace RabotoraX.Core.Videos;

public class RVideoPlayer : Component2D
{
	private const int BufferCount = 12;

	public VideoClip? Clip { get; set; }
	public Texture2D? Texture { get; private set; }
	public bool IsPlaying => !_isPaused;

	private IVideoDecoder? _decoder;
	private uint _alSource;
	private readonly Queue<uint> _alBuffers = new();
	private readonly ConcurrentQueue<VideoFrame> _videoQueue = new();
	private readonly ConcurrentQueue<AudioFrame> _audioQueue = new();

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

		_decoder.Initialize(Clip);

		Texture = new Texture2D();
		Texture.CreateEmpty2D(Clip.Width, Clip.Height);

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

	public void Pause()
	{
		if (_decoder == null) return;

		if (!_isPaused)
		{
			_isPaused = true;
			AudioService.AL.SourcePause(_alSource);
		}
	}

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
			IsBackground = true
		};

		_decodeThread.Start();
	}

	private void DecodeThreadLoop()
	{
		while (_running)
		{
			if (_decoder == null) continue;

			// Video Queue
			if (_videoQueue.Count < 30 && _decoder.TryReadNextVideoFrame(out var pixels, out double vPts))
			{
				_videoQueue.Enqueue(new VideoFrame {Pixels = pixels.ToArray(), Pts = vPts});
			}

			// Audio Queue
			if (_audioQueue.Count < 60 && _decoder.TryReadNextAudioBlock(out var a))
			{
				_audioQueue.Enqueue(new AudioFrame
				{
					Samples = a.Samples.ToArray(),
					SampleRate = a.SampleRate,
					Channels = a.Channels,
					BitDepth = a.BitDepth,
					Pts = a.Pts
				});
			}

			Thread.Sleep(1);
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
		}

		while (_alBuffers.Count > 0 && _audioQueue.TryDequeue(out var frame))
		{
			uint buf = _alBuffers.Dequeue();

			fixed (byte* p = frame.Samples)
			{
				AudioService.AL.BufferData(buf, GetFormat(frame),
					p, frame.Samples.Length, frame.SampleRate);
			}

			uint[] tmp = [buf];
			fixed (uint* p = tmp)
			{
				AudioService.AL.SourceQueueBuffers(_alSource, 1, p);
			}

			_audioClock = frame.Pts;
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
			if (frame.Pts > masterTime)
				break;

			_videoQueue.TryDequeue(out frameToRender);
			hasFrame = true;
		}

		if (hasFrame && frameToRender.Pixels != null)
		{
			UpdateRenderTexturePixels(frameToRender.Pixels);
		}
	}

	private void UpdateRenderTexturePixels(byte[] pixels)
	{
		if (Texture == null) return;

		GraphicsService.API.UpdateTexture2D(Texture.NativeTexture, pixels);
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
		while (_videoQueue.TryDequeue(out _))
		{
		}

		while (_audioQueue.TryDequeue(out _))
		{
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
			_running = false;
			_decodeThread?.Join();

			AudioService.AL.SourceStop(_alSource);
			AudioService.AL.DeleteSource(_alSource);

			_decoder?.Dispose();
		}

		base.Dispose(disposing);
	}
}