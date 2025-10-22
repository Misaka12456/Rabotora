namespace RabotoraEngine.Graphics;

public interface ITexture2D : IObject
{
	int Width { get; }
	int Height { get; }
	object NativeTexture => NativeData;
}