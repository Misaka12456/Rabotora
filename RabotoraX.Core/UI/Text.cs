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
				Invalidate();
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
				Invalidate();
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
				Invalidate();
			}
		}
	} = 24.0f;

	public Color Color { get; set; } = Color.Black;

	public override float PreferredWidth => GetLayout().Size.X;
	public override float PreferredHeight => GetLayout().Size.Y;

	private INativeTextLayout? _cachedLayout;
	private bool _isDirty = true;

	private INativeTextLayout GetLayout()
	{
		if (_isDirty || _cachedLayout == null)
		{
			_cachedLayout?.Dispose();
			_cachedLayout = RUIService.TextFactory.CreateTextLayout(Content, FontName, FontSize);
			_isDirty = false;
		}

		return _cachedLayout!;
	}

	private void Invalidate()
	{
		_isDirty = true;
		if (Layout is RUILayout layout)
		{
			layout.RequestLayout();
		}
	}


	public override void OnRender2D(INative2DRenderContext context)
	{
		if (string.IsNullOrEmpty(Content)) return;

		GetLayout(); // this will refresh the cached layout if needed
		
		context.SetTransform(GetCanvasWorldMatrix());
		
		context.DrawTextLayout(_cachedLayout!, 0, 0, Color.R, Color.G, Color.B, Color.A * Opacity);
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