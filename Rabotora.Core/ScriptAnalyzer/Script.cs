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
		private byte[] _data;
		/// <summary>
		/// 使用包含脚本数据的 <see cref="Stream"/> 类实例 初始化 <see cref="Script"/> 类的新实例。
		/// </summary>
		/// <param name="stream">包含脚本数据的数据流。</param>
		/// <exception cref="ScriptLoadException" />
		public Script(Stream stream)
		{
			try
			{
				if (stream.CanRead && stream.CanSeek)
				{
					stream.Seek(0, SeekOrigin.Begin);
					_data = new byte[stream.Length];
					stream.Read(_data, 0, _data.Length);
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
		public Script(byte[] data)
		{
			_data = data;
		}


	}
}
