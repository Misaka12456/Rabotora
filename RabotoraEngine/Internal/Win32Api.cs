using System.Runtime.InteropServices;
using System.Text;

namespace Rabotora.Internal;

internal static class Win32Api
{
	// 常量定义
	public const int WM_DESTROY = 0x0002;
	public const int WM_CLOSE = 0x0010;
	public const int WM_QUIT = 0x0012;
	public const int WM_SIZE = 0x0005;
	public const int WM_LBUTTONDOWN = 0x0201;
	public const int WM_RBUTTONDOWN = 0x0204;
	public const int WM_ENTERSIZEMOVE = 0x0231;
	public const int WM_SIZING = 0x0214;
	public const int WM_EXITSIZEMOVE = 0x0232;
	public const int WM_WINDOWPOSCHANGED = 0x0047;
	private const uint WM_GETTEXTLENGTH = 0x000E;

	public const uint WS_OVERLAPPEDWINDOW = 0x00CF0000;
	public const uint CS_HREDRAW = 0x0002;
	public const uint CS_VREDRAW = 0x0001;

	public const int SW_SHOWNORMAL = 1;
	
	public const int PM_REMOVE = 0x0001;

	// Win32 结构体
	[StructLayout(LayoutKind.Sequential)]
	public struct WNDCLASSEX
	{
		public uint cbSize;
		public uint style;
		public WndProc lpfnWndProc;
		public int cbClsExtra;
		public int cbWndExtra;
		public IntPtr hInstance;
		public IntPtr hIcon;
		public IntPtr hCursor;
		public IntPtr hbrBackground;
		public string lpszMenuName;
		public string lpszClassName;
		public IntPtr hIconSm;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MSG
	{
		public IntPtr hWnd;
		public uint message;
		public IntPtr wParam;
		public IntPtr lParam;
		public uint time;
		public POINT pt;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct POINT
	{
		public int X;
		public int Y;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct RECT
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
	}

	// Win32 函数声明
	[DllImport("user32.dll", EntryPoint = "RegisterClassEx", SetLastError = true)]
	public static extern ushort RegisterClassEx(ref WNDCLASSEX lpWndClass);

	[DllImport("user32.dll", EntryPoint = "CreateWindowExW", SetLastError = true, CharSet = CharSet.Unicode)]
	public static extern IntPtr CreateWindowEx(uint dwExStyle, [MarshalAs(UnmanagedType.LPWStr)] string lpClassName, [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName, uint dwStyle,
		int X, int Y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
	
	[DllImport("user32.dll", EntryPoint = "SetWindowTextW", SetLastError = true, CharSet = CharSet.Unicode)]
	public static extern bool SetWindowText(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] string lpString);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool UpdateWindow(IntPtr hWnd);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool TranslateMessage([In] ref MSG lpMsg);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

	[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	public static extern IntPtr GetModuleHandle(string lpModuleName);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern void PostQuitMessage(int nExitCode);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool DestroyWindow(IntPtr hWnd);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);
	
	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
	
	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

	// 委托定义
	public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
}