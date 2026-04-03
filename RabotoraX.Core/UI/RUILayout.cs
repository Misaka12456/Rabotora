using System.Numerics;
using JetBrains.Annotations;
using RabotoraX.Core.Mathematics;
using RabotoraX.Core.Serialization;

namespace RabotoraX.Core.UI;

/// <summary>
/// Represents a layout component that can be used to position and size UI elements in a 2D (or 3D Hybrid) stage.<br />
/// The coordinate system used by <see cref="RUILayout"/> is directly Screen Space coordinates, but it will be transformed to the local Cartesian coordinate system for the basic <see cref="RLayout"/> functionality.
/// </summary>

public sealed class RUILayout : RLayout
{
    [field: RSerializableField]
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

    [field: RSerializableField]
    public Vector2 AnchorMin
    {
        get;
        [UsedImplicitly]
        set
        {
            field = value;
            _isDirty = true;
        }
    } = new(0.5f, 0.5f);

    [field: RSerializableField]
    public Vector2 AnchorMax
    {
        get;
        [UsedImplicitly]
        set
        {
            field = value;
            _isDirty = true;
        }
    } = new(0.5f, 0.5f);

    [field: RSerializableField]
    public Vector2 OffsetMin
    {
        get;
        [UsedImplicitly]
        set
        {
            field = value;
            _isDirty = true;
        }
    } = Vector2.Zero;

    [field: RSerializableField]
    public Vector2 OffsetMax
    {
        get;
        [UsedImplicitly]
        set
        {
            field = value;
            _isDirty = true;
        }
    } = new(100, 100);

    /// <summary>
    /// The scale of the ui layout. This hides <see cref="RLayout.Scale"/> intentionally.<br />
    /// The scaling is applied around <see cref="Pivot"/>.
    /// </summary>
    [field: RSerializableField]
    public new Vector2 Scale
    {
        get;
        [UsedImplicitly]
        set
        {
            field = value;
            _isDirty = true;
        }
    } = Vector2.One;

    public Vector2 Size
    {
        get => new(Rect.Width, Rect.Height);
        [UsedImplicitly]
        set
        {
            var currentBaseSize = GetBaseSize();
            var targetBaseSize = DivideByScale(value, Scale);
            var delta = targetBaseSize - currentBaseSize;
            
            OffsetMin -= delta * Pivot;
            OffsetMax += delta * (Vector2.One - Pivot);
            
            Recalculate();
        }
    }

    public Vector2 AnchoredPosition
    {
        get => CalculateAnchoredPosition();
        [UsedImplicitly]
        set
        {
            var current = CalculateAnchoredPosition();
            var delta = value - current;
            
            OffsetMin += delta;
            OffsetMax += delta;
            
            Recalculate();
        }
    }

    public Rect Rect
    {
        get
        {
            if (_isDirty) Recalculate();
            return _rect;
        }
    }

    private Rect _rect = new(0, 0, 100, 100);
    private bool _isDirty = true;

    public void SetDirty()
    {
        _isDirty = true;
    }

    public void Recalculate()
    {
        _isDirty = false;

        var baseRect = CalculateBaseRect();

        // Pivot position in screen space, before scale is applied.
        float pivotScreenX = baseRect.X + baseRect.Width * Pivot.X;
        float pivotScreenY = baseRect.Y + baseRect.Height * Pivot.Y;

        // Apply scale around pivot.
        float finalWidth = baseRect.Width * Scale.X;
        float finalHeight = baseRect.Height * Scale.Y;

        float finalX = pivotScreenX - finalWidth * Pivot.X;
        float finalY = pivotScreenY - finalHeight * Pivot.Y;

        _rect = new Rect(finalX, finalY, finalWidth, finalHeight);

        if (Parent is not RUILayout parent)
        {
            // Root layout: convert screen-space pivot position to local Cartesian space.
            // ReSharper disable once InlineTemporaryVariable
            float localCartesianX = pivotScreenX;
            float localCartesianY = _rect.Height - pivotScreenY;

            Position = new Vector3(localCartesianX, localCartesianY, 0f);
        }
        else
        {
            var parentRect = parent.Rect;
            float parentPivotScreenX = parentRect.X + parentRect.Width * parent.Pivot.X;
            float parentPivotScreenY = parentRect.Y + parentRect.Height * parent.Pivot.Y;

            float localCartesianX = pivotScreenX - parentPivotScreenX;
            float localCartesianY = -(pivotScreenY - parentPivotScreenY);

            Position = new Vector3(localCartesianX, localCartesianY, 0f);
        }
        
        foreach (var child in Children)
        {
            if (child is RUILayout childLayout)
            {
                childLayout.Recalculate();
            }
        }
    }

    public void RequestLayout()
    {
        if (_isDirty) return;
        SetDirty();
        if (Parent is RUILayout parent)
        {
            parent.RequestLayout();
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        if (_isDirty) Recalculate();
    }

    private Rect CalculateBaseRect()
    {
        if (Parent is not RUILayout parent)
        {
            float width = OffsetMax.X - OffsetMin.X;
            float height = OffsetMax.Y - OffsetMin.Y;

            return new Rect(OffsetMin.X, OffsetMin.Y, width, height);
        }

        var p = parent.Rect;

        var anchorMinPx = new Vector2(p.Width * AnchorMin.X, p.Height * AnchorMin.Y);
        var anchorMaxPx = new Vector2(p.Width * AnchorMax.X, p.Height * AnchorMax.Y);

        var min = anchorMinPx + OffsetMin;
        var max = anchorMaxPx + OffsetMax;
        var size = max - min;

        return new Rect(min.X, min.Y, size.X, size.Y);
    }
    
    private Vector2 GetBaseSize()
    {
        var r = CalculateBaseRect();
        return new Vector2(r.Width, r.Height);
    }

    private static Vector2 DivideByScale(Vector2 size, Vector2 scale)
    {
        const float epsilon = 0.000001f;

        float x = MathF.Abs(scale.X) < epsilon ? 0f : size.X / scale.X;
        float y = MathF.Abs(scale.Y) < epsilon ? 0f : size.Y / scale.Y;

        return new Vector2(x, y);
    }
    
    private Vector2 CalculateAnchoredPosition()
    {
        var r = Rect;
        
        if (Parent is not RUILayout parent)
        {
            return new Vector2(r.X + r.Width * Pivot.X, r.Y + r.Height * Pivot.Y);
        }

        var p = parent.Rect;
        
        var anchorRefX = RMath.Lerp(p.Width * AnchorMin.X, p.Width * AnchorMax.X, Pivot.X);
        var anchorRefY = RMath.Lerp(p.Height * AnchorMin.Y, p.Height * AnchorMax.Y, Pivot.Y);
        
        float pivotX = r.X + r.Width * Pivot.X;
        float pivotY = r.Y + r.Height * Pivot.Y;
        
        return new Vector2(pivotX - anchorRefX, pivotY - anchorRefY);
    }
}