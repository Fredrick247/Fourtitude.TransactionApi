using System.Globalization;
using Fourtitude.TransactionApi.Api.Models;
using Fourtitude.TransactionApi.Api.Services;
namespace Fourtitude.TransactionApi.Api.Validation
{
    public class TransactionValidator : ITransactionValidator
    {
        private const int MaxFieldLength = 50;
        private const int MaxItemNameLength = 100;
        private const int MinQuantity = 1;
        private const int MaxQuantity = 5;
        private static readonly TimeSpan AllowedTimeDifference = TimeSpan.FromMinutes(5);

        private readonly IPartnerService _partnerService;
        private readonly ISignatureService _signatureService;
        private readonly TimeProvider _timeProvider;

        public TransactionValidator(IPartnerService partnerService, ISignatureService signatureService, TimeProvider timeProvider)
        {
            _partnerService = partnerService;
            _signatureService = signatureService;
            _timeProvider = timeProvider;
        }

        public ValidationResult Validate(SubmitTransactionRequest request )
        {
            var error = ValidateRequiredFields(request)
                ?? ValidateFieldValues(request)
                ?? ValidateItems(request.Items)
                ?? ValidateAccess(request)
                ?? ValidateExpiry(request.Timestamp!)
                ?? ValidateItemsTotal(request);

            return error is null ? ValidationResult.Success() : ValidationResult.Failure(error);
        }

        private static string? ValidateRequiredFields(SubmitTransactionRequest request)
        {
            return CheckRequired(request.PartnerKey, "partnerkey")
                  ?? CheckRequired(request.PartnerRefNo, "partnerrefno")
                  ?? CheckRequired(request.PartnerPassword, "partnerpassword")
                  ?? CheckRequired(request.TotalAmount, "totalamount")
                  ?? CheckRequired(request.Timestamp, "timestamp")
                  ?? CheckRequired(request.Sig, "sig");
        }
        private static string? ValidateFieldValues(SubmitTransactionRequest request)
        {
            return CheckMaxLength(request.PartnerKey!, MaxFieldLength, "partnerkey")
                ?? CheckMaxLength(request.PartnerRefNo!, MaxFieldLength, "partnerrefno")
                ?? CheckMaxLength(request.PartnerPassword!, MaxFieldLength, "partnerpassword")
                ?? CheckPositive(request.TotalAmount!.Value, "totalamount")
                ?? CheckTimestampFormat(request.Timestamp!);
        }

        private static string? ValidateItems(List<ItemDetail>? items)
        {
            if (items is null)
            {
                return null;
            }

            foreach (var item in items)
            {
                if (item is null)
                {
                    return ErrorMessages.InvalidItem;
                }

                var error = CheckRequired(item.PartnerItemRef, "partneritemref")
                    ?? CheckMaxLength(item.PartnerItemRef!, MaxFieldLength, "partneritemref")
                    ?? CheckRequired(item.Name, "name")
                    ?? CheckMaxLength(item.Name!, MaxItemNameLength, "name")
                    ?? CheckRequired(item.Qty, "qty")
                    ?? CheckRange(item.Qty!.Value, MinQuantity, MaxQuantity, "qty")
                    ?? CheckRequired(item.UnitPrice, "unitprice")
                    ?? CheckPositive(item.UnitPrice!.Value, "unitprice");

                if (error is not null)
                {
                    return error;
                }
            }

            return null;
        }

        private string? ValidateAccess(SubmitTransactionRequest request)
        {
            var isAuthorized = _partnerService.IsAuthorized(request.PartnerKey, request.PartnerPassword)
                && _signatureService.IsValid(request);

            return isAuthorized ? null : ErrorMessages.AccessDenied;
        }

        private string? ValidateExpiry(string timestamp)
        {
            TryParseTimestamp(timestamp, out var requestTime);
            var serverTime = _timeProvider.GetUtcNow().UtcDateTime;

            return (serverTime - requestTime).Duration() > AllowedTimeDifference
                ? ErrorMessages.Expired
                : null;
        }

        private static string? ValidateItemsTotal(SubmitTransactionRequest request)
        {
            if (request.Items is null || request.Items.Count == 0)
            {
                return null;
            }

            var itemsTotal = request.Items.Sum(item => (decimal)item.Qty!.Value * item.UnitPrice!.Value);

            return itemsTotal == request.TotalAmount ? null : ErrorMessages.InvalidTotalAmount;
        }

        private static bool TryParseTimestamp(string value, out DateTime result)
        {
            return DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal,
                out result);
        }

        private static string? CheckTimestampFormat(string value) =>
            TryParseTimestamp(value, out _) ? null : ErrorMessages.InvalidTimestamp;

        private static string? CheckRequired(string? value, string fieldName) =>
            string.IsNullOrWhiteSpace(value) ? ErrorMessages.Required(fieldName) : null;

        private static string? CheckRequired<T>(T? value, string fieldName) where T : struct =>
            value is null ? ErrorMessages.Required(fieldName) : null;

        private static string? CheckMaxLength(string value, int maxLength, string fieldName) =>
            value.Length > maxLength ? ErrorMessages.MaxLength(fieldName, maxLength) : null;

        private static string? CheckPositive(long value, string fieldName) =>
            value > 0 ? null : ErrorMessages.MustBePositive(fieldName);

        private static string? CheckRange(int value, int min, int max, string fieldName) =>
            value >= min && value <= max ? null : ErrorMessages.OutOfRange(fieldName, min, max);
    }
}
