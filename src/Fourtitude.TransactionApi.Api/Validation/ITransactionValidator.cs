using Fourtitude.TransactionApi.Api.Models;

namespace Fourtitude.TransactionApi.Api.Validation
{
    public interface ITransactionValidator
    {
        ValidationResult Validate(SubmitTransactionRequest request); 
    }
}
