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

	/// <summary>
	/// Presents Image Api for Rabotora Game Project which is in Runtime Environment.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static class Image
	{
		/// <summary>
		/// Draw a bitmap on the Render Target (Game Launcher Window).
		/// </summary>
		/// <param name="target">Destination render target which is ready to be drawn.</param>
		/// <param name="bitmapData">Bitmap data.</param>
		/// <param name="windowSize">Destination window size.</param>
		/// <param name="rect">Which rectangle range on the render target(window) should be drawn by the bitmap?</param>
		/// <param name="opacity">What opacity we should use to draw the bitmap? (Range: [0,1])</param>
		/// <param name="isFlushInstantly">Should we flush render target instantly after drawing the bitmap?</param>
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

		/// <summary>
		/// Draw a bitmap on the Render Target (Game Launcher Window).
		/// </summary>
		/// <param name="target">Destination render target which is ready to be drawn.</param>
		/// <param name="bitmapData">Bitmap data.</param>
		/// <param name="rect">Which rectangle range on the render target(window) should be drawn by the bitmap?</param>
		/// <param name="scaleMultiple">Which multiple should be used to scale the bitmap?</param>
		/// <param name="opacity">What opacity we should use to draw the bitmap? (Range: [0,1])</param>
		/// <param name="isFlushInstantly">Should we flush render target instantly after drawing the bitmap?</param>
		public static void DrawBitmap(D2D.RenderTarget target, byte[] bitmapData, RawRectangleF rect, float scaleMultiple, float opacity = 1.0f, bool isFlushInstantly = true)
		{
			var stream = new MemoryStream(bitmapData);
			stream.Seek(0, SeekOrigin.Begin);
			var bitmapWrapper = (Bitmap)Image2.FromStream(stream);
			bitmapWrapper = bitmapWrapper.ScaleBitmap(scaleMultiple);
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

		/// <summary>
		/// Draw a fading bitmap animation on the Render Target (Game Launcher Window).
		/// </summary>
		/// <param name="target">Destination render target which is ready to be drawn.</param>
		/// <param name="bitmapData">Bitmap data.</param>
		/// <param name="windowSize">Destination window size.</param>
		/// <param name="rect">Which rectangle range on the render target(window) should be drawn by the bitmap?</param>
		/// <param name="type">Which type of fade should we use to do the animation?</param>
		/// <param name="isStopDoing">
		/// The variable reference of the sign which indicates 'Should we stop fading animation now'.
		/// <para>Project Rabotora Launcher will <b>instantly finish</b> fading bitmap draw process when drawing fading bitmap and <b>player clicks in the window of Rabotora Launcher</b>.<br />
		/// This argument will pass the 'Instant finish' command from caller to animation method.</para>
		/// <para>If you do not want player finish the animation by clicking in the window, please give a <see langword="bool" /> variable whose value is <b>fixed <see langword="false" /></b> .</para>
		/// </param>
		/// <param name="duration">The animation duration of drawing fading bitmap in seconds. Default is <see langword="2.5f" /> .</param>
		/// <param name="segmentAlpha">How much alpha we should increase/decrease in the every round of inner drawing circulation? (Range: [0, 0.5]; Default is <see langword="0.005f" /> .)</param>
		/// <param name="isBeginDrawBeforeStart">Should we called <see cref="D2D.RenderTarget.BeginDraw()"/> before starting fading animation? (Default is <see langword="false" /> .)</param>
		/// <exception cref="ArgumentException" />
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

		/// <summary>
		/// Draw a fading bitmap animation on the Render Target (Game Launcher Window).
		/// </summary>
		/// <param name="target">Destination render target which is ready to be drawn.</param>
		/// <param name="bitmapData">Bitmap data.</param>
		/// <param name="scaleMultiple">Which multiple should be used to scale the bitmap?</param>
		/// <param name="rect">Which rectangle range on the render target(window) should be drawn by the bitmap?</param>
		/// <param name="type">Which type of fade should we use to do the animation?</param>
		/// <param name="isStopDoing">
		/// The variable reference of the sign which indicates 'Should we stop fading animation now'.
		/// <para>Project Rabotora Launcher will <b>instantly finish</b> fading bitmap draw process when drawing fading bitmap and <b>player clicks in the window of Rabotora Launcher</b>.<br />
		/// This argument will pass the 'Instant finish' command from caller to animation method.</para>
		/// <para>If you do not want player finish the animation by clicking in the window, please give a <see langword="bool" /> variable whose value is <b>fixed <see langword="false" /></b> .</para>
		/// </param>
		/// <param name="duration">The animation duration of drawing fading bitmap in seconds. Default is <see langword="2.5f" /> .</param>
		/// <param name="segmentAlpha">How much alpha we should increase/decrease in the every round of inner drawing circulation? (Range: [0, 0.5]; Default is <see langword="0.005f" /> .)</param>
		/// <param name="isBeginDrawBeforeStart">Should we called <see cref="D2D.RenderTarget.BeginDraw()"/> before starting fading animation? (Default is <see langword="false" /> .)</param>
		/// <exception cref="ArgumentException" />
		public static void DrawFadeBitmap(D2D.RenderTarget target, byte[] bitmapData, RawRectangleF rect, float scaleMultiple, FadeType type, ref bool isStopDoing, float duration = 2.5f, float segmentAlpha = 0.005f, bool isBeginDrawBeforeStart = false)
		{
			if (segmentAlpha < 0.0f || segmentAlpha > 0.5f)
			{
				throw new ArgumentException("Segment alpha value must in [0.0,0.5].", nameof(segmentAlpha));
			}
			var stream = new MemoryStream(bitmapData);
			stream.Seek(0, SeekOrigin.Begin);
			var bitmapWrapper = (Bitmap)Image2.FromStream(stream);
			bitmapWrapper = bitmapWrapper.ScaleBitmap(scaleMultiple);
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
