using System.Diagnostics.CodeAnalysis;
using RabotoraX.Core.Mathematics;

namespace RabotoraX.Core.Tweening;

[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
public static class RaEasing
{
	public static float Evaluate(Ease ease, float t)
	{
		return ease switch
		{
			Ease.InSine => 1 - RMath.Cos(t * RMath.Pi / 2),
			Ease.OutSine => RMath.Sin(t * RMath.Pi / 2),
			Ease.InOutSine => -(RMath.Cos(RMath.Pi * t) - 1) / 2,
			Ease.InQuad => t * t,
			Ease.OutQuad => 1 - (1 - t) * (1 - t),
			Ease.InOutQuad => t < 0.5f ? (2 * t * t) : (1 - RMath.Pow(-2 * t + 2, 2) / 2),
			Ease.InCubic => t * t * t,
			Ease.OutCubic => 1 - RMath.Pow(1 - t, 3),
			Ease.InOutCubic => t < 0.5f ? (4 * t * t * t) : (1 - RMath.Pow(-2 * t + 2, 3) / 2),
			_ => t
		};
	}
}