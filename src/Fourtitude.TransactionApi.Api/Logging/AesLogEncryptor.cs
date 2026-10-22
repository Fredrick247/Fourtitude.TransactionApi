using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Fourtitude.TransactionApi.Api.Logging
{
    public class AesLogEncryptor : ILogEncryptor
    {
        private readonly byte[] _key;
        public  AesLogEncryptor(IOptions<LogEncryptionOptions> options)
        {
            _key = Convert.FromBase64String(options.Value.Key);
        }

        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            var cipherBytes =
                aes.EncryptCbc(Encoding.UTF8.GetBytes(plainText), aes.IV);

            return
                Convert.ToBase64String(aes.IV.Concat(cipherBytes).ToArray());
        }
        public string Decrypt(string cipherText)
        {
            var data = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = _key;

            var iv = data[..16];
            var cipherBytes = data[16..];

            return Encoding.UTF8.GetString(aes.DecryptCbc(cipherBytes, iv));
        }
    }
}
