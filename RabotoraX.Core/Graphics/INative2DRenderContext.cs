using System.Numerics;

namespace RabotoraX.Core.Graphics;

public interface INative2DRenderContext : IDisposable
{
	void BeginDraw();
	void EndDraw();
	void Clear(float r, float g, float b, float a);
	
	ITexture2D CreateTexture(string path);
	ITexture2D CreateTexture(byte[] data);
	ITexture2D CreateTexture(Stream stream, bool leaveOpen = false);

	void DrawRectangle(float x, float y, float width, float height, float r, float g, float b, float a, float strokeWidth);
	void FillRectangle(float x, float y, float width, float height, float r, float g, float b, float a);

	void DrawImage(ITexture2D texture, float x, float y, float width, float height, float opacity = 1.0f);
	void DrawText(string text, string fontName, float fontSize, float x, float y, float r, float g, float b, float a);
	
	void SetTransform(Matrix3x2 matrix);
}