namespace RabotoraX.Core.Mathematics;

public static class RMath // this function can also be used in RIntepretedScript (derived from RScript)
{
	public static float Lerp(float a, float b, float t)
	{
		return (1 - t) * a + t * b; // same as 'a + (b - a) * t'
	}
}