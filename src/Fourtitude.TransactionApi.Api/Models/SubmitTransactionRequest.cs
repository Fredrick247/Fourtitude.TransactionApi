using System.Text.Json.Serialization;

namespace Fourtitude.TransactionApi.Api.Models
{
    public class SubmitTransactionRequest
    {
        [JsonPropertyName("partnerkey")]
        public string? PartnerKey { get; set; }

        [JsonPropertyName("partnerrefno")]
        public string? PartnerRefNo { get; set; }

        [JsonPropertyName("partnerpassword")]
        public string? PartnerPassword { get; set; }

        [JsonPropertyName("totalamount")]
        public long? TotalAmount { get; set; }

        [JsonPropertyName("items")]
        public List<ItemDetail>? Items { get; set; }

        [JsonPropertyName("timestamp")]
        public string? Timestamp { get; set; }

        [JsonPropertyName("sig")]
        public string? Sig { get; set; }
    }
}
