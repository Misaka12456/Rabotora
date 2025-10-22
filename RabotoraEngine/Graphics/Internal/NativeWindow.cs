using System.ComponentModel;
using System.Runtime.InteropServices;
using RabotoraEngine.Internal.InteropServices;

namespace RabotoraEngine.Graphics.Internal;

public abstract class NativeWindow : IDisposable
{
	public bool IsRunning { get; private set; } = true;
	
	public string Title { get; private set; }
	public event EventHandler? Resized, Updating, Rendering;
	public event Func<bool>? WindowClosing;
	
	protected IntPtr Handle { get; set; }
	public int Width { get; private	set; }
	public int Height { get; private set; }
	protected bool FixedRatio { get; set; } = false;
	protected Resolution RatioRefResolution { get; set; } = new(1280, 720); // ref resolution, default 16:9@720p

	private readonly string _className;
	private bool _disposed, _isResizing = false;
	
	private GCHandle _wndProcDelegateHandle;

	protected NativeWindow(string title, int width, int height, string className = "RabotoraGameWindowClass")
	{
		Width = width;
		Height = height;
		className += $"_{Guid.NewGuid():N}";
		_className = className;
		var wndClass = new Win32Api.WNDCLASSEX()
		{
			cbSize = (uint) Marshal.SizeOf<Win32Api.WNDCLASSEX>(),
			style = Win32Api.CS_HREDRAW | Win32Api.CS_VREDRAW,
			lpfnWndProc = WndProc,
			hInstance = Win32Api.GetModuleHandle(null!),
			hCursor = Win32Api.LoadCursor(IntPtr.Zero, 32512), // IDC_ARROW
			hbrBackground = Win32Api.GetStockObject(Win32Api.BLACK_BRUSH),
			lpszClassName = _className
		};
		_wndProcDelegateHandle = GCHandle.Alloc(wndClass.lpfnWndProc);
		if (Win32Api.RegisterClassEx(ref wndClass) == 0)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to register window class.");
		}

		Title = title.Replace('\0', '_');
		
		Title += '\0';

		Handle = Win32Api.CreateWindowEx(0, _className, Title, Win32Api.WS_OVERLAPPEDWINDOW, 100, 100, width, height, IntPtr.Zero, IntPtr.Zero,
			Win32Api.GetModuleHandle(null!), IntPtr.Zero);
		if (Handle == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to create window.");
		}
		
		Win32Api.ShowWindow(Handle, Win32Api.SW_SHOWNORMAL);
		Win32Api.UpdateWindow(Handle);
	}

	private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
	{
		switch (msg)
		{
			case Win32Api.WM_CLOSE:
				bool? cancel = WindowClosing?.Invoke();
				if (cancel != true)
				{
					Win32Api.DestroyWindow(hWnd);
				}
				return IntPtr.Zero;
			case Win32Api.WM_DESTROY:
				IsRunning = false;
				Win32Api.PostQuitMessage(0);
				return IntPtr.Zero;
			case Win32Api.WM_ENTERSIZEMOVE:
				_isResizing = true;
				break;
			case Win32Api.WM_EXITSIZEMOVE:
				_isResizing = false;
				Resized?.Invoke(this, EventArgs.Empty);
				break;
			case Win32Api.WM_SIZING:
				if (FixedRatio)
				{
					var rect = Marshal.PtrToStructure<Win32Api.RECT>(lParam);
					
					int proposedWidth = rect.Right - rect.Left;
					int proposedHeight = rect.Bottom - rect.Top;
					double targetAspect = (double)RatioRefResolution.Width / RatioRefResolution.Height;
					double currentAspect = (double)proposedWidth / proposedHeight;

					if (currentAspect > targetAspect)
					{
						int newWidth = (int)(proposedHeight * targetAspect);
						rect.Right = rect.Left + newWidth;
					}
					else
					{
						int newHeight = (int)(proposedWidth / targetAspect);
						rect.Bottom = rect.Top + newHeight;
					}
					Marshal.StructureToPtr(rect, lParam, false);
				}
				Resized?.Invoke(this, EventArgs.Empty); // update during resizing to avoid black areas outside original window area
				return IntPtr.Zero;
			case Win32Api.WM_SIZE:
				Width = (int)(lParam.ToInt64() & 0xFFFF);
				Height = (int)((lParam.ToInt64() >> 16) & 0xFFFF);
				if (!_isResizing) // direct update in non-drag resize
				{
					Resized?.Invoke(this, EventArgs.Empty);
				}
				break;
			default:
				return Win32Api.DefWindowProc(hWnd, msg, wParam, lParam);
		}
		return IntPtr.Zero;
	}

	[Obsolete("Use Run(Action? update, Action? render) instead.")]
	public void RunMessageLoop() => Run();

	public virtual void Run()
	{
		while (IsRunning)
		{
			InternalUpdate(out _);
		}
	}

	private void InternalUpdate(out Win32Api.MSG msg)
	{
		while (Win32Api.PeekMessage(out msg, IntPtr.Zero, 0, 0, Win32Api.PM_REMOVE))
		{
			if (msg.message == Win32Api.WM_QUIT)
			{
				IsRunning = false;
				break;
			}
			
			Win32Api.TranslateMessage(ref msg);
			Win32Api.DispatchMessage(ref msg);
		}

		if (_isResizing)
		{
			Win32Api.GetWindowRect(Handle, out var rect);
			int newWidth = rect.Right - rect.Left;
			int newHeight = rect.Bottom - rect.Top;
			if (newWidth != Width || newHeight != Height)
			{
				Width = newWidth;
				Height = newHeight;
				Resized?.Invoke(this, EventArgs.Empty);
			}
		}
		
		Updating?.Invoke(this, EventArgs.Empty);
		Rendering?.Invoke(this, EventArgs.Empty);
		
		Thread.Sleep(1); // prevent high CPU usage
	}
	
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				Win32Api.DestroyWindow(Handle);
				Win32Api.UnregisterClass(_className, Win32Api.GetModuleHandle(null!));
				_wndProcDelegateHandle.Free();
			}
			_disposed = true;
		}
	}
	
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	
	~NativeWindow() => Dispose(false);
}