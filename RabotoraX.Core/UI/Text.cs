using System.Numerics;
using RabotoraX.Core.Graphics;

namespace RabotoraX.Core.UI;

/// <summary>
/// Represents a component that can render text in a 2D UI stage.
/// </summary>
public class Text : Component2D
{
	public string Content { get; set; } = string.Empty;
	public string FontName { get; set; } = "Microsoft YaHei";
	public float FontSize { get; set; } = 24.0f;
	public Vector4 Color { get; set; } = new(0, 0, 0, 1); // black

	public override void OnRender2D(INative2DRenderContext context)
	{
		if (string.IsNullOrEmpty(Content)) return;
		
		context.SetTransform(GetCanvasWorldMatrix());
		
		context.DrawText(Content, FontName, FontSize, 0, 0, Color.X, Color.Y, Color.Z, Color.W);
	}
}