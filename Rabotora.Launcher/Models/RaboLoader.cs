#nullable enable
using Newtonsoft.Json.Linq;
using Rabotora.Launcher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rabotora.Launcher
{
	class RaboLoader
	{
		/// <summary>
		/// 启动游戏。
		/// </summary>
		/// <exception cref="RabotoraException" />
		internal static void StartGame()
		{
			try
			{
				byte[]? runConfData = Assembly.GetExecutingAssembly().GetResourceData("Rabotora.Launcher.Run.rbtconfig");
				if (runConfData != null)
				{
					var conf = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(runConfData))));
				}
				else
				{
					throw new RabotoraException("Cannot find file Run.rbtconfig.\n" +
						"If you executed Launcher before building the game, please build the game first.\n" +
						"(Single Launcher (without Run.rbtconfig) Starting is not supported)");
				}
			}
			catch (RabotoraException)
			{
				throw;
			}
			catch(Exception ex)
			{
				throw new RabotoraException($"An unexpected error occurred: {ex.Message}\nThe game cannot start.");
			}
		}
	}
}
