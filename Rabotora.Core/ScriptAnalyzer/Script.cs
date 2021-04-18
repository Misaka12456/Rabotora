using Rabotora.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rabotora.Core.ScriptAnalyzer
{
	/// <summary>
	/// 表示一个 Rabotora 视觉小说(Visual Novel)游戏脚本。
	/// </summary>
	public class Script
	{
		private string _data;

		public List<ScriptExpression> Expressions { get; internal set; }
		
		/// <summary>
		/// 使用包含脚本数据的 <see cref="Stream"/> 类实例 初始化 <see cref="Script"/> 类的新实例。
		/// </summary>
		/// <param name="stream">包含脚本数据的数据流。</param>
		/// <param name="analyze">是否在初始化时解析内含的所有语句。默认为 <see langword="true" /> 。</param>
		/// <exception cref="ScriptLoadException" />
		public Script(Stream stream, bool analyze = true)
		{
			try
			{
				if (stream.CanRead && stream.CanSeek)
				{
					stream.Seek(0, SeekOrigin.Begin);
					byte[] data = new byte[stream.Length];
					stream.Read(data, 0, data.Length);
					_data = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(data)));
					if (analyze)
					{
						string[] lines = _data.Split(';');
						var expList = new List<ScriptExpression>();
						foreach (string line in lines)
						{
							expList.Add(new ScriptExpression(line));
						}
					}
				}
				else
				{
					throw new ScriptLoadException();
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
		/// 使用二进制脚本数据初始化 <see cref="Script"/> 类的新实例。
		/// </summary>
		/// <param name="data">二进制的脚本数据。</param>
		/// <param name="analyze">是否在初始化时解析内含的所有语句。默认为 <see langword="true" /> 。</param>
		public Script(byte[] data,bool analyze = true)
		{
			try
			{
				_data = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(data)));
				if (analyze)
				{
					string[] lines = _data.Split(';');
					var expList = new List<ScriptExpression>();
					foreach (string line in lines)
					{
						expList.Add(new ScriptExpression(line));
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

	}
}
