using RabotoraX.Core.Graphics;

namespace RabotoraX.Core.UI;

public enum SelectableTransition
{
	None = 0,
	Opacity = 1,
	TextureSwap = 2
}

public struct ButtonStateTextures
{
	public Texture2D? NormalTexture { get; set; }
	public Texture2D? HoverTexture { get; set; }
	public Texture2D? PressedTexture { get; set; }
	public Texture2D? DisabledTexture { get; set; }
}

public struct ButtonStateOpacities
{
	public float NormalOpacity { get; set; } = 1.0f;
	public float HoverOpacity { get; set; } = 0.8f;
	public float PressedOpacity { get; set; } = 0.6f;
	public float DisabledOpacity { get; set; } = 0.4f;
	
	public ButtonStateOpacities()
	{
	}
}

public class Button : UIInteractable
{
	public SelectableTransition Transition { get; set; } = SelectableTransition.Opacity;
	public UIRenderable? TargetRenderable { get; set; }

	public ButtonStateOpacities StateOpacities { get; set; } = new();
	public ButtonStateTextures StateTextures { get; set; } = new();

	public event Action? EventOnClick, EventOnPointerEnter, EventOnPointerExit;
	
	private bool _isHovered, _isPressed;

	public override void OnStart()
	{
		base.OnStart();
		TargetRenderable ??= GetComponent<UIRenderable>();
		UpdateVisualState();
	}

	protected override void OnInteractableChanged()
	{
		UpdateVisualState();
	}

	protected override void OnPointerEnter()
	{
		_isHovered = true;
		EventOnPointerEnter?.Invoke();
		UpdateVisualState();
	}

	protected override void OnPointerExit()
	{
		_isHovered = false;
		EventOnPointerExit?.Invoke();
		UpdateVisualState();
	}

	protected override void OnPointerDown()
	{
		_isPressed = true;
		UpdateVisualState();
	}

	protected override void OnPointerUp()
	{
		_isPressed = false;
		UpdateVisualState();
	}

	protected override void OnPointerClick()
	{
		if (Interactable)
		{
			EventOnClick?.Invoke();
		}
	}

	private void UpdateVisualState()
	{
		if (TargetRenderable == null) return;

		switch (Transition)
		{
			case SelectableTransition.Opacity:
				TargetRenderable.Opacity = GetCurrentOpacity();
				break;
			case SelectableTransition.TextureSwap:
				var tex = GetCurrentTexture();
				if (tex != null) TargetRenderable.Texture = tex;
				break;
		}
	}

	private float GetCurrentOpacity()
	{
		if (!Interactable) return StateOpacities.DisabledOpacity;
		if (_isPressed) return StateOpacities.PressedOpacity;
		if (_isHovered) return StateOpacities.HoverOpacity;
		return StateOpacities.NormalOpacity;
	}
	
	private Texture2D? GetCurrentTexture()
	{
		if (!Interactable) return StateTextures.DisabledTexture ?? StateTextures.NormalTexture;
		if (_isPressed) return StateTextures.PressedTexture ?? StateTextures.NormalTexture;
		if (_isHovered) return StateTextures.HoverTexture ?? StateTextures.NormalTexture;
		return StateTextures.NormalTexture;
	}
}