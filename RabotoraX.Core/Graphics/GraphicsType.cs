namespace RabotoraX.Core.Graphics;

public enum GraphicsType
{
	None = 0,
	D3D11 = 1,
	D3D12 = 2,
	OpenGL = 3,
	OpenGLES3 = 4, // for mobile platforms
	Vulkan = 5,
	Metal = 6, // for Apple platforms
}