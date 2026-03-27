using System.Diagnostics.CodeAnalysis;
using RabotoraX.Core.Graphics;

namespace RabotoraX.Core.UI;

[SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
public abstract class UIRenderable : Component2D, IUIAutoLayoutable
{
	public abstract Texture2D? Texture { get; set; }
	public abstract float Opacity { get; set; }
	public abstract INativeShader? CustomShader { get; set; }
	public abstract float PreferredWidth { get; }
	public abstract float PreferredHeight { get; }

	protected abstract void Render(INative2DRenderContext context);

	public override void OnRender2D(INative2DRenderContext context)
	{
		context.SetTransform(GetCanvasWorldMatrix());

		if (CustomShader != null)
		{
			context.SetShader(CustomShader);
		}

		Render(context);

		if (CustomShader != null)
		{
			context.SetShader(null); // Reset shader after drawing
		}
	}
}