using System.Numerics;
using JetBrains.Annotations;
using RabotoraX.Core.Mathematics;
using ZLinq;

namespace RabotoraX.Core.UI;

/// <summary>
/// Represents a layout group component that can automatically arrange its child <see cref="RUILayout"/> components in a specific manner (e.g., horizontally or vertically).
/// </summary>
public abstract class UILayoutGroup : Component2D
{
	protected RUILayout UILayout { get; private set; } = null!;

	[UsedImplicitly]
	public float PaddingLeft
	{
		get;
		set
		{
			if (RMath.Approx(field, value, RMath.CommonTolerance)) return;
			field = value;
			Invalidate();
		}
	} = 0;
	
	[UsedImplicitly]
	public float PaddingRight
	{
		get;
		set
		{
			if (RMath.Approx(field, value, RMath.CommonTolerance)) return;
			field = value;
			Invalidate();
		}
	} = 0;
	
	[UsedImplicitly]
	public float PaddingTop
	{
		get;
		set
		{
			if (RMath.Approx(field, value, RMath.CommonTolerance)) return;
			field = value;
			Invalidate();
		}
	} = 0;
	
	[UsedImplicitly]
	public float PaddingBottom
	{
		get;
		set
		{
			if (RMath.Approx(field, value, RMath.CommonTolerance)) return;
			field = value;
			Invalidate();
		}
	} = 0;

	[UsedImplicitly]
	public Vector2 Spacing
	{
		get;
		set
		{
			if (RMath.Approx(field, value, RMath.CommonTolerance)) return; // RMath.Approx supports Vector2 overload
			field = value;
			Invalidate();
		}
	} = Vector2.Zero;
	
	[UsedImplicitly]
	public UIAlignment ChildAlignment
	{
		get;
		set
		{
			if (field == value) return;
			field = value;
			Invalidate();
		}
	} = UIAlignment.UpperLeft;
	
	[UsedImplicitly]
	public bool ControlChildWidth
	{
		get;
		set
		{
			if (field == value) return;
			field = value;
			Invalidate();
		}
	} = true; // 控制子物体大小 (Control Children Size)
	
	[UsedImplicitly]
	public bool ControlChildHeight
	{
		get;
		set
		{
			if (field == value) return;
			field = value;
			Invalidate();
		}
	} = true;
	
	[UsedImplicitly]
	public bool ChildForceExpandWidth
	{
		get;
		set
		{
			if (field == value) return;
			field = value;
			Invalidate();
		}
	} = false; // 子力扩展 (Child Force Expand)
	
	[UsedImplicitly]
	public bool ChildForceExpandHeight
	{
		get;
		set
		{
			if (field == value) return;
			field = value;
			Invalidate();
		}
	} = false;
	
	private bool _isDirty = true;
	private Vector2 _lastUILayoutSize = new(-1, -1);
	private int _lastChildCount = -1;

    public override void OnAwake()
    {
        base.OnAwake();
        if (Layout is not RUILayout uiLayout)
		{
			throw new InvalidOperationException("UILayoutGroup must be attached to an object with RUILayout.");
		}
		UILayout = uiLayout;
    }

    public override void OnUpdate(float deltaTime)
	{
		base.OnUpdate(deltaTime);

		TryRefreshState();
		if (_isDirty)
		{
			RecalculateLayout();
		}
	}

	protected void TryRefreshState()
	{
		bool stateChanged = false;
		
		var currentSize = UILayout.Size;
		if (_lastUILayoutSize != currentSize)
		{
			_lastUILayoutSize = currentSize;
			stateChanged = true;
		}

		var childrenCount = UILayout.Children.AsValueEnumerable().OfType<RUILayout>().Count();
		if (_lastChildCount != childrenCount)
		{
			_lastChildCount = childrenCount;
			stateChanged = true;
		}

		if (stateChanged)
		{
			Invalidate();
		}
	}

	public void Invalidate()
	{
		_isDirty = true;
	}

	public void RecalculateLayout()
	{
		if (!_isDirty) return;

		RecalculateHorizontal();
		RecalculateVertical();
		
		_isDirty = false;
	}
	
	protected abstract void RecalculateHorizontal();
	protected abstract void RecalculateVertical();
	
	protected virtual void GetChildPreferredSize(RUILayout child, out float preferredWidth, out float preferredHeight)
	{
		foreach (var c in child.RObject.EnumerateComponents())
		{
			if (c is IUIAutoLayoutable autoLayoutable)
			{
				preferredWidth = autoLayoutable.PreferredWidth;
				preferredHeight = autoLayoutable.PreferredHeight;
				return;
			}
		}
		
#if RABOTORA_STRICT
		// In strict mode, if the child doesn't have an IUIAutoLayoutable component, we treat its preferred size as zero to avoid unintended layout issues.
		preferredWidth = preferredHeight = 0;
#else
		preferredWidth = child.Size.X;
		preferredHeight = child.Size.Y;
#endif
	}

	protected float GetStartOffset(UIAxis axis, float innerSize, float contentSize, UIAlignment alignment)
	{
		return axis switch
		{
			UIAxis.Horizontal => alignment switch
			{
				UIAlignment.UpperCenter or UIAlignment.MiddleCenter or UIAlignment.LowerCenter => (innerSize - contentSize) / 2f,
				UIAlignment.UpperRight or UIAlignment.MiddleRight or UIAlignment.LowerRight => innerSize - contentSize,
				_ => 0 // Left
			},
			UIAxis.Vertical => alignment switch
			{
				UIAlignment.MiddleLeft or UIAlignment.MiddleCenter or UIAlignment.MiddleRight => (innerSize - contentSize) / 2f,
				UIAlignment.LowerLeft or UIAlignment.LowerCenter or UIAlignment.LowerRight => innerSize - contentSize,
				_ => 0 // Top
			},
			_ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "Invalid axis")
		};
	}
}