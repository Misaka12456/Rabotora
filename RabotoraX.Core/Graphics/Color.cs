using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace RabotoraX.Core.Graphics;

/// <summary>
/// Represents a color with red, green, blue, and alpha components. Each component is a float in the range [0, 1].
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public struct Color
{
	public readonly static Color White = new(1, 1, 1);
	public readonly static Color WhiteTransparent = new(1, 1, 1, 0);
	public readonly static Color Black = new(0, 0, 0);
	public readonly static Color Transparent = new(0, 0, 0, 0);
	public readonly static Color Red = new(1, 0, 0);
	public readonly static Color Green = new(0, 1, 0);
	public readonly static Color Blue = new(0, 0, 1);
	
	/// <summary>
	/// The red component of the color, in the range [0, 1].
	/// </summary>
	public float R { get; set; }
	
	/// <summary>
	/// The green component of the color, in the range [0, 1].
	/// </summary>
	public float G { get; set; }
	
	/// <summary>
	/// The blue component of the color, in the range [0, 1].
	/// </summary>
	public float B { get; set; }
	
	/// <summary>
	/// The alpha (opacity) component of the color, in the range [0, 1].
	/// </summary>
	public float A { get; set; }

	public Color(float r, float g, float b, float a = 1.0f)
	{
		R = r;
		G = g;
		B = b;
		A = a;
	}
	
	/// <summary>
	/// Converts a <see cref="Color"/> to a <see cref="Vector4"/>, where the X, Y, Z, and W components correspond to the R, G, B, and A components of the Color, respectively.
	/// </summary>
	/// <param name="c">The <see cref="Color"/> to convert.</param>'
	/// <returns>A <see cref="Vector4"/> representation of the <see cref="Color"/>.</returns>
	public static explicit operator Vector4(Color c) => new(c.R, c.G, c.B, c.A);
	
	/// <summary>
	/// Converts a <see cref="Vector4"/> to a <see cref="Color"/>, where the X, Y, Z, and W components of the Vector4 correspond to the R, G, B, and A components of the Color, respectively.
	/// </summary>
	/// <param name="v">The <see cref="Vector4"/> to convert.</param>
	/// <returns>A <see cref="Color"/> representation of the <see cref="Vector4"/>.</returns>
	public static explicit operator Color(Vector4 v) => new(v.X, v.Y, v.Z, v.W);
	
	public static Color operator *(Color c, float scalar) => new(c.R * scalar, c.G * scalar, c.B * scalar, c.A * scalar);
	public static Color operator *(float scalar, Color c) => c * scalar;
	public static Color operator /(Color c, float scalar) => new(c.R / scalar, c.G / scalar, c.B / scalar, c.A / scalar);
	public static Color operator /(float scalar, Color c) => new(scalar / c.R, scalar / c.G, scalar / c.B, scalar / c.A);
	public static Color operator +(Color a, Color b) => new(a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A);
	public static Color operator -(Color a, Color b) => new(a.R - b.R, a.G - b.G, a.B - b.B, a.A - b.A);
	public static Color operator *(Color a, Color b) => new(a.R * b.R, a.G * b.G, a.B * b.B, a.A * b.A);
	public static Color operator /(Color a, Color b) => new(a.R / b.R, a.G / b.G, a.B / b.B, a.A / b.A);
}