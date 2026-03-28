using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using RabotoraX.Core.Graphics;

namespace RabotoraX.Core.Mathematics;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static partial class RMath // this function can also be used in RIntepretedScript (derived from RScript)
{
	public const float CommonTolerance = 0.001f;
	
	public static float Lerp(float a, float b, float t)
	{
		return (1 - t) * a + t * b; // same as 'a + (b - a) * t'
	}
	
	public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
	{
		return (1 - t) * a + t * b;
	}
	
	public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
	{
		return (1 - t) * a + t * b;
	}
	
	public static Vector4 Lerp(Vector4 a, Vector4 b, float t)
	{
		return (1 - t) * a + t * b;
	}
	
	public static Color Lerp(Color a, Color b, float t)
	{
		return (1 - t) * a + t * b;
	}
	
	public static Color32 Lerp(Color32 a, Color32 b, float t)
	{
		var nA = (Color)a; // "n"ormalized A
		var nB = (Color)b;
		return (Color32)Lerp(nA, nB, t);
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
	
	public static float Pow(float x, int n)
	{
		if (n == 0) return 1;
		if (n == 1) return x;
		
		float result = 1;
		for (int i = 0; i < n; i++)
		{
			result *= x;
		}
		return result;
	}
}