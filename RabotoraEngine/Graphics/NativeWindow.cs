using System.ComponentModel;
using System.Runtime.InteropServices;
using Rabotora.Core;
using Rabotora.Internal;

namespace Rabotora.Graphics;

public abstract class NativeWindow : IDisposable
{
	public bool IsRunning { get; private set; } = true;

	public string Title { get; private set; }
	public event Action? Resized;
	public event Action? OnUpdate, OnRender; // 更新和渲染事件
	public event Func<bool>? WindowClosing; // 关闭窗口时触发的事件，返回值决定是否取消关闭(类似winform的e.Cancel，为true就会取消关闭)
	
	protected IntPtr Handle { get; set; }
	public int Width { get; private set; }
	public int Height { get; private set; }
	protected bool FixedRatio { get; set; } = false;
	protected Resolution RatioRefResolution { get; set; } = new(1280, 720); // 参考分辨率，默认16:9 720p

	private readonly string _className;
	private bool _disposed, _isResizing = false;

	// ReSharper disable once NotAccessedField.Local
	private Win32Api.WndProc _wndProcDelegate; // 保留对委托的引用，防止被垃圾回收导致Process terminated (-2146232797)

	protected NativeWindow(string title, int width, int height, string className = "RabotoraGameWindowClass")
	{
		Width = width;
		Height = height;
		className += $"_{Guid.NewGuid():N}"; // 窗口类名，使用GUID避免冲突
		_className = className;
		// 注册窗口类
		var wndClass = new Win32Api.WNDCLASSEX()
		{
			cbSize = (uint)Marshal.SizeOf<Win32Api.WNDCLASSEX>(),
			style = Win32Api.CS_HREDRAW | Win32Api.CS_VREDRAW,
			lpfnWndProc = _wndProcDelegate = WndProc,
			hInstance = Win32Api.GetModuleHandle(null!),
			hCursor = Win32Api.LoadCursor(IntPtr.Zero, 32512), // IDC_ARROW
			lpszClassName = _className
		};
		if (Win32Api.RegisterClassEx(ref wndClass) == 0)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to register window class");
		}

		Title = title.Replace('\0', '_');

		Title += '\0';

		// 创建窗口
		Handle = Win32Api.CreateWindowEx(0, _className, Title, Win32Api.WS_OVERLAPPEDWINDOW, 100, 100, width, height, IntPtr.Zero, IntPtr.Zero,
			Win32Api.GetModuleHandle(null!), IntPtr.Zero);
		if (Handle == IntPtr.Zero)
			throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to create window");
		
		Win32Api.ShowWindow(Handle, Win32Api.SW_SHOWNORMAL);
		Win32Api.UpdateWindow(Handle);
	}

	// 消息处理入口
	private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
	{
		switch (msg)
		{
			case Win32Api.WM_CLOSE:
				bool? cancel = WindowClosing?.Invoke(); // 触发关闭事件
				if (cancel != true)
				{
					Win32Api.DestroyWindow(hWnd); // 如果未取消，则销毁窗口
				}
				return IntPtr.Zero;

			case Win32Api.WM_DESTROY:
				IsRunning = false; // 标记窗口已关闭
				Win32Api.PostQuitMessage(0);
				return IntPtr.Zero;
			
			case Win32Api.WM_ENTERSIZEMOVE:
				_isResizing = true; // 开始拖拽
				break;
			
			case Win32Api.WM_EXITSIZEMOVE:
				_isResizing = false;
				Resized?.Invoke(); // 拖拽结束后强制更新
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
				Resized?.Invoke(); // 拖拽过程中更新 (防止拖拽过程中原始窗口外的范围直接显示为黑色)
				return IntPtr.Zero;

			case Win32Api.WM_SIZE:
				Width = (int)(lParam.ToInt64() & 0xFFFF);
				Height = (int)((lParam.ToInt64() >> 16) & 0xFFFF);
				if (!_isResizing) // 非拖拽时直接更新
				{
					Resized?.Invoke();
				}
				break;
			default:
				return Win32Api.DefWindowProc(hWnd, msg, wParam, lParam);
		}
		return IntPtr.Zero;
	}

	// 主消息循环
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
		// 处理所有待处理的消息（非阻塞）
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
			Win32Api.RECT rect;
			Win32Api.GetWindowRect(Handle, out rect);
			int newWidth = rect.Right - rect.Left;
			int newHeight = rect.Bottom - rect.Top;
			if (newWidth != Width || newHeight != Height)
			{
				Width = newWidth;
				Height = newHeight;
				Resized?.Invoke(); // 处理窗口大小变化
			}
		}

		// 调用更新和渲染逻辑
		OnUpdate?.Invoke();
		OnRender?.Invoke();

		// 避免 CPU 占用过高
		Thread.Sleep(1);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				Win32Api.DestroyWindow(Handle);
				Win32Api.UnregisterClass(_className, Win32Api.GetModuleHandle(null!));
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