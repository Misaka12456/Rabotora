using SkiaSharp;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using Color = Vortice.Mathematics.Color;
using Rectangle = Rabotora.Core.Rectangle;

namespace Rabotora.Graphics;

public class DirectXRenderContext : IRenderContext
{
	private readonly RWindow _window;
	private ID3D11RenderTargetView _currentRenderTarget;

	public DirectXRenderContext(RWindow window)
	{
		_window = window;
		_currentRenderTarget = _window.RenderTarget;
	}

	public void SetRenderTarget(object renderTarget)
	{
		if (renderTarget.GetType() != typeof(ID3D11RenderTargetView))
		{
			throw new ArgumentException("Invalid render target type. Expected ID3D11RenderTargetView.");
		}
		_currentRenderTarget = (renderTarget as ID3D11RenderTargetView) ?? throw new ArgumentNullException(nameof(renderTarget), "Render target cannot be null.");
	}

	public void BeginFrame()
	{
		_window.Device.ImmediateContext.ClearRenderTargetView(_currentRenderTarget, new Color4(0, 0, 0, 1));
		_window.Device.ImmediateContext.OMSetRenderTargets(1, [_currentRenderTarget]);
	}

	public void EndFrame()
	{
		_window.SwapChain.Present(1, PresentFlags.None);
	}

	public void DrawTexture(SKBitmap texture, Rectangle rect, float alpha = 1.0f)
	{
		_window.RenderWithSkia(surface =>
		{
			using var paint = new SKPaint();
			paint.Color = SKColors.White.WithAlpha((byte) (alpha * 255));
			surface.Canvas.DrawBitmap(texture, new SKRect(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height), paint);
		});
	}

	public void Clear(Color color)
	{
		var dxColor4 = new Color4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
		_window.Device.ImmediateContext.ClearRenderTargetView(_currentRenderTarget, dxColor4);
	}

	public void DrawText(string text, SKFont font, SKColor color, Point position, SKTextAlign textAlign = SKTextAlign.Left, float alpha = 1.0f)
	{
		_window.RenderWithSkia(surface =>
		{
			using var paint = new SKPaint();
			paint.Color = color.WithAlpha((byte)(alpha * 255));
			paint.IsAntialias = true;

			// 使用新的 DrawText API
			surface.Canvas.DrawText(text: text, x: position.X, y: position.Y + font.Size, // 调整基线位置
				textAlign, font: font, paint: paint);
		});
	}
}