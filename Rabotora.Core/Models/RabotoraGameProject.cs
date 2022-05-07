using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rabotora.Core.Package;
using System.Text;

namespace Rabotora.Core
{
	/// <summary>
	/// Represents a Visual Novel(VN) Game Project developed by Rabotora Game Engine.
	/// </summary>
	public class RabotoraGameProject : IDisposable
	{
		private bool disposedValue;

		/// <summary>
		/// The information of the Rabotora Game Project.
		/// </summary>
		public RabotoraGameProjectInfo Info { get; private set; }

		/// <summary>
		/// The system data pack of the Rabotora Game Project.
		/// </summary>
		public RabotoraDataPack SystemPack { get; private set; }

		/// <summary>
		/// List of all data packs availabling in the Rabotora Game Project.
		/// </summary>
		public Dictionary<PackageType, RabotoraDataPack> PackList { get; private set; }

		/// <summary>
		/// Is current project running in Rabotora Debug Mode?
		/// </summary>
		public bool IsDebugMode { get; private set; }

		private RabotoraGameProject(Dictionary<PackageType, RabotoraDataPack> packList)
		{
			PackList = packList;
			SystemPack = (from pack in PackList
						  where pack.Key == PackageType.SystemPack
						  select pack.Value).First();
			var infoDataWrapper = SystemPack["/project.json"];
			if (infoDataWrapper.HasValue)
			{
				var infoData = JsonConvert.DeserializeObject<RabotoraGameProjectInfo>(Encoding.UTF8.GetString(infoDataWrapper.Value.ToArray()));
				Info = infoData;
			}
			else
			{
				throw new RabotoraInitializeException("Cannot find the information data of the Rabotora Game Project.");
			}
		}

		/// <summary>
		/// Create a new <see cref="RabotoraGameProject"/> instance by absolute base path of target game project.
		/// </summary>
		/// <param name="gameBasePath">Absolute base path of the Rabotora game project which is ready to be initialized.</param>
		/// <param name="isDebugMode">Determines whatever Rabotora Debug Mode is on after initialization or not. Default is <see langword="false"/> .</param>
		/// <returns>New <see cref="RabotoraGameProject"/> instance of the target Rabotora game project when success; otherwise, null.</returns>
		/// <exception cref="RabotoraInitializeException" />
		public async static Task<RabotoraGameProject?> CreateFromGamePathAsync(string gameBasePath, bool isDebugMode = false)
		{
			try
			{
				RabotoraDataPack.CheckPackTreeIntegrity(gameBasePath);
				var packList = new Dictionary<PackageType, RabotoraDataPack>();
				foreach (var packType in RabotoraDataPack.RequiredPackageTypes)
				{
					var pack = await RabotoraDataPack.LoadAsync(Path.Combine(gameBasePath, RabotoraDataPack.PackTypeToFileName(packType)));
					packList.Add(packType, pack);
				}
				return new RabotoraGameProject(packList)
				{
					IsDebugMode = isDebugMode
				};
			}
			catch (RabotoraInitializeException)
			{
				throw;
			}
			catch (RabotoraFormatException)
			{
				return null;
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					foreach (var pack in PackList)
					{
						if (pack.Key != PackageType.SystemPack)
						{
							pack.Value.Dispose();
						}
					}
					PackList.Clear();
				}
				SystemPack.Dispose();
				disposedValue = true;
			}
		}

		// // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
		// ~RabotoraGameProject()
		// {
		//     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
		//     Dispose(disposing: false);
		// }

		/// <summary>
		/// Release all the resources used by current <see cref="RabotoraGameProject"/> instance.
		/// <para>
		/// This method should be called when Main Game Launcher(Rabotora.Launcher) exits.
		/// </para>
		/// </summary>
		public void Dispose()
		{
			// 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
