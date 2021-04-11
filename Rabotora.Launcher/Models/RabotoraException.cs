using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabotora.Launcher.Models
{
	/// <summary>
	/// 表示一个 Rabotora 视觉小说(Visual Novel)游戏启动器的异常。
	/// </summary>
	class RabotoraException : Exception
	{
		/// <summary>
		/// 初始化 <see cref="RabotoraException"/> 类的新实例。
		/// </summary>
		public RabotoraException() : base("An error occurred when starting the game.")
		{
		}

		/// <summary>
		/// 使用指定的错误消息初始化 <see cref="RabotoraException"/> 类的新实例。
		/// </summary>
		public RabotoraException(string message) : base(message)
		{

		}

		public override string ToString()
		{
			return base.Message;
		}
	}
}
