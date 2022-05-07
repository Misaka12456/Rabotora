using System.Enhance.Array;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace System.Enhance.Security.Cryptography
{
	public static class AesHelper
	{
		private static readonly byte[] DataTagSeperator = { 0xFF, 0xFF, 0x14, 0x95, 0x61, 0x74, 0xFF, 0xFF };
		private static readonly byte[] TagNonceSeperator = { 0xFE, 0xFE, 0x12, 0x45, 0x67, 0x9F, 0xFE, 0xFE };
		public static byte[] Encrypt(byte[] plainData, byte[] key)
		{
			var aes = new AesGcm(key);
			var nonce = Encoding.UTF8.GetBytes(BitConverter.ToString(key).GetHashCode().ToString());
			byte[] cipherData = new byte[16 * (plainData.Length / 16 + 1)];
			byte[] tag = new byte[AesGcm.TagByteSizes.MaxSize];
			aes.Encrypt(nonce, plainData, cipherData, tag);
			aes.Dispose();
			using var stream = new MemoryStream();
			stream.Write(cipherData, 0, cipherData.Length);
			stream.Write(DataTagSeperator, 0, DataTagSeperator.Length);
			stream.Write(tag, 0, tag.Length);
			stream.Write(TagNonceSeperator, 0, TagNonceSeperator.Length);
			stream.Write(nonce, 0, nonce.Length);
			byte[] r = stream.ToArray();
			stream.Close();
			return r;
		}

		public static byte[] Decrypt(byte[] cipherData, byte[] key)
		{
			var aes = new AesGcm(key);
			try
			{
				int tagIndex = cipherData.IndexOfSubByteArray(DataTagSeperator);
				if (tagIndex != -1)
				{
					tagIndex += DataTagSeperator.Length;
				}
				else
				{
					throw new IndexOutOfRangeException();
				}
				int nonceIndex = cipherData.IndexOfSubByteArray(TagNonceSeperator);
				if (nonceIndex != -1)
				{
					nonceIndex += TagNonceSeperator.Length;
				}
				else
				{
					throw new IndexOutOfRangeException();
				}
				byte[] tag = cipherData[tagIndex..(nonceIndex - TagNonceSeperator.Length + 1)];
				byte[] nonce = cipherData[nonceIndex..];
				cipherData = cipherData[0..(tagIndex - DataTagSeperator.Length + 1)];
				byte[] plainData = new byte[cipherData.Length / 16];
				aes.Decrypt(nonce, cipherData, tag, plainData);
				return plainData;
			}
			catch (IndexOutOfRangeException)
			{
				throw new CryptographicException("Decrypt failed");
			}
			catch (CryptographicException ex)
			{
				throw new CryptographicException($"Decrypt failed: {ex.Message}");
			}
			finally
			{
				aes.Dispose();
			}
		}
	}
}