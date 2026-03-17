using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using K4os.Compression.LZ4;
using RabotoraX.Core.Resources;

namespace RabotoraX.Core.Resources
{
	/// <summary>
	/// Represents the metadata of a single chunk of file data in the data package.
	/// </summary>
	public readonly struct ChunkDescriptor
	{
		/// <summary>
		/// Absolute offset in the big package file, -1 means Sparse/All-Zero chunk
		/// </summary>
		public readonly long PhysicalOffset;

		/// <summary>
		/// Compressed physical size in the big package file.
		/// </summary>
		public readonly int CompressedSize;

		/// <summary>
		/// Uncompressed logical size of the chunk, usually the same as BlockSize especially the last chunk
		/// </summary>
		public readonly int UncompressedSize;

		public readonly bool IsCompressed;
		public readonly bool IsEncrypted;

		[UsedImplicitly]
		public ChunkDescriptor(long offset, int compSize, int unCompSize, bool isCompressed, bool isEncrypted)
		{
			PhysicalOffset = offset;
			CompressedSize = compSize;
			UncompressedSize = unCompSize;
			IsCompressed = isCompressed;
			IsEncrypted = isEncrypted;
		}

		public ChunkDescriptor(RawChunkDescriptor raw)
		{
			PhysicalOffset = raw.Offset;
			CompressedSize = raw.CompressedSize;
			UncompressedSize = raw.UncompressedSize;
			IsCompressed = (raw.Flags & (byte) ChunkFlags.Compressed) != 0;
			IsEncrypted = (raw.Flags & (byte) ChunkFlags.Encrypted) != 0;
		}
	}

	/// <summary>
	/// Flags for chunk data in the package file.
	/// </summary>
	[Flags]
	public enum ChunkFlags : byte
	{
		None = 0,
		Compressed = 1,
		Encrypted = 2
	}

	/// <summary>
	/// Represents the raw metadata structure of a chunk as stored in the data package.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct RawChunkDescriptor
	{
		public long Offset;
		public int CompressedSize;
		public int UncompressedSize;
		public byte Flags;
	}

	/// <summary>
	/// Represents the raw header structure of the data package.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct RawDataPackageHeader
	{
		public ulong Magic;
		public int Version;
		public int FileCount;
		public long TableOffset;
	}

	/// <summary>
	/// Represents the raw metadata structure of a file entry as stored in the data package.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct RawEntry
	{
		public int LogicalPathLength;
		public long LogicalLength;
		public int LogicalBlockSize;
		public int ChunkCount;
	}

	/// <summary>
	/// Represents a single file entry in the data package, containing its logical path, size, block size, and chunk descriptors.
	/// </summary>
	public sealed class RFileEntry
	{
		public string Path { get; }
		public long LogicalLength { get; }
		public int LogicalBlockSize { get; }
		public ReadOnlyMemory<ChunkDescriptor> Chunks { get; }

		public RFileEntry(string path, long length, int blockSize, params ChunkDescriptor[] chunks)
		{
			Path = path;
			LogicalLength = length;
			LogicalBlockSize = blockSize;
			Chunks = chunks;
		}
	}

	/// <summary>
	/// Interface for decrypting encrypted chunks in the data package.<br />
	/// Implementations should provide the logic to transform encrypted chunk data back into its original form using the provided key and chunk index.
	/// </summary>
	public interface IDataDecryptor : IDisposable
	{
		void Decrypt(ReadOnlySpan<byte> input, Span<byte> output, int chunkIndex);
	}

	/// <summary>
	/// Represents a stream for reading the logical content of a file entry from the data package, handling chunk loading, decompression, and decryption as needed.
	/// </summary>
	[UsedImplicitly]
	public partial class RDataEntryReader : Stream
	{
		private const string ReadReturnValueUsage = "Reading operations may return results that have fewer bytes than requested. Use the return value to determine how many bytes were actually read.";
		private const int MaxCacheCapacity = 4;

		private sealed partial class ChunkCacheNode : IDisposable;

		public override bool CanRead => true;
		public override bool CanSeek => true;
		public override bool CanWrite => false;
		public override long Length => _entry.LogicalLength;

		private readonly RDataPackage _srcPackage;
		private readonly RFileEntry _entry;
		private readonly IDataDecryptor? _cryptor;
		private readonly SemaphoreSlim _asyncLock = new(1, 1);
		private readonly LinkedList<ChunkCacheNode> _lruList = [];
		private readonly Dictionary<int, LinkedListNode<ChunkCacheNode>> _cacheMap = [];

		private long _position;
		private int _currentChunkIndex = -1;
		private byte[]? _chunkBuffer;
		private int _chunkBufferValidLength;

		private bool _isDisposed;

		internal RDataEntryReader(RDataPackage package, RFileEntry entry, IDataDecryptor? decryptor = null)
		{
			_srcPackage = package ?? throw new ArgumentNullException(nameof(package));
			_entry = entry ?? throw new ArgumentNullException(nameof(entry));
			_cryptor = decryptor;
		}

		public override long Position
		{
			get
			{
				ObjectDisposedException.ThrowIf(_isDisposed, this);
				return _position;
			}
			set => Seek(value, SeekOrigin.Begin);
		}

		[MustUseReturnValue(ReadReturnValueUsage)]
		public override int Read(byte[] b, int o, int c)
		{
			return Read(b.AsSpan(o, c));
		}

		[MustUseReturnValue(ReadReturnValueUsage)]
		public override int Read(Span<byte> buffer)
		{
			ObjectDisposedException.ThrowIf(_isDisposed, this);
			if (_position >= _entry.LogicalLength) return 0; // EOF

			int totalRead = 0;
			while (buffer.Length > 0 && _position < _entry.LogicalLength)
			{
				int targetChunkIndex = (int) (_position / _entry.LogicalBlockSize);
				PrepareChunk(targetChunkIndex);

				int offsetInChunk = (int) (_position % _entry.LogicalBlockSize);
				int bytesAvailable = _chunkBufferValidLength - offsetInChunk;
				int bytesToRead = Math.Min(buffer.Length, bytesAvailable);

				if (bytesToRead <= 0) break; // Maybe corrupted entry or incorrect chunk descriptors, treated as EOF

				_chunkBuffer.AsSpan(offsetInChunk, bytesToRead).CopyTo(buffer);

				_position += bytesToRead;
				totalRead += bytesToRead;
				buffer = buffer[bytesToRead..]; // buffer.Slice(bytesToRead);
			}

			return totalRead;
		}

		[MustUseReturnValue(ReadReturnValueUsage)]
		public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			ObjectDisposedException.ThrowIf(_isDisposed, this);
			if (_position >= _entry.LogicalLength) return 0; // EOF

			int totalRead = 0;
			while (buffer.Length > 0 && _position < _entry.LogicalLength)
			{
				int targetChunkIndex = (int) (_position / _entry.LogicalBlockSize);
				await PrepareChunkAsync(targetChunkIndex, cancellationToken);

				int offsetInChunk = (int) (_position % _entry.LogicalBlockSize);
				int bytesAvailable = _chunkBufferValidLength - offsetInChunk;
				int bytesToRead = Math.Min(buffer.Length, bytesAvailable);

				if (bytesToRead <= 0) break;

				_chunkBuffer.AsSpan(offsetInChunk, bytesToRead).CopyTo(buffer.Span);

				_position += bytesToRead;
				totalRead += bytesToRead;
				buffer = buffer[bytesToRead..];
			}

			return totalRead;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PrepareChunk(int chunkIndex)
		{
			if (_currentChunkIndex == chunkIndex) return; // Already prepared

			_asyncLock.Wait();
			try
			{
				if (_currentChunkIndex == chunkIndex) return; // Double-check after acquiring lock
				if (_cacheMap.TryGetValue(chunkIndex, out var node))
				{
					_lruList.Remove(node);
					_lruList.AddFirst(node);
					UpdateCurrentState(node.Value);
					return;
				}

				var newNode = LoadChunkToCache(chunkIndex);
				if (_cacheMap.Count >= MaxCacheCapacity)
				{
					var last = _lruList.Last;
					if (last != null)
					{
						_cacheMap.Remove(last.Value.ChunkIndex);
						_lruList.RemoveLast();
						ArrayPool<byte>.Shared.Return(last.Value.Data);
					}
				}

				var listNode = _lruList.AddFirst(newNode);
				_cacheMap[chunkIndex] = listNode;
				UpdateCurrentState(newNode);
			}
			finally
			{
				_asyncLock.Release();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void UpdateCurrentState(ChunkCacheNode node)
		{
			_currentChunkIndex = node.ChunkIndex;
			_chunkBuffer = node.Data;
			_chunkBufferValidLength = node.ValidLength;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private async ValueTask PrepareChunkAsync(int chunkIndex, CancellationToken ct)
		{
			if (_currentChunkIndex == chunkIndex) return;

			await _asyncLock.WaitAsync(ct);
			try
			{
				if (_currentChunkIndex == chunkIndex) return;

				// 修正 1: 异步同样需要先检查 LRU 缓存
				if (_cacheMap.TryGetValue(chunkIndex, out var node))
				{
					_lruList.Remove(node);
					_lruList.AddFirst(node);
					UpdateCurrentState(node.Value);
					return;
				}

				// 修正 2: 调用正确的异步加载方法
				var newNode = await LoadChunkToCacheAsync(chunkIndex, ct);

				if (_cacheMap.Count >= MaxCacheCapacity)
				{
					var last = _lruList.Last;
					if (last != null)
					{
						_cacheMap.Remove(last.Value.ChunkIndex);
						_lruList.RemoveLast();
						ArrayPool<byte>.Shared.Return(last.Value.Data);
					}
				}

				var listNode = _lruList.AddFirst(newNode);
				_cacheMap[chunkIndex] = listNode;
				UpdateCurrentState(newNode);
			}
			finally
			{
				_asyncLock.Release();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ChunkCacheNode LoadChunkToCache(int chunkIndex)
		{
			var desc = _entry.Chunks.Span[chunkIndex];
			byte[] targetBuffer = ArrayPool<byte>.Shared.Rent(_entry.LogicalBlockSize);
			int validLen;

			if (desc.PhysicalOffset == -1)
			{
				Array.Clear(targetBuffer, 0, targetBuffer.Length);
				validLen = _entry.LogicalBlockSize;
			}
			else
			{
				byte[] rentedRaw = ArrayPool<byte>.Shared.Rent(desc.CompressedSize);
				try
				{
					_srcPackage.ReadPhysicalData(desc.PhysicalOffset, rentedRaw.AsSpan(0, desc.CompressedSize));
					validLen = ProcessRawDataToBuffer(desc, rentedRaw.AsSpan(0, desc.CompressedSize), targetBuffer, chunkIndex);
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(rentedRaw);
				}
			}

			return new ChunkCacheNode(chunkIndex, targetBuffer, validLen);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private async ValueTask<ChunkCacheNode> LoadChunkToCacheAsync(int chunkIndex, CancellationToken ct)
		{
			var desc = _entry.Chunks.Span[chunkIndex];
			byte[] targetBuffer = ArrayPool<byte>.Shared.Rent(_entry.LogicalBlockSize);
			int validLen;

			if (desc.PhysicalOffset == -1)
			{
				Array.Clear(targetBuffer, 0, targetBuffer.Length);
				validLen = _entry.LogicalBlockSize;
			}
			else
			{
				byte[] rentedRaw = ArrayPool<byte>.Shared.Rent(desc.CompressedSize);
				try
				{
					await _srcPackage.ReadPhysicalDataAsync(desc.PhysicalOffset, rentedRaw.AsMemory(0, desc.CompressedSize), ct);
					validLen = ProcessRawDataToBuffer(desc, rentedRaw.AsSpan(0, desc.CompressedSize), targetBuffer, chunkIndex);
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(rentedRaw);
				}
			}

			return new ChunkCacheNode(chunkIndex, targetBuffer, validLen);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int ProcessRawDataToBuffer(ChunkDescriptor desc, Span<byte> rawData, byte[] targetBuffer, int chunkIndex)
		{
			if (desc.IsEncrypted)
			{
				if (_cryptor == null) throw new InvalidOperationException("Data is encrypted but no decryptor was provided");
				_cryptor.Decrypt(rawData, rawData, chunkIndex);
			}

			if (desc.IsCompressed)
			{
				return LZ4Codec.Decode(rawData, targetBuffer.AsSpan(0, _entry.LogicalBlockSize));
			}
			else
			{
				rawData.CopyTo(targetBuffer);
				return desc.UncompressedSize;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			ObjectDisposedException.ThrowIf(_isDisposed, this);
			long n = origin switch
			{
				SeekOrigin.Begin => offset,
				SeekOrigin.Current => _position + offset,
				SeekOrigin.End => _entry.LogicalLength + offset,
				_ => throw new ArgumentOutOfRangeException(nameof(origin), "Invalid seek origin")
			};

			if (n < 0) throw new IOException("Cannot seek to a negative position");
			// if (n > _entry.LogicalLength) throw new EndOfStreamException("Cannot seek beyond the end of the stream");

			_position = n;
			return _position;
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed) return;

			if (disposing)
			{
				_asyncLock.Dispose();
				foreach (var node in _lruList)
				{
					node.Dispose();
				}

				_lruList.Clear();
				_cacheMap.Clear();
				_chunkBuffer = null;
			}

			base.Dispose(disposing);
			_isDisposed = true;
		}

		public override void Flush()
		{
		}

		public override void SetLength(long value) => throw new NotSupportedException("Cannot set length of a read-only stream");
		public override void Write(byte[] b, int o, int c) => throw new NotSupportedException("Cannot write to a read-only stream");
	}

	public partial class RDataEntryReader
	{
		private sealed partial class ChunkCacheNode
		{
			public readonly int ChunkIndex;
			public readonly byte[] Data;
			public readonly int ValidLength;

			public ChunkCacheNode(int index, byte[] data, int length)
			{
				ChunkIndex = index;
				Data = data;
				ValidLength = length;
			}

			public void Dispose()
			{
				ArrayPool<byte>.Shared.Return(Data);
			}
		}
	}

	/// <summary>
	/// Provides read-only access to a Rabotora package file containing multiple compressed and/or encrypted files.<br />
	/// Manages file entries, stream access, and handles chunked data reading with caching support.
	/// </summary>
	[UsedImplicitly]
	public partial class RDataPackage : IDisposable, IAsyncDisposable
	{
		private readonly Stream _baseStream;
		private readonly Dictionary<string, RFileEntry> _entries;
		private readonly IDataDecryptor? _decryptor;

		private readonly SemaphoreSlim _streamLock = new(1, 1);
		private readonly bool _canUseRandomAccess;
		private bool _isDisposed;

		public RDataPackage(Stream baseStream, IEnumerable<RFileEntry> entries, IDataDecryptor? decryptor = null)
		{
			_baseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
			_decryptor = decryptor;

			_entries = new Dictionary<string, RFileEntry>(StringComparer.OrdinalIgnoreCase);
			foreach (var entry in entries)
			{
				_entries[entry.Path] = entry;
			}

			// Windows 10 Version 1909+ supports unbuffered I/O which can be used for random access without affecting the file cache
			if (_baseStream is FileStream && OperatingSystem.IsWindows() && OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18363))
			{
				_canUseRandomAccess = true;
			}
		}

		/// <summary>
		/// Check if the given file entry exists.
		/// </summary>
		/// <param name="path">The logical path of the file entry to check, case-insensitive.</param>
		/// <returns><see langword="true"/> if the file entry exists in the package; otherwise, <see langword="false"/>.</returns>
		public bool Exists(string path)
		{
			ObjectDisposedException.ThrowIf(_isDisposed, this);
			return _entries.ContainsKey(path);
		}

		/// <summary>
		/// Opens a file entry stream for reading.
		/// </summary>
		/// <param name="path">The logical path of the file entry to open, case-insensitive.</param>
		/// <returns>An <see cref="RDataEntryReader"/> instance for reading the specified file entry.</returns>
		/// <exception cref="RabotoraException">Thrown if any error occurs while trying to open the file entry, such as the entry not existing or issues with the underlying stream.</exception>
		public RDataEntryReader OpenEntry(string path)
		{
			ObjectDisposedException.ThrowIf(_isDisposed, this);
			try
			{
				if (!_entries.TryGetValue(path, out var entry)) throw new FileNotFoundException($"File '{path}' not found in package");

				return new RDataEntryReader(this, entry, _decryptor);
			}
			catch (Exception ex)
			{
				throw new RabotoraException("Failed to open file entry stream", ex);
			}
		}
		
		/// <summary>
		/// Tries to open a file entry for reading.
		/// </summary>
		/// <param name="path">The logical path of the file entry to open, case-insensitive.</param>
		/// <param name="reader">When this method returns, contains the <see cref="RDataEntryReader"/> instance for reading the specified file entry if it exists; otherwise, <see langword="null"/>.</param>
		/// <returns><see langword="true"/> if the file entry exists and was opened successfully; otherwise, <see langword="false"/>.</returns>
		public bool TryOpenEntry(string path, out RDataEntryReader? reader)
		{
			ObjectDisposedException.ThrowIf(_isDisposed, this);
			try
			{
				if (_entries.TryGetValue(path, out var entry))
				{
					reader = new RDataEntryReader(this, entry, _decryptor);
					return true;
				}
			}
			catch
			{
				// ignore exceptions and return false, as this is a Try* method
			}

			reader = null;
			return false;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~RDataPackage()
		{
			Dispose(false);
		}

		public async ValueTask DisposeAsync()
		{
			await DisposeAsyncInternal();
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_isDisposed) return;

			if (disposing)
			{
				_streamLock.Dispose();
				_baseStream.Dispose();
				_decryptor?.Dispose();
			}

			_isDisposed = true;
		}

		protected virtual async ValueTask DisposeAsyncInternal()
		{
			if (_isDisposed) return;

			await _streamLock.WaitAsync();
			try
			{
				await _baseStream.DisposeAsync();
			}
			finally
			{
				_streamLock.Release();
				_streamLock.Dispose();
			}

			_decryptor?.Dispose();

			_isDisposed = true;
		}
	}

	public partial class RDataPackage
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void ReadPhysicalData(long physicalOffset, Span<byte> buffer)
		{
			ObjectDisposedException.ThrowIf(_isDisposed, this);
			if (_canUseRandomAccess && _baseStream is FileStream fs)
			{
				RandomAccess.Read(fs.SafeFileHandle, buffer, physicalOffset);
				return;
			}

			_streamLock.Wait();
			try
			{
				_baseStream.Position = physicalOffset;
				_baseStream.ReadExactly(buffer);
			}
			finally
			{
				_streamLock.Release();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal async ValueTask ReadPhysicalDataAsync(long physicalOffset, Memory<byte> buffer, CancellationToken ct)
		{
			ObjectDisposedException.ThrowIf(_isDisposed, this);
			if (_canUseRandomAccess && _baseStream is FileStream fs)
			{
				await RandomAccess.ReadAsync(fs.SafeFileHandle, buffer, physicalOffset, ct);
				return;
			}

			await _streamLock.WaitAsync(ct);
			try
			{
				_baseStream.Position = physicalOffset;
				await _baseStream.ReadExactlyAsync(buffer, ct);
			}
			finally
			{
				_streamLock.Release();
			}
		}
	}

	public partial class RDataPackage
	{
		private readonly static ReadOnlyMemory<byte> PackageHeader = "RABOTORA"u8.ToArray();
		private const int PackageLatestVersion = 1;

		/// <summary>
		/// Opens a Rabotora Data Package from the specified file path.
		/// </summary>
		/// <param name="filePath">The path to the package file to open.</param>
		/// <param name="decryptor">
		/// An optional <see cref="IDataDecryptor"/> instance to handle decryption of encrypted chunks.<br />
		/// If the package contains encrypted data, a valid decryptor must be provided; otherwise, an exception will be thrown when attempting to read encrypted chunks.
		/// </param>
		/// <returns>An instance of <see cref="RDataPackage"/> representing the opened package file, ready for accessing its contents.</returns>
		/// <exception cref="RabotoraException">Thrown if there is an error opening the package file, such as invalid format, unsupported version, or I/O issues.</exception>
		public static RDataPackage Open(string filePath, IDataDecryptor? decryptor = null)
		{
			ArgumentNullException.ThrowIfNull(filePath);
			try
			{
				// Package Struct: [Header][FileTable...][FileData...]
				var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.RandomAccess); // enable random access if supported by the OS
				Span<byte> headerBuf = stackalloc byte[Marshal.SizeOf<RawDataPackageHeader>()];
				fs.ReadExactly(headerBuf);
				var header = MemoryMarshal.Read<RawDataPackageHeader>(headerBuf);
				if (header.Magic != BinaryPrimitives.ReadUInt64LittleEndian(PackageHeader.Span)) throw new InvalidDataException("Invalid package format");
				if (header.Version > PackageLatestVersion) throw new InvalidDataException($"Unsupported package version: {header.Version}. Try upgrading the engine to a newer version that supports this package format.");
				fs.Position = header.TableOffset;
				var entries = new List<RFileEntry>(header.FileCount);
				Span<byte> entryBuf = stackalloc byte[Marshal.SizeOf<RawEntry>()];
				Span<byte> chunkBuf = stackalloc byte[Marshal.SizeOf<RawChunkDescriptor>()];

				// FileTable Struct: [Entry][Entry...]
				for (int i = 0; i < header.FileCount; i++)
				{
					// Entry Struct: [RawEntry][LogicalPath (UTF-8)][ChunkDescriptor...]
					fs.ReadExactly(entryBuf);
					var raw = MemoryMarshal.Read<RawEntry>(entryBuf);

					byte[] pathBytes = ArrayPool<byte>.Shared.Rent(raw.LogicalPathLength);
					string path;
					try
					{
						fs.ReadExactly(pathBytes.AsSpan(0, raw.LogicalPathLength));
						path = Encoding.UTF8.GetString(pathBytes, 0, raw.LogicalPathLength);
					}
					finally
					{
						ArrayPool<byte>.Shared.Return(pathBytes);
					}

					var chunks = new ChunkDescriptor[raw.ChunkCount];

					for (int c = 0; c < raw.ChunkCount; c++)
					{
						fs.ReadExactly(chunkBuf);
						var rc = MemoryMarshal.Read<RawChunkDescriptor>(chunkBuf);
						chunks[c] = new ChunkDescriptor(rc);
					}

					entries.Add(new RFileEntry(path, raw.LogicalLength, raw.LogicalBlockSize, chunks));
				}

				// FileData Struct: [Chunk0 (Raw Bytes)][Chunk1 (Raw Bytes)]...
				return new RDataPackage(fs, entries, decryptor);
			}
			catch (Exception ex)
			{
				throw new RabotoraException("Failed to open data package file", ex);
			}
		}

		/// <summary>
		/// Asynchronously opens a Rabotora Data Package from the specified file path.
		/// </summary>
		/// <param name="filePath">The path to the package file to open.</param>
		/// <param name="decryptor">
		/// An optional <see cref="IDataDecryptor"/> instance to handle decryption of encrypted chunks.<br />
		/// If the package contains encrypted data, a valid decryptor must be provided; otherwise, an exception will be thrown when attempting to read encrypted chunks.
		/// </param>
		/// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the asynchronous operation to complete.</param>
		/// <returns>
		/// A <see cref="ValueTask{RDataPackage}"/> that represents the asynchronous operation of opening the package file.<br />
		/// The result contains an instance of <see cref="RDataPackage"/> representing the opened package file, ready for accessing its contents.
		/// </returns>
		/// <exception cref="RabotoraException">Thrown if there is an error opening the package file, such as invalid format, unsupported version, or I/O issues.</exception>
		public static async ValueTask<RDataPackage> OpenAsync(string filePath, IDataDecryptor? decryptor = null, CancellationToken ct = default)
		{
			ArgumentNullException.ThrowIfNull(filePath);
			try
			{
				// Package Struct: [Header][FileTable...][FileData...]
				var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.RandomAccess);

				int headerSize = Marshal.SizeOf<RawDataPackageHeader>();
				using var headerOwner = MemoryPool<byte>.Shared.Rent(headerSize);
				var memory = headerOwner.Memory[..headerSize];
				await fs.ReadExactlyAsync(memory, ct);
				var header = MemoryMarshal.Read<RawDataPackageHeader>(memory.Span);

				if (header.Magic != BinaryPrimitives.ReadUInt64LittleEndian(PackageHeader.Span))
					throw new InvalidDataException("Invalid package format");

				fs.Position = header.TableOffset;

				var entries = new List<RFileEntry>(header.FileCount);
				int entrySize = Marshal.SizeOf<RawEntry>();
				int chunkDescSize = Marshal.SizeOf<RawChunkDescriptor>();

				using var entryOwner = MemoryPool<byte>.Shared.Rent(1024);

				// FileTable Struct: [Entry][Entry...]
				for (int i = 0; i < header.FileCount; i++)
				{
					// Entry Struct: [RawEntry][LogicalPath (UTF-8)][ChunkDescriptor...]
					await fs.ReadExactlyAsync(entryOwner.Memory[..entrySize], ct);
					var raw = MemoryMarshal.Read<RawEntry>(entryOwner.Memory.Span[..entrySize]);

					string path;
					using (var pathOwner = MemoryPool<byte>.Shared.Rent(raw.LogicalPathLength))
					{
						var pathMem = pathOwner.Memory[..raw.LogicalPathLength];
						await fs.ReadExactlyAsync(pathMem, ct);
						path = Encoding.UTF8.GetString(pathMem.Span);
					}

					var chunks = new ChunkDescriptor[raw.ChunkCount];
					for (int c = 0; c < raw.ChunkCount; c++)
					{
						await fs.ReadExactlyAsync(entryOwner.Memory[..chunkDescSize], ct);
						var rc = MemoryMarshal.Read<RawChunkDescriptor>(entryOwner.Memory.Span[..chunkDescSize]);
						chunks[c] = new ChunkDescriptor(rc);
					}

					entries.Add(new RFileEntry(path, raw.LogicalLength, raw.LogicalBlockSize, chunks));
				}

				// FileData Struct: [Chunk0 (Raw Bytes)][Chunk1 (Raw Bytes)]...
				return new RDataPackage(fs, entries, decryptor);
			}
			catch (Exception ex)
			{
				throw new RabotoraException("Failed to open data package file", ex);
			}
		}
	}
}

namespace RabotoraX.Core.Cryptography
{
	/// <summary>
	/// Represents the implementation of the Rabotora Standard Cryptography (RaboCrypt) algorithm, based on LEA (Lightweight Encryption Algorithm) with modifications for the specific use case in Rabotora packages.
	/// </summary>
	[UsedImplicitly]
	public sealed class RaboCrypt : IDataDecryptor
	{
		private const int BlockSize = 16;
		private readonly ReadOnlyMemory<byte> _key;
		private readonly uint[] _roundKeys;
		private readonly int _rounds;

		public RaboCrypt(ReadOnlySpan<byte> masterKey)
		{
			_rounds = masterKey.Length switch
			{
				16 => 24,
				24 => 28,
				32 => 32,
				_ => throw new ArgumentException("Invalid key length. Supported lengths are 16, 24, or 32 bytes.", nameof(masterKey))
			};

			_roundKeys = new uint[_rounds * 4];
			KeySchedule(masterKey);
			_key = masterKey.ToArray(); // Store a copy of the key for counter generation
		}

		public void Decrypt(ReadOnlySpan<byte> input, Span<byte> output, int chunkIndex)
		{
			Span<byte> counter = stackalloc byte[BlockSize];
			BinaryPrimitives.WriteInt32LittleEndian(counter, chunkIndex);

			if (_key.Length >= 12)
			{
				_key[..12].Span.CopyTo(counter[4..]);
			}

			int remaining = input.Length;
			int offset = 0;
			Span<byte> keyStream = stackalloc byte[BlockSize];

			while (remaining > 0)
			{
				EncryptBlock(counter, keyStream);

				int bytesToProcess = Math.Min(remaining, BlockSize);
				for (int i = 0; i < bytesToProcess; i++)
				{
					output[offset + i] = (byte) (input[offset + i] ^ keyStream[i]);
				}

				IncrementCounter(counter);

				remaining -= bytesToProcess;
				offset += bytesToProcess;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EncryptBlock(Span<byte> block, Span<byte> output)
		{
			uint x0 = BinaryPrimitives.ReadUInt32LittleEndian(block[..4]);
			uint x1 = BinaryPrimitives.ReadUInt32LittleEndian(block[4..8]);
			uint x2 = BinaryPrimitives.ReadUInt32LittleEndian(block[8..12]);
			uint x3 = BinaryPrimitives.ReadUInt32LittleEndian(block[12..16]);

			for (int i = 0; i < _rounds; i++)
			{
				int rk = i * 4;
				x3 = BitOperations.RotateRight((x2 ^ _roundKeys[rk + 2]) + (x3 ^ _roundKeys[rk + 3]), 3);
				x2 = BitOperations.RotateRight((x1 ^ _roundKeys[rk + 1]) + (x2 ^ _roundKeys[rk + 1]), 5);
				x1 = BitOperations.RotateRight((x0 ^ _roundKeys[rk]) + (x1 ^ _roundKeys[rk + 1]), 9);
				uint tmp = x0;
				x0 = x1;
				x1 = x2;
				x2 = x3;
				x3 = tmp;
			}

			BinaryPrimitives.WriteUInt32LittleEndian(output[..4], x0);
			BinaryPrimitives.WriteUInt32LittleEndian(output[4..8], x1);
			BinaryPrimitives.WriteUInt32LittleEndian(output[8..12], x2);
			BinaryPrimitives.WriteUInt32LittleEndian(output[12..16], x3);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void KeySchedule(ReadOnlySpan<byte> key)
		{
			uint[] delta = [0xc3efe9db, 0x44626b02, 0x79e27c8a, 0x78df30ec, 0x715ea49e, 0xc785da0a, 0xe04ef22a, 0xe5c40957];
			uint[] t = new uint[8];

			for (int i = 0; i < key.Length / 4; i++)
			{
				t[i] = BinaryPrimitives.ReadUInt32LittleEndian(key.Slice(i * 4, 4));
			}

			for (int i = 0; i < _rounds; i++)
			{
				uint d = BitOperations.RotateLeft(delta[i % 8], i);
				t[0] = BitOperations.RotateLeft(t[0] + d, 1);
				t[1] = BitOperations.RotateLeft(t[1] + BitOperations.RotateLeft(d, 1), 3);
				t[2] = BitOperations.RotateLeft(t[2] + BitOperations.RotateLeft(d, 2), 6);
				t[3] = BitOperations.RotateLeft(t[3] + BitOperations.RotateLeft(d, 3), 11);

				_roundKeys[i * 4] = t[0];
				_roundKeys[i * 4 + 1] = t[1];
				_roundKeys[i * 4 + 2] = t[2];
				_roundKeys[i * 4 + 3] = t[1];
			}
		}

		public void Dispose()
		{
			Array.Clear(_roundKeys);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void IncrementCounter(Span<byte> counter)
		{
			for (int i = 0; i < counter.Length; i++)
			{
				if (++counter[i] != 0) break;
			}
		}
	}
}