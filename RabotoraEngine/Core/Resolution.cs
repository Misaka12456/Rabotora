using Vortice.DXGI;

namespace Rabotora.Core;

public record Resolution
{
	public Resolution(int width, int height) : this(width, height, new Rational(60, 1)) { }

	public Resolution(int Width, int Height, Rational Rate)
	{
		this.Width = Width;
		this.Height = Height;
		this.Rate = Rate;
	}

	/// <summary>
	/// The width of the resolution.
	/// </summary>
	public int Width { get; init; }
	
	/// <summary>
	/// The height of the resolution.
	/// </summary>
	public int Height { get; init; }
	
	/// <summary>
	/// The refresh rate of the resolution, default as 60Hz (60/1).
	/// </summary>
	public Rational Rate { get; init; }

	// ReSharper disable ParameterHidesMember
	public void Deconstruct(out int Width, out int Height, out Rational Rate)
	{
		Width = this.Width;
		Height = this.Height;
		Rate = this.Rate;
	}
}