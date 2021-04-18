#nullable enable
using Newtonsoft.Json.Linq;
using Rabotora.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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
				var r_if = Regex.Match(expr, @"^if\s*([a-zA-Z][0-9a-zA-Z]*)\s*(>|==|<|>=|<=|!=|\|\||&&)\s*\s*(([0-9]*)+|(""\S"")+)\s*:\s*(\S+);$", RegexOptions.IgnoreCase);
				var r_set = Regex.Match(expr, @"^(var){0,1}\s*[a-zA-Z][0-9a-zA-Z]*\s*=\s*(([0-9]?)|(""\S*""){1})\s*;$", RegexOptions.IgnoreCase);
				var r_declare = Regex.Match(expr, @"^var\s*([a-zA-Z][0-9a-zA-Z]*)\s*;$", RegexOptions.IgnoreCase);
				if (true) // TODO
				{

				}
				///基本语句的解析顺序在其它所有Visual Novel(视觉小说)游戏常用已封装语句之后
				#region 基本判断语句(if x == "y": zzzzz;(z是一个语句))
				else if (r_if.Success)
				{
					Type = ScriptExpressionType.BaseCheckAction;
					string target = r_if.Result("$1"); //判断的目标对象
					string @operator = r_if.Result("$2"); //运算符
					string value = r_if.Result("$3").Trim('"'); //判断的值(对于字符串值去除首尾引号)
					string subExp = r_if.Result("$4"); //若满足条件则执行的子语句
					AnalyzedData = new JObject()
					{
						{ "target", target },
						{ "operator", @operator },
						{ "value", value },
						{ "subExp", subExp }
					};
				}
				#endregion
				#region 基本赋值语句(x = "y";)
				else if (r_set.Success)
				{
					Type = ScriptExpressionType.BaseSetAction;
					string target = r_set.Result("$1"); //赋值对象(不存在则创建对象)
					string value = r_set.Result("$2").Trim('"'); //要赋的值(对于字符串值去除首尾引号)
					AnalyzedData = new JObject()
					{
						{ "target", target },
						{ "value", value }
					};
				}
				#endregion
				#region 基本声明语句(var x;)
				else if (r_declare.Success)
				{
					Type = ScriptExpressionType.BaseCheckAction;
					string target = r_declare.Result("$1"); //声明的对象名
					AnalyzedData = new JObject()
					{
						{ "target", target }
					};
				}
				#endregion
				else
				{
					throw new ScriptLoadException($"Failed to load script: Invalid expression: {expr}");
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
