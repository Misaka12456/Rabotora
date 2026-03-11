using System.Numerics;
using JetBrains.Annotations;
using RabotoraX.Core.Mathematics;

namespace RabotoraX.Core.UI;

public sealed class RUILayout : RLayout
{
	public Vector2 Pivot
	{
		get;
		[UsedImplicitly]
		set
		{
			field = value;
			_isDirty = true;
		}
	} = new(0.5f, 0.5f);

	public Vector2 Size
	{
		get;
		[UsedImplicitly]
		set
		{
			var delta = value - field;
			field = value;

			OffsetMin -= delta * Pivot;
			OffsetMax += delta * (Vector2.One - Pivot);
			
			_isDirty = true;
		}
	}
	public Vector2 AnchorMin
	{
		get;
		[UsedImplicitly]
		set
		{
			field = value;
			_isDirty = true;
		}
	}
	public Vector2 AnchorMax
	{
		get;
		[UsedImplicitly]
		set
		{
			field = value;
			_isDirty = true;
		}
	}
	public Vector2 OffsetMin
	{
		get;
		[UsedImplicitly]
		set
		{
			field = value;
			_isDirty = true;
		}
	}
	public Vector2 OffsetMax
	{
		get;
		[UsedImplicitly]
		set
		{
			field = value;
			_isDirty = true;
		}
	}

	public Vector2 AnchoredPosition
	{
		get => CalculateAnchoredPosition();
		set => SetDirtyFromAnchoredPosition(value);
	}
	public Rect Rect => _rect;

	private Rect _rect = new(0, 0, 100, 100);
	private bool _isDirty = true;

	public void Recalculate()
	{
		if (Parent is not RUILayout parent)
		{
			_rect = new Rect(0, 0, Size.X, Size.Y);
			return;
		}

		var p = parent._rect;
		var anchorMin = new Vector2(p.Width * AnchorMin.X, p.Height * AnchorMin.Y);
		var anchorMax = new Vector2(p.Width * AnchorMax.X, p.Height * AnchorMax.Y);
		var min = anchorMin + OffsetMin;
		var max = anchorMax + OffsetMax;
		var size = max - min;
		
		_rect = new Rect(min.X, min.Y, size.X, size.Y);
		
		Position = new Vector3(_rect.X + size.X * Pivot.X, _rect.Y + size.Y * Pivot.Y, Position.Z);
	}

	public override void OnUpdate(float deltaTime)
	{
		if (_isDirty)
		{
			Recalculate();
			_isDirty = false;
		}
	}

	private Vector2 CalculateAnchoredPosition()
	{
		if (Parent is not RUILayout parent) return new Vector2(Position.X, Position.Y);

		var p = parent.Rect;
		var anchorRefX = RMath.Lerp(p.Width * AnchorMin.X, p.Width * AnchorMax.X, Pivot.X);
		var anchorRefY = RMath.Lerp(p.Height * AnchorMin.Y, p.Height * AnchorMax.Y, Pivot.Y);
		
		return new Vector2(Position.X - anchorRefX, Position.Y - anchorRefY);
	}

	private void SetDirtyFromAnchoredPosition(Vector2 value)
	{
		if (Parent is not RUILayout parent)
		{
			Position = new Vector3(value.X, value.Y, Position.Z);
			return;
		}
		
		var p = parent.Rect;
		var anchorRefX = RMath.Lerp(p.Width * AnchorMin.X, p.Width * AnchorMax.X, Pivot.X);
		var anchorRefY = RMath.Lerp(p.Height * AnchorMin.Y, p.Height * AnchorMax.Y, Pivot.Y);
		
		Position = new Vector3(value.X + anchorRefX, value.Y + anchorRefY, Position.Z);
		UpdateOffsetsBasedOnPosition();
	}
	
	private void UpdateOffsetsBasedOnPosition()
	{
		var p = (Parent as RUILayout)?.Rect ?? Rect.Zero;
		
		var anchorMinPx = new Vector2(p.Width * AnchorMin.X, p.Height * AnchorMin.Y);
		var anchorMaxPx = new Vector2(p.Width * AnchorMax.X, p.Height * AnchorMax.Y);

		float left = Position.X - (Size.X * Pivot.X);
		float right = left + Size.X;
		float top = Position.Y - (Size.Y * Pivot.Y);
		float bottom = top + Size.Y;
		
		OffsetMin = new Vector2(left - anchorMinPx.X, top - anchorMinPx.Y);
		OffsetMax = new Vector2(right - anchorMaxPx.X, bottom - anchorMaxPx.Y);
		_isDirty = true;
	}
}