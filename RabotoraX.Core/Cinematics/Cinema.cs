namespace RabotoraX.Core.Cinematics;

public static class Cinema
{
	public static RStage? PerformingStage { get; private set; }
		
	public static void Ready(RStage stage)
	{
		PerformingStage?.Dispose();
		PerformingStage = stage;
	}
		
	public static void Update(float deltaTime)
	{
		PerformingStage?.Update(deltaTime);
	}

	public static void Render()
	{
		PerformingStage?.Render();
	}
}