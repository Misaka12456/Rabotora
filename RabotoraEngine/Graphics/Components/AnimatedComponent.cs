namespace RabotoraEngine.Graphics.Components;

public abstract class AnimatedComponent : UIComponent
{
	protected float _alpha = 1.0f;
	protected CancellationTokenSource? _animationCts;

	public async Task FadeTo(float targetAlpha, int duration)
	{
		if (_animationCts != null)
		{
			await _animationCts.CancelAsync();
		}
		_animationCts = new CancellationTokenSource();
		
		float startAlpha = _alpha;
		float step = (targetAlpha - startAlpha) / (duration / 10f);
		while (!_animationCts.IsCancellationRequested && (step > 0 ? _alpha < targetAlpha : _alpha > targetAlpha))
		{
			_alpha += step;
			_alpha = Math.Clamp(_alpha, 0.0f, 1.0f);
			await Task.Delay(10);
		}
		_alpha = targetAlpha;
	}

	public async Task FadeTo(float targetAlpha, float duration)
	{
		await FadeTo(targetAlpha, (int)(duration * 1000));
	}
}