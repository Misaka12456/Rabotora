using System.Numerics;
using RabotoraX.Core.Graphics;

namespace RabotoraX.Core.Cinematics;

public enum AudienceClearFlags
{
	DoNotClear = 0,
	Color = 1,
}

public sealed class AudienceClearData
{
	public AudienceClearFlags ClearFlags { get; set; } = AudienceClearFlags.Color;
	public Vector4 ClearColor { get; set; } = new(0, 0, 0, 1); // black
}

public class Audience : Component
{
	public static Audience? Main { get; private set; }
	
	public float FieldOfView { get; set; } = 60f * (MathF.PI / 180f);
	public float NearClipPlane { get; set; } = 0.1f;
	public float FarClipPlane { get; set; } = 1000f;
	
	public AudienceClearData ClearConfig { get; } = new();

	public override void OnAwake()
	{
		Main ??= this;
	}
	
	public void Clear()
	{
		if (ClearConfig.ClearFlags == AudienceClearFlags.DoNotClear) return;
		switch (ClearConfig.ClearFlags)
		{
			case AudienceClearFlags.Color:
				GraphicsService.API.Clear(ClearConfig.ClearColor.X, ClearConfig.ClearColor.Y, ClearConfig.ClearColor.Z, ClearConfig.ClearColor.W);
				break;
		}
	}

	public Matrix4x4 GetViewMatrix()
	{
		// forward is -Z, up is +Y, right is +X (left-handed)
		var forward = Vector3.Transform(-Vector3.UnitZ, Layout.Rotation);
		var up = Vector3.Transform(Vector3.UnitY, Layout.Rotation);
		
		return Matrix4x4.CreateLookAt(Layout.Position, Layout.Position + forward, up);
	}
	
	public Matrix4x4 GetProjectionMatrix(float aspectRatio)
	{
		return Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, aspectRatio, NearClipPlane, FarClipPlane);
	}

	protected override void Dispose(bool disposing)
	{
		if (Main == this) Main = null;
		base.Dispose(disposing);
	}
}