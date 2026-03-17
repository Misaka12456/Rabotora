namespace RabotoraX.Core.Graphics;

public static class GraphicsService
{
	public static INativeGraphicsAPI API { get; private set; } = null!;
	public static WindowStateSnapshot LatestWindowState { get; internal set; }
	
	public static void Initialize(INativeGraphicsAPI api)
	{
		if (API != null)
		{
			throw new InvalidOperationException("GraphicsService is already initialized.");
		}
		API = api;
	}
	
	public static void Update(WindowStateSnapshot snapshot)
	{
		if (LatestWindowState.Width != snapshot.Width || LatestWindowState.Height != snapshot.Height)
		{
			if (API is {IsInitialized: true})
			{
				// 这一步会触发 DirectX11.cs 里的 ResizeBuffers
				API.Resize(snapshot.Width, snapshot.Height);
			}
		}
		LatestWindowState = snapshot;
	}
}