using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System.Runtime.Versioning;

namespace Rabotora.Runtime
{
	/// <summary>
	/// Represents a bitmap drawing command.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public class BitmapDrawCmd : RabotoraD2DCmd
	{
		/// <summary>
		/// Bitmap data.
		/// </summary>
		public byte[] BitmapData { get; }

		/// <summary>
		/// Multiple which is used to scale the bitmap.
		/// </summary>
		public float ScaleMultiple { get; } = 1.0f;

		/// <summary>
		/// Destination window size.
		/// <para>
		/// This can be <see langword="null" /> when not give the window size while instantiating current <see cref="BitmapDrawCmd"/> instance.
		/// </para>
		/// </summary>
		public Size2F? TargetWindowSize { get; } = null;

		/// <summary>
		/// Opacity which is used to draw the bitmap.
		/// </summary>
		public float Opacity { get; }

		/// <summary>
		/// The destination rectangle range on the render target(window) which is ready to be drawn by the bitmap.
		/// </summary>
		public RawRectangleF TargetRect { get; }

		/// <summary>
		/// Should we flush render target instantly after drawing the bitmap?
		/// </summary>
		public bool FlushInstantly { get; }

		/// <summary>
		/// Instantiate a new <see cref="BitmapDrawCmd"/> instance.
		/// </summary>
		/// <param name="bitmapData">Bitmap data.</param>
		/// <param name="targetRect">Which rectangle range on the render target(window) should be drawn by the bitmap?</param>
		/// <param name="scaleMultiple">Which multiple should be used to scale the bitmap?</param>
		/// <param name="opacity">What opacity we should use to draw the bitmap? (Range: [0,1])</param>
		/// <param name="flushInstantly">Should we flush render target instantly after drawing the bitmap?</param>
		public BitmapDrawCmd(byte[] bitmapData, RawRectangleF targetRect, float scaleMultiple = 1.0f, float opacity = 1.0f, bool flushInstantly = false)
		{
			BitmapData = bitmapData;
			ScaleMultiple = scaleMultiple;
			Opacity = opacity;
			TargetRect = targetRect;
			FlushInstantly = flushInstantly;
		}

		/// <summary>
		/// Instantiate a new <see cref="BitmapDrawCmd"/> instance.
		/// </summary>
		/// <param name="bitmapData">Bitmap data.</param>
		/// <param name="targetRect">Which rectangle range on the render target(window) should be drawn by the bitmap?</param>
		/// <param name="targetWindowSize">Destination window size.</param>
		/// <param name="opacity">What opacity we should use to draw the bitmap? (Range: [0,1])</param>
		/// <param name="flushInstantly">Should we flush render target instantly after drawing the bitmap?</param>
		public BitmapDrawCmd(byte[] bitmapData, RawRectangleF targetRect, Size2F targetWindowSize, float opacity = 1.0f, bool flushInstantly = false)
		{
			BitmapData = bitmapData;
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
				Api.Image.DrawBitmap(renderTarget, BitmapData, TargetRect, ScaleMultiple, Opacity, FlushInstantly);
			}
		}
	}
