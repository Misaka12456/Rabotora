namespace RabotoraX.Core.Cinematics;

public enum StageType
{
	None = 0,
	Render2D = 1,
	Render3D = 2,
	Render3DHybrid = 3, // Based on Render3D, but with 2D Screen Space rendering support (above all 3D objects)
}