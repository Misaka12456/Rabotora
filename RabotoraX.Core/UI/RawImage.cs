using RabotoraX.Core.Graphics;
using RabotoraX.Core.Mathematics;

namespace RabotoraX.Core.UI;

/// <summary>
/// Represents an image component that can be rendered in a 2D UI stage.
/// </summary>
public class RawImage : UIRenderable
{
	public override Texture2D? Texture { get; set; }
	public override float Opacity { get; set; } = 1.0f;
	public override INativeShader? CustomShader { get; set; }
	
	public Rect UVRect { get; set; } = new(0, 0, 1, 1);

	protected override void Render(INative2DRenderContext context)
	{
		if (Texture == null) return;
		if (Layout is RUILayout uiLayout)
		{
			var sourceRect = new Rect(
				UVRect.X * Texture!.Width,
				UVRect.Y * Texture.Height,
				UVRect.Width * Texture.Width,
				UVRect.Height * Texture.Height
			);
			context.DrawImage(Texture.NativeTexture, sourceRect, 0, 0, uiLayout.Size.X, uiLayout.Size.Y, Opacity);
		}
	}
}