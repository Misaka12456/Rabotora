using Rabotora.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rabotora.Core.ScriptAnalyzer
{
	/// <summary>
	/// 表示一个Rabotora游戏脚本的语句。
	/// </summary>
	public class ScriptExpression
	{
		/// <summary>
		/// 获取当前语句的分析器。
		/// </summary>
		public ScriptExpressionAnalyzer BaseAnalyzer { get; }

		/// <summary>
		/// 使用指定的语句数据和语句类型初始化 <see cref="ScriptExpression"/> 的新实例。
		/// </summary>
		/// <param name="expStr">语句的数据。</param>
		/// <exception cref="ScriptLoadException" />
		public ScriptExpression(string expStr)
		{
			try
			{
				BaseAnalyzer = new ScriptExpressionAnalyzer(expStr);
			}
			catch(ScriptLoadException)
			{
				throw;
			}
			catch (Exception)
			{
				throw new ScriptLoadException();
			}
		}
	}
}
