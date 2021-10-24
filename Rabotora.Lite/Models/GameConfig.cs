using Ionic.Zip;
using Newtonsoft.Json.Linq;
using Rabotora.Lite.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabotora.Lite.Models
{
	/// <summary>
	/// 表示一个视觉小说的配置文件。
	/// </summary>
	public struct GameConfig : IUserData
	{
		public UserDataType Type { get => UserDataType.Config; }

		public DateTime? CreatedTime { get; }

		/// <summary>
		/// 包含配置数据的键值对。
		/// </summary>
		public Dictionary<string, dynamic> ConfDict { get; set; }

		public string FilePath { get; }

		/// <summary>
		/// 使用指定的配置文件路径初始化 <see cref="GameConfig"/> 的新实例。
		/// </summary>
		/// <param name="confPath">配置文件的完整绝对路径。</param>
		public GameConfig(string confPath)
		{
			try
			{
				if (!File.Exists(confPath))
				{
					throw new FileNotFoundException("Cannot find Rabotora Game Config File");
				}
				else
				{
					var fs = new FileStream(confPath, FileMode.Open, FileAccess.Read);
					var br = new BinaryReader(fs);
					if (Encoding.UTF8.GetString(br.ReadBytes(14)) == "RABOTORA.CONF!")
					{
						fs.Seek(14, SeekOrigin.Begin);
						byte[] data = new byte[fs.Length - 14];
						fs.Read(data, 0, (int)fs.Length - 14);
						br.Close();
						var conf = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(data))));
						ConfDict = conf.Value<JArray>("confDict").ToDictionary(pair => pair.Value<string>("id")!, pair => pair.Value<dynamic>("value")!);
						CreatedTime = File.GetCreationTime(confPath);
						FilePath = confPath;
					}
					else
					{
						throw new FormatException();
					}
				}
			}
			catch (FileNotFoundException)
			{
				throw;
			}
			catch
			{
				throw new FormatException("Invalid format of Rabotora Game Config File.");
			}
		}

		private GameConfig(DateTime? createdTime, string filePath)
		{
			CreatedTime = createdTime;
			FilePath = filePath;
			ConfDict = new Dictionary<string, dynamic>();
		}

		/// <summary>
		/// 创建一个新的 <see cref="GameConfig"/> 实例。
		/// </summary>
		/// <returns>创建后的 <see cref="GameConfig"/> 实例。</returns>
		public static GameConfig Create()
		{
			var r = new GameConfig(null, string.Empty)
			{
				ConfDict = new Dictionary<string, dynamic>()
			};
			return r;
		}

		/// <summary>
		/// 将当前 <see cref="GameConfig"/> 实例保存为一个配置文件。
		/// <para>若首次保存请不要调用本方法, 请调用 <see cref="Save(string)"/> 重载方法。</para>
		/// </summary>
		/// <returns>成功保存返回 <see langword="true"/> , 否则返回 <see langword="false"/> 。</returns>
		public bool Save()
		{
			try
			{
				if (File.Exists(FilePath))
				{
					using var fs = new FileStream(FilePath, FileMode.Truncate, FileAccess.ReadWrite);
					var bw = new BinaryWriter(fs);
					bw.Write("RABOTORA.CONF!");
					var conf = new JObject()
					{
						{ "confDict", JArray.FromObject(from confPair in ConfDict select new JObject()
							{
								{ "id", confPair.Key },
								{ "value", confPair.Value }
							})
						}
					};
					byte[] confData = Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes(conf.ToString())));
					bw.Write(confData);
					bw.Close();
					return true;
				}
				else
				{
					throw new FileNotFoundException();
				}
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// 将当前 <see cref="GameConfig"/> 实例保存为一个配置文件。
		/// </summary>
		/// <param name="firstSaveConfPath">要将配置文件保存到的路径。</param>
		/// <returns>配置文件保存后对应的新 <see cref="GameConfig"/> 实例。</returns>
		/// <exception cref="RabotoraException" />
		public GameConfig Save(string firstSaveConfPath)
		{
			try
			{
				if (!File.Exists(firstSaveConfPath))
				{
					using var fs = new FileStream(firstSaveConfPath, FileMode.CreateNew, FileAccess.ReadWrite);
					var bw = new BinaryWriter(fs);
					bw.Write("RABOTORA.CONF!");
					var conf = new JObject()
					{
						{ "confDict", JArray.FromObject(from confPair in ConfDict select new JObject()
							{
								{ "id", confPair.Key },
								{ "value", confPair.Value }
							})
						}
					};
					byte[] confData = Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes(conf.ToString())));
					bw.Write(confData);
					bw.Close();
					var newCreateTime = File.GetCreationTime(firstSaveConfPath);
					return new GameConfig(newCreateTime, firstSaveConfPath)
					{
						ConfDict = ConfDict
					};
				}
				else
				{
					throw new IOException("The file or directory of Rabotora Game Config File first save path already exists");
				}
			}
			catch (RabotoraException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new RabotoraException(ex);
			}
		}
	}
}
