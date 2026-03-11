using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.Inputs;
using RabotoraX.Interop.Win32.InputImpl;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;
using static TerraFX.Interop.Windows.CS;
using static TerraFX.Interop.Windows.WM;
using static TerraFX.Interop.Windows.WS;
using static TerraFX.Interop.Windows.SW;
using static TerraFX.Interop.Windows.GWLP;
using static TerraFX.Interop.Windows.GWL;
using static TerraFX.Interop.Windows.MONITOR;
using static TerraFX.Interop.Windows.SWP;

namespace RabotoraX.Interop.Win32.RenderImpl;

[SupportedOSPlatform("windows")]
public unsafe class Win32NativeWindow : INativeWindow
{
	public nint Handle => _hwnd;
	public string Title
	{
		get => GetTitle();
		set => SetTitle(value);
	}
	public (int Width, int Height) Size { get; set; }

	public bool IsClosing { get; private set; }
	public bool IsVisible { get; private set; }
	public bool IsFullScreen { get; private set; }
	public INativeInput Input => _input ?? throw new ObjectDisposedException(nameof(Win32NativeWindow));

	public event EventHandler? SwitchingFullScreen;
	public event EventHandler? SwitchedFullScreen;
	public event EventHandler<(int, int)>? Resized;
	public event EventHandler<bool>? Closing;
	public event EventHandler<bool>? FocusChanged;
	public event EventHandler? Paint;

	private readonly Lock _lockSnapshot = new();
	private HWND _hwnd;
	private string _className = "RabotoraXWindowClass";
	private GCHandle _this;
	private WindowStateSnapshot _latestSnapshot;
	private RawInputBackend? _input;
	private bool _isFullScreen;
	private WINDOWPLACEMENT _prevPlacement = new() { length = (uint)sizeof(WINDOWPLACEMENT) };
	private bool _isDisposed;

	public void Create(int width, int height, string title, string? className = "RabotoraXWindowClass")
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);
		Size = (width, height);
		_latestSnapshot = new WindowStateSnapshot();
		(_latestSnapshot.Width, _latestSnapshot.Height) = Size;
		GraphicsService.Update(_latestSnapshot);
		if (className != null)
		{
			_className = className;
		}
		
		fixed (char* classPtr = _className)
		fixed (char* titlePtr = title)
		{
			var wc = new WNDCLASSEXW()
			{
				cbSize = (uint) Marshal.SizeOf<WNDCLASSEXW>(),
				style = CS_HREDRAW | CS_VREDRAW,
				lpfnWndProc = &NativeWndProc,
				hInstance = GetModuleHandleW(null),
				hCursor = LoadCursorW(HINSTANCE.NULL, IDC.IDC_ARROW),
				// hbrBackground = (HBRUSH) (COLOR.COLOR_WINDOW + 1),
				hbrBackground = HBRUSH.NULL, // to avoid white flash when starting the game or resizing the window. The game will handle clearing the background itself.
				lpszClassName = classPtr
			};

			if (RegisterClassExW(&wc) == 0)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error(), "RegisterClassExW failed");
			}
			
			_hwnd = CreateWindowExW(0, classPtr, titlePtr, WS.WS_OVERLAPPEDWINDOW,
				CW_USEDEFAULT, CW_USEDEFAULT, width, height,
				HWND.NULL, HMENU.NULL, wc.hInstance, null);
			
			if (_hwnd == HWND.NULL)
			{
				throw new Win32Exception("CreateWindowExW failed");
			}
			
			_this = GCHandle.Alloc(this);
			SetWindowLongPtrW(_hwnd, GWLP_USERDATA, GCHandle.ToIntPtr(_this));
			_input = new RawInputBackend(_hwnd);
		}

		FocusChanged += (sender, focus) =>
		{
			lock (_lockSnapshot)
			{
				_latestSnapshot.IsFocused = focus;
			}
			GraphicsService.Update(_latestSnapshot);
		};
	}

	public void Show()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);
		ShowWindow(_hwnd, SW_SHOW);
		UpdateWindow(_hwnd);
		IsVisible = true;
		lock (_lockSnapshot)
		{
			_latestSnapshot.IsVisible = true;
		}
		GraphicsService.Update(_latestSnapshot);
	}

	public void Hide()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);
		ShowWindow(_hwnd, SW_HIDE);
		IsVisible = false;
		lock (_lockSnapshot)
		{
			_latestSnapshot.IsVisible = false;
		}
		GraphicsService.Update(_latestSnapshot);
	}

	public void Close()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);
		if (_hwnd == HWND.NULL) return;
		DestroyWindow(_hwnd);
	}

	public void DoEvents()
	{
		MSG msg;

		while (PeekMessageW(&msg, HWND.NULL, 0, 0, PM.PM_REMOVE) != 0)
		{
			if (msg.message == WM_QUIT)
			{
				IsClosing = true;
				break;
			}
			
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}
	}

	public WindowStateSnapshot GetStateSnapshot()
	{
		lock (_lockSnapshot)
		{
			return _latestSnapshot;
		}
	}

	public void ToggleFullScreen()
	{
		SwitchingFullScreen?.Invoke(this, EventArgs.Empty);
		var style = (nuint)GetWindowLongPtrW(_hwnd, GWL_STYLE);
		if (!_isFullScreen)
		{
			fixed (WINDOWPLACEMENT* pPrev = &_prevPlacement)
			{
				GetWindowPlacement(_hwnd, pPrev);
			}
			
			var hMonitor = MonitorFromWindow(_hwnd, MONITOR_DEFAULTTONEAREST);
			var mi = new MONITORINFO() { cbSize = (uint)sizeof(MONITORINFO) };
			GetMonitorInfoW(hMonitor, &mi);
			
			// WS_OVERLAPPEDWINDOW = WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX
			SetWindowLongPtrW(_hwnd, GWL_STYLE, (nint)(style & ~(nuint)WS_OVERLAPPEDWINDOW));
			
			SetWindowPos(_hwnd, HWND.HWND_TOP, 
				mi.rcMonitor.left, mi.rcMonitor.top,
				mi.rcMonitor.right - mi.rcMonitor.left,
				mi.rcMonitor.bottom - mi.rcMonitor.top,
				SWP_NOOWNERZORDER | SWP_FRAMECHANGED);
			SwitchedFullScreen?.Invoke(this, EventArgs.Empty);
			
			_isFullScreen = true;
		}
		else
		{
			SetWindowLongPtrW(_hwnd, GWL_STYLE, (nint)(style | WS_OVERLAPPEDWINDOW));
			fixed (WINDOWPLACEMENT* pPrev = &_prevPlacement)
			{
				SetWindowPlacement(_hwnd, pPrev);
			}
			SetWindowPos(_hwnd, HWND.NULL, 0, 0, 0, 0,
				SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOOWNERZORDER | SWP_FRAMECHANGED);
			SwitchedFullScreen?.Invoke(this, EventArgs.Empty);

			_isFullScreen = false;
		}
	}

	private string GetTitle()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);
		Span<char> buffer = stackalloc char[256];

		fixed (char* ptr = buffer)
		{
			int len = GetWindowTextW(_hwnd, ptr, buffer.Length);
			return new string(buffer[..len]);
		}
	}

	private void SetTitle(string title)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);
		fixed (char* ptr = title)
		{
			SetWindowTextW(_hwnd, ptr);
		}
	}

	[UnmanagedCallersOnly]
	private static LRESULT NativeWndProc(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam)
	{
		var managed = GetThis(hWnd);
		managed?._input?.ProcessMessage(msg, wParam, lParam);
		switch (msg)
		{
			case WM_CLOSE:
				DestroyWindow(hWnd);
				return 0;
			
			case WM_DESTROY:
				SetWindowLongPtrW(hWnd, GWLP_USERDATA, 0); // clear userdata to avoid 0xc0000005 (Access Freed Memory) especially in Debug Mode
				PostQuitMessage(0);
				return 0;

			case WM_SIZE:
			{
				int width = LOWORD(lParam);
				int height = HIWORD(lParam);
				if (managed?._lockSnapshot != null)
				{
					lock (managed._lockSnapshot)
					{
						managed._latestSnapshot.Width = width;
						managed._latestSnapshot.Height = height;
					}
				}

				managed?.Resized?.Invoke(managed, (width, height));
				return 0;
			}

			case WM_SETFOCUS:
			{
				managed?.FocusChanged?.Invoke(managed, true);
				break;
			}
			
			case WM_KILLFOCUS:
			{
				managed?.FocusChanged?.Invoke(managed, false);
				break;
			}

			case WM_PAINT:
			{
				if (managed?.Paint != null)
				{
					PAINTSTRUCT ps;
					BeginPaint(hWnd, &ps);
					
					managed.Paint.Invoke(managed, EventArgs.Empty);
					
					EndPaint(hWnd, &ps);
				}
				else
				{
					return DefWindowProcW(hWnd, msg, wParam, lParam);
				}
				return 0;
			}
			case WM_SYSKEYDOWN:
			{
				if (wParam == 0x0D && (lParam & (1 << 29)) != 0) // Alt + Enter
				{
					managed?.ToggleFullScreen();
					return 0;
				}
				break;
			}
			case WM_ERASEBKGND:
				return 1; // Indicate that we handled background erasing to prevent flickering. The game will handle clearing the background itself.
		}
		
		return DefWindowProcW(hWnd, msg, wParam, lParam);
	}

	private static Win32NativeWindow? GetThis(HWND hWnd)
	{
		var ptr = GetWindowLongPtrW(hWnd, GWLP_USERDATA);
		if (ptr == 0) return null;

		var handle = GCHandle.FromIntPtr(ptr);
		return handle.Target as Win32NativeWindow;
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	
	protected virtual void Dispose(bool disposing)
	{
		if (_isDisposed) return;
		if (disposing)
		{
			_input?.Dispose();
			_input = null;
			if (_this.IsAllocated)
			{
				_this.Free();
			}
		}
			
		if (_hwnd != HWND.NULL)
		{
			DestroyWindow(_hwnd);
			_hwnd = HWND.NULL;
		}
			
		_isDisposed = true;
	}
	
	~Win32NativeWindow()
	{
		Dispose(false);
	}
}