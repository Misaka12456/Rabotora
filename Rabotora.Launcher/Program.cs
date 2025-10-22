using System.Diagnostics;
using RabotoraEngine.Graphics;

namespace Rabotora.Launcher;

public static class Program
{
	[STAThread]
	public static int Main(string[] args)
	{
		try
		{
			using var window = new RWindow(1280, 720, "Rabotora DirectX Release");
			window.Run();
			return 0;
		}
		catch (Exception ex)
		{
			Debug.WriteLine("Unhandled exception: " + ex);
			return -1;
		}
	}
}