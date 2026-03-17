using System.Numerics;
using JetBrains.Annotations;
using RabotoraX.Core.Mathematics;

namespace RabotoraX.Core.UI;

public sealed class RUILayout : RLayout
{
    // === 底层笛卡尔属性 (左下角为0, Y轴向上) ===
    
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

    public Vector2 Size
    {
        get => new(Rect.Width, Rect.Height);
        [UsedImplicitly]
        set
        {
            var currentSize = new Vector2(Rect.Width, Rect.Height);
            var delta = value - currentSize;
            OffsetMin -= delta * Pivot;
            OffsetMax += delta * (Vector2.One - Pivot);
            
            _isDirty = true;
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
            
            _isDirty = true;
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

    public void Recalculate()
    {
        float pivotLocalX, pivotLocalY;
        float parentPivotLocalX = 0f, parentPivotLocalY = 0f;

        if (Parent is not RUILayout parent)
        {
            var width = OffsetMax.X - OffsetMin.X;
            var height = OffsetMax.Y - OffsetMin.Y;
            _rect = new Rect(OffsetMin.X, OffsetMin.Y, width, height);
            
            pivotLocalX = _rect.X + width * Pivot.X;
            pivotLocalY = _rect.Y + height * Pivot.Y;
        }
        else
        {
            var p = parent.Rect; 
            
            var anchorMinPx = new Vector2(p.Width * AnchorMin.X, p.Height * AnchorMin.Y);
            var anchorMaxPx = new Vector2(p.Width * AnchorMax.X, p.Height * AnchorMax.Y);
            
            var min = anchorMinPx + OffsetMin;
            var max = anchorMaxPx + OffsetMax;
            var size = max - min;
            
            _rect = new Rect(min.X, min.Y, size.X, size.Y);
            
            pivotLocalX = _rect.X + size.X * Pivot.X;
            pivotLocalY = _rect.Y + size.Y * Pivot.Y;
            
            parentPivotLocalX = p.Width * parent.Pivot.X;
            parentPivotLocalY = p.Height * parent.Pivot.Y;
        }

        float localCartesianX = pivotLocalX - parentPivotLocalX;
        float localCartesianY = pivotLocalY - parentPivotLocalY; 
        
        Position = new Vector3(localCartesianX, localCartesianY, 0f);
        
        _isDirty = false;
    }

    public override void OnUpdate(float deltaTime)
    {
        if (_isDirty) Recalculate();
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