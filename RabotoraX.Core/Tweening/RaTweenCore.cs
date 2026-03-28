using System.Diagnostics.CodeAnalysis;

namespace RabotoraX.Core.Tweening;

/// <summary>
/// Represents a RaTween instance that interpolates a value of type T from a start value to an end value over a specified duration, using a provided tween plugin for interpolation logic.
/// </summary>
/// <typeparam name="T">The type of value to be tweened (e.g., float, Vector2, Vector3).</typeparam>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class RaTweenCore<T> : RaTweener
{
	private readonly Action<T> _setter;
	private readonly ITweenPlugin<T> _plugin;

	private readonly T _startValue;
	private readonly T _endValue;

	internal RaTweenCore(Func<T> getter, Action<T> setter, T endValue, float duration, ITweenPlugin<T> plugin)
	{
		_setter = setter;
		_endValue = endValue;
		Duration = duration;
		_plugin = plugin;

		_startValue = getter();
	}

	public RaTweenCore<T> SetEase(Ease ease)
	{
		_easeType = ease;
		return this;
	}

	public RaTweenCore<T> SetEase(EaseFunction customEase)
	{
		_easeType = Ease.Linear;
		_customEase = customEase;
		return this;
	}

	public RaTweenCore<T> OnComplete(Action action)
	{
		_onComplete = action;
		return this;
	}

	public RaTweenCore<T> OnUpdate(Action<float> action)
	{
		_onUpdate = action;
		return this;
	}

	public override bool KeepWaiting(float deltaTime)
	{
		if (!IsActive) return false;

		Elapsed += deltaTime;
		float t = Math.Clamp(Elapsed / Duration, 0f, 1f);

		float easedT = _customEase?.Invoke(t) ?? RaEasing.Evaluate(_easeType, t);

		T currentValue = _plugin.Interpolate(_startValue, _endValue, easedT);
		_setter(currentValue);

		_onUpdate?.Invoke(t);

		if (Elapsed >= Duration)
		{
			_setter(_endValue);
			IsActive = false;
			_onComplete?.Invoke();
			return false;
		}

		return true;
	}
}