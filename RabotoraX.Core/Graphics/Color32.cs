using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using MemoryPack;

namespace RabotoraX.Core.Graphics;

/// <summary>
/// Represents a color with red, green, blue, and alpha components. Each component is a byte in the range [0, 255].
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[MemoryPackable]
public partial struct Color32
{
	public readonly static Color32 White = new(0xFF, 0xFF, 0xFF);
	public readonly static Color32 WhiteTransparent = new(0xFF, 0xFF, 0xFF, 0x00);
	public readonly static Color32 Black = new(0x00, 0x00, 0x00);
	public readonly static Color32 Transparent = new(0x00, 0x00, 0x00, 0x00);
	public readonly static Color32 Red = new(0xFF, 0x00, 0x00);
	public readonly static Color32 Green = new(0x00, 0xFF, 0x00);
	public readonly static Color32 Blue = new(0x00, 0x00, 0xFF);
	
	/// <summary>
	/// The red component of the color, in the range [0, 255].
	/// </summary>
	public byte R { get; set; }
	
	/// <summary>
	/// The green component of the color, in the range [0, 255].
	/// </summary>
	public byte G { get; set; }
	
	/// <summary>
	/// The blue component of the color, in the range [0, 255].
	/// </summary>
	public byte B { get; set; }
	
	/// <summary>
	/// The alpha (opacity) component of the color, in the range [0, 255].
	/// </summary>
	public byte A { get; set; }

	public Color32(byte r, byte g, byte b, byte a = 255)
	{
		R = r;
		G = g;
		B = b;
		A = a;
	}
	
	/// <summary>
	/// Converts a <see cref="Color32"/> to a <see cref="Color"/>, where each component of the Color is the corresponding component of the Color32 divided by 255.
	/// </summary>
	/// <param name="c">The <see cref="Color32"/> to convert.</param>
	/// <returns>A <see cref="Color"/> representation of the <see cref="Color32"/>.</returns>
	public static explicit operator Color(Color32 c) => new(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
	
	/// <summary>
	/// Converts a <see cref="Color"/> to a <see cref="Color32"/>, where each component of the Color32 is the corresponding component of the Color multiplied by 255 and cast to a byte.
	/// </summary>
	/// <param name="c">The <see cref="Color"/> to convert.</param>
	/// <returns>A <see cref="Color32"/> representation of the <see cref="Color"/>.</returns>
	public static explicit operator Color32(Color c) => new((byte)(c.R * 255), (byte)(c.G * 255), (byte)(c.B * 255), (byte)(c.A * 255));
	
	/// <summary>
	/// Converts a <see cref="Color32"/> to a <see cref="Vector4"/>, where the X, Y, Z, and W components of the Vector4 correspond to the R, G, B, and A components of the Color32 divided by 255, respectively.
	/// </summary>
	/// <param name="c">The <see cref="Color32"/> to convert.</param>
	/// <returns>A <see cref="Vector4"/> representation of the <see cref="Color32"/>.</returns>
	public static explicit operator Vector4(Color32 c) => (Vector4)(Color)c;
	
	/// <summary>
	/// Converts a <see cref="Vector4"/> to a <see cref="Color32"/>, where each component of the Color32 is the corresponding component of the Vector4 multiplied by 255 and cast to a byte.
	/// </summary>
	/// <param name="v">The <see cref="Vector4"/> to convert.</param>
	/// <returns>A <see cref="Color32"/> representation of the <see cref="Vector4"/>.</returns>
	public static explicit operator Color32(Vector4 v) => (Color32)(Color)v;
}