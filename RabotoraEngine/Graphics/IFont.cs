using Vortice.DirectWrite;

namespace RabotoraEngine.Graphics;

public interface IFont : IObject
{
	string FontFamily { get; }
	float Size { get; }
	FontWeight Weight { get; }
	object NativeFont => NativeData;
}