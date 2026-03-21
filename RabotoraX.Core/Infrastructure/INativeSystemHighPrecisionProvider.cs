using System.Diagnostics.CodeAnalysis;

namespace RabotoraX.Core.Infrastructure;

internal sealed class DummyHighPrecisionProvider : INativeSystemHighPrecisionProvider
{
	public bool TryEnableHighPrecision()
	{
		return false;
	}

	public void Dispose()
	{
		// Do nothing
	}
}

public interface INativeSystemHighPrecisionProvider : IDisposable
{
	bool TryEnableHighPrecision();

	[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, "RabotoraX.Interop.Win32.InfraImpl.HighPrecisionTimer", "RabotoraX.Interop.Win32")]
	[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, "RabotoraX.Interop.Linux.InfraImpl.HighPrecisionTimer", "RabotoraX.Interop.Linux")]
	[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, "RabotoraX.Interop.MacOS.InfraImpl.HighPrecisionTimer", "RabotoraX.Interop.MacOS")]
	[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, "RabotoraX.Interop.Android.InfraImpl.HighPrecisionTimer", "RabotoraX.Interop.Android")]
	[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, "RabotoraX.Interop.IOS.InfraImpl.HighPrecisionTimer", "RabotoraX.Interop.IOS")]
	public static INativeSystemHighPrecisionProvider PlatformCreate()
	{
		Type? implType;
		try
		{
			implType = OperatingSystem.IsWindows() ? Type.GetType("RabotoraX.Interop.Win32.InfraImpl.HighPrecisionTimer, RabotoraX.Interop.Win32") :
				OperatingSystem.IsLinux() ? Type.GetType("RabotoraX.Interop.Linux.InfraImpl.HighPrecisionTimer, RabotoraX.Interop.Linux") :
				OperatingSystem.IsMacOS() ? Type.GetType("RabotoraX.Interop.MacOS.InfraImpl.HighPrecisionTimer, RabotoraX.Interop.MacOS") :
				OperatingSystem.IsAndroid() ? Type.GetType("RabotoraX.Interop.Android.InfraImpl.HighPrecisionTimer, RabotoraX.Interop.Android") :
				OperatingSystem.IsIOS() ? Type.GetType("RabotoraX.Interop.IOS.InfraImpl.HighPrecisionTimer, RabotoraX.Interop.IOS") :
				throw new PlatformNotSupportedException("Unsupported platform");
		}
		catch
		{
			implType = null;
		}

		if (implType == null)
		{
			return new DummyHighPrecisionProvider();
		}

		return (INativeSystemHighPrecisionProvider)Activator.CreateInstance(implType)!;
	}
}