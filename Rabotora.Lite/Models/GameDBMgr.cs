using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Environment;

namespace Rabotora.Lite.Models
{
	/// <summary>
	/// 表示一个 <see cref="Game"/> 的Rabotora视觉小说(VN)游戏数据库的管理工具。
	/// </summary>
	public class GameDBMgr : IDisposable
	{
		private bool disposedValue;
		/// <summary>
		/// 玩家的数据存储目录。
		/// </summary>
		public string PlayerDataPath { get; }

		/// <summary>
		/// 玩家的进度存档列表。
		/// </summary>
		public List<GameSave> ProgSaves { get; set; }

		/// <summary>
		/// 游戏的主配置信息。
		/// </summary>
		public GameConfig MainConfig { get; set; }
		
		/// <summary>
		/// 游戏的子配置信息列表。
		/// <para>Key=配置信息的id Value=配置信息的 <see cref="GameConfig"/> 实例</para>
		/// </summary>
		public Dictionary<string, GameConfig> AuxiliConfigs { get; set; }

		/// <summary>
		/// 使用指定的玩家数据存储目录初始化 <see cref="GameDBMgr"/> 的新实例。
		/// </summary>
		/// <exception cref="RabotoraException" />
		public GameDBMgr(string pDataPath)
		{
			try
			{
				if (Directory.Exists(pDataPath))
				{
					if (File.Exists(Path.Combine(pDataPath, "system.rdat")))
					{
						var rDataFileList = Directory.GetFiles(pDataPath, "*.rdat").ToList();  // rdat: [R]abotora [Dat]a File
						rDataFileList = (from rDat in rDataFileList select rDat.ToLower()).ToList(); // 将所有文件的名称转为小写
						rDataFileList.RemoveAt(rDataFileList.FindIndex(rDat => (rDat.ToLower().EndsWith("/system.rdat") || rDat.ToLower().EndsWith(@"\system.rdat")))); // 移除主配置文件system.rdat的路径元素
						var progSaveFiles = from rDat in rDataFileList where rDat.EndsWith(".s.rdat") select rDat; // 获取所有.s.rdat结尾的游戏存档文件列表
						rDataFileList.RemoveAll(rDat => progSaveFiles.Contains(rDat)); // 从主文件列表中移除所有游戏存档文件列表
						var auxConfs = (from rDat in rDataFileList where rDat.EndsWith(".rdat") select new KeyValuePair<string, GameConfig>(rDat, new GameConfig(rDat)))
							.ToDictionary(conf => conf.Key, conf => conf.Value); // 获取所有.rdat结尾的子配置文件列表
						var progSaves = (from saveFile in progSaveFiles select GameSave.Load(saveFile)).ToList();
						var mainConf = new GameConfig(Path.Combine(pDataPath, "system.rdat"));
						PlayerDataPath = pDataPath;
						ProgSaves = progSaves;
						MainConfig = mainConf;
						AuxiliConfigs = auxConfs;
					}
					else
					{
						throw new FileNotFoundException($"Cannot find Rabotora Game Main Config File \"system.rdat\" in folder \"{pDataPath}\"");
					}
				}
			}
			catch(Exception ex)
			{
				throw new RabotoraException(ex);
			}
		}

		private GameDBMgr(string pDataPath, GameConfig newMainConfig)
		{
			PlayerDataPath = pDataPath;
			ProgSaves = new List<GameSave>();
			MainConfig = newMainConfig;
			AuxiliConfigs = new Dictionary<string, GameConfig>();
		}

		/// <summary>
		/// 使用指定的游戏开发公司/会社名及游戏的原名从当前Windows用户的"文档"文件夹创建新的或加载已有的数据对应的 <see cref="GameDBMgr"/> 类的实例。
		/// </summary>
		/// <param name="companyName">视觉小说(VN)游戏的开发公司/会社名(优先级:日本語->中文->English)。</param>
		/// <param name="gameName">视觉小说(VN)游戏的名称(优先级:日本語->中文->English)。</param>
		/// <param name="isCreateNew">在当前方法返回时, 若创建了一个新的 <see cref="GameDBMgr"/> 实例则值为 <see langword="true" /> , 否则值为 <see langword="false" /> 。</param>
		/// <returns>创建或加载完成的 <see cref="GameDBMgr"/> 类的新实例。</returns>
		/// <exception cref="RabotoraException" />
		public static GameDBMgr CreateOrLoadFromDocuments(string companyName, string gameName, out bool isCreateNew)
		{
			try
			{
				if (!Directory.Exists(Path.Combine(GetFolderPath(SpecialFolder.MyDocuments), companyName, gameName, "save")))
				{
					Directory.CreateDirectory(Path.Combine(GetFolderPath(SpecialFolder.MyDocuments), companyName, gameName, "save"));
					var mainConfig = GameConfig.Create().Save(Path.Combine(GetFolderPath(SpecialFolder.MyDocuments), companyName, gameName, "save", "system.rdat"));
					isCreateNew = true;
					return new GameDBMgr(Path.Combine(GetFolderPath(SpecialFolder.MyDocuments), companyName, gameName, "save"), mainConfig);
				}
				else
				{
					isCreateNew = false;
					return new GameDBMgr(Path.Combine(GetFolderPath(SpecialFolder.MyDocuments), companyName, gameName, "save"));
				}
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
		/// 添加新的进度存档。
		/// </summary>
		/// <param name="save">要添加的存档的 <see cref="GameSave"/> 实例。</param>
		/// <returns>成功添加返回 <see langword="true" /> , 否则返回 <see langword="false"/> 。</returns>
		/// <exception cref="RabotoraException" />
		public bool AddNewSave(GameSave save)
		{
			try
			{
				if (ProgSaves.FindIndex(s => s.SaveName == save.SaveName) == -1) //不存在指定的存档信息
				{
					save.Save(Path.Combine(PlayerDataPath, $"{save.SaveName}.s.rdat"));
					ProgSaves.Add(save);
					return true;
				}
				else
				{
					return false;
				}
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
		/// 使用指定的存档名移除进度存档(文件将同时删除)。
		/// </summary>
		/// <param name="save">要移除的存档的名称。</param>
		/// <returns>成功移除返回 <see langword="true" /> , 否则返回 <see langword="false"/> 。</returns>
		/// <exception cref="RabotoraException" />
		public bool RemoveSave(string saveName)
		{
			try
			{
				int saveIndex = ProgSaves.FindIndex(s => s.SaveName == saveName);
				if (saveIndex != -1) //存在指定的存档信息
				{
					File.Delete(ProgSaves[saveIndex].FilePath);
					ProgSaves.RemoveAt(saveIndex);
					return true;
				}
				else
				{
					return false;
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

		/// <summary>
		/// 使用指定存档的 <see cref="GameSave"/> 实例移除进度存档(文件将同时删除)。
		/// </summary>
		/// <param name="save">要移除的存档的 <see cref="GameSave"/> 实例。</param>
		/// <returns>成功移除返回 <see langword="true" /> , 否则返回 <see langword="false"/> 。</returns>
		/// <exception cref="RabotoraException" />
		public bool RemoveSave(GameSave save)
		{
			try
			{
				int saveIndex = ProgSaves.FindIndex(s => s.SaveName == save.SaveName);
				if (saveIndex != -1) //存在指定的存档信息
				{
					File.Delete(ProgSaves[saveIndex].FilePath);
					ProgSaves.RemoveAt(saveIndex);
					return true;
				}
				else
				{
					return false;
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

		/// <summary>
		/// 添加新的子配置信息文件。
		/// </summary>
		/// <param name="confName">子配置信息的名称。</param>
		/// <param name="config">要添加的子配置信息的 <see cref="GameConfig"/> 实例。</param>
		/// <returns>成功添加返回 <see langword="true" /> , 否则返回 <see langword="false"/> 。</returns>
		/// <exception cref="RabotoraException" />
		public bool AddNewAuxiliConfig(string confName, GameConfig config)
		{
			try
			{
				if (!AuxiliConfigs.ContainsKey(confName) && !(from auxConf in AuxiliConfigs where auxConf.Value.FilePath == config.FilePath select auxConf).Any())
				{ //不存在指定的存档信息(配置数据id和配置数据文件均不存在)
					config.Save(Path.Combine(PlayerDataPath, $"{confName}.rdat"));
					AuxiliConfigs.Add(confName, config);
					return true;
				}
				else
				{
					return false;
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

		/// <summary>
		/// 更新已有的子配置信息文件的数据。
		/// </summary>
		/// <param name="confName">要更新的子配置信息的名称。</param>
		/// <param name="config">更新完毕的对应子配置信息的 <see cref="GameConfig"/> 实例。</param>
		/// <returns>成功更新返回 <see langword="true" /> , 否则返回 <see langword="false"/> 。</returns>
		/// <exception cref="RabotoraException" />
		public bool UpdateAuxiliConfig(string confName, GameConfig newConfig)
		{
			try
			{
				if (AuxiliConfigs.TryGetValue(confName, out var oldConfig))
				{ //存在指定的存档信息
					AuxiliConfigs.Remove(confName);
					AuxiliConfigs.Add(confName, newConfig);
					newConfig.Save();
					return true;
				}
				else
				{
					return false;
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

		/// <summary>
		/// 使用指定的子配置信息的名称移除子配置信息(文件将同时删除)。
		/// </summary>
		/// <param name="confName">要移除的子配置信息的名称。</param>
		/// <returns>成功移除返回 <see langword="true" /> , 否则返回 <see langword="false"/> 。</returns>
		/// <exception cref="RabotoraException" />
		public bool RemoveAuxiliConfig(string confName)
		{
			try
			{
				if (AuxiliConfigs.TryGetValue(confName,out var config)) //存在指定的配置信息
				{
					File.Delete(config.FilePath);
					AuxiliConfigs.Remove(confName);
					return true;
				}
				else
				{
					return false;
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

		/// <summary>
		/// 更新主配置信息文件的数据。
		/// </summary>
		/// <param name="newMainConfig">更新完毕的主配置信息的 <see cref="GameConfig"/> 实例。</param>
		/// <exception cref="RabotoraException" />
		public void UpdateMainConfig(GameConfig newMainConfig)
		{
			try
			{
				MainConfig = newMainConfig;
				newMainConfig.Save();
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

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					ProgSaves.Clear();
					AuxiliConfigs.Clear();
				}

				// TODO: 释放未托管的资源(未托管的对象)并重写终结器
				// TODO: 将大型字段设置为 null
				disposedValue = true;
			}
		}

		// // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
		// ~GameDBMgr()
		// {
		//     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
		//     Dispose(disposing: false);
		// }

		/// <summary>
		/// 释放当前 <see cref="GameDBMgr"/> 类实例所占用的所有资源。
		/// </summary>
		public void Dispose()
		{
			// 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
