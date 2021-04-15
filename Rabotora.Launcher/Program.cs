using Rabotora.Core.Models;
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
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			try
			{
				RaboLoader.StartGame();
			}
			catch(RabotoraException ex)
			{
				MessageBox.Show(ex.ToString(),Assembly.GetExecutingAssembly().GetAssemblyTitle(), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
