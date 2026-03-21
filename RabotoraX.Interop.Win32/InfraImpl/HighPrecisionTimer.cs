using System;
using System.Runtime.Versioning;
using RabotoraX.Core.Infrastructure;
using static TerraFX.Interop.Windows.Windows;

namespace RabotoraX.Interop.Win32.InfraImpl;

[SupportedOSPlatform("windows")]
public sealed class HighPrecisionTimer : INativeSystemHighPrecisionProvider
{
	private bool _enabled;
	
	public bool TryEnableHighPrecision()
	{
		_enabled = timeBeginPeriod(1) == 0;
		if (_enabled)
		{
#if DEBUG
			Console.WriteLine("High precision timer enabled for Win32 platform by timeBeginPeriod(1).");
#endif
			return true;
		}
		return false;
	}
	
	public void Dispose()
	{
		if (_enabled)
		{
			_ = timeEndPeriod(1);
		}
	}
}