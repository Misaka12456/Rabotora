using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Rabotora.Core.ScriptAnalyzer
{
	/// <summary>
	/// 提供所有 Rabotora 游戏脚本语句的正则匹配 <see cref="Regex"/> 对象的类。
	/// </summary>
	public class ScriptRegex
	{
		private string _expr { get; }
		/// <summary>
		/// 使用指定的游戏脚本语句内容初始化 <see cref="ScriptRegex"/> 类的新实例。
		/// </summary>
		/// <param name="expr">要使用的游戏脚本的语句内容。</param>
		public ScriptRegex(string expr)
		{
			_expr = expr;
		}

		#region "R" - Rabotora核心运行库对象 / Rabotora Core Runtime Library Object
		/// <summary>
		/// [核心运行库(R)]设置游戏窗口大小
		/// <para><code>R.SetWindowSize({$width},{$height});</code><br />
		/// $width=窗口宽度(px)<br />
		/// $height=窗口高度(px)</para>
		/// </summary>
		public Match R_SetWindowSize 
		{ 
			get => Regex.Match(_expr,@"^R.SetWindowSize\((?<width>\d+)\,(?<height>\d+)\);$");
		}


		/// <summary>
		/// [核心运行库(R)]绘制字符串
		/// <para><code>R.DrawString('{$value}',$left,$top,'$font'[,$fontId]);</code><br />
		/// $value=字符串内容<br />
		/// $left=起始位置X(%)<br />
		/// $top=起始位置Y(%)<br />
		/// $font=字体家族资源id<br />
		/// $fontId=字体编号</para>
		/// </summary>
		public Match R_DrawString
		{
			get => Regex.Match(_expr, @"^R\s*.\s*DrawString\s*\(\s*'(?<value>[\s\S]+)'\s*,\s*(?<left>\d+),\s*(?<top>\d+),\s*'(?<font>\S+)'\s*(,\s*(?<fontId>\d+)\s*)?\)\s*;$");
		}

		/// <summary>
		/// [核心运行库(R)]绘制图片
		/// <para><code>R.DrawImage('{$imgId}','{$prop}'[,{$left},{$top}[,{$width},{$height}]]);</code><br />
		/// $imgId=资源id<br />
		/// $prop=操作的属性对象<br />
		/// $left=起始位置X(%)<br />
		/// $top=起始位置Y(%)<br />
		/// $width=图片相较于原始大小的宽度(%)<br />
		/// $height=图片相较于原始大小的高度(%)</para>
		/// </summary>
		public Match R_DrawImage
		{
			get => Regex.Match(_expr, @"^R\s*.\s*DrawImage\s*\(\s*'(?<imgId>[\s\S]+)'\s*(,\s*'(?<prop>[\s\S]+)'\s*,\s*(?<left>\d+)\s*,\s*(?<top>\d+)\s*(,\s*(?<width>\d+)\s*,\s*(?<height>\d+))?\s*)?\)\s*;$");
		}

		/// <summary>
		/// [核心运行库(R)]播放音频
		/// <para><code>R.PlaySound('{$audioId}'[,{$count}]);</code><br />
		/// $audioId=资源id<br />
		/// $count=循环播放次数(null/0=播放一次; -1=永久循环播放,直到停止)</para>
		/// </summary>
		public Match R_PlaySound
		{
			get => Regex.Match(_expr, @"^R\s*.\s*PlaySound\s*\(\s*'(?<audioId>[\s\S]+)'\s*(,\s*(?<count>\d+)\s*)?\)\s*;$");
		}
		#endregion

		#region 基本语句 / Base Statements
		/// <summary>
		/// [基本语句]判断语句
		/// <para><code>if {$variable}{$operator}{$value}: {$subExpr};</code><br />
		/// $variable=要判断的变量<br />
		/// $operator=运算符<br />
		/// $value=要判断的值<br />
		/// $subExpr=判断成功后执行的子语句</para>
		/// </summary>
		public Match Base_If { get => Regex.Match(_expr,
			@"^if\s*(?<variable>[a-zA-Z][0-9a-zA-Z]+)\s*(?<operator>>|==|<|>=|<=|!=|\|\||&&)\s*\s*(?<value>([0-9]+)|('\S+'))\s*:\s?(?<subExpr>[\S\s]+);$",
			RegexOptions.IgnoreCase); }

		/// <summary>
		/// [基本语句]赋值语句
		/// <para><code>(var) {$variable}={$value};</code><br />
		/// $variable=目标变量名<br />
		/// $value=目标值</para>
		/// </summary>
		public Match Base_Set { get => Regex.Match(_expr,
			@"^(var){0,1}\s*(?<variable>[a-zA-Z][0-9a-zA-Z]+)\s*=\s*(?<value>([0-9]+)|('\S+'){1})\s*;$",
			RegexOptions.IgnoreCase); }

		/// <summary>
		/// [基本语句]声明语句
		/// <para><code>var {$variable};</code><br />
		/// $variable=要声明的变量名</para>
		/// </summary>
		public Match Base_Declare { get => Regex.Match(_expr,
			@"^var\s*(?<variable>[a-zA-Z][0-9a-zA-Z]+)\s*;$",
			RegexOptions.IgnoreCase); }
		#endregion
	}
}
