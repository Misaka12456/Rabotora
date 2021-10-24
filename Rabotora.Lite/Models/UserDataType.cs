using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rabotora.Lite.Interfaces;

namespace Rabotora.Lite.Models
{
	/// <summary>
	/// 表示一种基于 <see cref="IUserData"/> 的基本用户数据文件的类型。
	/// </summary>
	public enum UserDataType
	{
		/// <summary>
		/// 视觉小说的配置文件。
		/// </summary>
		Config = 0,

		/// <summary>
		/// 视觉小说的进度存档文件。
		/// </summary>
		Save = 1,

		/// <summary>
		/// 视觉小说的“收藏”列表文件。
		/// </summary>
		Favorite = 2
	}
}
