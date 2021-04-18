using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using Env = System.Environment;

namespace Rabotora.Core.IO
{
	/// <summary>
	/// 提供适用于单个 Rabotora 视觉小说(Visual Novel)游戏的环境信息。
	/// </summary>
	public class Environment
	{
		/// <summary>
		/// 获取当前Rabotora游戏环境对应的游戏厂商名。
		/// </summary>
		public string Company { get; }

		/// <summary>
		/// 获取当前Rabotora游戏环境对应的游戏名。
		/// </summary>
		public string GameName { get; }

		/// <summary>
		/// 获取当前Rabotora游戏环境的存档路径。
		/// <para>若调用本属性时路径未初始化则会自动初始化。</para>
		/// </summary>
		public DirectoryInfo SavePath
		{
			get
			{
				if (!IsSavePathInitialized) InitializeSavePath();
				return new DirectoryInfo(Path.Combine(Env.GetFolderPath(Env.SpecialFolder.MyDocuments), Company, GameName, "save"));
			}
		}

		/// <summary>
		/// 使用游戏的厂商名和游戏名初始化 <see cref="Environment"/> 类的新实例。
		/// </summary>
		/// <param name="company">游戏的厂商名。</param>
		/// <param name="game">游戏的名称。</param>
		public Environment(string company,string game)
		{
			Company = company;
			GameName = game;
		}

		/// <summary>
		/// 获取当前Rabotora游戏环境的存档路径的初始化状态。
		/// <para>默认存档路径: %userprofile%\Documents\{游戏厂商名}\{游戏名}\save </para>
		/// </summary>
		/// <returns>若存档路径已初始化则返回 <see langword="true" /> , 否则返回 <see langword="false" /> 。</returns>
		public bool IsSavePathInitialized { get => (Directory.Exists(Path.Combine(Env.GetFolderPath(Env.SpecialFolder.MyDocuments), Company, GameName, "save")) &&
				File.Exists(Path.Combine(Env.GetFolderPath(Env.SpecialFolder.MyDocuments), Company, GameName, "save", "config.dat"))); }

		/// <summary>
	/// 初始化当前Rabotora游戏环境的存档文件夹。
	/// </summary>
	/// <returns>若在调用方法前已初始化过或初始化失败返回 <see langword="false" /> , 否则返回 <see langword="true" /> 。</returns>
		public bool InitializeSavePath()
		{
			if (!Directory.Exists(Path.Combine(Env.GetFolderPath(Env.SpecialFolder.MyDocuments), Company, GameName, "save")) ||
				!File.Exists(Path.Combine(Env.GetFolderPath(Env.SpecialFolder.MyDocuments), Company, GameName, "save", "config.dat")))
			{
				Directory.CreateDirectory(Path.Combine(Env.GetFolderPath(Env.SpecialFolder.MyDocuments), Company, GameName, "save"));
				SQLiteConnection.CreateFile(Path.Combine(Env.GetFolderPath(Env.SpecialFolder.MyDocuments), Company, GameName, "save", "config.dat"));
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
