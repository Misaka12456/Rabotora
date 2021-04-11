using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;

namespace Rabotora.Compiler.Models
{
	public struct CompilingGameProject
	{
		#region Rabotora 视觉小说(Visual Novel)游戏项目信息
		/// <summary>
		/// 项目名
		/// </summary>
		public string ProjectName { get; }

		/// <summary>
		/// 游戏名
		/// </summary>
		public string GameName { get; }

		/// <summary>
		/// 游戏标题(主程序清单中的标题及游戏窗口的标题)
		/// </summary>
		public string Title { get; }

		/// <summary>
		/// 开发者名(若为公司开发则为对应的公司/品牌名)
		/// </summary>
		public string Author { get; }

		/// <summary>
		/// 游戏的描述(主程序清单中的描述信息)
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// 游戏的版本号
		/// </summary>
		private string VersionStr { get; }

		/// <summary>
		/// 游戏的版本
		/// </summary>
		public Version Version { get => Version.Parse(VersionStr); }

		/// <summary>
		/// 游戏的版权信息
		/// </summary>
		public string Copyright { get; }

		/// <summary>
		/// 项目的文件忽略列表
		/// </summary>
		public List<string> IgnoreList { get; }
		#endregion

		internal CSharpCodeProvider CodeProvider { get; }

		/// <summary>
		/// 使用 Rabotora 游戏项目的信息初始化 <see cref="CompilingGameProject"/> 的新实例。
		/// </summary>
		/// <param name="projName">新 Rabotora 视觉小说游戏项目的名称。</param>
		/// <param name="gameName">游戏的名称(支持Unicode字符)。</param>
		/// <param name="title">游戏标题。</param>
		/// <param name="author">游戏的开发者名。</param>
		/// <param name="version">游戏的版本。</param>
		/// <param name="copyright">游戏的版权信息。</param>
		/// <param name="description">游戏的描述。</param>
		/// <param name="ignoreList">游戏项目的文件忽略列表。</param>
		public CompilingGameProject(string projName, string gameName, string title, string author, Version version, string description,
			string copyright,List<string> ignoreList)
		{
			ProjectName = projName;
			GameName = gameName;
			Title = title;
			Author = author;
			VersionStr = version.ToString();
			Description = description;
			Copyright = copyright;
			IgnoreList = ignoreList;
			CodeProvider = new CSharpCodeProvider();
		}
	}
}