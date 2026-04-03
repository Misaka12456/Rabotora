namespace RabotoraX.Core.Videos;

public enum VideoPixelFormat
{
	/// <summary>
	/// 32-bit BGRA format (8 bits per channel, with alpha), commonly used for video textures.
	/// </summary>
	Bgra32 = 0,
	/// <summary>
	/// YUV 4:2:0 format, where the Y (luminance) plane is full resolution and the U and V (chrominance) planes are half resolution.<br />
	/// This format is more efficient for video decoding and is widely supported by hardware decoders, but it requires conversion to RGB for rendering, and some platforms may not support it directly as a texture format.
	/// </summary>
	NV12 = 1,
}