using JetBrains.Annotations;

namespace RabotoraX.Core.Utility;

[MustDisposeResource]
public class AutoScope : IDisposable
{
	public Action? OnEnter { get; set; }
	public Action? OnExit { get; set; }
	
	public AutoScope(Action? onEnter = null, Action? onExit = null)
	{
		OnEnter = onEnter;
		OnExit = onExit;
		OnEnter?.Invoke();
	}

	public void Dispose()
	{
		OnExit?.Invoke();
		GC.SuppressFinalize(this);
	}
}