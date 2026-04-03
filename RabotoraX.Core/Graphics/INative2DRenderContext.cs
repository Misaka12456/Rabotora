using System.Numerics;
using RabotoraX.Core.Mathematics;
using RabotoraX.Core.UI;
using RabotoraX.Core.Videos;

namespace RabotoraX.Core.Graphics;

public interface INative2DRenderContext : IDisposable
{
	void BeginDraw();
	void EndDraw();
	void Clear(float r, float g, float b, float a);

	INativeTexture2D CreateTexture(Stream stream, bool leaveOpen = false);
	INativeTexture2D CreateTexture(int width, int height, ReadOnlyMemory<byte> pixelData);
	/// <summary>
	/// Creates a texture that can be used for video frames. <br />
	/// Regardless of the underlying API, this texture should use Ignore alpha mode (while the regular one uses premultiplied alpha) and be optimized for dynamic updates.
	/// </summary>
	/// <param name="width">The width of the texture in pixels, must be greater than 0.</param>
	/// <param name="height">The height of the texture in pixels, must be greater than 0.</param>
	/// <param name="initialData">
	/// The initial pixel data for the texture, in BGRA format (4 bytes per pixel).<br />
	/// The length of this data must be equal to width * height * 4.<br />
	/// This parameter is optional; if not provided, the texture will be initialized with undefined content.
	/// </param>
	/// <returns>The created video texture, which should be optimized for dynamic updates and use Ignore alpha mode.</returns>
	INativeTexture2D CreateVideoTexture(int width, int height, ReadOnlyMemory<byte>? initialData = null);
	INativeTexture2D CreateVideoTexture(int width, int height, VideoPixelFormat format, ReadOnlyMemory<byte>? initialData = null);

	INativeTexture2D CreateEmptyTexture(int width, int height);
	
	INativeTextLayout CreateTextLayout(string text, string fontName, float fontSize, float maxWidth = float.MaxValue, float maxHeight = float.MaxValue);

	void DrawRectangle(float x, float y, float width, float height, float r, float g, float b, float a, float strokeWidth);
	void FillRectangle(float x, float y, float width, float height, float r, float g, float b, float a);

	void DrawImage(INativeTexture2D texture, float x, float y, float width, float height, float opacity = 1.0f);
	void DrawImage(INativeTexture2D texture, Rect sourceRect, float x, float y, float width, float height, float opacity = 1.0f);
	void DrawText(string text, string fontName, float fontSize, float x, float y, float r, float g, float b, float a);
	void DrawTextLayout(INativeTextLayout textLayout, float x, float y, float r, float g, float b, float a);
	
	void SetTransform(Matrix3x2 matrix);
	void SetShader(INativeShader? shader);
}