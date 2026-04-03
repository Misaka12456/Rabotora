using System.Reflection;
using System.Runtime.InteropServices;
using RabotoraX.Core.Graphics;

namespace RabotoraX.Core.Diagnostics;

public static class Debug
{
	public static bool IsNativeAot => RuntimeInformation.FrameworkDescription.StartsWith(".NET Native", StringComparison.OrdinalIgnoreCase);
	
	public static void Initialize()
	{
#if DEBUG
		Console.WriteLine("RabotoraX is running in DEBUG mode. This may cause reduced performance. For best performance, use the RELEASE configuration.");
#elif !RABOTORA_STRICT 
		Console.WriteLine("Note: Use RABOTORA_STRICT to completely disable debug logs.");
#endif

#if DEBUG || !RABOTORA_STRICT
		Console.WriteLine($"""
		                   --- RabotoraX Debug Console Log Output ---
		                   RabotoraX Core Version: {Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.1.0"}
		                   Running based on {RuntimeInformation.FrameworkDescription})
		                   Using Graphics Backend: {GraphicsService.API.ApiName} ({GraphicsService.API.GetType().FullName})
		                   """);
#endif
	}
}