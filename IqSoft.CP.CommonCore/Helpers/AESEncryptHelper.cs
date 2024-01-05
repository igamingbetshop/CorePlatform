using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace IqSoft.CP.Common.Helpers
{
    public static class AESEncryptHelper
    {
        public static string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using var aes = Aes.Create();
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write);
            using var streamWriter = new StreamWriter((Stream)cryptoStream);
            streamWriter.Write(plainText);
            array = memoryStream.ToArray();
            return Convert.ToBase64String(array);
        }

        public static string DecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var memoryStream = new MemoryStream(buffer);
            using var cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader((Stream)cryptoStream);
            return streamReader.ReadToEnd();
        }

        //public static string GenerateAESKey()
        //{
        //    var crypto = new AesCryptoServiceProvider
        //    {
        //        KeySize = 128,
        //        BlockSize = 128
        //    };
        //    crypto.GenerateKey();
        //    byte[] keyGenerated = crypto.Key;
        //    return Convert.ToBase64String(keyGenerated);
        //}

        private static readonly string DistributionHashKey = "53PvJn3dqCcenYpL1PJoMg==";
        public static string EncryptDistributionString(string cipherText)
        {
            return EncryptString(DistributionHashKey, cipherText);
        }
        public static string DecryptDistributionString(string cipherText)
        {
            return DecryptString(DistributionHashKey, cipherText);
        }

        public static string Encryption(string data, string key)
        {
            using (var aes = new AesManaged { Key = Encoding.UTF8.GetBytes(key), Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
            {
                var crypt = aes.CreateEncryptor();
                byte[] encBytes = Encoding.UTF8.GetBytes(data);
                byte[] resultBytes = crypt.TransformFinalBlock(encBytes, 0, encBytes.Length);
                return Convert.ToBase64String(resultBytes);
            }
        }
    }
}