using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using K4os.Compression.LZ4;
using RabotoraX.Core.Cryptography;
using RabotoraX.Core.Resources;
using RabotoraX.Utility.Packer.Models;

namespace RabotoraX.Utility.Packer;

public sealed class RDataPacker
{
	private readonly static byte[] Magic = "RABOTORA"u8.ToArray();

	[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
	public static async Task PackAsync(string outputPath, IEnumerable<PackItem> items, int blockSize = 65536, bool useCompression = true,
		ReadOnlyMemory<byte>? encryptionKey = null, IProgress<float>? progress = null, CancellationToken ct = default)
	{
		await using var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous);

		int headerSize = Marshal.SizeOf<RawDataPackageHeader>();
		fs.Seek(headerSize, SeekOrigin.Begin);

		var fileEntries = new List<(RawEntry Raw, byte[] PathBytes, List<RawChunkDescriptor> Chunks)>();
		using var encryptor = encryptionKey.HasValue && encryptionKey.Value.Length > 0 ? new RaboCrypt(encryptionKey.Value.Span) : null;
		var u8 = new UTF8Encoding(false);

		int totalFiles = 0;
		foreach (var _ in items) totalFiles++;
		int processedFiles = 0;

		foreach (var item in items)
		{
			ct.ThrowIfCancellationRequested();

			byte[] pathBytes = u8.GetBytes(item.LogicalPath.Replace('\\', '/'));
			await using var src = new FileStream(item.PhysicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);

			long logicalLength = src.Length;
			int chunkCount = (int) ((logicalLength + blockSize - 1) / blockSize);
			var chunks = new List<RawChunkDescriptor>(chunkCount);

			byte[] rawBuffer = ArrayPool<byte>.Shared.Rent(blockSize);
			byte[] compressedBuffer = ArrayPool<byte>.Shared.Rent(LZ4Codec.MaximumOutputSize(blockSize));

			try
			{
				for (int i = 0; i < chunkCount; i++)
				{
					int bytesRead = await src.ReadAsync(rawBuffer.AsMemory(0, blockSize), ct);
					if (bytesRead <= 0) break;

					var currentSpan = rawBuffer.AsSpan(0, bytesRead);
					if (IsAllZeros(currentSpan))
					{
						chunks.Add(new RawChunkDescriptor {Offset = -1, CompressedSize = 0, UncompressedSize = bytesRead, Flags = 0});
						continue;
					}

					byte[] dataToBuffer;
					int dataLen;
					byte flags = 0;
					if (useCompression)
					{
						int compSize = LZ4Codec.Encode(currentSpan, compressedBuffer);
						if (compSize < bytesRead)
						{
							dataToBuffer = compressedBuffer;
							dataLen = compSize;
							flags |= (byte) ChunkFlags.Compressed;
						}
						else
						{
							dataToBuffer = rawBuffer;
							dataLen = bytesRead;
						}
					}
					else
					{
						dataToBuffer = rawBuffer;
						dataLen = bytesRead;
					}

					if (encryptor != null)
					{
						encryptor.Decrypt(dataToBuffer.AsSpan(0, dataLen), dataToBuffer.AsSpan(0, dataLen), i);
						flags |= (byte) ChunkFlags.Encrypted;
					}

					long physicalOffset = fs.Position;
					await fs.WriteAsync(dataToBuffer.AsMemory(0, dataLen), ct);

					chunks.Add(new RawChunkDescriptor {Offset = physicalOffset, CompressedSize = dataLen, UncompressedSize = bytesRead, Flags = flags});
				}
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(rawBuffer);
				ArrayPool<byte>.Shared.Return(compressedBuffer);
			}

			fileEntries.Add((new RawEntry
			{
				LogicalPathLength = pathBytes.Length,
				LogicalLength = logicalLength,
				LogicalBlockSize = blockSize,
				ChunkCount = chunks.Count
			}, pathBytes, chunks));

			processedFiles++;
			progress?.Report((float) processedFiles / totalFiles);
		}

		long tableOffset = fs.Position;

		byte[] entryBuffer = new byte[Marshal.SizeOf<RawEntry>()];
		byte[] chunkDescriptorBuffer = new byte[Marshal.SizeOf<RawChunkDescriptor>()];

		foreach (var entry in fileEntries)
		{
			var rawEntry = entry.Raw;
			MemoryMarshal.Write(entryBuffer, in rawEntry);
			await fs.WriteAsync(entryBuffer.AsMemory(), ct);

			await fs.WriteAsync(entry.PathBytes.AsMemory(), ct);

			foreach (var chunk in entry.Chunks)
			{
				var c = chunk;
				MemoryMarshal.Write(chunkDescriptorBuffer, in c);
				await fs.WriteAsync(chunkDescriptorBuffer.AsMemory(), ct);
			}
		}

		fs.Seek(0, SeekOrigin.Begin);
		var header = new RawDataPackageHeader
		{
			Magic = BinaryPrimitives.ReadUInt64LittleEndian(Magic),
			Version = 1,
			FileCount = fileEntries.Count,
			TableOffset = tableOffset
		};

		byte[] headerBuffer = new byte[headerSize];
		MemoryMarshal.Write(headerBuffer, in header);
		await fs.WriteAsync(headerBuffer.AsMemory(), ct);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsAllZeros(ReadOnlySpan<byte> span)
	{
		foreach (byte b in span)
		{
			if (b != 0) return false;
		}

		return true;
	}
}