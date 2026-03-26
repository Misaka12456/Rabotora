using System.Numerics;
using RabotoraX.Core.Graphics;

namespace RabotoraX.Core.UI;

/// <summary>
/// Represents a component that can render text in a 2D UI stage.
/// </summary>
public class Text : UIRenderable
{
	public override Texture2D? Texture { get; set; } = null;
	public override float Opacity { get; set; } = 1.0f;
	public override INativeShader? CustomShader { get; set; }

	public string Content
	{
		get;
		set
		{
			if (field != value)
			{
				field = value;
				_isDirty = true;
			}
		}
	} = string.Empty;

	public string FontName
	{
		get;
		set
		{
			if (field != value)
			{
				field = value;
				_isDirty = true;
			}
		}
	} = "Microsoft YaHei";

	public float FontSize
	{
		get;
		set
		{
			if (Math.Abs(field - value) > 0.01f)
			{
				field = value;
				_isDirty = true;
			}
		}
	} = 24.0f;

	public Color Color { get; set; } = Color.Black;
	
	private INativeTextLayout? _cachedLayout;
	private bool _isDirty = true;


	public override void OnRender2D(INative2DRenderContext context)
	{
		if (string.IsNullOrEmpty(Content)) return;

		if (_isDirty || _cachedLayout == null)
		{
			_cachedLayout?.Dispose();
			_cachedLayout = context.CreateTextLayout(Content, FontName, FontSize);
			_isDirty = false;
		}
		
		context.SetTransform(GetCanvasWorldMatrix());
		
		context.DrawTextLayout(_cachedLayout, 0, 0, Color.R, Color.G, Color.B, Color.A * Opacity);
	}

	protected override void Render(INative2DRenderContext context)
	{
		// Do nothing here since we're handling rendering in OnRender2D
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_cachedLayout?.Dispose();
			_cachedLayout = null;
		}
		base.Dispose(disposing);
	}
}