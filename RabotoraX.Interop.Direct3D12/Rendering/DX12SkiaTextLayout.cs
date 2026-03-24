using System.Numerics;
using RabotoraX.Core.UI;
using SkiaSharp;

namespace RabotoraX.Interop.Direct3D12.Rendering;

public class DX12SkiaTextLayout : INativeTextLayout
{
	public string Text { get; }
	public SKFont Font { get; }
	public float FontSize { get; }
	public float MaxWidth { get; }
	public float MaxHeight { get; }
	public Vector2 Size { get; }
	public float LineSpacing => FontSize * 0.09f; // 1.2 is a common line spacing multiplier, you can adjust this as needed

	public DX12SkiaTextLayout(string text, string fontName, float fontSize, float maxWidth, float maxHeight)
	{
		Text = text;
		FontSize = fontSize;
		MaxWidth = maxWidth;
		MaxHeight = maxHeight;
		Font = new SKFont(SKTypeface.FromFamilyName(fontName), fontSize);
        
		using var paint = new SKPaint(Font);
		var rect = new SKRect();
		paint.MeasureText(text, ref rect);
		Size = new Vector2(rect.Width, rect.Height);
	}

	public void Dispose()
	{
		Font.Dispose();
		GC.SuppressFinalize(this);
	}
}