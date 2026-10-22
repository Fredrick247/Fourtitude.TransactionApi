using System;
using System.Collections.Generic;
using System.Text;
using Fourtitude.TransactionApi.Api.Models;
using Fourtitude.TransactionApi.Api.Services;
using Microsoft.AspNetCore.Components;

namespace Fourtitude.TransactionApi.Tests
{
    public class SignatureServiceTests
    {
        private const string SampleSignature =
            "MDE3ZTBkODg4ZDNhYzU0ZDBlZWRmNmU2NmUyOWRhZWU4Y2M1NzQ1OTIzZGRjYTc1ZGNjOTkwYzg2MWJlMDExMw==";

        private readonly SignatureService _signatureService = new();

        [Fact]
        public void
            GenerateSignature_WithSampleRequest_ReturnsExpectedSignature()
        {
            var request = CreateSampleRequest();
            var result = _signatureService.GenerateSignature(request);
            Assert.Equal(SampleSignature,result);
        }

        [Fact]
        public void IsValid_WithCorrectSignature_ReturnsTrue()
        {
            var request = CreateSampleRequest();
            Assert.True(_signatureService.IsValid(request));
        }

        [Fact]
        public void IsValid_WithTamperedTotalAmount_ReturnFalse()
        {
            var request = CreateSampleRequest();
            request.TotalAmount = 2000;

            Assert.False(_signatureService.IsValid(request));
        }

        [Fact]
        public void IsValid_WithMissingSignature_ReturnsFalse()
        {
            var request = CreateSampleRequest();
            request.Sig = null;

            Assert.False(_signatureService.IsValid(request));
        }

        private static SubmitTransactionRequest CreateSampleRequest()
        {
            return new SubmitTransactionRequest
            {
                PartnerKey = "FAKEGOOGLE",
                PartnerRefNo = "FG-00001",
                PartnerPassword = "RkFLRVBBU1NXT1JEMTIzNA==",
                TotalAmount = 1000,
                Timestamp = "2024-08-15T02:11:22.0000000Z",
                Sig = " " + SampleSignature
            };
        }
    }
}
