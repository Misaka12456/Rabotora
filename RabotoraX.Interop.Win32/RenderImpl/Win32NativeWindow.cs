using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.Inputs;
using RabotoraX.Core.Mathematics;
using RabotoraX.Interop.Win32.InputImpl;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;
using static TerraFX.Interop.Windows.CS;
using static TerraFX.Interop.Windows.WM;
using static TerraFX.Interop.Windows.WMSZ;
using static TerraFX.Interop.Windows.WS;
using static TerraFX.Interop.Windows.SC;
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

	private HWND _hwnd;
	private string _className = "RabotoraXWindowClass";
	private GCHandle _this;
	private WindowStateSnapshot _latestSnapshot;
	private RawInputBackend? _input;
	private bool _isFullScreen;
	private WINDOWPLACEMENT _prevPlacement = new() {length = (uint) sizeof(WINDOWPLACEMENT)};
	private Fractional? _fixedAspectRatio;
	private bool _isDisposed;

	public void Create(int width, int height, string title, string? className = "RabotoraXWindowClass", Fractional? fixedAspectRatio = null)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);
		InitializeDpiAwareness();
		Size = (width, height);
		_fixedAspectRatio = fixedAspectRatio;
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

			var rect = new RECT() {left = 0, top = 0, right = width, bottom = height};
			if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 14393)) // Windows 10 1607 Anniversary Update
			{
				AdjustWindowRectExForDpi(&rect, WS_OVERLAPPEDWINDOW, false, 0, USER_DEFAULT_SCREEN_DPI);
			}
			else
			{
				AdjustWindowRectEx(&rect, WS_OVERLAPPEDWINDOW, false, 0);
			}

			int actualWidth = rect.right - rect.left;
			int actualHeight = rect.bottom - rect.top;

			uint style = WS_OVERLAPPEDWINDOW;
			if (fixedAspectRatio.HasValue)
			{
				style &= ~(uint) WS_MAXIMIZEBOX; // Disable maximize to avoid aspect ratio breaking. Users can still use Alt+Enter to toggle full screen mode, which does not break the aspect ratio.
			}

			_hwnd = CreateWindowExW(0, classPtr, titlePtr, style,
				CW_USEDEFAULT, CW_USEDEFAULT, actualWidth, actualHeight,
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
			_latestSnapshot.IsFocused = focus;
			GraphicsService.Update(_latestSnapshot);
		};
	}

	public void Show()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);
		ShowWindow(_hwnd, SW_SHOW);
		UpdateWindow(_hwnd);
		IsVisible = true;
		_latestSnapshot.IsVisible = true;
		GraphicsService.Update(_latestSnapshot);
	}

	public void Hide()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);
		ShowWindow(_hwnd, SW_HIDE);
		IsVisible = false;
		_latestSnapshot.IsVisible = false;
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

		int bRet = GetMessageW(&msg, HWND.NULL, 0, 0); // GetMessage is blocked, so that the CPU usage will be significantly reduced
		if (bRet > 0)
		{
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}
		else if (bRet == 0) // WM_QUIT received
		{
			IsClosing = true;
		}
		else
		{
#if DEBUG
			System.Diagnostics.Debug.WriteLine($"GetMessageW failed with error code {Marshal.GetLastWin32Error()}");
#endif
			IsClosing = true;
		}
	}

	public WindowStateSnapshot GetStateSnapshot()
	{
		return _latestSnapshot;
	}

	public void ToggleFullScreen()
	{
		SwitchingFullScreen?.Invoke(this, EventArgs.Empty);

		var style = (nuint)GetWindowLongPtrW(_hwnd, GWL_STYLE);
		uint newStyle;
		RECT targetRect;
    
		if (!_isFullScreen)
		{
			fixed (WINDOWPLACEMENT* pPrev = &_prevPlacement) GetWindowPlacement(_hwnd, pPrev);
			var hMonitor = MonitorFromWindow(_hwnd, MONITOR_DEFAULTTONEAREST);
			var mi = new MONITORINFO() { cbSize = (uint)sizeof(MONITORINFO) };
			GetMonitorInfoW(hMonitor, &mi);
        
			newStyle = (uint)(style & ~(nuint)WS_OVERLAPPEDWINDOW);
			targetRect = mi.rcMonitor;
		}
		else
		{
			newStyle = (uint)(style | WS_OVERLAPPEDWINDOW);
			if (_fixedAspectRatio.HasValue) newStyle &= ~(uint)WS_MAXIMIZEBOX;
			targetRect = _prevPlacement.rcNormalPosition;
		}

		SetWindowLongPtrW(_hwnd, GWL_STYLE, (nint)newStyle);
    
		if (!_isFullScreen)
		{
			SetWindowPos(_hwnd, HWND.HWND_TOP, 
				targetRect.left, targetRect.top,
				targetRect.right - targetRect.left,
				targetRect.bottom - targetRect.top,
				SWP_NOOWNERZORDER | SWP_FRAMECHANGED | SWP_SHOWWINDOW);
			_isFullScreen = true;
		}
		else
		{
			// 恢复窗口模式
			fixed (WINDOWPLACEMENT* pPrev = &_prevPlacement) SetWindowPlacement(_hwnd, pPrev);
			_isFullScreen = false;
		}

		// 3. 只有在更新状态快照时才加锁，确保渲染线程看到的是一致的尺寸
		lock (GraphicsService.API.RenderLock)
		{
			RECT rect;
			GetClientRect(_hwnd, &rect);
			_latestSnapshot.Width = rect.right - rect.left;
			_latestSnapshot.Height = rect.bottom - rect.top;
			GraphicsService.Update(_latestSnapshot);
		}
		InvalidateRect(_hwnd, null, false);
		UpdateWindow(_hwnd);
		SwitchedFullScreen?.Invoke(this, EventArgs.Empty);
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
				if (managed != null)
				{
					int width = LOWORD(lParam);
					int height = HIWORD(lParam);
					if (managed._latestSnapshot.Width != width || managed._latestSnapshot.Height != height)
					{
						managed._latestSnapshot.Width = width;
						managed._latestSnapshot.Height = height;
						GraphicsService.Update(managed._latestSnapshot);
						managed.Resized?.Invoke(managed, (width, height));
					}
				}

				return 0;
			}

			case WM_SIZING:
			{
				if (managed != null && managed._fixedAspectRatio.HasValue)
				{
					var ratio = (double) managed._fixedAspectRatio.Value.Numerator / managed._fixedAspectRatio.Value.Denominator;
					var rect = (RECT*) lParam;

					var adjRect = new RECT();
					uint style = (uint) GetWindowLongPtrW(hWnd, GWL_STYLE);
					uint exStyle = (uint) GetWindowLongPtrW(hWnd, GWL_EXSTYLE);
					AdjustWindowRectEx(&adjRect, style, false, exStyle);

					int borderWidth = (adjRect.right - adjRect.left);
					int borderHeight = (adjRect.bottom - adjRect.top);

					int totalWidth = rect->right - rect->left;
					int totalHeight = rect->bottom - rect->top;

					switch ((int) wParam)
					{
						case WMSZ_LEFT:
						case WMSZ_RIGHT:
							int targetClientHeight = (int) ((totalWidth - borderWidth) / ratio);
							rect->bottom = rect->top + targetClientHeight + borderHeight;
							break;

						case WMSZ_TOP:
						case WMSZ_BOTTOM:
							int targetClientWidth = (int) ((totalHeight - borderHeight) * ratio);
							rect->right = rect->left + targetClientWidth + borderWidth;
							break;

						default:
							int clientHeightFromWidth = (int) ((totalWidth - borderWidth) / ratio);
							rect->bottom = rect->top + clientHeightFromWidth + borderHeight;
							break;
					}
				}

				return 1; // Return non-zero to prevent the system from resizing the window, since we have already adjusted the size in place.
			}

			case WM_SYSCOMMAND:
			{
				ulong command = wParam & 0xFFF0;
				if (managed != null && managed._fixedAspectRatio.HasValue)
				{
					if (command == SC_MAXIMIZE && !managed._isFullScreen)
					{
						return 0; // Block the maximize command to prevent aspect ratio breaking. Full screen toggle will bypass SC_MAXIMIZE and will work fine.
					}
				}

				break;
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

	private static void InitializeDpiAwareness()
	{
		if (!OperatingSystem.IsWindows()) return;
		if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 15063))
		{
			SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
		}
		else if (OperatingSystem.IsWindowsVersionAtLeast(8, 1))
		{
			SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
		}
		else
		{
			SetProcessDPIAware();
		}
	}
}