using Rabotora.Runtime.Api;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using SharpDX.MediaFoundation;
using System.Runtime.Versioning;

namespace Rabotora.Runtime
{
	[SupportedOSPlatform("windows")]
	/// <summary>
	/// Represents a fading bitmap drawing command.
	/// </summary>
	public class BitmapFadeDrawCmd : RabotoraD2DCmd, IStoppableCmd
	{
		/// <summary>
		/// Bitmap data.
		/// </summary>
		public byte[] BitmapData { get; }

		/// <summary>
		/// Type of the fading animation.
		/// </summary>
		public FadeType Type { get; }

		/// <summary>
		/// Multiple which is used to scale the bitmap.
		/// </summary>
		public float ScaleMultiple { get; }

		/// <summary>
		/// Destination window size.
		/// <para>
		/// This can be <see langword="null" /> when not give the window size while instantiating current <see cref="BitmapFadeDrawCmd"/> instance.
		/// </para>
		/// </summary>
		public Size2F? TargetWindowSize { get; } = null;

		/// <summary>
		/// The destination rectangle range on the render target(window) which is ready to be drawn by the bitmap.
		/// </summary>
		public RawRectangleF TargetRect { get; }

		/// <summary>
		/// Duration of the fading animation (in seconds).
		/// </summary>
		public float Duration { get; }

		/// <summary>
		/// Should we called <see cref="RenderTarget.BeginDraw()"/> before starting fading animation?
		/// </summary>
		public bool BeginDrawBeforeStart { get; }

		/// <summary>
		/// The segment alpha which is used to increase/decrease bitmap alpha in the every round of inner drawing circulation.
		/// </summary>
		private float SegmentAlpha { get; } = 0.005f;

		public bool IsStopDoing { get => _isStopDoing; set => _isStopDoing = value; }
		
		private bool _isStopDoing = false;

		/// <summary>
		/// Instantiate a new <see cref="BitmapFadeDrawCmd"/> instance.
		/// </summary>
		/// <param name="bitmapData">Bitmap data.</param>
		/// <param name="targetRect">Which rectangle range on the render target(window) should be drawn by the bitmap?</param>
		/// <param name="type">Which type of fade should we use to do the animation?</param>
		/// <param name="scaleMultiple">Which multiple should be used to scale the bitmap?</param>
		/// <param name="duration">The animation duration of drawing fading bitmap in seconds. Default is <see langword="2.5f" /> .</param>
		/// <param name="segmentAlpha">How much alpha we should increase/decrease in the every round of inner drawing circulation? (Range: [0, 0.5]; Default is <see langword="0.005f" /> .)</param>
		/// <param name="beginDrawBeforeStart">Should we called <see cref="RenderTarget.BeginDraw()"/> before starting fading animation? (Default is <see langword="false" /> .)</param>
		public BitmapFadeDrawCmd(byte[] bitmapData, RawRectangleF targetRect, FadeType type, float scaleMultiple = 1.0f, float duration = 2.5f, float segmentAlpha = 0.005f, bool beginDrawBeforeStart = false)
		{
			BitmapData = bitmapData;
			Type = type;
			ScaleMultiple = scaleMultiple;
			TargetRect = targetRect;
			Duration = duration;
			BeginDrawBeforeStart = beginDrawBeforeStart;
			SegmentAlpha = segmentAlpha;
		}

		/// <summary>
		/// Instantiate a new <see cref="BitmapFadeDrawCmd"/> instance.
		/// </summary>
		/// <param name="bitmapData">Bitmap data.</param>
		/// <param name="targetRect">Which rectangle range on the render target(window) should be drawn by the bitmap?</param>
		/// <param name="type">Which type of fade should we use to do the animation?</param>
		/// <param name="targetWindowSize">Destination window size.</param>
		/// <param name="duration">The animation duration of drawing fading bitmap in seconds. Default is <see langword="2.5f" /> .</param>
		/// <param name="segmentAlpha">How much alpha we should increase/decrease in the every round of inner drawing circulation? (Range: [0, 0.5]; Default is <see langword="0.005f" /> .)</param>
		/// <param name="beginDrawBeforeStart">Should we called <see cref="RenderTarget.BeginDraw()"/> before starting fading animation? (Default is <see langword="false" /> .)</param>
		public BitmapFadeDrawCmd(byte[] bitmapData, RawRectangleF targetRect, FadeType type, Size2F targetWindowSize, float duration = 2.5f, float segmentAlpha = 0.005f, bool beginDrawBeforeStart = false)
		{
			BitmapData = bitmapData;
			Type = type;
			TargetWindowSize = targetWindowSize;
			if ((targetRect.Right - targetRect.Left) > targetWindowSize.Width)
			{
				ScaleMultiple = targetWindowSize.Width / (targetRect.Right - targetRect.Left);
			}
			else if ((targetRect.Bottom - targetRect.Top) > targetWindowSize.Height)
			{
				ScaleMultiple = targetWindowSize.Height / (targetRect.Bottom - targetRect.Top);
			}
			else
			{
				ScaleMultiple = 1.0f;
			}
			TargetRect = targetRect;
			BeginDrawBeforeStart = beginDrawBeforeStart;
			SegmentAlpha = segmentAlpha;
		}

		public override void DoCommand(RenderTarget renderTarget)
		{
			if (TargetWindowSize.HasValue)
			{
				Api.Image.DrawFadeBitmap(renderTarget, BitmapData, TargetWindowSize.Value, TargetRect, Type, ref _isStopDoing, Duration, SegmentAlpha, BeginDrawBeforeStart);
			}
			else
			{
				Api.Image.DrawFadeBitmap(renderTarget, BitmapData, TargetRect, ScaleMultiple, Type, ref _isStopDoing, Duration, SegmentAlpha, BeginDrawBeforeStart);
			}
		}
	}

	/// <summary>
	/// Represents a video play command.
	/// </summary>
	public class VideoPlayCmd : RabotoraD2DCmd, IStoppableCmd
	{
		/// <summary>
		/// Video Data.
		/// </summary>
		public byte[] VideoData { get; }

		/// <summary>
		/// The handle of which window we should play the video on.
		/// </summary>
		public IntPtr WindowHandle { get; }

		/// <summary>
		/// The action which will restore the render target after video stopped playing. Can be <see langword="null" /> .
		/// </summary>
		public Action? RenderTargetRestore { get; }

		/// <summary>
		/// Should we startup the media manager before playing the video?
		/// </summary>
		public bool StartupMediaManager { get; }

		/// <summary>
		/// Should we shut down the media manager after finished playing the video?
		/// </summary>
		public bool ShutdownMediaManager { get; }

		public bool IsStopDoing { get => _isStopDoing; set => _isStopDoing = value; }
		private bool _isStopDoing;

		/// <summary>
		/// Instantiate a new <see cref="VideoPlayCmd"/> instance.
		/// </summary>
		/// <param name="videoData">Video data.</param>
		/// <param name="hVideoWnd">The handle of which window we should play the video on.</param>
		/// <param name="renderTargetRestore">Which action will we called to recreate(repaint) render target after the video stopped playing? (Default is <see langword="null" />.)</param>
		/// <param name="startUpMediaManager">Should we startup the media manager before playing the video?</param>
		/// <param name="shutdownMediaManager">Should we shut down the media manager after finished playing the video?</param>
		public VideoPlayCmd(byte[] videoData, IntPtr hVideoWnd, Action? renderTargetRestore = null, bool startUpMediaManager = false, bool shutdownMediaManager = false)
		{
			VideoData = videoData;
			WindowHandle = hVideoWnd;
			RenderTargetRestore = renderTargetRestore;
			StartupMediaManager = startUpMediaManager;
			ShutdownMediaManager = shutdownMediaManager;
		}

		public override void DoCommand(RenderTarget renderTarget)
		{
			bool stoppedPlaying = false;
			Media.PlayVideo(VideoData, WindowHandle, out var sessionId, StartupMediaManager,new Action(() =>
			{
				if (ShutdownMediaManager)
				{
					MediaManager.Shutdown();
				}
				stoppedPlaying = true;
			}));
			if (sessionId.HasValue)
			{
				new Thread(() =>
				{
					for (; ; )
					{
						if (_isStopDoing)
						{
							Media.StopVideo(sessionId.Value, ShutdownMediaManager, RenderTargetRestore);
							break;
						}
						else if (stoppedPlaying)
						{
							break;
						}
					}
				}) { IsBackground = true }.Start(); 
			}
		}
	}

	/// <summary>
	/// Represents a audio(music/voice/sound) play command.
	/// <para>
	/// We recommended that:
	/// <list type="bullet">
	/// <item><b>music</b> - <see cref="IsLoop"/> = <see langword="true"/> ; <see cref="StartupMediaManager"/> = <see langword="true"/> ; <see cref="ShutdownMediaManager"/> = <see langword="true"/></item>
	/// <item><b>voice</b> - All is <see langword="false" /></item>
	/// <item><b>sound</b> - All is <see langword="false" /></item>
	/// </list>
	/// </para>
	/// </summary>
	public class AudioPlayCmd : RabotoraD2DCmd, IStoppableCmd
	{
		/// <summary>
		/// Audio Data.
		/// </summary>
		public byte[] AudioData { get; }

		/// <summary>
		/// Should we loop playing the audio(play the audio after it ends)?
		/// </summary>
		public bool IsLoop { get; }

		/// <summary>
		/// Should we startup the media manager before playing the audio?
		/// </summary>
		public bool StartupMediaManager { get; }

		/// <summary>
		/// Should we shut down the media manager after finished playing the audio?
		/// </summary>
		public bool ShutdownMediaManager { get; }

		public bool IsStopDoing { get => _isStopDoing; set => _isStopDoing = value; }
		private bool _isStopDoing;

		/// <summary>
		/// Instantiate a new <see cref="AudioPlayCmd"/> instance.
		/// </summary>
		/// <param name="audioData">Audio data.</param>
		/// <param name="isLoop">Should we loop playing the audio(play the audio after it ends)?</param>
		/// <param name="startUpMediaManager">Should we startup the media manager before playing the audio?</param>
		/// <param name="shutdownMediaManager">Should we shut down the media manager after finished playing the audio?</param>
		public AudioPlayCmd(byte[] audioData, bool isLoop = false, bool startUpMediaManager = false, bool shutdownMediaManager = false)
		{
			AudioData = audioData;
			IsLoop = isLoop;
			StartupMediaManager = startUpMediaManager;
			ShutdownMediaManager = shutdownMediaManager;
		}

		public override void DoCommand(RenderTarget renderTarget)
		{
			bool stoppedPlaying = false;
			Media.PlayAudio(AudioData, out var sessionId, IsLoop, StartupMediaManager, new Action(() =>
			{
				if (IsLoop && ShutdownMediaManager)
				{
					MediaManager.Shutdown();
				}
				stoppedPlaying = true;
			}));
			if (sessionId.HasValue)
			{
				new Thread(() =>
				{
					for (; ; )
					{
						if (_isStopDoing)
						{
							Media.StopAudio(sessionId.Value, ShutdownMediaManager);
							break;
						}
						else if (stoppedPlaying)
						{
							break;
						}
					}
				})
				{ IsBackground = true }.Start();
			}
		}
	}
}
