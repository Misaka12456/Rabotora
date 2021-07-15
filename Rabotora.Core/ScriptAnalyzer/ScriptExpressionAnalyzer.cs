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

		internal ScriptRegex? Regex { get; }
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
				if (false) //占位用 
				{

				}
				#region R - Rabotora核心运行库对象
				#region 设置游戏窗口大小(R.SetWindowSize)
				else if (Regex.R_SetWindowSize.Success)
				{
					Type = ScriptExpressionType.UIAction;
					var r = Regex.R_SetWindowSize;
					string subType = "R.SetWindowSize";
					if (uint.TryParse(r.Result("$width"),out uint width) && width > 300)
					{
						if (uint.TryParse(r.Result("$height"),out uint height) && height > 200)
						{
							AnalyzedData = new JObject()
							{
								{ "subType", subType },
								{ "value", new JObject()
									{
										{ "width", width },
										{ "height", height }
									}
								}
							};
						}
						else
						{
							throw new ScriptLoadException($"Failed to load script: Invalid Height Value: {r.Result("$height")}");
						}
					}
					else
					{
						throw new ScriptLoadException($"Failed to load script: Invalid Width Value: {r.Result("$width")}");
					}
				}
				#endregion
				#region 绘制字符串(R.DrawString)
				else if (Regex.R_DrawString.Success)
				{
					Type = ScriptExpressionType.SentenceSpeak;
					var r = Regex.R_DrawString;
					string subType = "R.DrawString";
					if (uint.TryParse(r.Result("$left"), out uint left))
					{
						if (uint.TryParse(r.Result("$top"), out uint top))
						{
							if (uint.TryParse(r.Result("$font"),out uint font))
							{
								uint fontId;
								try
								{
									fontId = uint.Parse(r.Result("$fontId"));
								}
								catch
								{
									fontId = 0;
								}
								AnalyzedData = new JObject()
								{
									{ "subType", subType },
									{ "value", new JObject()
										{
											{ "data", r.Result("$value") },
											{ "left", left },
											{ "top", top },
											{ "font", font },
											{ "fontId", fontId }
										}
									}
								};
							}
							else
							{
								throw new ScriptLoadException($"Failed to load script: Invalid Font Family Number: {r.Result("$font")}");
							}
						}
						else
						{
							throw new ScriptLoadException($"Failed to load script: Invalid String Top Location Value: {r.Result("$top")}");
						}
					}
					else
					{
						throw new ScriptLoadException($"Failed to load script: Invalid String Left Location Value: {r.Result("$left")}");
					}
				}
				#endregion
				#region 绘制静态图片(R.DrawImage)
				else if (Regex.R_DrawImage.Success)
				{
					Type = ScriptExpressionType.CGAction;
					var r = Regex.R_DrawImage;
					string subType = "R.DrawImage";
					string? prop = null;
					uint left, top, width, height;
					if (!string.IsNullOrEmpty(r.Result("$prop")))
					{
						prop = r.Result("$prop");
					}
					if (!uint.TryParse(r.Result("$left"), out left))
					{
						left = 0;
					}
					if (!uint.TryParse(r.Result("$top"), out top))
					{
						top = 0;
					}
					if (!uint.TryParse(r.Result("$width"), out width))
					{
						width = 100;
					}
					if (!uint.TryParse(r.Result("$height"), out height))
					{
						height = 100;
					}
					AnalyzedData = new JObject()
					{
						{ "subType", subType },
						{ "value",new JObject()
							{
								{ "imgId", r.Result("$imgId") },
								{ "prop", prop },
								{ "left", left },
								{ "top", top },
								{ "width", width },
								{ "height", height }
							}
						}
					};
					
				}
				#endregion
				#region 播放音频(R.PlaySound)
				else if (Regex.R_PlaySound.Success)
				{
					Type = ScriptExpressionType.SentenceSpeak;
					var r = Regex.R_PlaySound;
					string subType = "R.PlaySound";
					if (!int.TryParse(r.Result("$count"),out int count))
					{
						count = 0;
					}
					AnalyzedData = new JObject()
					{
						{ "subType", subType },
						{ "value",new JObject()
							{
								{ "audioId", r.Result("$audioId") },
								{ "count", count }
							}
						}
					};
				}
				#endregion
				#region 停止播放音频(R.StopSound)
				else if (Regex.R_PlaySound.Success)
				{
					Type = ScriptExpressionType.SentenceSpeak;
					var r = Regex.R_StopSound;
					string subType = "R.StopSound";
					AnalyzedData = new JObject()
					{
						{ "subType", subType },
						{ "value",new JObject()
							{
								{ "audioPlayObj", r.Result("$audioPlayObj") }
							}
						}
					};
				}
				#endregion
				#endregion
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
