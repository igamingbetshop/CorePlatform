using System;
using System.Security.Cryptography;
using System.Text;

namespace IqSoft.CP.Common.Helpers
{
    public class RijndaelEncrypt
    {
        private readonly string _password;
        private readonly string _salt;
        private readonly string _iv;

        public RijndaelEncrypt(string password, string salt, string iv)
        {
            _password = password;
            _salt = salt;
            _iv = iv;
        }

        public string Encrypt(string raw)
        {
            var buffer = Encoding.UTF8.GetBytes(raw);
            var output = GetCryptoTransform(true, buffer);
            var encrypted = Convert.ToBase64String(output);
            return encrypted;
        }

        public string Decrypt(string encrypted)
        {
            var buffer = Convert.FromBase64String(encrypted);
            var output = GetCryptoTransform(false, buffer);
            var decypted = Encoding.UTF8.GetString(output);
            return decypted;
        }

        private byte[] GetCryptoTransform(bool encrypting, byte[] buffer)
        {
            using (var csp = new AesCryptoServiceProvider())
            {
                csp.Mode = CipherMode.CBC;
                csp.Padding = PaddingMode.PKCS7;
                var spec = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(_password),
                    Encoding.UTF8.GetBytes(_salt), 65536);
                var key = spec.GetBytes(16);
                csp.IV = Encoding.UTF8.GetBytes(_iv);
                csp.Key = key;
                var e = (encrypting ? csp.CreateEncryptor() : csp.CreateDecryptor());
                var output = e.TransformFinalBlock(buffer, 0, buffer.Length);
                return output;
            }
        }
    }
}
