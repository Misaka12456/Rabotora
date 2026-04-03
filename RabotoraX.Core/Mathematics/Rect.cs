using MemoryPack;

namespace RabotoraX.Core.Mathematics;

[MemoryPackable]
public partial struct Rect
{
	public readonly static Rect Zero = new(0, 0, 0, 0);
	
	public float X;
	public float Y;
	public float Width;
	public float Height;
	
	public Rect(float x, float y, float width, float height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}
}