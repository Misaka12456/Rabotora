using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using RabotoraX.Core.Inputs;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using JetBrains.Annotations;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;
using static TerraFX.Interop.Windows.RIDEV;
using static TerraFX.Interop.Windows.WM;
using static TerraFX.Interop.Windows.RI;

namespace RabotoraX.Interop.Win32.InputImpl;

[MustDisposeResource, SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
[SupportedOSPlatform("windows")]
public unsafe class RawInputBackend : INativeInput
{
	private readonly nint _hwnd;
	private readonly KeyboardState _keyboard = new();
	private readonly MouseState _mouse = new();
	private readonly bool[] _currentKeys = new bool[256];

	public Vector2 MousePosition => _mouse.Position;
	public bool AnyKeyDown { get; private set; }
	public Vector2 MouseDelta { get; private set; }
	
	private Vector2 _tempMouseDelta = Vector2.Zero;
	private bool _isDisposed;

	public RawInputBackend(nint hwnd)
	{
		_hwnd = hwnd;
		RegisterDevices();
	}

	private void RegisterDevices()
	{
		RAWINPUTDEVICE* devices = stackalloc RAWINPUTDEVICE[2];

		// Keyboard
		devices[0].usUsagePage = 0x01;
		devices[0].usUsage = 0x06;
		devices[0].dwFlags = RIDEV_INPUTSINK;
		devices[0].hwndTarget = (HWND)_hwnd;
		
		// Mouse
		devices[1].usUsagePage = 0x01;
		devices[1].usUsage = 0x02;
		devices[1].dwFlags = RIDEV_INPUTSINK;
		devices[1].hwndTarget = (HWND)_hwnd;
		
		if (!RegisterRawInputDevices(devices, 2, (uint)sizeof(RAWINPUTDEVICE)))
		{
			throw new Win32Exception("RegisterRawInputDevices failed.");
		}
	}

	public void Update()
	{
		POINT p;
		if (GetCursorPos(&p))
		{
			ScreenToClient((HWND)_hwnd, &p);
			_mouse.AdvanceFrame(p.x, p.y);
		}
		else
		{
			var newPos = MousePosition + _tempMouseDelta;
			_mouse.AdvanceFrame((int)newPos.X, (int)newPos.Y);
		}
		
		_keyboard.AdvanceFrame();
		MouseDelta = _tempMouseDelta;
		_tempMouseDelta = Vector2.Zero;
		AnyKeyDown = CheckAnyKeyDown();
	}

	public void ProcessMessage(uint msg, nuint wParam, nint lParam)
	{
		if (msg != WM_INPUT) return;

		uint size = 0;

		_ = GetRawInputData((HRAWINPUT) lParam, RID_INPUT, null, &size, (uint) sizeof(RAWINPUTHEADER)); // ignore the return value as we only want the size
		
		byte* buffer = stackalloc byte[(int)size];
		if (GetRawInputData((HRAWINPUT)lParam, RID_INPUT, buffer, &size, (uint)sizeof(RAWINPUTHEADER)) != size)
		{
			return; // Failed to get data, ignore this message
		}
		
		var raw = (RAWINPUT*)buffer;

		switch (raw->header.dwType)
		{
			case RIM_TYPEKEYBOARD:
				HandleKeyboard(raw->data.keyboard);
				break;
			
			case RIM_TYPEMOUSE:
				HandleMouse(raw->data.mouse);
				break;
		}
	}

	private void HandleKeyboard(RAWKEYBOARD keyboard)
	{
		int key = keyboard.VKey;
		if (key >= 256) return; // Invalid key code
		
		bool isDown = (keyboard.Flags & RI_KEY_BREAK) == 0;
		
		_keyboard.SetPhysicalState(key, isDown);
	}

	private void HandleMouse(RAWMOUSE mouse)
	{
		_tempMouseDelta.X += mouse.lLastX;
		_tempMouseDelta.Y += mouse.lLastY;
		if ((mouse.usButtonFlags & RI_MOUSE_BUTTON_1_DOWN) != 0) _mouse.SetPhysicalState(0, true);
		if ((mouse.usButtonFlags & RI_MOUSE_BUTTON_1_UP) != 0) _mouse.SetPhysicalState(0, false);
		
		if ((mouse.usButtonFlags & RI_MOUSE_BUTTON_2_DOWN) != 0) _mouse.SetPhysicalState(1, true);
		if ((mouse.usButtonFlags & RI_MOUSE_BUTTON_2_UP) != 0) _mouse.SetPhysicalState(1, false);
		
		if ((mouse.usButtonFlags & RI_MOUSE_BUTTON_3_DOWN) != 0) _mouse.SetPhysicalState(2, true);
		if ((mouse.usButtonFlags & RI_MOUSE_BUTTON_3_UP) != 0) _mouse.SetPhysicalState(2, false);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool CheckAnyKeyDown()
	{
		for (int i = 0; i < 8; i++)
		{
			if (_mouse.GetButtonDown(i)) return true;
		}
		for (int i = 0; i < 256; i++)
		{
			if (_keyboard.GetKeyDown((KeyCode)i)) return true;
		}
		
		return false;
	}

	public bool GetKey(KeyCode key) => _keyboard.GetKey(key);
	public bool GetKeyDown(KeyCode key) => _keyboard.GetKeyDown(key);
	public bool GetKeyUp(KeyCode key) => _keyboard.GetKeyUp(key);
	public bool GetMouseButton(int button) => _mouse.GetButton(button);
	public bool GetMouseButtonDown(int button) => _mouse.GetButtonDown(button);
	public bool GetMouseButtonUp(int button) => _mouse.GetButtonUp(button);

	private void UnregisterDevices()
	{
		var devices = stackalloc RAWINPUTDEVICE[2];
		
		devices[0].usUsagePage = 0x01;
		devices[0].usUsage = 0x06;
		devices[0].dwFlags = RIDEV_REMOVE;
		devices[0].hwndTarget = HWND.NULL;
		
		devices[1].usUsagePage = 0x01;
		devices[1].usUsage = 0x02;
		devices[1].dwFlags = RIDEV_REMOVE;
		devices[1].hwndTarget = HWND.NULL;
		
		RegisterRawInputDevices(devices, 2, (uint)sizeof(RAWINPUTDEVICE));
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	
	protected virtual void Dispose(bool disposing)
	{
		if (_isDisposed) return;
		UnregisterDevices();
		_isDisposed = true;
	}
	
	~RawInputBackend()
	{
		Dispose(false);
	}
}