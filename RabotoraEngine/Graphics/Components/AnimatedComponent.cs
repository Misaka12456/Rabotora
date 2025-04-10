namespace Rabotora.Graphics.Components;

public abstract class AnimatedComponent : UIComponent
{
	protected float _alpha = 1.0f;
	protected CancellationTokenSource? _animationCts;

	public async Task FadeTo(float targetAlpha, int durationMs)
	{
		if (_animationCts != null)
		{
			await _animationCts.CancelAsync();
		}
		_animationCts = new CancellationTokenSource();
		
		float startAlpha = _alpha;
		float step = (targetAlpha - startAlpha) / (durationMs / 10f); // 10ms pace
		while (!_animationCts.IsCancellationRequested && (step > 0 ? _alpha < targetAlpha : _alpha > targetAlpha))
		{
			_alpha += step;
			await Task.Delay(10);
		}
		_alpha = targetAlpha;
	}
}