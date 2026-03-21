using System.Numerics;
using RabotoraX.Core.Graphics;

namespace RabotoraX.Core.UI;

/// <summary>
/// Represents a component that can render text in a 2D UI stage.
/// </summary>
public class Text : Component2D
{
	public string Content
	{
		get => _content;
		set
		{
			if (_content != value)
			{
				_content = value;
				_isDirty = true;
			}
		}
	}

	public string FontName
	{
		get => _fontName;
		set
		{
			if (_fontName != value)
			{
				_fontName = value;
				_isDirty = true;
			}
		}
	}

	public float FontSize
	{
		get => _fontSize;
		set
		{
			if (Math.Abs(_fontSize - value) > 0.01f)
			{
				_fontSize = value;
				_isDirty = true;
			}
		}
	}
	
	public Vector4 Color { get; set; } = new(0, 0, 0, 1); // black
	
	private string _content = string.Empty;
	private string _fontName = "Microsoft YaHei";
	private float _fontSize = 24.0f;
	
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
		
		context.DrawTextLayout(_cachedLayout, 0, 0, Color.X, Color.Y, Color.Z, Color.W);
	}

	protected override void Dispose(bool disposing)
	{
		_cachedLayout?.Dispose();
		_cachedLayout = null;
	}
}