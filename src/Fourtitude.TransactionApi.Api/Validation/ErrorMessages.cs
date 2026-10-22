namespace Fourtitude.TransactionApi.Api.Validation
{
    public static class ErrorMessages
    {
        public const string AccessDenied = "Access Denied!";
        public const string InvalidTotalAmount = "Invalid Total Amount.";
        public const string Expired = "Expired.";
        public const string InvalidTimestamp = "Invalid timestamp format.";
        public const string InvalidItem = "items cannot contain empty entries.";

        public static string Required(string fieldName) => $"{fieldName} is Required.";
        public static string MaxLength(string fieldName, int maxLength) =>
           $"{fieldName} cannot exceed {maxLength} characters.";

        public static string MustBePositive(string fieldName) => $"{fieldName} must be a positive value.";

        public static string OutOfRange(string fieldName, int min, int max) =>
            $"{fieldName} must be between {min} and {max}.";
        public const string InvalidRequest = "Invalid request format.";

    }
}
