using RabotoraX.Core.Threading;
using ZLinq;

namespace RabotoraX.Core.Cinematics;

public static class Cinema
{
	public static RStage? PerformingStage { get; private set; }
		
	public static void Ready(RStage stage)
	{
		if (!MultiThreadService.IsRenderThread)
		{
			MultiThreadService.Invoke(() => Ready(stage));
			return;
		}
		
		PerformingStage?.Dispose();
		PerformingStage = stage;
		if (PerformingStage == null!) return;
		
		InitializeStage(PerformingStage);
	}

	private static void InitializeStage(RStage stage)
	{
		var allComponents = new List<Component>();
		var objectQueue = new Queue<RObject>(stage.RootObjects);

		while (objectQueue.Count > 0)
		{
			var currentObject = objectQueue.Dequeue();

			foreach (var comp in currentObject.EnumerateComponents(includeInactive: true))
			{
				allComponents.Add(comp);
			}
			
			foreach (var childLayout in currentObject.Layout.Children)
			{
				objectQueue.Enqueue(childLayout.RObject);
			}
		}

		foreach (var comp in allComponents.AsValueEnumerable().Where(comp => !comp.IsAwakened))
		{
			comp.OnAwake();
		}

		foreach (var comp in allComponents.AsValueEnumerable().Where(comp => comp.IsEnabled))
		{
			comp.OnStart();
		}
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