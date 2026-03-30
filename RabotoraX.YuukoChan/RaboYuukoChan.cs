using RabotoraX.Core;
using RabotoraX.Core.Mathematics;

namespace RabotoraX.YuukoChan;

/// <summary>
/// Represents the main application class for the RabotoraX YuukoChan Visual Novel Extension Framework.<br />
/// Based on the RabotoraX Core Engine, this class serves as the entry point for creating visual novel games using the YuukoChan extension.<br />
/// To see Visual Novel Scripting module, refer to <see cref="RabotoraX.YuukoChan.Scripting.YuukoScript"/>.
/// </summary>
public class RaboYuukoChan : Rabotora
{
	public RaboYuukoChan(string title, int width = 1280, int height = 720, Fractional? fixedAspectRatio = null) : base(title, width, height, fixedAspectRatio ?? new Fractional(16, 9))
	{
		// todo: initialize YuukoScript Parsing & Execution Engine here (RabotoraX v0.7+)
	}
}