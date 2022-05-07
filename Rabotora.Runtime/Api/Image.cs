using D2D = SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System.Runtime.Versioning;
using System.Drawing;
using Image2 = System.Drawing.Image;
using SharpDX;
using System.Enhance.Drawing;
using System.Enhance.SharpDX.Direct2D;

namespace Rabotora.Runtime.Api
{
	public enum FadeType
	{
		FadeIn = 0,
		FadeOut = 1
	}

	[SupportedOSPlatform("windows")]
	public static class Image
	{
		public static void DrawBitmap(D2D.RenderTarget target, byte[] bitmapData, Size2F windowSize, RawRectangleF rect, float opacity = 1.0f, bool isFlushInstantly = true)
		{
			var stream = new MemoryStream(bitmapData);
			stream.Seek(0, SeekOrigin.Begin);
			var bitmapWrapper = (Bitmap)Image2.FromStream(stream);
			if (bitmapWrapper.Width > windowSize.Width)
			{
				bitmapWrapper = bitmapWrapper.ScaleBitmap((double)windowSize.Width / bitmapWrapper.Width);
			}
			else if (bitmapWrapper.Height > windowSize.Height)
			{
				bitmapWrapper = bitmapWrapper.ScaleBitmap((double)windowSize.Height / bitmapWrapper.Height);
			}
			var bitmap = target.LoadD2DBitmapFromBitmap(bitmapWrapper);
			if (bitmap != null)
			{
				target.DrawBitmap(bitmap, opacity, D2D.BitmapInterpolationMode.Linear, rect);
				if (isFlushInstantly)
				{
					target.Flush();
				}
			}
		}

		public static void DrawBitmap(D2D.RenderTarget target, byte[] bitmapData, RawRectangleF rect, float scaleMultiply, float opacity = 1.0f, bool isFlushInstantly = true)
		{
			var stream = new MemoryStream(bitmapData);
			stream.Seek(0, SeekOrigin.Begin);
			var bitmapWrapper = (Bitmap)Image2.FromStream(stream);
			bitmapWrapper = bitmapWrapper.ScaleBitmap(scaleMultiply);
			var bitmap = target.LoadD2DBitmapFromBitmap(bitmapWrapper);
			if (bitmap != null)
			{
				target.DrawBitmap(bitmap, opacity, D2D.BitmapInterpolationMode.Linear, rect);
				if (isFlushInstantly)
				{
					target.Flush();
				}
			}
		}

		public static void DrawFadeBitmap(D2D.RenderTarget target, byte[] bitmapData, Size2F windowSize, RawRectangleF rect, FadeType type, ref bool isStopDoing, float duration = 2.5f, float segmentAlpha = 0.005f, bool isBeginDrawBeforeStart = false)
		{
			if (segmentAlpha < 0.0f || segmentAlpha > 0.5f)
			{
				throw new ArgumentException("Segment alpha value must in [0.0,0.5].", nameof(segmentAlpha));
			}
			var stream = new MemoryStream(bitmapData);
			stream.Seek(0, SeekOrigin.Begin);
			var bitmapWrapper = (Bitmap)Image2.FromStream(stream);
			if (bitmapWrapper.Width > windowSize.Width)
			{
				bitmapWrapper = bitmapWrapper.ScaleBitmap((double)windowSize.Width / bitmapWrapper.Width);
			}
			else if (bitmapWrapper.Height > windowSize.Height)
			{
				bitmapWrapper = bitmapWrapper.ScaleBitmap((double)windowSize.Height / bitmapWrapper.Height);
			}
			var bitmap = target.LoadD2DBitmapFromBitmap(bitmapWrapper);
			if (bitmap != null)
			{
				if (isBeginDrawBeforeStart)
				{
					target.BeginDraw();
				}
				int waitMS = (int)(duration * 1000 / (1 / segmentAlpha));
				if (type == FadeType.FadeIn)
				{
					for (float i = 0f; i < 1f; i += segmentAlpha)
					{
						if (!isStopDoing)
						{
							target.DrawBitmap(bitmap, i, D2D.BitmapInterpolationMode.Linear, rect);
							target.EndDraw();
							target.BeginDraw();
							Thread.Sleep(waitMS);
						}
						else
						{
							target.DrawBitmap(bitmap, 1.0f, D2D.BitmapInterpolationMode.Linear, rect);
							target.EndDraw();
							target.BeginDraw();
							break;
						}
					}
				}
				else
				{
					for (float i = 1f; i > 0f; i -= segmentAlpha)
					{
						if (!isStopDoing)
						{
							target.DrawBitmap(bitmap, i, D2D.BitmapInterpolationMode.Linear, rect);
							target.EndDraw();
							target.BeginDraw();
							Thread.Sleep(waitMS);
						}
						else
						{
							target.DrawBitmap(bitmap, 0.0f, D2D.BitmapInterpolationMode.Linear, rect);
							target.EndDraw();
							target.BeginDraw();
							break;
						}
					}
				}
			}
		}

		public static void DrawFadeBitmap(D2D.RenderTarget target, byte[] bitmapData, RawRectangleF rect, float scaleMultiply, FadeType type, ref bool isStopDoing, float duration = 2.5f, float segmentAlpha = 0.005f, bool isBeginDrawBeforeStart = false)
		{
			if (segmentAlpha < 0.0f || segmentAlpha > 0.5f)
			{
				throw new ArgumentException("Segment alpha value must in [0.0,0.5].", nameof(segmentAlpha));
			}
			var stream = new MemoryStream(bitmapData);
			stream.Seek(0, SeekOrigin.Begin);
			var bitmapWrapper = (Bitmap)Image2.FromStream(stream);
			bitmapWrapper = bitmapWrapper.ScaleBitmap(scaleMultiply);
			var bitmap = target.LoadD2DBitmapFromBitmap(bitmapWrapper);
			if (bitmap != null)
			{
				if (isBeginDrawBeforeStart)
				{
					target.BeginDraw();
				}
				int waitMS = (int)(duration * 1000 / (1 / segmentAlpha));
				if (type == FadeType.FadeIn)
				{
					for (float i = 0f; i < 1f; i += segmentAlpha)
					{
						if (!isStopDoing)
						{
							target.DrawBitmap(bitmap, i, D2D.BitmapInterpolationMode.Linear, rect);
							target.EndDraw();
							target.BeginDraw();
							Thread.Sleep(waitMS);
						}
						else
						{
							target.DrawBitmap(bitmap, 1.0f, D2D.BitmapInterpolationMode.Linear, rect);
							target.EndDraw();
							target.BeginDraw();
							break;
						}
					}
				}
				else
				{
					for (float i = 1f; i > 0f; i -= segmentAlpha)
					{
						if (!isStopDoing)
						{
							target.DrawBitmap(bitmap, i, D2D.BitmapInterpolationMode.Linear, rect);
							target.EndDraw();
							target.BeginDraw();
							Thread.Sleep(waitMS);
						}
						else
						{
							target.DrawBitmap(bitmap, 0.0f, D2D.BitmapInterpolationMode.Linear, rect);
							target.EndDraw();
							target.BeginDraw();
							break;
						}
					}
				}
			}
		}
	}
}
