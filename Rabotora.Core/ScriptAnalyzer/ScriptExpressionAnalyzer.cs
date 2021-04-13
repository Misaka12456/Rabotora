﻿#nullable enable
using Newtonsoft.Json.Linq;
using Rabotora.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rabotora.Core.ScriptAnalyzer
{
	/// <summary>
	/// 表示一个 Rabotora 游戏脚本语句的分析器。
	/// </summary>
	public class ScriptExpressionAnalyzer
	{
		/// <summary>
		/// 语句的类型。
		/// </summary>
		public ScriptExpressionType? Type { get; }

		/// <summary>
		/// 分析后的语句数据。
		/// </summary>
		public JObject? AnalyzedData { get; }
		/// <summary>
		/// 使用指定的语句信息初始化 <see cref="ScriptExpressionAnalyzer"/> 类的新实例。
		/// </summary>
		/// <param name="expr">语句的数据。</param>
		/// <exception cref="ScriptLoadException" />
		internal ScriptExpressionAnalyzer(string expr)
		{
			expr = expr.ToLower().Trim();
			if (!string.IsNullOrWhiteSpace(expr))
			{
				if (expr.StandaloneMatch("if") && expr.IndexOf("if", StringComparison.InvariantCultureIgnoreCase) == 0)
				{ //存在"if"且"if"在语句头
					Type = ScriptExpressionType.BaseCheckAction;
				}
				else if (expr.Contains("=") && expr.IndexOf('=') > 0) //存在"="且"="不在语句头
				{
					if (expr.LastIndexOf('=') == expr.IndexOf('=')) //如果语句中只有一个"="
					{
						Type = ScriptExpressionType.BaseSetAction;
						string target = expr.Split(new[] { ' ' }, count: 2, StringSplitOptions.RemoveEmptyEntries)[0];
						string value = expr.Split(new[] { ' ' }, count: 2, StringSplitOptions.RemoveEmptyEntries)[1];
						if (!string.IsNullOrWhiteSpace(target) && !string.IsNullOrWhiteSpace(value))
						{
							AnalyzedData = new JObject()
							{
								{ "target", target },
								{ "value",value }
							};
						}
						else
						{
							throw new ScriptLoadException();
						}
					}
					else
					{
						throw new ScriptLoadException();
					}
				}
			}
			else
			{
				AnalyzedData = null;
				Type = null;
			}
		}
	}
}