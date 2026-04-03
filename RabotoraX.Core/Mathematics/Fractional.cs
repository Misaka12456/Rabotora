using MemoryPack;

namespace RabotoraX.Core.Mathematics;

[MemoryPackable]
public partial struct Fractional
{
	public int Numerator;
	public int Denominator;
	
	public Fractional(int numerator, int denominator)
	{
		Numerator = numerator;
		Denominator = denominator;
	}
}