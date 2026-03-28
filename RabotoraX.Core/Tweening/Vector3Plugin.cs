using System.Numerics;
using RabotoraX.Core.Mathematics;

namespace RabotoraX.Core.Tweening;

/// <summary>
/// Represents a Vector3-value RaTween interpolation plugin.
/// </summary>
public sealed class Vector3Plugin : ITweenPlugin<Vector3>
{
	public Vector3 Interpolate(Vector3 from, Vector3 to, float t)
	{
		return RMath.Lerp(from, to, t);
	}
}