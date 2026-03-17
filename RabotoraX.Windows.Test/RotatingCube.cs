using System.Diagnostics;
using System.Numerics;
using RabotoraX.Core.Scripting;

namespace RabotoraX.Windows.Test.Demo3D;

public sealed class RotatingCube : RManagedScript
{
	private Stopwatch _timer = null!;
	
	public override void OnStart()
	{
		_timer = Stopwatch.StartNew();
	}

	public override void OnUpdate(float deltaTime)
	{
		var t = (float)_timer.Elapsed.TotalSeconds;
		var rot = Quaternion.CreateFromAxisAngle(Vector3.UnitY, t * 0.5f);
		Layout.Rotation = rot;
	}
}