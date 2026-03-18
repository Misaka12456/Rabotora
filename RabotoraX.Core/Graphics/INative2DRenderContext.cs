using System.Numerics;
using RabotoraX.Core.Mathematics;

namespace RabotoraX.Core.Graphics;

public interface INative2DRenderContext : IDisposable
{
	void BeginDraw();
	void EndDraw();
	void Clear(float r, float g, float b, float a);
	
	INativeTexture2D CreateTexture(string path);
	INativeTexture2D CreateTexture(byte[] data);
	INativeTexture2D CreateTexture(Stream stream, bool leaveOpen = false);
	INativeTexture2D CreateEmptyTexture(int width, int height);

	void DrawRectangle(float x, float y, float width, float height, float r, float g, float b, float a, float strokeWidth);
	void FillRectangle(float x, float y, float width, float height, float r, float g, float b, float a);

	void DrawImage(INativeTexture2D texture, float x, float y, float width, float height, float opacity = 1.0f);
	void DrawImage(INativeTexture2D texture, Rect sourceRect, float x, float y, float width, float height, float opacity = 1.0f);
	void DrawText(string text, string fontName, float fontSize, float x, float y, float r, float g, float b, float a);
	
	void SetTransform(Matrix3x2 matrix);
	void SetShader(INativeShader? shader);
}