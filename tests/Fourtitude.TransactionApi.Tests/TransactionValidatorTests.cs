using Fourtitude.TransactionApi.Api.Models;
using Fourtitude.TransactionApi.Api.Services;
using Fourtitude.TransactionApi.Api.Validation;
using Microsoft.Extensions.Options;

namespace Fourtitude.TransactionApi.Tests
{
    public class TransactionValidatorTests
    {
        private static readonly DateTimeOffset ServerTime = new(2024, 8, 15, 2, 11, 22, TimeSpan.Zero);

        private readonly SignatureService _signatureService = new();
        private readonly TransactionValidator _validator;

        public TransactionValidatorTests()
        {
            var partners = new List<Partner>
            {
                new() { PartnerNo = "FG-00001", PartnerKey = "FAKEGOOGLE", Password = "FAKEPASSWORD1234" }
            };

            _validator = new TransactionValidator(
                new PartnerService(Options.Create(partners)),
                _signatureService,
                new FixedTimeProvider(ServerTime));
        }

        [Fact]
        public void Validate_WithSampleRequest_ReturnsSuccess()
        {
            var result = _validator.Validate(CreateSignedRequest());

            Assert.True(result.IsValid);
            Assert.Null(result.ErrorMessage);
        }

        [Theory]
        [InlineData("partnerkey")]
        [InlineData("partnerrefno")]
        [InlineData("partnerpassword")]
        [InlineData("totalamount")]
        [InlineData("timestamp")]
        [InlineData("sig")]
        public void Validate_WithMissingField_ReturnsRequired(string fieldName)
        {
            var request = CreateSignedRequest();
            switch (fieldName)
            {
                case "partnerkey": request.PartnerKey = null; break;
                case "partnerrefno": request.PartnerRefNo = ""; break;
                case "partnerpassword": request.PartnerPassword = " "; break;
                case "totalamount": request.TotalAmount = null; break;
                case "timestamp": request.Timestamp = null; break;
                case "sig": request.Sig = null; break;
            }

            var result = _validator.Validate(request);

            Assert.Equal($"{fieldName} is Required.", result.ErrorMessage);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1000)]
        public void Validate_WithNonPositiveTotalAmount_ReturnsError(long totalAmount)
        {
            var request = CreateSignedRequest(r => r.TotalAmount = totalAmount);

            var result = _validator.Validate(request);

            Assert.Equal("totalamount must be a positive value.", result.ErrorMessage);
        }

        [Fact]
        public void Validate_WithFieldTooLong_ReturnsError()
        {
            var request = CreateSignedRequest(r => r.PartnerRefNo = new string('A', 51));

            var result = _validator.Validate(request);

            Assert.Equal("partnerrefno cannot exceed 50 characters.", result.ErrorMessage);
        }

        [Fact]
        public void Validate_WithInvalidTimestampFormat_ReturnsError()
        {
            var request = CreateSignedRequest();
            request.Timestamp = "not-a-date";

            var result = _validator.Validate(request);

            Assert.Equal("Invalid timestamp format.", result.ErrorMessage);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public void Validate_WithQtyOutOfRange_ReturnsError(int qty)
        {
            var request = CreateSignedRequest(r => r.Items![0].Qty = qty);

            var result = _validator.Validate(request);

            Assert.Equal("qty must be between 1 and 5.", result.ErrorMessage);
        }

        [Fact]
        public void Validate_WithNegativeUnitPrice_ReturnsError()
        {
            var request = CreateSignedRequest(r => r.Items![0].UnitPrice = -200);

            var result = _validator.Validate(request);

            Assert.Equal("unitprice must be a positive value.", result.ErrorMessage);
        }

        [Fact]
        public void Validate_WithMissingItemName_ReturnsRequired()
        {
            var request = CreateSignedRequest(r => r.Items![0].Name = null);

            var result = _validator.Validate(request);

            Assert.Equal("name is Required.", result.ErrorMessage);
        }

        [Fact]
        public void Validate_WithWrongPassword_ReturnsAccessDenied()
        {
            var request = CreateSignedRequest(r => r.PartnerPassword = "V1JPTkdQQVNTV09SRA==");

            var result = _validator.Validate(request);

            Assert.Equal("Access Denied!", result.ErrorMessage);
        }

        [Fact]
        public void Validate_WithTamperedSignature_ReturnsAccessDenied()
        {
            var request = CreateSignedRequest();
            request.TotalAmount = 2000;

            var result = _validator.Validate(request);

            Assert.Equal("Access Denied!", result.ErrorMessage);
        }

        [Theory]
        [InlineData(-6)]
        [InlineData(6)]
        public void Validate_WithTimestampOutsideFiveMinutes_ReturnsExpired(int minutesOffset)
        {
            var timestamp = ServerTime.AddMinutes(minutesOffset).UtcDateTime.ToString("o");
            var request = CreateSignedRequest(r => r.Timestamp = timestamp);

            var result = _validator.Validate(request);

            Assert.Equal("Expired.", result.ErrorMessage);
        }

        [Theory]
        [InlineData(-5)]
        [InlineData(5)]
        public void Validate_WithTimestampWithinFiveMinutes_ReturnsSuccess(int minutesOffset)
        {
            var timestamp = ServerTime.AddMinutes(minutesOffset).UtcDateTime.ToString("o");
            var request = CreateSignedRequest(r => r.Timestamp = timestamp);

            var result = _validator.Validate(request);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_WithItemsNotMatchingTotal_ReturnsInvalidTotalAmount()
        {
            var request = CreateSignedRequest(r => r.Items![0].Qty = 3);

            var result = _validator.Validate(request);

            Assert.Equal("Invalid Total Amount.", result.ErrorMessage);
        }

        [Fact]
        public void Validate_WithoutItems_ReturnsSuccess()
        {
            var request = CreateSignedRequest(r => r.Items = null);

            var result = _validator.Validate(request);

            Assert.True(result.IsValid);
        }

        private SubmitTransactionRequest CreateSignedRequest(Action<SubmitTransactionRequest>? modify = null)
        {
            var request = new SubmitTransactionRequest
            {
                PartnerKey = "FAKEGOOGLE",
                PartnerRefNo = "FG-00001",
                PartnerPassword = "RkFLRVBBU1NXT1JEMTIzNA==",
                TotalAmount = 1000,
                Items =
                [
                    new() { PartnerItemRef = "i-00001", Name = "Pen", Qty = 4, UnitPrice = 200 },
                    new() { PartnerItemRef = "i-00002", Name = "Ruler", Qty = 2, UnitPrice = 100 }
                ],
                Timestamp = "2024-08-15T02:11:22.0000000Z"
            };

            modify?.Invoke(request);
            request.Sig = _signatureService.GenerateSignature(request);

            return request;
        }

        private sealed class FixedTimeProvider : TimeProvider
        {
            private readonly DateTimeOffset _utcNow;

            public FixedTimeProvider(DateTimeOffset utcNow)
            {
                _utcNow = utcNow;
            }

            public override DateTimeOffset GetUtcNow() => _utcNow;
        }
    }
}