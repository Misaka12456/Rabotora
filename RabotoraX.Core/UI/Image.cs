using RabotoraX.Core.Graphics;
using RabotoraX.Core.Serialization;

namespace RabotoraX.Core.UI;

public class Image : UIRenderable
{
	public override Texture2D? Texture
	{
		get => Sprite?.Texture;
		set => throw new NotSupportedException("Use the Sprite property to set the texture for an Image component.");
	}

	[field: RSerializableField]
	public override float Opacity { get; set; } = 1;

	public override INativeShader? CustomShader { get; set; }
	public override float PreferredWidth => Sprite?.SourceRect.Width ?? 0;
	public override float PreferredHeight => Sprite?.SourceRect.Height ?? 0;

	[field: RSerializableField]
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