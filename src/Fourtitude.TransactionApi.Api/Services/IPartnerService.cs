namespace Fourtitude.TransactionApi.Api.Services
{
    public interface IPartnerService
    {
        bool IsAuthorized(string? partnerKey, string? encodedPassword);
    }
}
