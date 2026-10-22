namespace Fourtitude.TransactionApi.Api.Validation
{
    public class ValidationResult
    {
        public bool IsValid { get; }
        public string? ErrorMessage { get;}
        private ValidationResult(bool isValid, string? errorMessage)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public static ValidationResult Success() => new(true, null);
        public static ValidationResult Failure(string errorMessage) => new(false, errorMessage);
    }
}
