using Rabotora.Graphics;
using Rabotora.Graphics.Components;
using SkiaSharp;
using Rectangle = Rabotora.Core.Rectangle;

namespace MyFirstGame;

public class SplashComponent : AnimatedComponent
{
	private readonly SKBitmap[] _splashes;
	private int _currentIndex = -1;

	public SplashComponent(string[] imagePaths, Rectangle bounds)
	{
		_splashes = imagePaths.Select(SKBitmap.Decode).ToArray();
		Bounds = bounds;
	}

	public override void Draw(IRenderContext context)
	{
		if (_currentIndex >= _splashes.Length) return;
		if (_currentIndex == -1)
		{
			var black = SKImage.Create(new SKImageInfo(Bounds.Width, Bounds.Height, SKColorType.Bgra8888, SKAlphaType.Premul));
		}
		else
		{
			context.DrawTexture(_splashes[_currentIndex], Bounds, _alpha);
		}
	}

	public async Task PlayAnimationAsync()
	{
		for (_currentIndex = 0; _currentIndex < _splashes.Length; _currentIndex++)
		{
			await FadeTo(1.0f, 1500);
			await Task.Delay(2500);
			await FadeTo(0.0f, 1500);
		}
	}
}