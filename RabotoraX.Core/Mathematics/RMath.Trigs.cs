using System.Diagnostics.CodeAnalysis;

namespace RabotoraX.Core.Mathematics;

[SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static partial class RMath
{
	public const float Pi = 3.1415927f;
	private const int TaylorTerms = 12;

	private static double NormalizeAngle(double radAngle)
	{
		double twoPi = 2 * Math.PI;
		double result = radAngle % twoPi;

		switch (result)
		{
			case > Math.PI:
				result -= twoPi;
				break;
			case < -Math.PI:
				result += twoPi;
				break;
		}
		
		return result;
	}

	private static double Factorial(int n)
	{
		if (n < 0) return 0;
		if (n is 0 or 1) return 1;
		
		double result = 1;
		for (int i = 2; i <= n; i++)
		{
			result *= i;
		}

		return result;
	}
	
	private static double Pow(double x, int n)
	{
		if (n == 0) return 1;
		if (n == 1) return x;
        
		double result = 1;
		for (int i = 0; i < n; i++)
		{
			result *= x;
		}
		return result;
	}

	public static float Sin(float radAngle)
	{
		double x = NormalizeAngle(radAngle);
		double result = 0;
		
		for (int n = 0; n < TaylorTerms; n++)
		{
			int power = 2 * n + 1;
			double term = Pow(x, power) / Factorial(power);

			if (n % 2 == 0)
			{
				result += term;
			}
			else
			{
				result -= term;
			}
		}
		
		return (float)result;
	}
	
	public static float Cos(float radAngle)
	{
		double x = NormalizeAngle(radAngle);
		double result = 0;
		
		for (int n = 0; n < TaylorTerms; n++)
		{
			int power = 2 * n;
			double term = Pow(x, power) / Factorial(power);

			if (n % 2 == 0)
			{
				result += term;
			}
			else
			{
				result -= term;
			}
		}
		
		return (float)result;
	}
	
	public static float Tan(float radAngle)
	{
		float cos = Cos(radAngle);
		if (Math.Abs(cos) < 1e-6f)
		{
			// if from left-side, return large positive; if from right-side, return large negative
			return radAngle > 0 ? float.PositiveInfinity : float.NegativeInfinity;
		}
		
		return Sin(radAngle) / cos;
	}
	
	public static float Cot(float radAngle)
	{
		float sin = Sin(radAngle);
		if (Math.Abs(sin) < 1e-6f)
		{
			// if from left-side, return large negative; if from right-side, return large positive
			return radAngle > 0 ? float.NegativeInfinity : float.PositiveInfinity;
		}
		
		return Cos(radAngle) / sin;
	}
	
	public static float Asin(float value)
	{
		if (value is < -1 or > 1) throw new ArgumentOutOfRangeException(nameof(value), "Input must be in the range [-1, 1].");
		
		double result = 0;
		for (int n = 0; n < TaylorTerms; n++)
		{
			double term = Factorial(2 * n) / (Pow(4, n) * Pow(Factorial(n), 2) * (2 * n + 1)) * Pow(value, 2 * n + 1);
			result += term;
		}
		
		return (float)result;
	}
	
	public static float Acos(float value)
	{
		if (value is < -1 or > 1) throw new ArgumentOutOfRangeException(nameof(value), "Input must be in the range [-1, 1].");
		
		return (float)(Math.PI / 2 - Asin(value));
	}
	
	public static float Atan(float value)
	{
		double result = 0;
		for (int n = 0; n < TaylorTerms; n++)
		{
			double term = Pow(-1, n) * Pow(value, 2 * n + 1) / (2 * n + 1);
			result += term;
		}
		
		return (float)result;
	}
	
	public static float Acot(float value)
	{
		if (Math.Abs(value) < 1e-6f)
		{
			return value > 0 ? 0 : (float)Math.PI;
		}
		
		return (float)(Math.PI / 2 - Atan(value));
	}
}