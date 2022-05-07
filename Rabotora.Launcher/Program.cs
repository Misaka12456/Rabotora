#pragma warning disable 8618
using System;
using System.Threading.Tasks;
using System.Windows;
using Rabotora.Core;
using Rabotora.Runtime;

namespace Rabotora.Launcher
{
	public static class Program
	{
		internal static RabotoraGameProject MainProject;
		internal static RabotoraScript EntryScript; 
		
		[STAThread]
		public static void Main(string[] args)
		{
			// await InitializeLauncher();
			var app = new Application();
			app.Run(new Win_Start());
		}

		private static async Task InitializeLauncher()
		{
			try
			{
				var projGroup = await RabotoraStartup.InitializeAsync(AppContext.BaseDirectory);
				MainProject = projGroup.Item1;
				EntryScript = projGroup.Item2;
			}
			catch (RabotoraException ex)
			{
				MessageBox.Show(ex.Message, "Failed to initialize Rabotora Launcher", MessageBoxButton.OK, MessageBoxImage.Error);
#if DEBUG
				return;
#else
				Environment.Exit(0);
#endif
			}
		}
	}
}
