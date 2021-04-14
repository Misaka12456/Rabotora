#nullable enable
using Newtonsoft.Json.Linq;
using Rabotora.Core.Models;
using static Rabotora.Core.Globals;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
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
		public ScriptExpressionAnalyzer? BaseAnalyzer { get; }

		/// <summary>
		/// 获取当前语句的类型。可能为 <see langword="null" /> 。
		/// </summary>
		public ScriptExpressionType? Type { get; }

		/// <summary>
		/// 获取当前语句分析完成后的数据。可能为 <see langword="null" /> 。
		/// </summary>
		public JObject? AnalyzedData { get; }

		/// <summary>
		/// 表示一个空的 <see cref="ScriptExpression"/> 实例。
		/// </summary>
		public static readonly ScriptExpression Empty = new ScriptExpression(string.Empty);

		/// <summary>
		/// 使用指定的语句数据和语句类型初始化 <see cref="ScriptExpression"/> 的新实例。
		/// </summary>
		/// <param name="expStr">语句的数据。</param>
		/// <exception cref="ScriptLoadException" />
		public ScriptExpression(string expStr)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(expStr))
				{
					BaseAnalyzer = null;
					AnalyzedData = null;
					Type = null;
				}
				else
				{
					BaseAnalyzer = new ScriptExpressionAnalyzer(expStr);
					if (BaseAnalyzer.AnalyzedData == null)
					{
						AnalyzedData = null;
						Type = null;
					}
					else
					{
						AnalyzedData = BaseAnalyzer.AnalyzedData;
						Type = BaseAnalyzer.Type;
					}
				}
			}
			catch (ScriptLoadException)
			{
				throw;
			}
			catch (Exception)
			{
				throw new ScriptLoadException();
			}
		}

		/// <summary>
		/// 判断两个 <see cref="ScriptExpression"/> 实例是否相等。
		/// </summary>
		/// <param name="x">要判断的第一个实例。</param>
		/// <param name="y">要判断的第二个实例。</param>
		/// <returns>判断结果。</returns>
		public static bool operator ==(ScriptExpression x,ScriptExpression y)
		{
			return x.Equals(y);
		}

		/// <summary>
		/// 判断两个 <see cref="ScriptExpression"/> 实例是否不相等。
		/// </summary>
		/// <param name="x">要判断的第一个实例。</param>
		/// <param name="y">要判断的第二个实例。</param>
		/// <returns>判断结果。</returns>
		public static bool operator !=(ScriptExpression x, ScriptExpression y)
		{
			return !x.Equals(y);
		}

		/// <summary>
		/// 判断指定的对象是否与当前 <see cref="ScriptExpression"/> 实例相等。
		/// </summary>
		/// <param name="obj">要判断的对象。</param>
		/// <returns>判断结果。</returns>
		public override bool Equals(object obj)
		{
			try
			{
				if (obj.GetType() != GetType())
				{
					return false;
				}
				else
				{
					var target = (ScriptExpression)obj;
					if (Type == target.Type)
					{
						if (AnalyzedData == target.AnalyzedData)
						{
							return true;
						}
						else if ((AnalyzedData != null && target.AnalyzedData == null) || 
							(AnalyzedData == null && target.AnalyzedData != null))
						{
							return false;
						}
						else
						{
							return JToken.DeepEquals(AnalyzedData, target.AnalyzedData);
						}
					}
					else
					{
						return false;
					}
				}
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// 计算当前 <see cref="ScriptExpression"/> 实例的哈希值。
		/// </summary>
		/// <returns>计算结果。</returns>
		public override int GetHashCode()
		{
			if (AnalyzedData != null && Type != null)
			{
				int h1 = JToken.EqualityComparer.GetHashCode(AnalyzedData);
				int h2 = Type.Value.GetHashCode();
				return NumericsHelpers.CombineHash(h1, h2);
			}
			else
			{
				return 0;
			}
		}
	}
}
