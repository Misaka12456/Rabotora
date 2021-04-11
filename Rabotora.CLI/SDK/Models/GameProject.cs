using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabotora.CLI.SDK.Models
{
	/// <summary>
	/// 表示一个 Rabotora 视觉小说(Visual Novel)游戏项目。
	/// </summary>
	public struct GameProject
	{
		/// <summary>
		/// 项目名
		/// </summary>
		[JsonProperty("ProjectName")]
		public string ProjectName { get; }

		/// <summary>
		/// 游戏名
		/// </summary>
		[JsonProperty("GameName")]
		public string GameName { get; }

		/// <summary>
		/// 游戏标题(主程序清单中的标题及游戏窗口的标题)
		/// </summary>
		[JsonProperty("Title")]
		public string Title { get; }

		/// <summary>
		/// 开发者名(若为公司开发则为对应的公司/品牌名)
		/// </summary>
		[JsonProperty("Author")]
		public string Author { get; }


		/// <summary>
		/// 游戏的描述(主程序清单中的描述信息)
		/// </summary>
		[JsonProperty("Description")]
		public string Description { get; set; }

		/// <summary>
		/// 游戏的版本号
		/// </summary>
		[JsonProperty("Version")]
		private string VersionStr { get; }

		/// <summary>
		/// 游戏的版本
		/// </summary>
		[JsonIgnore]
		public Version Version { get => Version.Parse(VersionStr); }

		/// <summary>
		/// 游戏的版权信息
		/// </summary>
		[JsonProperty("Copyright")]
		public string Copyright { get; set; }

		/// <summary>
		/// 使用项目名,游戏名,游戏标题,开发者名和游戏的版本初始化 <see cref="GameProject"/> 的新实例。
		/// </summary>
		/// <param name="projName">新 Rabotora 视觉小说游戏项目的名称。</param>
		/// <param name="gameName">游戏的名称(支持Unicode字符)。</param>
		/// <param name="title">游戏标题。</param>
		/// <param name="author">游戏的开发者名。</param>
		/// <param name="version">游戏的版本。</param>
		public GameProject(string projName,string gameName,string title,string author,Version version)
		{
			ProjectName = projName;
			GameName = gameName;
			Title = title;
			Author = author;
			VersionStr = version.ToString();
			Description = string.Empty;
			Copyright = string.Empty;
		}
	}
}
