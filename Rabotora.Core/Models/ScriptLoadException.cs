using System;
using System.Collections.Generic;
using System.Text;

namespace Rabotora.Core.Models
{
	/// <summary>
	/// 表示加载Rabotora游戏脚本时发生的异常。
	/// </summary>
	public class ScriptLoadException : RabotoraException
	{
		/// <summary>
		/// 初始化 <see cref="ScriptLoadException"/> 类的新实例。
		/// </summary>
		public ScriptLoadException() : base("Failed to load game script.")
		{

		}

		public ScriptLoadException(string message) : base(message)
		{

		}
	}
}
