namespace RabotoraX.Core.Graphics;

public enum GpuFormat
{
	Unknown,
	R32G32B32_Float, // usually for Position/Normal
	R32G32_Float, // usually for TexCoord
	R32G32B32A32_Float, // usually for Color
	R8G8B8A8_UNorm, // usually for Color (Byte4)
}