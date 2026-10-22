using System.Security.Cryptography;
using System.Text;
using Fourtitude.TransactionApi.Api.Models;
using Microsoft.Extensions.Options;
namespace Fourtitude.TransactionApi.Api.Services
{
    public class PartnerService : IPartnerService
    {
        private readonly List<Partner> _partners;
        public PartnerService(IOptions<List<Partner>> partners)
        {
            _partners = partners.Value;
        }

        public bool IsAuthorized(string? partnerKey, string? encodedPassword)
        {
            if(string.IsNullOrEmpty(partnerKey) || string.IsNullOrEmpty(encodedPassword))
            {
                return false;
            }

            var partner = _partners.FirstOrDefault((p => p.PartnerKey == partnerKey));
            if(partner is null)
            {
                return false;
            }

            var expectedPassword =
                Convert.ToBase64String(Encoding.UTF8.GetBytes(partner.Password));

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedPassword),
                Encoding.UTF8.GetBytes(encodedPassword));
        }
    }
}
