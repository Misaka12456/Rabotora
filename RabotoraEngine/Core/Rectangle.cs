using SkiaSharp;

namespace Rabotora.Core;

public readonly struct Rectangle
{
	public Rectangle(int x, int y, int width, int height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	public int X { get; }
	public int Y { get; }
	public int Width { get; }
	public int Height { get; }

	public static implicit operator SKRect(Rectangle rect) => new(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
}