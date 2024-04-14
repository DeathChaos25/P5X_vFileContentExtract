using System.Security.Cryptography;
using System.Text;

namespace P5X_vFileContentExtract
{
    internal class DecryptBMS
    {
        private static readonly string aesKey = "b341ad2e901c4f3e";
        private static readonly string aesIV = "b4f0d9e3c2a2351f";

        static byte[] Decrypt(byte[] encryptedData, byte[] aesKey, byte[] aesIV)
        {
            using Aes aes = Aes.Create();
            aes.Key = aesKey;
            aes.IV = aesIV;

            using MemoryStream memoryStream = new();
            using ICryptoTransform iCryptoTransform = aes.CreateDecryptor();
            using CryptoStream cryptoStream = new(memoryStream, iCryptoTransform, CryptoStreamMode.Write);
            cryptoStream.Write(encryptedData, 0, encryptedData.Length);
            cryptoStream.FlushFinalBlock();

            return memoryStream.ToArray();
        }

        public static async Task DecryptBMSFile(string filePath, byte[] fileData)
        {
            byte[] keyBytes = Encoding.Default.GetBytes(aesKey);
            byte[] IVbytes = Encoding.Default.GetBytes(aesIV);

            byte[] decryptedData = Decrypt(fileData, keyBytes, IVbytes);
            await File.WriteAllBytesAsync(filePath, decryptedData);
        }
    }
}
