using Rabotora.Graphics;
using Rabotora.Graphics.Components;
using SkiaSharp;
using Rectangle = Rabotora.Core.Rectangle;

namespace MyFirstGame;

public class SplashComponent : AnimatedComponent
{
	private readonly SKBitmap[] _splashes;
	private int _currentIndex;

	public SplashComponent(string[] imagePaths, Rectangle bounds)
	{
		_splashes = imagePaths.Select(SKBitmap.Decode).ToArray();
		Bounds = bounds;
	}

	public override void Draw(IRenderContext context)
	{
		if (_currentIndex >= _splashes.Length) return;
		context.DrawTexture(_splashes[_currentIndex], Bounds, _alpha);
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