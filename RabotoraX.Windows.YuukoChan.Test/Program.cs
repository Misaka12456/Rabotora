using RabotoraX.Core.Cinematics;
using RabotoraX.Core.Graphics;
using RabotoraX.YuukoChan;

namespace RabotoraX.Windows.YuukoChan.Test;

public static class Program
{
	[STAThread]
	public static int Main(string[] args)
	{
		var rabotora = new RaboYuukoChan("Example Visual Novel");
		var stage = new RStage("MainStage")
		{
			Type = StageType.Render2D,
			ClearColor = Color.Black
		};

		// TODO: Visual Novel example (RabotoraX v0.7+)
		
		return rabotora.Run(stage);
	}
}