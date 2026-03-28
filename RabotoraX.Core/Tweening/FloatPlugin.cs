using RabotoraX.Core.Mathematics;

namespace RabotoraX.Core.Tweening;

/// <summary>
/// Represents a float-value RaTween interpolation plugin.
/// </summary>
public sealed class FloatPlugin : ITweenPlugin<float>
{
	public float Interpolate(float from, float to, float t)
	{
		return RMath.Lerp(from, to, t);
	}
}