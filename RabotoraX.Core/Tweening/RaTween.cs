using System.Collections;

namespace RabotoraX.Core.Tweening;

/// <summary>
/// Represents the static entry point for RaTween (RabotoraX Animated Tween).
/// </summary>
public static class RaTween
{
	public static RaTweenCore<T> To<T>(RObject target, ITweenPlugin<T> plugin, Func<T> getter, Action<T> setter, T endValue, float duration)
	{
		var tween = new RaTweenCore<T>(getter, setter, endValue, duration, plugin);
		target.StartCoroutine(TweenRunner(tween));
		
		return tween;
	}

	private static IEnumerator TweenRunner(RaTweener tween)
	{
		yield return tween; // Just yield return it. RaTweener is a valid RYieldData that can be yielded until it completes.
	}
}