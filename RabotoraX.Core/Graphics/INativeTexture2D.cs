namespace RabotoraX.Core.Graphics;

public interface INativeTexture2D : IDisposable
{
	int Width { get; }
	int Height { get; }
}