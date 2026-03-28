using System.Numerics;
using RabotoraX.Core.Mathematics;

namespace RabotoraX.Core.Tweening;

/// <summary>
/// Represents a Vector2-value RaTween interpolation plugin.
/// </summary>
public sealed class Vector2Plugin : ITweenPlugin<Vector2>
{
	public Vector2 Interpolate(Vector2 from, Vector2 to, float t)
	{
		return RMath.Lerp(from, to, t);
	}
}