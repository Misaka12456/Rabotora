using SkiaSharp;
using Rectangle = Rabotora.Core.Rectangle;

namespace Rabotora.Graphics;

public interface IRenderContext
{
	void BeginFrame();
	void EndFrame();
	void DrawTexture(SKBitmap texture, Rectangle rect, float alpha = 1.0f);
	void DrawText(string text, SKFont font, SKColor color, Point position, float alpha = 1.0f);
}