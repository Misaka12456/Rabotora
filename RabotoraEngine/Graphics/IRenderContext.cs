using SkiaSharp;
using Color = Vortice.Mathematics.Color;
using Rectangle = Rabotora.Core.Rectangle;

namespace Rabotora.Graphics;

public interface IRenderContext
{
	void BeginFrame();
	void EndFrame();
	void DrawTexture(SKBitmap texture, Rectangle rect, float alpha = 1.0f);
	void Clear(Color color);
	void DrawText(string text, SKFont font, SKColor color, Point position, SKTextAlign textAlign = SKTextAlign.Left, float alpha = 1.0f);
	void SetRenderTarget(object renderTarget);
}