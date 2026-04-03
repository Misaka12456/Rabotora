using System.Diagnostics.CodeAnalysis;

namespace RabotoraX.Core.Diagnostics;

public interface INativeGraphicsAPISelectDialog : IDisposable
{
	bool GetIsUserRequestedToSelect();
	
	/// <summary>
	/// Shows the dialog to the user and returns the selected graphics API type, or null if the user cancels the dialog or an error occurs.
	/// </summary>
	/// <returns>The selected graphics API type, targeted to the class who implements the <see cref="INativeGraphicsAPI" /> interface, or null if the user cancels the dialog or an error occurs.</returns>
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	Type? ShowDialog();
	
	[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, "RabotoraX.Interop.Win32.InfraImpl.VistaStyleGraphicsAPISelectDialog", "RabotoraX.Interop.Win32")]
	[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, "RabotoraX.Interop.Linux.InfraImpl.GtkGraphicsAPISelectDialog", "RabotoraX.Interop.Linux")]
	[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, "RabotoraX.Interop.MacOS.InfraImpl.CocoaGraphicsAPISelectDialog", "RabotoraX.Interop.MacOS")]
	public static INativeGraphicsAPISelectDialog? PlatformCreate()
	{
		Type? implType;
		try
		{
			implType = OperatingSystem.IsWindows() ? Type.GetType("RabotoraX.Interop.Win32.InfraImpl.VistaStyleGraphicsAPISelectDialog, RabotoraX.Interop.Win32") :
				OperatingSystem.IsLinux() ? Type.GetType("RabotoraX.Interop.Linux.InfraImpl.GtkGraphicsAPISelectDialog, RabotoraX.Interop.Linux") :
				OperatingSystem.IsMacOS() ? Type.GetType("RabotoraX.Interop.MacOS.InfraImpl.CocoaGraphicsAPISelectDialog, RabotoraX.Interop.MacOS") :
				null; // Other platforms are not supported for this dialog, fallback to default graphics API without showing a dialog.
		}
		catch
		{
			implType = null;
		}
		
		return implType == null ? null : (INativeGraphicsAPISelectDialog)Activator.CreateInstance(implType)!;
	}
}