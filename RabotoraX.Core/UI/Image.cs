using System.Numerics;
using RabotoraX.Core.Graphics;

namespace RabotoraX.Core.UI;

/// <summary>
/// Represents an image component that can be rendered in a 2D UI stage.
/// </summary>
public class Image : Component2D
{
	public string ImagePath { get; set; } = string.Empty;
	public float Opacity { get; set; } = 1.0f;

	private ITexture2D? _texture;
	private string? _lastLoadedPath;

	public override void OnRender2D(INative2DRenderContext context)
	{
		if (string.IsNullOrEmpty(ImagePath)) return;

		if (_texture == null || _lastLoadedPath != ImagePath)
		{
			_texture?.Dispose();
			_texture = context.CreateTexture(ImagePath);
			_lastLoadedPath = ImagePath;
		}

		context.SetTransform(GetCanvasWorldMatrix());

		if (Layout is RUILayout uiLayout)
		{
			context.DrawImage(_texture, 0, 0, uiLayout.Size.X, uiLayout.Size.Y, Opacity);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing) _texture?.Dispose();
		base.Dispose(disposing);
	}
}