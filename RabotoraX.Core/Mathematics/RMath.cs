using System.Numerics;

namespace RabotoraX.Core.Mathematics;

public static class RMath // this function can also be used in RIntepretedScript (derived from RScript)
{
	public static float Lerp(float a, float b, float t)
	{
		return (1 - t) * a + t * b; // same as 'a + (b - a) * t'
	}
	
	public static bool Approx(float a, float b, float tolerance = float.Epsilon)
	{
		return Math.Abs(a - b) <= tolerance;
	}
	
	public static bool Approx(double a, double b, double tolerance = double.Epsilon)
	{
		return Math.Abs(a - b) <= tolerance;
	}
	
	public static bool Approx(Vector2 a, Vector2 b, float tolerance = float.Epsilon)
	{
		return (a - b).Length() <= tolerance;
	}
}