using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabotora.Lite.Models
{
	/// <summary>
	/// 表示一个视觉小说(VN)游戏。
	/// </summary>
	public class Game
	{
		/// <summary>
		/// 游戏的名称。
		/// <para>优先级:日本語->中文->English</para>
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// 游戏的开发公司/会社。
		/// <para>优先级:日本語->中文->English</para>
		/// </summary>
		public string Company { get; }

		/// <summary>
		/// 游戏的Rabotora托管数据(存档&配置文件)的管理器。
		/// </summary>
		public GameDBMgr MainDBMgr { get; set; }

		/// <summary>
		/// 使用指定的视觉小说(VN)游戏名和其开发公司/会社初始化 <see cref="Game"/> 类的新实例。
		/// </summary>
		/// <param name="name">视觉小说(VN)的游戏名(优先级:日本語->中文->English)。</param>
		/// <param name="company">视觉小说(VN)游戏的开发公司/会社(优先级:日本語->中文->English)。</param>
		/// <exception cref="RabotoraException" />
		public Game(string name, string company)
		{
			try
			{
				Name = name;
				Company = company;
				MainDBMgr = GameDBMgr.CreateOrLoadFromDocuments(company, name, out _);
			}
			catch(RabotoraException)
			{
				throw;
			}
			catch(Exception ex)
			{
				throw new RabotoraException(ex);
			}
		}
	}
}
