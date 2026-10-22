using System.Text.Json.Serialization;

namespace Fourtitude.TransactionApi.Api.Models
{
    public class SubmitTransactionResponse
    {
        [JsonPropertyName("result")]
        public int Result { get; set; }

        [JsonPropertyName("totalamount")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? TotalAmount { get; set; }

        [JsonPropertyName("totaldiscount")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? TotalDiscount { get; set; }

        [JsonPropertyName("finalamount")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? FinalAmount { get; set; }

        [JsonPropertyName("resultmessage")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ResultMessage { get; set; }

        public static SubmitTransactionResponse Success(long totalAmount, long totalDiscount, long finalAmount)
        {
            return new SubmitTransactionResponse
            {
                Result = 1,
                TotalAmount = totalAmount,
                TotalDiscount = totalDiscount,
                FinalAmount = finalAmount
            };
        }

        public static SubmitTransactionResponse Failure(string message)
        {
            return new SubmitTransactionResponse
            {
                Result = 0,
                ResultMessage = message
            };
        }
    }
}
