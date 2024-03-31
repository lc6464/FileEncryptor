using System.Text;
using System.Security.Cryptography;

namespace FileEncryptor;
public static class Encryptor {
	public static readonly RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

	public static void Encrypt256(Stream dataStream, Stream outputStream, byte[] key, byte[] iv) {
		using var aes = Aes.Create();
		aes.Key = key;
		aes.IV = iv;
		aes.Padding = PaddingMode.PKCS7;

		using MemoryStream memoryStream = new();
		using var encryptor = aes.CreateEncryptor();
		using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
		dataStream.CopyTo(cryptoStream);
		cryptoStream.FlushFinalBlock();
		memoryStream.Position = 0;
		memoryStream.CopyTo(outputStream);
	}

	public static void Decrypt256(Stream encryptedDataStream, Stream outputStream, byte[] key, byte[] iv) {
		using var aes = Aes.Create();
		aes.Key = key;
		aes.IV = iv;
		aes.Padding = PaddingMode.PKCS7;

		using var decryptor = aes.CreateDecryptor();
		using CryptoStream cryptoStream = new(encryptedDataStream, decryptor, CryptoStreamMode.Read);
		cryptoStream.CopyTo(outputStream);
		cryptoStream.Clear();
	}

	public static byte[] GetRandomBytes(int length) {
		var bytes = new byte[length];
		randomNumberGenerator.GetBytes(bytes);
		return bytes;
	}

	public static byte[] GetRandomIV() => GetRandomBytes(16);

	public static byte[] HashPassword(string password, byte[] iv) {
		HMACSHA256 hmac = new(iv);
		return hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
	}
}