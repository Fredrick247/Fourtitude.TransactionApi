using Fourtitude.TransactionApi.Api.Models;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Fourtitude.TransactionApi.Api.Services
{
    public class SignatureService : ISignatureService
    {
        private const string SignatureTimestampFormat = "yyyyMMddHHmmss";

        public string GenerateSignature(SubmitTransactionRequest request)
        {
            var timestamp = DateTime.Parse(
                request.Timestamp!,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal);

            var rawSignature = 
                timestamp.ToString(SignatureTimestampFormat,
                CultureInfo.InvariantCulture) + 
                request.PartnerKey +
                request.PartnerRefNo + 
                request.TotalAmount?.ToString(CultureInfo.InvariantCulture) + 
                request.PartnerPassword;

            var hashBytes =
             SHA256.HashData(Encoding.UTF8.GetBytes(rawSignature));
            var hexHash = Convert.ToHexStringLower(hashBytes);

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(hexHash));
        }

        public bool IsValid(SubmitTransactionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Sig))
            {
                return false;
            }
            var expected = Encoding.UTF8.GetBytes(GenerateSignature(request));
            var actual = Encoding.UTF8.GetBytes(request.Sig.Trim());

            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }
    }
}
