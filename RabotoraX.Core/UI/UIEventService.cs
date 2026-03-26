using System.Diagnostics.CodeAnalysis;
using RabotoraX.Core.Inputs;

namespace RabotoraX.Core.UI;

public static class UIEventService
{
	private readonly static List<UIInteractable> _interactables = [];

	private static UIInteractable? _currentHovered, _currentPressed;
	
	public static void Register(UIInteractable interactable)
	{
		if (!_interactables.Contains(interactable))
		{
			_interactables.Add(interactable);
			_interactables.Sort((a, b) => b.SortOrder.CompareTo(a.SortOrder));
		}
	}
	
	public static void Unregister(UIInteractable interactable)
	{
		_interactables.Remove(interactable);
		if (_currentHovered == interactable) _currentHovered = null;
		if (_currentPressed == interactable) _currentPressed = null;
	}

	[SuppressMessage("ReSharper", "ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator")]
	public static void Update()
	{
		UIInteractable? hoveredThisFrame = null;
		var mousePos = Input.MousePosition;

		foreach (var element in _interactables)
		{
			if (element.Raycast(mousePos))
			{
				hoveredThisFrame = element;
				if (element.BlockRaycast) break;
			}
		}

		if (hoveredThisFrame != _currentHovered)
		{
			_currentHovered?.TriggerPointerExit();
			_currentHovered = hoveredThisFrame;
			_currentHovered?.TriggerPointerEnter();
		}

		if (Input.GetMouseButtonDown(0))
		{
			_currentPressed = _currentHovered;
			_currentPressed?.TriggerPointerDown();
		}
		else if (Input.GetMouseButtonUp(0))
		{
			if (_currentPressed != null)
			{
				_currentPressed.TriggerPointerUp();
				if (_currentPressed == _currentHovered)
				{
					_currentPressed.TriggerPointerClick();
				}
				_currentPressed = null;
			}
		}
	}
}