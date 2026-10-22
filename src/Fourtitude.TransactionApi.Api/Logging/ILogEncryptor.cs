namespace Fourtitude.TransactionApi.Api.Logging
{
    public interface ILogEncryptor
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}
