using System.Diagnostics;
using System.Numerics;
using RabotoraX.Core.Scripting;
using RabotoraX.Core.Test;

namespace RabotoraX.Windows.Test.Demo3D;

public sealed class ColorTilt : RManagedScript
{
	private QuadRenderer _quadRenderer = null!;
	private Stopwatch _timer = null!;
	
	public override void OnStart()
	{
		_quadRenderer = GetComponent<QuadRenderer>()!;
		_timer = Stopwatch.StartNew();
	}

	public override void OnUpdate(float deltaTime)
	{
		var t = (float)_timer.Elapsed.TotalSeconds;
		var r = (float)(Math.Sin(t) * 0.5 + 0.5);
		var g = (float)(Math.Sin(t + Math.PI * 2 / 3) * 0.5 + 0.5);
		var b = (float)(Math.Sin(t + Math.PI * 4 / 3) * 0.5 + 0.5);
		_quadRenderer.Color = new Vector4(r, g, b, 1);
	}
}