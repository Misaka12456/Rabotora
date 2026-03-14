#pragma warning disable 618

using Silk.NET.OpenAL;
using StbVorbisSharp;

namespace RabotoraX.Core.Audios;

public enum AudioFormatType
{
	Unspecified = 0,
	Unsupported = 1,
	WavePCM = 10,
	WaveADPCM = 11,
	OggVorbis = 20,
	[Obsolete("Use OggVorbis instead as OggVorbis is a more modern and efficient audio format.")] Mp3 = 30
}

public sealed class AudioClip : IDisposable
{
	internal uint BufferId { get; private set; }
	public int Channels { get; private set; }
	public int SampleRate { get; private set; }

	private bool _isDisposed;

	private AudioClip()
	{
		_isDisposed = false;
	}

	public static AudioClip LoadFromStream(Stream stream, AudioFormatType format)
	{
		if (format == AudioFormatType.Unspecified)
		{
			throw new InvalidOperationException("This is a lower level API that requires you to specify the audio format type.");
		}
		var clip = new AudioClip();
		clip.BufferId = AudioService.AL.GenBuffer();
		
		byte[] pcmData;
		int bitsPerSample = 16; // Default to 16 bits per sample for PCM formats

		switch (format)
		{
			case AudioFormatType.WavePCM:
			case AudioFormatType.WaveADPCM:
				pcmData = DecodeWav(stream, out int wavChannels, out int wavSampleRate, out int wavBitsPerSample);
				clip.Channels = wavChannels;
				clip.SampleRate = wavSampleRate;
				bitsPerSample = wavBitsPerSample;
				break;
			case AudioFormatType.OggVorbis:
				using (var ms = new MemoryStream())
				{
					stream.CopyTo(ms);
					var vorbis = StbVorbis.decode_vorbis_from_memory(ms.ToArray(), out int oggSampleRate, out int oggChannels);
					pcmData = new byte[vorbis.Length * 2];
					Buffer.BlockCopy(vorbis, 0, pcmData, 0, pcmData.Length);
					clip.Channels = oggChannels;
					clip.SampleRate = oggSampleRate;
				}
				break;
			case AudioFormatType.Mp3:
				throw new NotImplementedException("MP3 decoding is not implemented yet. Please use OggVorbis format instead for better performance and quality.");
			default:
				throw new NotSupportedException($"Audio format {format} is not supported.");
		}
		
		var alFormat = GetOpenALFormat(clip.Channels, bitsPerSample);

		unsafe
		{
			fixed (byte* ptr = pcmData)
			{
				AudioService.AL.BufferData(clip.BufferId, alFormat, ptr, pcmData.Length, clip.SampleRate);
			}
		}
		
		return clip;
	}

	private static BufferFormat GetOpenALFormat(int channels, int bitsPerSample)
	{
		return channels switch
		{
			1 => bitsPerSample == 8 ? BufferFormat.Mono8 : BufferFormat.Mono16,
			2 => bitsPerSample == 8 ? BufferFormat.Stereo8 : BufferFormat.Stereo16,
			_ => throw new NotSupportedException($"Unsupported channel count: {channels}. Only Mono and Stereo audio formats are supported.")
		};
	}

	private static byte[] DecodeWav(Stream stream, out int channels, out int sampleRate, out int bitsPerSample)
	{
		using var reader = new BinaryReader(stream);
		if (new string(reader.ReadChars(4)) != "RIFF") throw new InvalidDataException("Invalid WAV file format.");
		reader.ReadInt32(); // Skip file size
		if (new string(reader.ReadChars(4)) != "WAVE") throw new InvalidDataException("Invalid WAV file format.");
		if (new string(reader.ReadChars(4)) != "fmt ") throw new InvalidDataException("Invalid WAV file format.");
		reader.ReadInt32(); // Skip fmt chunk size
		
		int audioFormat = reader.ReadInt16(); // 1 = PCM, 2 = ADPCM
		channels = reader.ReadInt16();
		sampleRate = reader.ReadInt32();
		reader.ReadInt32(); // Skip byte rate
		reader.ReadInt16(); // Skip block align
		bitsPerSample = reader.ReadInt16();

		byte[]? data = null;
		while (stream.Position < stream.Length)
		{
			string chunkId = new string(reader.ReadChars(4));
			int chunkSize = reader.ReadInt32();
			if (chunkId == "data")
			{
				byte[] chunkData = reader.ReadBytes(chunkSize);
				if (audioFormat == 1)
				{
					data = chunkData;
				}
				else if (audioFormat == 2)
				{
					data = DecodeADPCM(chunkData, channels);
					bitsPerSample = 16; // ADPCM is decoded to 16 bits per sample PCM
				}
				else
				{
					throw new NotSupportedException($"Unsupported WAV audio format: {audioFormat}. Only PCM and ADPCM formats are supported.");
				}
				break;
			}

			stream.Seek(chunkSize, SeekOrigin.Current); // Skip non-data chunks
		}
		
		if (data == null) throw new FormatException("Invalid WAV file format: No data chunk found.");
		return data;
	}

	private static byte[] DecodeADPCM(byte[] adpcmData, int channels)
	{
		int[] indexTable =
		[
			-1, -1, -1, -1, 2, 4, 6, 8,
			-1, -1, -1, -1, 2, 4, 6, 8
		];

		int[] stepTables =
		[
			7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 19, 21,
			23, 25, 28, 31, 34, 37, 41, 45, 50, 55, 60,
			66, 73, 80, 88, 97, 107, 118, 130, 143, 157,
			173, 190, 209, 230, 253, 279, 307, 337, 371,
			408, 449, 494, 544, 598, 658, 724, 796, 876,
			963, 1060, 1166, 1282, 1411, 1552, 1707, 1878,
			2066, 2272, 2499, 2749, 3024, 3327, 3660, 4026,
			4428, 4871, 5358, 5894, 6484, 7132, 7845, 8630,
			9493, 10442, 11487, 12635, 13899, 15289, 16818,
			18500, 20350, 22385, 24623, 27086, 29794, 32767
		];
		
		using var ms = new MemoryStream();
		var br = new BinaryReader(new MemoryStream(adpcmData));

		while (br.BaseStream.Position < br.BaseStream.Length)
		{
			int[] predictor = new int[channels];
			int[] index = new int[channels];

			for (int ch = 0; ch < channels; ch++)
			{
				predictor[ch] = br.ReadInt16();
				index[ch] = br.ReadByte();
				br.ReadByte(); // Skip reserved byte (padding)
			}

			while (true)
			{
				if (br.BaseStream.Position >= br.BaseStream.Length) break;
				byte b = br.ReadByte();
				for (int nib = 0; nib < 2; nib++)
				{
					int ch = nib % channels;
					int code = (nib == 0) ? (b & 0x0F) : ((b >> 4) & 0x0F);
					
					int step = stepTables[index[ch]];
					int diff = step >> 3;
					if ((code & 1) != 0) diff += step >> 2;
					if ((code & 2) != 0) diff += step >> 1;
					if ((code & 4) != 0) diff += step;
					if ((code & 8) != 0) diff = -diff;
					
					predictor[ch] += diff;
					if (predictor[ch] > 32767) predictor[ch] = 32767;
					else if (predictor[ch] < -32768) predictor[ch] = -32768;
					
					index[ch] += indexTable[code];
					if (index[ch] < 0) index[ch] = 0;
					else if (index[ch] > 88) index[ch] = 88;
					
					ms.WriteByte((byte)(predictor[ch] & 0xFF));
					ms.WriteByte((byte)((predictor[ch] >> 8) & 0xFF));
				}
			}
		}
		
		return ms.ToArray();
	}

	public void Dispose()
	{
		if (_isDisposed) return;
		AudioService.AL.DeleteBuffer(BufferId);
		_isDisposed = true;
	}
}