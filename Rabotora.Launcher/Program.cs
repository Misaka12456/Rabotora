using Rabotora.Launcher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rabotora.Launcher
{
	static class Program
	{
		/// <summary>
		/// 应用程序的主入口点。
		/// </summary>
		[STAThread]
		static void Main()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			try
			{
				RaboLoader.StartGame();
			}
			catch(RabotoraException ex)
			{
				MessageBox.Show(ex.ToString(),Assembly.GetExecutingAssembly().GetAssemblyTitle());
			}
		}

		static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs e)
		{
			string dllName = e.Name.Contains(",") ? e.Name.Substring(0, e.Name.IndexOf(',')) : e.Name.Replace(".dll", string.Empty);
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Rabotora.Launcher.Dependencies." + dllName + ".dll");
			byte[] r = new byte[stream.Length];
			stream.Read(r, 0, (int)stream.Length);
			stream.Dispose();
			return Assembly.Load(r);
		}
	}
}
