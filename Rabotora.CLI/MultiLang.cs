using Rabotora.MultiLanguage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Rabotora.CLI
{
	/// <summary>
	/// Rabotora CLI多语言支持类
	/// </summary>
	public static class MultiLang
	{
		/// <summary>
		/// 获取指定id和指定区域 <see cref="CultureInfo"/> 对应的本地化资源。
		/// </summary>
		/// <typeparam name="T">资源的类型。</typeparam>
		/// <param name="id">资源id。</param>
		/// <param name="info">区域的 <see cref="CultureInfo"/> 类实例。</param>
		/// <returns>成功返回本地化的资源; 存在该资源但不存在该区域对应的本地化资源返回en-US(英文)本地化的资源; 失败返回 <see langword="null"/> 或指定值类型 <typeparamref name="T"/> 的默认值。</returns>
		public static T? GetRes<T>(string id, CultureInfo info)
		{
			if (string.IsNullOrWhiteSpace("id"))
			{
				return default;
			}
			else if (typeof(T) == typeof(string))
			{
				var multiLang = Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory,"Rabotora.MultiLanguage.dll"));
				var type = multiLang.GetType($"Rabotora.MultiLanguage.{info.Name.Replace("-", "_")}");
				if (type != null)
				{
					string? r = ((ResourceManager)type.GetProperty("ResourceManager")!.GetGetMethod()!.Invoke(null, null)!).GetString(id);
					if (r != null)
					{
						return (T?)Convert.ChangeType(r.Replace("\\t","\t"), typeof(T));
					}
					else
					{
						string? r2 = en_US.ResourceManager.GetString(id);
						if (r2 != null)
						{
							return (T?)Convert.ChangeType(r2.Replace("\\t", "\t"), typeof(T));
						}
						else
						{
							return default;
						}
					}
				}
				else
				{
					string? r2 = en_US.ResourceManager.GetString(id);
					if (r2 != null)
					{
						return (T?)Convert.ChangeType(r2.Replace("\\t", "\t"), typeof(T));
					}
					else
					{
						return default;
					}
				}
			}
			else
			{
				var type = Type.GetType($"Rabotora.MultiLanguage.{info.Name.Replace("-", "_")}");
				if (type != null)
				{
					object? r = ((ResourceManager)type.GetProperty("ResourceManager")!.GetGetMethod()!.Invoke(null, null)!).GetObject(id);
					if (r != null)
					{
						return (T?)r;
					}
					else
					{
						return (T?)en_US.ResourceManager.GetObject(id);
					}
				}
				else
				{
					return (T?)en_US.ResourceManager.GetObject(id);
				}
			}
		}

		/// <summary>
		/// 获取指定id和系统当前区域 <see cref="CultureInfo"/> 对应的本地化资源。
		/// </summary>
		/// <typeparam name="T">资源的类型。</typeparam>
		/// <param name="id">资源id。</param>
		/// <returns>成功返回本地化的资源; 存在该资源但不存在对应的本地化资源返回en-US(英文)本地化的资源; 失败返回 <see langword="null"/> 或指定值类型 <typeparamref name="T"/> 的默认值。</returns>
		public static T? GetRes<T>(string id)
		{
			return GetRes<T>(id, CultureInfo.InstalledUICulture);
		}
	}
}
