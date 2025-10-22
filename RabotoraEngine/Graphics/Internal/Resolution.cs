using Vortice.DXGI;

namespace RabotoraEngine.Graphics.Internal;

public record Resolution
{
	public Resolution(int width, int height) : this(width, height, new Rational(60, 1))
	{
	}
	
	public Resolution(int width, int height, Rational frameRate)
	{
		Width = width;
		Height = height;
		FrameRate = frameRate;
	}

	/// <summary>
	/// The width of the resolution.
	/// </summary>
	public int Width { get; init; }
	
	/// <summary>
	/// The height of the resolution.
	/// </summary>
	public int Height { get; init; }

	public Rational FrameRate { get; init; }

	// ReSharper disable ParameterHidesMember
	public void Deconstruct(out int Width, out int Height, out Rational FrameRate)
	{
		Width = this.Width;
		Height = this.Height;
		FrameRate = this.FrameRate;
	}
}