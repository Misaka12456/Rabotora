namespace RabotoraX.Core.Graphics;

public static class GraphicsManager
{
	public static INativeGraphicsAPI API { get; private set; } = null!;
	
	public static void Initialize(INativeGraphicsAPI api)
	{
		API = api;
	}
}