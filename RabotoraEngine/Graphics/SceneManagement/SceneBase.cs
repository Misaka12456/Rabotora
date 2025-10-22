using RabotoraEngine.Graphics.Components;
using RabotoraEngine.Graphics.Internal;

namespace RabotoraEngine.Graphics.SceneManagement;

public abstract class SceneBase : IScene
{
	protected readonly List<UIComponent> _components = [];
	protected bool _isDisposed;
	
	protected SceneManager SceneManager { get; }
	
	protected SceneBase(SceneManager sceneManager)
	{
		SceneManager = sceneManager ?? throw new ArgumentNullException(nameof(sceneManager));
	}

	public virtual void Load(IRenderContext context)
	{
		
	}
	
	public virtual void Unload()
	{
		
	}
	
	public virtual void Update(float deltaTime)
	{
		foreach (var component in _components)
		{
			component.Update(deltaTime);
		}
		foreach (var component in _components)
		{
			component.PostUpdate(deltaTime);
		}
	}

	public virtual void Render(IRenderContext context)
	{
		foreach (var component in _components.Where(c => c.IsVisible))
		{
			component.Render(context);
		}
	}
	
	public void AddComponent(UIComponent component) => _components.Add(component);
	public void RemoveComponent(UIComponent component) => _components.Remove(component);

	public virtual void Dispose()
	{
		if (_isDisposed) return;

		foreach (var component in _components)
		{
			component.Dispose();
		}
		_components.Clear();
		_isDisposed = true;
		GC.SuppressFinalize(this);
	}
}