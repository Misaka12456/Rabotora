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
	/// 表示一个视觉小说的存档数据。
	/// </summary>
	public struct GameSave : IUserData
	{
		public UserDataType Type { get => UserDataType.Save; }

		/// <summary>
		/// 存档的名称。
		/// </summary>
		public string SaveName { get; private set; }

		/// <summary>
		/// 存档数据中的二进制数据键值对。
		/// </summary>
		public Dictionary<string,byte[]> BinaryDict { get; set; }

		/// <summary>
		/// 存档数据中的文本数据键值对。
		/// </summary>
		public Dictionary<string,string> TextDict { get; set; }

		/// <summary>
		/// 存档文件的首次保存日期。
		/// </summary>
		public DateTime? CreatedTime { get; }

		/// <summary>
		/// 存档文件的完整绝对路径。
		/// </summary>
		public string FilePath { get; }
		
		private GameSave(string savePath)
		{
			try
			{
				if (!File.Exists(savePath))
				{
					throw new FileNotFoundException("Cannot find Rabotora Save File");
				}
				var save = ZipFile.Read(savePath); // 读取存档文件
				var metaDataEntry = save["metadata.json"]; // 读取存档元数据文件
				byte[] metaData = new byte[metaDataEntry.UncompressedSize];
				var metaDataStream = metaDataEntry.OpenReader();
				metaDataStream.Read(metaData, 0, (int)metaDataEntry.UncompressedSize);
				metaDataStream.Dispose();
				var meta = JObject.Parse(Encoding.UTF8.GetString(metaData)); // 存档元数据
				SaveName = meta.Value<string>("saveName")!;
				FilePath = savePath;
				CreatedTime = File.GetCreationTime(savePath);
				TextDict = meta.Value<JArray>("textDict")!.ToDictionary(pair => pair.Value<string>("id"), pair => pair.Value<string>("value"));
				BinaryDict = meta.Value<JArray>("binaryDict").ToDictionary(pair => pair.Value<string>("id")!,
					new Func<JToken, byte[]>((JToken pair) => 
					{
						var entry = save[$"bin/{pair.Value<string>("fileId")!}.dat"]; // 读取Key对应的二进制文件
						byte[] binaryData = new byte[entry.UncompressedSize];
						using var fs = entry.OpenReader();
						fs.Read(binaryData, 0, (int)entry.UncompressedSize);
						fs.Close();
						return binaryData; // 返回读取的结果
					}));
				save.Dispose();
			}
			catch (ZipException ex)
			{
				throw new FormatException("Invalid format of Rabotora Game Save File.", ex);
			}
			catch (FileNotFoundException)
			{
				throw;
			}
			catch
			{
				throw new FormatException("Invalid format of Rabotora Game Save File.");
			}
		}

		private GameSave(DateTime? createdTime, string savePath)
		{
			SaveName = string.Empty;
			BinaryDict = new Dictionary<string, byte[]>();
			TextDict = new Dictionary<string, string>();
			CreatedTime = createdTime;
			FilePath = savePath;
		}

		/// <summary>
		/// 使用指定的存档名称初始化 <see cref="GameSave"/> 的新实例。
		/// </summary>
		/// <param name="name">要使用的存档名称。</param>
		/// <exception cref="RabotoraException" />
		public static GameSave Create(string name)
		{
			try
			{
				var r = new GameSave(null, string.Empty)
				{
					SaveName = name
				};
				return r;
			}
			catch(RabotoraException)
			{
				throw;
			}
			catch(Exception ex)
			{
				throw new RabotoraException(ex);
			}
		}

		/// <summary>
		/// 使用指定的存档文件的完整路径初始化 <see cref="GameSave"/> 的新实例。
		/// </summary>
		/// <param name="savePath">存档文件的完整绝对路径。</param>
		/// <exception cref="RabotoraException" />
		public static GameSave Load(string savePath)
		{
			try
			{
				return new GameSave(savePath);
			}
			catch (RabotoraException)
			{
				throw;
			}
			catch(Exception ex)
			{
				throw new RabotoraException(ex);
			}
		}

		/// <summary>
		/// 保存当前 <see cref="GameSave"/> 实例所对应的存档文件到指定文件夹。
		/// </summary>
		/// <param name="savePath">要保存到的文件夹。</param>
		/// <returns>若存档文件不存在且保存成功返回 <see langword="true" /> , 否则返回 <see langword="false" /> 。</returns>
		public bool Save(string savePath)
		{
			try
			{
				if (CreatedTime == null && FilePath == string.Empty && !File.Exists(savePath))
				{
					var save = new ZipFile();
					var textDict = new List<JObject>();
					foreach (var textPair in TextDict)
					{
						textDict.Add(new JObject()
						{
							{ "id", textPair.Key },
							{ "value", textPair.Value }
						});
					}
					var binaryDict = new List<JObject>();
					uint binaryFileIndex = 0;
					foreach (var binaryPair in BinaryDict)
					{
						binaryDict.Add(new JObject()
						{
							{ "id", binaryPair.Key },
							{ "fileId", binaryFileIndex }
						});
						binaryFileIndex++;
					}
					var meta = new JObject()
					{
						{ "saveName", SaveName },
						{ "textDict", JArray.FromObject(textDict) },
						{ "binaryDict", JArray.FromObject(binaryDict) }
					};
					save.AddEntry("/metadata.json", Encoding.UTF8.GetBytes(meta.ToString()));
					foreach (var binaryPair2 in binaryDict)
					{
						string binaryId = binaryPair2.Value<string>("id")!; // 二进制数据id
						uint binaryFileId = binaryPair2.Value<uint>("fileId"); // 存档文件中二进制文件的内部id
						BinaryDict.TryGetValue(binaryId, out byte[] binaryData); // 获取二进制数据
						save.AddEntry($"/bin/{binaryFileId}.dat", binaryData);
					}
					save.Save(savePath);
					return true;
				}
				else
				{
					return false;
				}
			}
			catch
			{
				return false;
			}
		}
	}
}
