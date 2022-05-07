using FlatSharp;
using Newtonsoft.Json;
using System.Enhance.Array;
using System.Text;

namespace Rabotora.Core.Package
{
	public struct RabotoraDataPack : IDisposable
	{
		public static readonly PackageType[] RequiredPackageTypes = new[] { PackageType.SystemPack, PackageType.BGMPack, PackageType.BackgroundPack, PackageType.CGPack, PackageType.StandPack, PackageType.SDPack, PackageType.VoicePack };

		public static readonly PackageType[] OptionalPackageTypes = new[] { PackageType.AdditionalFacePack, PackageType.AdditionalMoviePack, PackageType.AdditionalSoundPack, PackageType.AdditionalMaskPack };

		public static readonly byte[] RabotoraPackHeader = { 0x12, 0x45, 0x67, 0x9A, 0x9F, (byte)'R', (byte)'a', (byte)'b', (byte)'o', (byte)'t', (byte)'o', (byte)'r', (byte)'a' };

		private RawDataPack RawPack;

		/// <summary>
		/// The information of current data pack.
		/// </summary>
		public RabotoraPackInfo Info { get; init; }

		/// <summary>
		/// The main data table of current data pack.
		/// <para>
		/// Key = Data Block Index<br />
		/// Value = Data Block
		/// </para>
		/// </summary>
		public Lazy<Dictionary<int, Memory<byte>>> DataTable { get; init; }

		/// <summary>
		/// Get data block using specified data block index.
		/// </summary>
		/// <param name="index">Specified data block index.</param>
		/// <returns>The raw data in the data block.</returns>
		/// <exception cref="IndexOutOfRangeException" />
		public Memory<byte> this[int index]
		{
			get
			{
				try
				{
					if (index < DataTable.Value.Count && DataTable.Value.TryGetValue(index, out var data))
					{
						return data;
					}
					else
					{
						throw new IndexOutOfRangeException("Data Block Index was out of range.");
					}
				}
				catch (IndexOutOfRangeException)
				{
					throw;
				}
			}
		}

		/// <summary>
		/// Get data block using specified data block path.
		/// </summary>
		/// <param name="dataBlockPath">Specified data block path.</param>
		/// <returns>The raw data in the data block when success; otherwise, <see langword="null" /> .</returns>
		public Memory<byte>? this[string dataBlockPath]
		{
			get
			{
				try
				{
					if (Info.DataContrastiveTable.TryGetValue(dataBlockPath, out int index))
					{
						return this[index];
					}
					else
					{
						return null;
					}
				}
				catch
				{
					return null;
				}
			}
		}

		private RabotoraDataPack(RawDataPack rawPack)
		{
			RawPack = rawPack;
			var infoData = RawPack.MetaInfoData;
			// If you want to use encrypted Rabotora Data Pack, uncomment the code below.
			// infoData = System.Enhance.Security.Cryptography.AesHelper.Decrypt(infoData, {Your Aes Encrypt Key});
			Info = JsonConvert.DeserializeObject<RabotoraPackInfo>(Encoding.UTF8.GetString(infoData));
			var table = new Dictionary<int, Memory<byte>>();
			var stream = new MemoryStream(RawPack.DataList);
			stream.Seek(0, SeekOrigin.Begin);
			var reader = new BinaryReader(stream);
			while (stream.Position < stream.Length - 1)
			{
				int dataIndex = reader.ReadInt32();
				long dataLength = reader.ReadInt64();
				byte[] data = reader.ReadBytes((int)dataLength);
				table.Add(dataIndex, data);
			}
			reader.Close();
			DataTable = new Lazy<Dictionary<int, Memory<byte>>>(table);
		}

		/// <summary>
		/// Load Rabotora Data Pack using specified file path.
		/// </summary>
		/// <param name="packFilePath">Path of Rabotora Data Pack File which is ready to be loaded.</param>
		/// <returns>Load result.</returns>
		/// <exception cref="RabotoraFormatException" />
		/// <exception cref="FileNotFoundException" />
		/// <exception cref="IOException" />
		public static RabotoraDataPack Load(string packFilePath)
		{
			try
			{
				Environment.CurrentDirectory = AppContext.BaseDirectory;
				if (File.Exists(packFilePath))
				{
					var stream = new FileStream(packFilePath, FileMode.Open, FileAccess.Read);
					var reader = new BinaryReader(stream);
					byte[] headerData = reader.ReadBytes(RabotoraPackHeader.Length);
					if (headerData.DeepEquals(RabotoraPackHeader))
					{
						byte[] packData = reader.ReadBytes((int)(stream.Length - RabotoraPackHeader.Length));
						reader.Close();
						var packDataMemory = packData.AsMemory();
						var rawPack = FlatBufferSerializer.Default.Parse<RawDataPack>(packDataMemory);
						return new(rawPack);
					}
					else
					{
						throw new RabotoraFormatException("Invalid header data of the specified Rabotora Data Pack File", packFilePath);
					}
				}
				else
				{
					throw new FileNotFoundException("Cannot find specified Rabotora Data Pack File", packFilePath);
				}
			}
			catch (RabotoraFormatException)
			{
				throw;
			}
			catch (FileNotFoundException)
			{
				throw;
			}
			catch (IOException)
			{
				throw;
			}
		}

		/// <summary>
		/// Asynchronously load Rabotora Data Pack using specified file path.
		/// </summary>
		/// <param name="packFilePath">Path of Rabotora Data Pack File which is ready to be loaded.</param>
		/// <returns>Load result.</returns>
		/// <exception cref="RabotoraFormatException" />
		/// <exception cref="FileNotFoundException" />
		/// <exception cref="IOException" />
		public static async Task<RabotoraDataPack> LoadAsync(string packFilePath)
		{
			return await Task.Run(() =>
			{
				return Load(packFilePath);
			});
		}

		/// <summary>
		/// Release all memory used by current <see cref="RabotoraDataPack"/> instance.
		/// <para>
		/// Call this method will remove all loaded pack data from the memory.
		/// </para>
		/// </summary>
		public void Dispose()
		{
			if (DataTable != null && DataTable.IsValueCreated)
			{
				DataTable.Value.Clear();
			}
			GC.SuppressFinalize(this);
			GC.Collect();
		}

		/// <summary>
		/// Get default pack file name of the specified Rabotora Data Pack type.
		/// </summary>
		/// <param name="type">Specified Rabotora Data Pack Type.</param>
		/// <returns>The default pack file name of the specified pack type.</returns>
		/// <exception cref="ArgumentException" />
		public static string PackTypeToFileName(PackageType type)
		{
			// If you want to change default package name, just edit the contrastive table below.
			return type switch
			{
				PackageType.SystemPack => "system.rpkg",
				PackageType.BGMPack => "bgm.rpkg",
				PackageType.BackgroundPack => "bg.rpkg",
				PackageType.CGPack => "ev.rpkg",
				PackageType.StandPack => "st.rpkg",
				PackageType.SDPack => "sd.rpkg",
				PackageType.VoicePack => "voice.rpkg",
				PackageType.AdditionalFacePack => "em.rpkg",
				PackageType.AdditionalMoviePack => "movie.rpkg",
				PackageType.AdditionalSoundPack => "etc.rpkg",
				PackageType.AdditionalMaskPack => "mask.rpkg",
				_ => throw new ArgumentException($"Invalid Rabotora Data Pack Type: {type}", nameof(type))
			};
		}

		/// <summary>
		/// Check the integrity of the Rabotora Game Project Data Package Tree.
		/// </summary>
		/// <param name="gameBasePath">The base path of the Rabotora Game Project.</param>
		/// <exception cref="RabotoraInitializeException" />
		public static void CheckPackTreeIntegrity(string gameBasePath)
		{
			var dirInfo = new DirectoryInfo(gameBasePath);
			if (dirInfo.Exists)
			{
				foreach (var pkgType in RequiredPackageTypes)
				{
					if (File.Exists(Path.Combine(dirInfo.FullName, PackTypeToFileName(pkgType))))
					{
						continue;
					}
					else
					{
						throw new RabotoraInitializeException($"Package Tree Integrity check failed: Cannot find required game data pack: {PackTypeToFileName(pkgType)} in {dirInfo.FullName}");
					}
				}
				return;
			}
			else
			{
				throw new RabotoraInitializeException($"Package Tree Integrity check failed: Cannot find Game Project Base Directory: {dirInfo.FullName}");
			}
		}
	}
}
