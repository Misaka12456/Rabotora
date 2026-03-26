using RabotoraX.Core.Graphics;

namespace RabotoraX.Core.UI;

public class Image : UIRenderable
{
	public override Texture2D? Texture
	{
		get => Sprite?.Texture;
		set => throw new NotSupportedException("Use the Sprite property to set the texture for an Image component.");
	}
	public override float Opacity { get; set; } = 1.0f;
	public override INativeShader? CustomShader { get; set; }

	public Sprite? Sprite { get; set; }

	protected override void Render(INative2DRenderContext context)
	{
		if (Sprite?.Texture == null) return;

		var sourceRect = Sprite.SourceRect;
		if (Layout is RUILayout uiLayout)
		{
			context.DrawImage(Sprite.NativeTexture, sourceRect, 0, 0, uiLayout.Size.X, uiLayout.Size.Y, Opacity);
		}
	}
}