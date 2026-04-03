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
		return _enabled;
	}
	
	public void Dispose()
	{
		if (_enabled)
		{
			_ = timeEndPeriod(1);
		}
	}
}