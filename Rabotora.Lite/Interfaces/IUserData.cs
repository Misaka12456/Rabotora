using Rabotora.Lite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabotora.Lite.Interfaces
{
	/// <summary>
	/// 表示视觉小说中一个基本用户数据文件结构的接口。
	/// </summary>
	public interface IUserData
	{
		/// <summary>
		/// 数据文件的类型。
		/// </summary>
		public UserDataType Type { get; }

		/// <summary>
		/// 数据文件的初始创建日期。
		/// </summary>
		public DateTime? CreatedTime { get; }
		
		/// <summary>
		/// 数据文件的完整绝对路径。
		/// </summary>
		public string FilePath { get; }
	}
}
