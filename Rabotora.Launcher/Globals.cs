#nullable enable
using Rabotora.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Environment = Rabotora.Core.IO.Environment;

namespace Rabotora.Launcher
{
	static class Globals
	{
		/// <summary>
		/// 获取当前Rabotora游戏的运行环境信息。
		/// </summary>
		public static Environment GameEnv { get; }

		static Globals()
		{
			object[] attributes = (object[])assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute));
			string company = ((AssemblyCompanyAttribute)attributes[0]).Company;
			string title = Assembly.GetExecutingAssembly().GetAssemblyTitle();
			GameEnv = new Environment(company, title);
		}
		public static string GetAssemblyTitle(this Assembly assembly)
		{
			object[] attributes = (object[])assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute));
			if (attributes.Length > 0)
			{
				var title = (AssemblyTitleAttribute)attributes[0];
				if (!string.IsNullOrEmpty(title.Title))
				{
					return title.Title;
				}
			}
			return Path.GetFileNameWithoutExtension(assembly.CodeBase);
		}


		public static byte[]? GetResourceData(this Assembly assembly,string resPath)
		{
			try
			{
				var stream = assembly.GetManifestResourceStream(resPath);
				if (stream != null)
				{
					byte[] r = new byte[stream.Length];
					stream.Read(r, 0, (int)stream.Length);
					stream.Dispose();
					return r;
				}
				else
				{
					return null;
				}
			}
			catch
			{
				return null;
			}
		}

		[DllImport("user32.dll",CharSet = CharSet.Auto)]
		public extern static bool DestroyIcon(IntPtr handle);

		[DllImport("user32", EntryPoint = "SetWindowPos")]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndlnsertAfter, int X, int Y, int cx, int cy, int flags);
	}
}
