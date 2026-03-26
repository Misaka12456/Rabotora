using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace RabotoraX.Core.UI;

[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
public abstract class UIInteractable : Component2D
{
	public bool Interactable
	{
		get;
		set
		{
			if (field == value) return;
			field = value;
			OnInteractableChanged();
		}
	} = true;

	public int SortOrder { get; set; } = 0;
	
	public bool BlockRaycast { get; set; } = true;

	public override void OnStart()
	{
		base.OnStart();
		UIEventService.Register(this);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			UIEventService.Unregister(this);
		}
		base.Dispose(disposing);
	}

	[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
	public virtual bool Raycast(Vector2 screenPos)
	{
		if (!Interactable || Layout is not RUILayout uiLayout) return false;
		float scaleFactor = 1.0f;
		var currentParent = Layout.Parent;
		while (currentParent != null)
		{
			if (currentParent.RObject.TryGetComponent<RCanvas>(out var canvas))
			{
				scaleFactor = canvas!.ScaleFactor;
				break;
			}
			currentParent = currentParent.Parent;
		}

		var canvasPos = screenPos / scaleFactor;

		float absoluteX = 0;
		float absoluteY = 0;
		var node = uiLayout;
		while (node != null)
		{
			absoluteX += node.Rect.X;
			absoluteY += node.Rect.Y;
			node = node.Parent as RUILayout;
		}

		var rect = uiLayout.Rect;
		return canvasPos.X >= absoluteX && canvasPos.X <= (absoluteX + rect.Width) &&
		       canvasPos.Y >= absoluteY && canvasPos.Y <= (absoluteY + rect.Height);
	}
	
	internal void TriggerPointerEnter() => OnPointerEnter();
	internal void TriggerPointerExit() => OnPointerExit();
	internal void TriggerPointerDown() => OnPointerDown();
	internal void TriggerPointerUp() => OnPointerUp();
	internal void TriggerPointerClick() => OnPointerClick();
	
	protected virtual void OnPointerEnter() { }
	protected virtual void OnPointerExit() { }
	protected virtual void OnPointerDown() { }
	protected virtual void OnPointerUp() { }
	protected virtual void OnPointerClick() { }
	protected virtual void OnInteractableChanged() { }
}