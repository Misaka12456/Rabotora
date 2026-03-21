using RabotoraX.Core.Cinematics;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.Scripting;
using RabotoraX.Core.UI;

namespace RabotoraX.Core;

public sealed class RObject : Object
{
	public string Name { get; set; }
	public bool IsActive { get; set; } = true;
	public RLayout Layout { get; private set; }
	public RStage Stage { get; internal set; } = null!;
	
	private readonly List<Component> _components = [];
	private bool _isStarted;

	public RObject(string name)
	{
		Name = name;
		Layout = new RLayout()
		{
			RObject = this
		};
		
		_components.Add(Layout);
		Layout.OnAwake();
	}

	public RObject(string name, RLayout layout)
	{
		Name = name;
		Layout = layout;
		Layout.RObject = this;

		_components.Add(Layout);
		Layout.OnAwake();
	}

	public T AddComponent<T>() where T : Component, new()
	{
		if (typeof(RLayout).IsAssignableFrom(typeof(T)))
		{
			throw new InvalidOperationException("Layout components cannot be added manually.");
		}
		if (typeof(Component2D).IsAssignableFrom(typeof(T)))
		{
			if (Stage.Type is StageType.Render2D or StageType.Render3DHybrid)
			{
				if (Layout is not RUILayout)
				{
					ReplaceLayout(new RUILayout());
				}
			}
			else
			{
				throw new InvalidOperationException($"Cannot add a 2D component to an object in a {Stage.Type} stage.");
			}
		}
		var component = new T { RObject = this };
		_components.Add(component);
		component.OnAwake();
		
		return component;
	}
	
	public T? GetComponent<T>() where T : Component
	{
		return _components.OfType<T>().FirstOrDefault();
	}
	
	public T? GetComponentInChildren<T>() where T : Component
	{
		foreach (var component in _components)
		{
			if (component is T tComponent)
			{
				return tComponent;
			}
		}
		
		foreach (var child in Layout.Children)
		{
			var result = child.RObject.GetComponentInChildren<T>();
			if (result != null) return result;
		}
		
		return null;
	}

	internal void Update(float deltaTime)
	{
		if (!IsActive) return;
		
		foreach (var component in _components)
		{
			if (!component.IsEnabled) continue;
			
			if (!_isStarted)
			{
				component.OnStart();
			}
			component.OnUpdate(deltaTime);
		}
		_isStarted = true;
		
		foreach (var child in Layout.Children)
		{
			child.RObject.Update(deltaTime);
		}
	}

	internal void Render()
	{
		if (!IsActive) return;
		foreach (var component in _components)
		{
			if (!component.IsEnabled) continue;
			
			component.OnRender();
		}
		
		foreach (var child in Layout.Children)
		{
			child.RObject.Render();
		}
	}
	
	internal void Render2D(INative2DRenderContext context)
	{
		if (!IsActive) return;
		foreach (var component in _components)
		{
			if (!component.IsEnabled) continue;
			
			component.OnRender2D(context);
		}
		
		foreach (var child in Layout.Children)
		{
			child.RObject.Render2D(context);
		}
	}

	private void ReplaceLayout(RLayout newLayout)
	{
		var oldLayout = Layout;
		
		newLayout.Parent = oldLayout.Parent;
		(newLayout.Children as List<RLayout>)?.AddRange(oldLayout.Children);

		foreach (var child in newLayout.Children)
		{
			child.Parent = newLayout;
		}
		
		_components.Remove(oldLayout);
		Layout = newLayout;
		newLayout.RObject = this;
		
		_components.Insert(0, newLayout);
		oldLayout.Dispose();
		newLayout.OnAwake();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			foreach (var component in _components)
			{
				if (component is RScript script)
				{
					script.OnDestroy();
				}
				component.Dispose();
			}
			_components.Clear();
			
			for (int i = Layout.Children.Count - 1; i >= 0; i--)
			{
				Layout.Children[i].RObject.Dispose();
			}
		}
		base.Dispose(disposing);
	}
}