using System.ComponentModel;

namespace RabotoraX.Core.Diagnostics;

public enum AppRunningType
{
	[Description("Managed (.NET JIT Runtime)")]
	Managed = 0,
	
	[Description("NativeAOT (.NET Native AOT Compilation)")]
	NativeAOT = 1
}