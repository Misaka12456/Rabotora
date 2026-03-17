using System.Numerics;
using System.Runtime.InteropServices;

namespace RabotoraX.Core.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vertex2D
{
	public Vector3 Position;
	public Vector2 TexCoord;

	public static InputElementDescription[] GetLayout()
	{
		return
		[
			new InputElementDescription("POSITION", 0, GpuFormat.R32G32B32_Float, 0, 0),
			new InputElementDescription("TEXCOORD", 0, GpuFormat.R32G32_Float, 12, 0)
		];
	}
}