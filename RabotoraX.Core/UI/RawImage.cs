using RabotoraX.Core.Graphics;
using RabotoraX.Core.Mathematics;

namespace RabotoraX.Core.UI;

/// <summary>
/// Represents an image component that can be rendered in a 2D UI stage.
/// </summary>
public class RawImage : Component2D
{
	public Texture2D? Texture { get; set; }
	public float Opacity { get; set; } = 1.0f;
	public Rect UVRect { get; set; } = new(0, 0, 1, 1);
	public INativeShader? CustomShader { get; set; }

	public override void OnRender2D(INative2DRenderContext context)
	{
		if (Texture == null) return;

		context.SetTransform(GetCanvasWorldMatrix());

		if (CustomShader != null)
		{
			context.SetShader(CustomShader);
		}

		if (Layout is RUILayout uiLayout)
		{
			var sourceRect = new Rect(
				UVRect.X * Texture.Width,
				UVRect.Y * Texture.Height,
				UVRect.Width * Texture.Width,
				UVRect.Height * Texture.Height
			);
			context.DrawImage(Texture.NativeTexture, sourceRect, 0, 0, uiLayout.Size.X, uiLayout.Size.Y, Opacity);
			
			if (CustomShader != null)
			{
				context.SetShader(null); // Reset shader after drawing
			}
		}
	}
}