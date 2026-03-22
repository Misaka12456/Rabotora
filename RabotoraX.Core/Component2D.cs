using System.Numerics;
using RabotoraX.Core.Cinematics;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.UI;

namespace RabotoraX.Core;

public abstract class Component2D : Component
{
	private RCanvas? _rootCanvas;
	private bool _hasSearchedRootCanvas;

	public override void OnStart()
	{
		if (GetRootCanvas() == null)
		{
			throw new InvalidOperationException("Component2D must be a child of a RCanvas, as it relies on RCanvas for proper scaling and rendering.");
		}
	}

	public override void OnRender(INativeCommandList cmd)
	{
		if (RObject.Stage.Type is StageType.Render2D or StageType.Render3DHybrid)
		{
			return; // Component2D is rendered in the 2D stage, so we don't do anything here. RStage render logic will call OnRender2D instead.
		}
		throw new InvalidOperationException("Component2D cannot be rendered in 3D stage.");
	}

	protected RCanvas? GetRootCanvas()
	{
		if (_hasSearchedRootCanvas) return _rootCanvas;
		var current = Layout;
		while (current != null)
		{
			var canvas = current.RObject.GetComponent<RCanvas>();
			if (canvas != null)
			{
				_rootCanvas = canvas;
				break;
			}
			current = current.Parent;
		}

		_hasSearchedRootCanvas = true;
		return _rootCanvas;
	}

	protected Matrix3x2 GetCanvasWorldMatrix()
	{
		var localMatrix = GetLocal2DMatrix();
		var canvas = GetRootCanvas();
		if (canvas != null)
		{
			var scaleMatrix = Matrix3x2.CreateScale(canvas.ScaleFactor);
			return localMatrix * scaleMatrix;
		}
		
		return localMatrix;
	}

	private Matrix3x2 GetLocal2DMatrix()
	{
		if (Layout is not RUILayout uiLayout)
		{
			return Matrix3x2.CreateTranslation(Layout.Position.X, Layout.Position.Y);
		}
		var pivotOffset = new Vector2(uiLayout.Size.X * uiLayout.Pivot.X, uiLayout.Size.Y * uiLayout.Pivot.Y);
		var pos = new Vector2(uiLayout.Rect.X, uiLayout.Rect.Y) + pivotOffset;
		
		float zRot = MathF.Atan2(2.0f * (Layout.Rotation.W * Layout.Rotation.Z + Layout.Rotation.X * Layout.Rotation.Y),
			1.0f - 2.0f * (Layout.Rotation.Y * Layout.Rotation.Y + Layout.Rotation.Z * Layout.Rotation.Z));
		
		return Matrix3x2.CreateTranslation(-pivotOffset) *
		       Matrix3x2.CreateScale(Layout.Scale.X, Layout.Scale.Y) *
		       Matrix3x2.CreateRotation(zRot) *
		       Matrix3x2.CreateTranslation(pos);
	}

	public override void OnRender2D(INative2DRenderContext context) { }
}