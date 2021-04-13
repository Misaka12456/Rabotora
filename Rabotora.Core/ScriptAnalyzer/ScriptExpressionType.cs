using System;
using System.Collections.Generic;
using System.Text;

namespace Rabotora.Core.ScriptAnalyzer
{
	/// <summary>
	/// 表示 Rabotora 游戏脚本语句的类型。
	/// </summary>
	public enum ScriptExpressionType : uint
	{
		/// <summary>
		/// 基本声明语句
		/// </summary>
		BaseDeclareAction = 1,

		/// <summary>
		/// 基本赋值语句
		/// </summary>
		BaseSetAction = 2,

		/// <summary>
		/// 基本判断语句
		/// </summary>
		BaseCheckAction = 3,

		/// <summary>
		/// 其它基本语句
		/// </summary>
		BaseAction = 9,

		/// <summary>
		/// 资源动作语句(读取/释放资源等)
		/// </summary>
		ResourceAction = 11,

		/// <summary>
		/// UI动作语句(显示/隐藏图片等)
		/// </summary>
		UIAction = 101,

		/// <summary>
		/// UI边缘动作语句(播放/暂停曲目等)
		/// </summary>
		UISideAction = 102,
		
		/// <summary>
		/// UI事件绑定语句(绑定/解绑指定按钮的事件)
		/// </summary>
		UIBindAction = 111,
		
		/// <summary>
		/// UI场景更换语句
		/// </summary>
		UISceneChange = 112,
		
		/// <summary>
		/// 人物语言表达语句(显示句子和/或播放人物语音)
		/// </summary>
		SentenceSpeak = 201,
		
		/// <summary>
		/// CG动作语句(显示/隐藏CG等;CG过渡变化等)
		/// </summary>
		CGAction = 202,
		
		/// <summary>
		/// 选择枝语句(显示/隐藏选项)
		/// </summary>
		BranchSelect = 301,
		
		/// <summary>
		/// 游戏进度动作语句(保存/读取进度)
		/// </summary>
		ProgressAction = 302,
		
		/// <summary>
		/// 其它类型语句
		/// </summary>
		Others = 1001
	}
}
