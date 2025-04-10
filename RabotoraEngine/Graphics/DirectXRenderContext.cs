using SkiaSharp;
using Vortice.DXGI;
using Vortice.Mathematics;
using Rectangle = Rabotora.Core.Rectangle;

namespace Rabotora.Graphics;

public class DirectXRenderContext : IRenderContext
{
	private readonly RWindow _window;
	
	public DirectXRenderContext(RWindow window) => _window = window;

	public void BeginFrame()
	{
		_window.Device.ImmediateContext.ClearRenderTargetView(_window.RenderTarget, new Color4(0, 0, 0, 1));
	}

	public void EndFrame()
	{
		_window.SwapChain.Present(1, PresentFlags.None);
	}

	public void DrawTexture(SKBitmap texture, Rectangle rect, float alpha = 1.0f)
	{
		_window.RenderWithSkia(surface =>
		{
			using var paint = new SKPaint {Color = SKColors.White.WithAlpha((byte) (alpha * 255))};
			surface.Canvas.DrawBitmap(texture, new SKRect(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height), paint);
		});
	}

	public void DrawText(string text, SKFont font, SKColor color, Point position, float alpha = 1)
	{
		// TODO.
		throw new NotImplementedException("stub. not implemented.");
	}
}