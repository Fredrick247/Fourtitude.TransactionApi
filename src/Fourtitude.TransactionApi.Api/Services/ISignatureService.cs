using Fourtitude.TransactionApi.Api.Models;
namespace Fourtitude.TransactionApi.Api.Services
{
    public interface ISignatureService
    {
         string GenerateSignature(SubmitTransactionRequest request);


         bool IsValid(SubmitTransactionRequest request);

    }
}
