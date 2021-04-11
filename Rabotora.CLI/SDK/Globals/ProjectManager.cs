using Newtonsoft.Json;
using Rabotora.CLI.SDK.Models;
using static Rabotora.CLI.MultiLang;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rabotora.CLI.SDK.Globals
{
	/// <summary>
	/// 提供适用于 Rabotora 视觉小说(Visual Novel)游戏项目的方法。
	/// </summary>
	public static class ProjectManager
	{
		/// <summary>
		/// 使用指定的项目名和缺省属性值初始化 <see cref="GameProject"/> 的新实例。
		/// </summary>
		/// <param name="name">新项目的名称。</param>
		/// <returns>表示创建完成的 Rabotora 游戏项目的 <see cref="GameProject"/> 实例。</returns>
		public static GameProject CreateProject(string name)
		{
			var r = new GameProject(name, name, name, Environment.UserName, new Version(1, 0, 0))
			{
				Copyright = $"Copyright © {DateTime.Now.Year} {Environment.UserName}"
			};
			return r;
		}

		/// <summary>
		/// 在指定路径下创建一个指定名称的 Rabotora 视觉小说游戏项目。
		/// </summary>
		/// <param name="name">要创建的游戏项目名。</param>
		/// <param name="path">新游戏项目所在的根目录的路径。</param>
		public static void CreateProjectFromCLI(string name,string path)
		{
			try
			{
				if (Directory.Exists(path))
				{
					var folder = new DirectoryInfo(Path.Combine(path, name));
					folder.Create();
					var proj = CreateProject(name);
					File.WriteAllText(Path.Combine(folder.FullName, name + ".rgp"), JsonConvert.SerializeObject(proj), Encoding.UTF8);
					Console.WriteLine(string.Format(GetRes<string>("CLI.ProjectManager.CreateSuccess")!, name, folder.FullName));
				}
				else
				{
					throw new DirectoryNotFoundException("Cannot find the folder to create project");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(string.Format(GetRes<string>("CLI.ProjectManager.CreateFailed")!, ex.Message));
			}
		}
	}
}
