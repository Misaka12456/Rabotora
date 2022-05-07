using Rabotora.Core;
using Rabotora.Core.Package;
using System.Diagnostics;

namespace Rabotora.Runtime
{
	public static class RabotoraStartup
	{
		public static async Task<Tuple<RabotoraGameProject, RabotoraScript>> InitializeAsync(string gameBasePath)
		{
			try
			{
				var project = await RabotoraGameProject.CreateFromGamePathAsync(gameBasePath);
				if (project != null)
				{
					var entryScriptDataWrapper = project.SystemPack["/main.rts"];
					if (entryScriptDataWrapper.HasValue)
					{
						var entryScript = RabotoraScript.LoadScript("%system%/main.rts", "main", entryScriptDataWrapper.Value.ToArray());
						return Tuple.Create(project, entryScript);
					}
					else
					{
						throw new RabotoraException($"Failed to initialize Rabotora Game Project: Cannot find entry script data in {RabotoraDataPack.PackTypeToFileName(PackageType.SystemPack)}.");
					}
				}
				else
				{
					throw new RabotoraException($"Failed to initialize Rabotora Game Project.");
				}
			}
			catch (RabotoraException)
			{
				throw;
			}
		}
	}
}
