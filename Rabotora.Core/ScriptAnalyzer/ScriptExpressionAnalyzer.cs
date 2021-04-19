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

		internal ScriptRegex Regex { get; }
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
				Regex = new ScriptRegex(expr);
				if (true) // TODO
				{

				}
				///基本语句的解析顺序在其它所有Visual Novel(视觉小说)游戏常用已封装语句之后
				#region 基本判断语句(if x == "y": zzzzz;(z是一个语句))
				else if (Regex.Base_If.Success)
				{
					Type = ScriptExpressionType.BaseCheckAction;
					var r = Regex.Base_If;
					string target = r.Result("$variable"); //判断的目标对象
					string @operator = r.Result("$operator"); //运算符
					string value = r.Result("$value"); //判断的值
					string subExp = r.Result("$subExpr"); //若满足条件则执行的子语句
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
				else if (Regex.Base_Set.Success)
				{
					Type = ScriptExpressionType.BaseSetAction;
					var r = Regex.Base_Set;
					string target = r.Result("$variable"); //赋值对象(不存在则创建对象)
					string value = r.Result("$value"); //要赋的值
					AnalyzedData = new JObject()
					{
						{ "target", target },
						{ "value", value }
					};
				}
				#endregion
				#region 基本声明语句(var x;)
				else if (Regex.Base_Declare.Success)
				{
					Type = ScriptExpressionType.BaseCheckAction;
					var r = Regex.Base_Declare;
					string target = r.Result("$variable"); //声明的对象名
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
