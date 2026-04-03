using RabotoraX.Core.Graphics;
using RabotoraX.Core.Mathematics;
using RabotoraX.Core.Serialization;

namespace RabotoraX.Core.UI;

/// <summary>
/// Represents an image component that can be rendered in a 2D UI stage.
/// </summary>
public class RawImage : UIRenderable
{
	public override Texture2D? Texture { get; set; }
	
	[field: RSerializableField]
	public override float Opacity { get; set; } = 1.0f;
	public override INativeShader? CustomShader { get; set; }
	
	[field: RSerializableField]
	public Rect UVRect { get; set; } = new(0, 0, 1, 1);

	public override float PreferredWidth => Texture != null ? Texture.Width * UVRect.Width : 0;
	public override float PreferredHeight => Texture != null ? Texture.Height * UVRect.Height : 0;

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