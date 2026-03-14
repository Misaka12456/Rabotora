using System.Diagnostics.CodeAnalysis;
using RabotoraX.Core.Inputs;

namespace RabotoraX.Core.Graphics;

public interface INativeWindow : IDisposable
{
	nint Handle { get; }
	
	string Title { get; set; }
	(int Width, int Height) Size { get; set; }
	bool IsClosing { get; }
	bool IsVisible { get; }
	INativeInput Input { get; }
	
	/// <summary>
	/// Creates the native window with the specified width, height, title and class name.<br />
	/// <see cref="Handle"/>, <see cref="IsClosing"/>, <see cref="IsVisible"/> and <see cref="Input"/> will be available after this method is called.
	/// </summary>
	/// <param name="width">The width of the window in pixels.</param>
	/// <param name="height">The height of the window in pixels.</param>
	/// <param name="title">The title of the window.</param>
	/// <param name="className">The class name of the window. This is only used on Windows and can be left as the default value for other platforms.</param>
	void Create(int width, int height, string title, string? className = "RabotoraXWindowClass");
	void DoEvents();
	void Show();
	void Hide();
	void Close();
	void ToggleFullScreen();
	WindowStateSnapshot GetStateSnapshot();
	
	event EventHandler<(int, int)>? Resized;
	event EventHandler? SwitchingFullScreen;
	event EventHandler? SwitchedFullScreen;
	event EventHandler<bool>? Closing;
	event EventHandler<bool>? FocusChanged;
	event EventHandler? Paint;

	[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, "RabotoraX.Interop.Win32.RenderImpl.Win32NativeWindow", "RabotoraX.Interop.Win32")]
	[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, "RabotoraX.Interop.Linux.RenderImpl.WaylandNativeWindow", "RabotoraX.Interop.Linux")]
	[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, "RabotoraX.Interop.MacOS.RenderImpl.NSNativeWindow", "RabotoraX.Interop.MacOS")]
	[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, "RabotoraX.Interop.Android.RenderImpl.AndroidNativeActivity", "RabotoraX.Interop.Android")]
	[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, "RabotoraX.Interop.IOS.RenderImpl.IOSNativeView", "RabotoraX.Interop.IOS")]
	public static INativeWindow PlatformCreate()
	{
		Type? implType;
		try
		{
			implType = OperatingSystem.IsWindows() ? Type.GetType("RabotoraX.Interop.Win32.RenderImpl.Win32NativeWindow, RabotoraX.Interop.Win32") :
				OperatingSystem.IsLinux() ? Type.GetType("RabotoraX.Interop.Linux.RenderImpl.WaylandNativeWindow, RabotoraX.Interop.Linux") :
				OperatingSystem.IsMacOS() ? Type.GetType("RabotoraX.Interop.MacOS.RenderImpl.NSNativeWindow, RabotoraX.Interop.MacOS") :
				OperatingSystem.IsAndroid() ? Type.GetType("RabotoraX.Interop.Android.RenderImpl.AndroidNativeActivity, RabotoraX.Interop.Android") :
				OperatingSystem.IsIOS() ? Type.GetType("RabotoraX.Interop.IOS.RenderImpl.IOSNativeView, RabotoraX.Interop.IOS") :
				throw new PlatformNotSupportedException("Unsupported platform");
		}
		catch
		{
			implType = null;
		}
		if (implType == null)
		{
			throw new NotImplementedException("This functionality is not implemented in the portable version of this assembly. " +
			                                  "You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
		}
		return (INativeWindow)Activator.CreateInstance(implType)!;
	}
}