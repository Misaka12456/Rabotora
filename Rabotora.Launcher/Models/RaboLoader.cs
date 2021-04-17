#nullable enable
using Ionic.Zip;
using Newtonsoft.Json.Linq;
using static Rabotora.Launcher.Globals;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Rabotora.Core.Models;
using Microsoft.VisualBasic;
using Rabotora.Core.ScriptAnalyzer;

namespace Rabotora.Launcher
{
	class RaboLoader
	{
		/// <summary>
		/// 启动游戏。
		/// </summary>
		/// <exception cref="RabotoraException" />
		internal static void StartGame()
		{
			try
			{
				string cmdArgs = Interaction.Command();
				if (!string.IsNullOrWhiteSpace(cmdArgs))
				{
					string BaseDirectory = cmdArgs.Split(new[] { ' ' }, count: 2)[1].Replace("\"",string.Empty);
					if (File.Exists(Path.Combine(BaseDirectory, "system.rpk"))) //rpk: Rabotora Package
					{
						var fs = new FileStream(Path.Combine(BaseDirectory, "system.rpk"), FileMode.Open, FileAccess.Read);
						var br = new BinaryReader(fs,Encoding.ASCII,true);
						byte[] readPckHead = br.ReadBytes(15);
						if (Encoding.ASCII.GetString(readPckHead) == "RabotoraPackage")
						{
							br.Close();
							fs.Seek(0, SeekOrigin.Begin);
							fs.Seek(15, SeekOrigin.Begin); //将流位置调整为数据包头("RabotoraPackage")后
							byte[] rpkHead = Encoding.ASCII.GetBytes("PK"); //获取zip压缩包头的二进制字节数据
							byte[] rpkData = new byte[fs.Length - 15 + 2]; //保存数据包数据的数组
							fs.Read(rpkData, 0, (int)fs.Length - 15); //从数据包头后读取数据直到数据包末尾
							fs.Dispose();
							Array.Copy(rpkHead, 0, rpkData, 0, 2); //将zip包头添加到数据包数据的最前面
							var ms = new MemoryStream(rpkData);
							if (ZipFile.IsZipFile(ms, false))
							{
								var rpk_system = ZipFile.Read(ms);
								if (rpk_system.EntryFileNames.Contains("Run.rbtconfig"))
								{
									var configStream = new MemoryStream();
									rpk_system["Run.rbtconfig"].Extract(configStream);
									byte[] configData = configStream.ToArray();
									configStream.Dispose();
									var runConf = JObject.Parse(Encoding.UTF8.GetString(configData));
									string title = runConf.Value<string>("title")!;
									bool isFullScreen = runConf.Value<bool>("isFullScreen")!;
									var main = new FormBase()
									{
										Text = title,
										Tag = BaseDirectory
									};
									IntPtr? hIcon = null;
									if (runConf.TryGetValue("icon",out var jtk_icon))
									{
										var iconStream = new MemoryStream(Convert.FromBase64String((string)jtk_icon!));
										var iconBMap = (Bitmap)Image.FromStream(iconStream);
										hIcon = iconBMap.GetHicon();
										var icon = Icon.FromHandle(hIcon.Value);
										main.Icon = icon;
										main.ShowIcon = true;
									}
									else
									{
										main.ShowIcon = false;
									}
									string splashFileName = runConf.Value<string>("splashScript");
									var splashStream = new MemoryStream();
									rpk_system[splashFileName].Extract(splashStream);
									byte[] splashScriptData = splashStream.ToArray();
									splashStream.Dispose();
									var splashScript = new Script(splashScriptData);
									// TODO:初次打开游戏的设置提示
									if (!GameEnv.IsSavePathInitialized) GameEnv.InitializeSavePath();
									new Thread(() =>
									{
										Application.Run(main);
									});
									Thread.Sleep(1000);
									if (hIcon.HasValue) DestroyIcon(hIcon.Value);
								}
								else
								{
									throw new RabotoraException("Cannot find file Run.rbtconfig (in system.rpk).\n" +
										"If you executed Launcher before building the game, please build the game first.\n" +
										"(Single Launcher (without Run.rbtconfig) Starting is not supported)");
								}
							}
							else
							{
								ms.Dispose();
								throw new RabotoraException("Corrupted Package: system.rpk");
							}
						}
						else
						{
							br.Close();
							throw new RabotoraException("Corrupted Package: system.rpk");
						}
					}
					else
					{
						throw new RabotoraException("Cannot find file Run.rbtconfig (in system.rpk).\n" +
							"If you executed Launcher before building the game, please build the game first.\n" +
							"(Single Launcher (without Run.rbtconfig) Starting is not supported)");
					}
				}
				else
				{
					throw new RabotoraException("Cannot find Launcher file base directory from command line.\n" +
										"If you executed Launcher before building the game, please build the game first.\n" +
										"(Single Launcher (without Run.rbtconfig) Starting is not supported)");
				}
			}
			catch (RabotoraException)
			{
				throw;
			}
			catch(Exception ex)
			{
				throw new RabotoraException($"An unexpected error occurred: {ex.Message}\nThe game cannot start.");
			}
		}
	}
}
