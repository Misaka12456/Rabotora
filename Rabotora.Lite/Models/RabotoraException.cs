using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabotora.Lite.Models
{
	/// <summary>
	/// 表示Rabotora游戏引擎执行过程中非引擎自身原因引发的异常。
	/// </summary>
	public class RabotoraException : AggregateException
	{
		/// <summary>
		/// 初始化 <see cref="RabotoraException"/> 类的新实例。
		/// </summary>
		public RabotoraException()
		{
		}

		/// <summary>
		/// 使用引发当前异常的异常实例列表初始化 <see cref="RabotoraException"/> 类的新实例。
		/// </summary>
		/// <param name="innerExceptions">引发当前 <see cref="RabotoraException"/> 异常的异常实例列表。</param>
		public RabotoraException(params Exception[] innerExceptions) : base(innerExceptions)
		{
		}

		/// <summary>
		/// 使用指定的错误提示信息初始化 <see cref="RabotoraException"/> 类的新实例。
		/// </summary>
		/// <param name="message">异常的错误提示信息。</param>
		public RabotoraException(string message) : base(message)
		{
		}

		/// <summary>
		/// 使用指定的错误提示信息和引发当前异常的异常实例列表初始化 <see cref="RabotoraException"/> 类的新实例。
		/// </summary>
		/// <param name="message">异常的错误提示信息。</param>
		/// <param name="innerExceptions">引发当前 <see cref="RabotoraException"/> 异常的异常实例列表。</param>
		public RabotoraException(string message, IEnumerable<Exception> innerExceptions) : base(message, innerExceptions)
		{
		}
	}
}
