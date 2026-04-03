using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using RabotoraX.Core.Cinematics;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.Scripting;
using RabotoraX.Core.Serialization;
using RabotoraX.Core.UI;

namespace RabotoraX.Core;

[SuppressMessage("Usage", "RAT0002")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class RObject : Object
{
	[RNonSerialized] public readonly static RObjectEqualityComparer EqualityComparer = new();
	
	public string Name { get; set; }
	public bool IsActive { get; set; } = true;
	public RLayout Layout { get; private set; }
	public RStage Stage { get; internal set; } = null!;
	
	private readonly List<Component> _components = [];
	private readonly List<RCoroutine> _coroutines = [];
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
		if (typeof(Component2D).IsAssignableFrom(typeof(T)) || typeof(T) == typeof(RUILayout))
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

		if (typeof(T) == typeof(RUILayout)) return (T)(Component)Layout;
		var component = new T { RObject = this };
		_components.Add(component);
		component.OnAwake();
		
		return component;
	}
	
	internal void UnsafeAddUninitializedComponent(Component component)
	{
		component.RObject = this;
		_components.Add(component);
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
	
	public T? GetComponentInParent<T>() where T : Component
	{
		foreach (var component in _components)
		{
			if (component is T tComponent)
			{
				return tComponent;
			}
		}
		
		if (Layout.Parent != null)
		{
			return Layout.Parent.RObject.GetComponentInParent<T>();
		}
		
		return null;
	}
	
	public bool TryGetComponent<T>(out T? component) where T : Component
	{
		component = GetComponent<T>();
		return component != null;
	}
	
	public bool TryGetComponentInChildren<T>(out T? component) where T : Component
	{
		component = GetComponentInChildren<T>();
		return component != null;
	}
	
	public bool TryGetComponentInParent<T>(out T? component) where T : Component
	{
		component = GetComponentInParent<T>();
		return component != null;
	}
	
	public IEnumerable<Component> EnumerateComponents(bool includeInactive = false, bool recursive = false)
	{
		foreach (var component in _components)
		{
			if (!includeInactive && !component.IsEnabled) continue;
			yield return component;
		}
		
		if (recursive)
		{
			foreach (var child in Layout.Children)
			{
				foreach (var component in child.RObject.EnumerateComponents(includeInactive, true))
				{
					yield return component;
				}
			}
		}
	}
	
	public void RemoveComponent(Component component)
	{
		if (component is RLayout)
		{
			throw new InvalidOperationException("Cannot remove the layout component from an object.");
		}
		
		if (_components.Remove(component))
		{
			component.Dispose();
		}
	}
	
	internal void RemoveComponentWithoutDispose(Component component)
	{
		if (component is RLayout)
		{
			throw new InvalidOperationException("Cannot remove the layout component from an object.");
		}
		
		_components.Remove(component);
	}

	public RCoroutine StartCoroutine(IEnumerator routine)
	{
		var coroutine = new RCoroutine(routine, this);
		_coroutines.Add(coroutine);
		return coroutine;
	}
	
	public void StopCoroutine(RCoroutine coroutine)
	{
		coroutine.Stop();
		_coroutines.Remove(coroutine);
	}
	
	public void StopAllCoroutines()
	{
		foreach (var coroutine in _coroutines)
		{
			coroutine.Stop();
		}
		_coroutines.Clear();
	}
	
	internal void Update(float deltaTime)
	{
		if (!IsActive) return;
		
		UpdateCoroutines(deltaTime);
		UpdateComponents(deltaTime);
		UpdateChildren(deltaTime);
	}

	private void UpdateCoroutines(float deltaTime)
	{
		for (int i = _coroutines.Count - 1; i >= 0; i--)
		{
			var coroutine = _coroutines[i];

			if (!coroutine.IsRunning)
			{
				_components.RemoveAt(i);
				continue;
			}

			bool shouldMoveNext = true;
			if (coroutine.Routine.Current is RYieldData yieldData)
			{
				shouldMoveNext = !yieldData.KeepWaiting(deltaTime);
			}

			if (shouldMoveNext)
			{
				if (!coroutine.Routine.MoveNext())
				{
					coroutine.IsRunning = false;
					_coroutines.RemoveAt(i);
				}
			}
		}
	}

	private void UpdateComponents(float deltaTime)
	{
		// foreach (var component in _components)
		// {
		// 	if (!component.IsEnabled) continue;
		// 	
		// 	if (!_isStarted)
		// 	{
		// 		component.OnStart();
		// 	}
		// 	component.OnUpdate(deltaTime); <- possible System.InvalidOperationException: Collection was modified; enumeration operation may not execute.
		// }
		// ReSharper disable once ForCanBeConvertedToForeach <- possible System.InvalidOperationException: Collection was modified; enumeration operation may not execute.
		for (int i = 0; i < _components.Count; i++)
		{
			var component = _components[i];
			if (!component.IsEnabled) continue;
			
			if (!_isStarted)
			{
				component.OnStart();
			}
			component.OnUpdate(deltaTime);
		}
		_isStarted = true;
	}
	
	private void UpdateChildren(float deltaTime)
	{
		// foreach (var child in Layout.Children)
		// {
		// 	child.RObject.Update(deltaTime); <- possible System.InvalidOperationException: Collection was modified; enumeration operation may not execute.
		// }
		// ReSharper disable once ForCanBeConvertedToForeach <- possible System.InvalidOperationException: Collection was modified; enumeration operation may not execute.
		for (int i = 0; i < Layout.Children.Count; i++)
		{
			var child = Layout.Children[i];
			child.RObject.Update(deltaTime);
		}
	}

	internal void Render(INativeCommandList cmd)
	{
		if (!IsActive) return;
		foreach (var component in _components)
		{
			if (!component.IsEnabled) continue;
			
			component.OnRender(cmd);
		}
		
		foreach (var child in Layout.Children)
		{
			child.RObject.Render(cmd);
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
			StopAllCoroutines();
			// foreach (var component in _components.ToImmutableArray())
			// {
			// 	if (component is RScript script)
			// 	{
			// 		script.OnDestroy();
			// 	}
			// 	component.Dispose(); <- System.InvalidOperationException: Collection was modified; enumeration operation may not execute.
			// }
			for (int i = _components.Count - 1; i >= 0; i--)
			{
				var component = _components[i];
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

public struct RObjectEqualityComparer : IEqualityComparer<RObject>
{
	public bool Equals(RObject? x, RObject? y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x is null || y is null) return false;
		return ReferenceEquals(x, y);
	}

	public int GetHashCode(RObject obj)
	{
		return RuntimeHelpers.GetHashCode(obj);
	}
}