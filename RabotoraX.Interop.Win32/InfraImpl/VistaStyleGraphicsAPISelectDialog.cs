using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using RabotoraX.Core.Diagnostics;
using RabotoraX.Core.Graphics;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;
using static TerraFX.Interop.Windows.VK;

namespace RabotoraX.Interop.Win32.InfraImpl;

[SupportedOSPlatform("windows")]
public sealed unsafe class VistaStyleGraphicsAPISelectDialog : INativeGraphicsAPISelectDialog
{
	private readonly static string[][] SupportedGraphicsAPIs =
	[
		["RabotoraX.Interop.Direct3D11.DirectX11, RabotoraX.Interop.Direct3D11", "DirectX 11"],
		["RabotoraX.Interop.Direct3D12.DirectX12, RabotoraX.Interop.Direct3D12", "DirectX 12"],
		// ["RabotoraX.Interop.Vulkan.Vulkan, RabotoraX.Interop.Vulkan", "Vulkan"] (TODO: We didn't implement Vulkan support yet, so hide this option for now)
	];
	
	public bool GetIsUserRequestedToSelect()
	{
		short keyState = GetAsyncKeyState(VK_SHIFT); // only when user pressing Shift when starting the app we show the dialog.
		return (keyState & 0x8000) != 0;
	}

	public Type? ShowDialog()
	{
		int apiCount = SupportedGraphicsAPIs.Length;
		
		var buttons = stackalloc TASKDIALOG_BUTTON[apiCount];
		var stringPtrs = new nint[apiCount];

		try
		{
			for (int i = 0; i < apiCount; i++)
			{
				stringPtrs[i] = Marshal.StringToHGlobalUni(SupportedGraphicsAPIs[i][1]);

				buttons[i].nButtonID = 1000 + i; // arbitrary ID, just needs to be unique
				buttons[i].pszButtonText = (char*) stringPtrs[i];
			}

			var config = new TASKDIALOGCONFIG()
			{
				cbSize = (uint) sizeof(TASKDIALOGCONFIG),
				hwndParent = HWND.NULL,
				// ReSharper disable BitwiseOperatorOnEnumWithoutFlags
				dwFlags = (int) (TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS | TASKDIALOG_FLAGS.TDF_ALLOW_DIALOG_CANCELLATION | TASKDIALOG_FLAGS.TDF_SIZE_TO_CONTENT),
				// ReSharper restore BitwiseOperatorOnEnumWithoutFlags
				cButtons = (uint) apiCount,
				pButtons = buttons
			};

			config.Anonymous1.pszMainIcon = (char*) TD.TD_INFORMATION_ICON;

			fixed (char* title = "RabotoraX Engine Debug Initialization")
			fixed (char* mainInstru = "Select Graphics API Backend")
			fixed (char* content = "Debug key (Default for Shift) detected.\nChoose the graphics backend you want to use for this debug session:")
			{
				config.pszWindowTitle = title;
				config.pszMainInstruction = mainInstru;
				config.pszContent = content;

				int clickedButtonId = 0;

				var hr = TaskDialogIndirect(&config, &clickedButtonId, null, null);

				if (hr.SUCCEEDED && clickedButtonId >= 1000 && clickedButtonId < 1000 + apiCount)
				{
					int selectedIndex = clickedButtonId - 1000;
					string typeDefinition = SupportedGraphicsAPIs[selectedIndex][0];
					return Type.GetType(typeDefinition);
				}
			}

			return null; // user canceled or closed the dialog
		}
		finally
		{
			for (int i = 0; i < apiCount; i++)
			{
				if (stringPtrs[i] != 0)
				{
					Marshal.FreeHGlobal(stringPtrs[i]);
				}
			}
		}
	}
	
	public void Dispose()
	{
		// We already free the unmanaged memory by try-finally, so there's nothing else to do here.
	}
}