using System.Numerics;
using RabotoraX.Core.Mathematics;

namespace RabotoraX.Core.Tweening;

/// <summary>
/// Represents a Vector4-value RaTween interpolation plugin.
/// </summary>
public sealed class Vector4Plugin : ITweenPlugin<Vector4>
{
	public Vector4 Interpolate(Vector4 from, Vector4 to, float t)
	{
		return RMath.Lerp(from, to, t);
	}
}