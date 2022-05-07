using Rabotora.Runtime.Api;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System.Runtime.Versioning;

namespace Rabotora.Runtime
{
	public interface IStoppableCmd
	{
		public bool IsStopDoing { get; set; }
	}

	public abstract class RabotoraD2DCmd
	{
		public abstract void DoCommand(RenderTarget renderTarget);
	}

	[SupportedOSPlatform("windows")]
	public class BitmapDrawCmd : RabotoraD2DCmd
	{
		public byte[] BitmapData { get; }

		public float ScaleMultiply { get; } = 1.0f;

		public Size2F? TargetWindowSize { get; } = null;

		public float Opacity { get; }

		public RawRectangleF TargetRect { get; }

		public bool FlushInstantly { get; }

		public BitmapDrawCmd(byte[] bitmapData, RawRectangleF targetRect, float scaleMultiply = 1.0f, float opacity = 1.0f, bool flushInstantly = false)
		{
			BitmapData = bitmapData;
			ScaleMultiply = scaleMultiply;
			Opacity = opacity;
			TargetRect = targetRect;
			FlushInstantly = flushInstantly;
		}

		public BitmapDrawCmd(byte[] bitmapData, RawRectangleF targetRect, Size2F targetWindowSize, float opacity = 1.0f, bool flushInstantly = false)
		{
			BitmapData = bitmapData;
			TargetWindowSize = targetWindowSize;
			if ((targetRect.Right - targetRect.Left) > targetWindowSize.Width)
			{
				ScaleMultiply = targetWindowSize.Width / (targetRect.Right - targetRect.Left);
			}
			else if ((targetRect.Bottom - targetRect.Top) > targetWindowSize.Height)
			{
				ScaleMultiply = targetWindowSize.Height / (targetRect.Bottom - targetRect.Top);
			}
			else
			{
				ScaleMultiply = 1.0f;
			}
			Opacity = opacity;
			TargetRect = targetRect;
			FlushInstantly = flushInstantly;
		}

		public override void DoCommand(RenderTarget renderTarget)
		{
			if (TargetWindowSize.HasValue)
			{
				Api.Image.DrawBitmap(renderTarget, BitmapData, TargetWindowSize.Value, TargetRect, Opacity, FlushInstantly);
			}
			else
			{
				Api.Image.DrawBitmap(renderTarget, BitmapData, TargetRect, ScaleMultiply, Opacity, FlushInstantly);
			}
		}
	}

	[SupportedOSPlatform("windows")]
	public class BitmapFadeDrawCmd : RabotoraD2DCmd, IStoppableCmd
	{

		public byte[] BitmapData { get; }

		public FadeType Type { get; }

		public float ScaleMultiply { get; }

		public Size2F? TargetWindowSize { get; } = null;

		public RawRectangleF TargetRect { get; }

		public float Duration { get; }

		public bool BeginDrawBeforeStart { get; }

		private float SegmentAlpha { get; } = 0.005f;

		public bool IsStopDoing { get => _isStopDoing; set => _isStopDoing = value; }

		public bool _isStopDoing = false;
		private FadeType fadeIn;

		public BitmapFadeDrawCmd(byte[] bitmapData, RawRectangleF targetRect, FadeType type, float scaleMultiply = 1.0f, float duration = 2.5f, float segmentAlpha = 0.005f, bool beginDrawBeforeStart = false)
		{
			BitmapData = bitmapData;
			Type = type;
			ScaleMultiply = scaleMultiply;
			TargetRect = targetRect;
			Duration = duration;
			BeginDrawBeforeStart = beginDrawBeforeStart;
			SegmentAlpha = segmentAlpha;
		}

		public BitmapFadeDrawCmd(byte[] bitmapData, RawRectangleF targetRect, FadeType type, Size2F targetWindowSize, float segmentAlpha = 0.005f, bool beginDrawBeforeStart = false)
		{
			BitmapData = bitmapData;
			Type = type;
			TargetWindowSize = targetWindowSize;
			if ((targetRect.Right - targetRect.Left) > targetWindowSize.Width)
			{
				ScaleMultiply = targetWindowSize.Width / (targetRect.Right - targetRect.Left);
			}
			else if ((targetRect.Bottom - targetRect.Top) > targetWindowSize.Height)
			{
				ScaleMultiply = targetWindowSize.Height / (targetRect.Bottom - targetRect.Top);
			}
			else
			{
				ScaleMultiply = 1.0f;
			}
			TargetRect = targetRect;
			BeginDrawBeforeStart = beginDrawBeforeStart;
			SegmentAlpha = segmentAlpha;
		}

		public BitmapFadeDrawCmd(byte[] bitmapData, RawRectangleF targetRect, FadeType fadeIn, Size2F targetWindowSize)
		{
			BitmapData = bitmapData;
			TargetRect = targetRect;
			this.fadeIn = fadeIn;
			TargetWindowSize = targetWindowSize;
		}

		public override void DoCommand(RenderTarget renderTarget)
		{
			if (TargetWindowSize.HasValue)
			{
				Api.Image.DrawFadeBitmap(renderTarget, BitmapData, TargetWindowSize.Value, TargetRect, Type, ref _isStopDoing, Duration, SegmentAlpha, BeginDrawBeforeStart);
			}
			else
			{
				Api.Image.DrawFadeBitmap(renderTarget, BitmapData, TargetRect, ScaleMultiply, Type, ref _isStopDoing, Duration, SegmentAlpha, BeginDrawBeforeStart);
			}
		}
	}
}
