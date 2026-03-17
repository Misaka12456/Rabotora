using RabotoraX.Core.Graphics;

namespace RabotoraX.Core.UI;

public class Image : Component2D
{
	public Sprite? Sprite { get; set; }
	public float Opacity { get; set; } = 1.0f;
	public INativeShader? CustomShader { get; set; }

	public override void OnRender2D(INative2DRenderContext context)
	{
		if (Sprite?.Texture == null) return;
		
		context.SetTransform(GetCanvasWorldMatrix());
		
		if (CustomShader != null)
		{
			context.SetShader(CustomShader);
		}

		if (Layout is RUILayout uiLayout)
		{
			context.DrawImage(Sprite.NativeTexture, Sprite.SourceRect, 0, 0, uiLayout.Size.X, uiLayout.Size.Y, Opacity);
		}
		
		if (CustomShader != null)
		{
			context.SetShader(null); // Reset shader after drawing
		}
	}
}