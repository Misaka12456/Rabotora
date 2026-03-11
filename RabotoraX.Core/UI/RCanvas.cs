using System.Numerics;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.Mathematics;

namespace RabotoraX.Core.UI;

public enum CanvasScaleMode
{
	ConstantPixelSize = 0,
	ScaleWithScreenSize = 1
}

public enum ScreenMatchMode
{
	MatchWidthOrHeight = 0,
	Expand = 1,
	Shrink = 2
}

/// <summary>
/// Represents a canvas component that can be used as the root of a 2D UI stage.
/// </summary>
public sealed class RCanvas : Component2D
{
	public CanvasScaleMode ScaleMode { get; set; } = CanvasScaleMode.ScaleWithScreenSize;
	public ScreenMatchMode MatchMode { get; set; } = ScreenMatchMode.MatchWidthOrHeight;
	
	public Vector2 ReferenceResolution { get; set; } = new(1280, 720);
	
	public float MatchWidthOrHeight { get; set; } = 0.5f;
	
	public float ScaleFactor { get; private set; } = 1.0f;
	
	private Vector2 _lastScreenSize = Vector2.Zero;

	public override void OnUpdate(float deltaTime)
	{
		var currentScreenSize = new Vector2(GraphicsService.LatestWindowState.Width, GraphicsService.LatestWindowState.Height);
		
		if (_lastScreenSize == currentScreenSize) return;
		
		_lastScreenSize = currentScreenSize;
		RecalculateScaleFactor(currentScreenSize);

		if (Layout is RUILayout uiLayout)
		{
			uiLayout.Size = ScaleMode == CanvasScaleMode.ConstantPixelSize ? currentScreenSize : ReferenceResolution;
			uiLayout.AnchoredPosition = uiLayout.Size * 0.5f;
			uiLayout.Recalculate();
		}
	}

	private void RecalculateScaleFactor(Vector2 screenSize)
	{
		if (ScaleMode == CanvasScaleMode.ConstantPixelSize)
		{
			ScaleFactor = 1.0f;
			return;
		}
		
		float scaleX = screenSize.X / ReferenceResolution.X;
		float scaleY = screenSize.Y / ReferenceResolution.Y;

		switch (MatchMode)
		{
			case ScreenMatchMode.MatchWidthOrHeight:
			{
				float logWidth = MathF.Log2(scaleX);
				float logHeight = MathF.Log2(scaleY);
				float logWeightedAverage = RMath.Lerp(logWidth, logHeight, MatchWidthOrHeight);
				ScaleFactor = MathF.Pow(2, logWeightedAverage);
				break;
			}
			case ScreenMatchMode.Expand:
				ScaleFactor = MathF.Min(scaleX, scaleY);
				break;
			case ScreenMatchMode.Shrink:
				ScaleFactor = MathF.Max(scaleX, scaleY);
				break;
		}
	}
}