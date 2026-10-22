using System;
using System.Collections.Generic;
using System.Text;
using Fourtitude.TransactionApi.Api.Models;
using Fourtitude.TransactionApi.Api.Services;
using Microsoft.Extensions.Options;


namespace Fourtitude.TransactionApi.Tests
{
    public class PartnerServiceTests
    {
        private readonly PartnerService _partnerService;
        public PartnerServiceTests()
        {
            var partners = new List<Partner>()
            {
                new() { PartnerNo = "FG-00001", PartnerKey = "FAKEGOOGLE", Password = "FAKEPASSWORD1234" },
                new() { PartnerNo = "FG-00002", PartnerKey = "FAKEPEOPLE", Password = "FAKEPASSWORD4578" }
            };

            _partnerService = new PartnerService(Options.Create(partners));
        }

        [Theory]
        [InlineData("FAKEGOOGLE", "RkFLRVBBU1NXT1JEMTIzNA==")]
        [InlineData("FAKEPEOPLE", "RkFLRVBBU1NXT1JENDU3OA==")]
        public void IsAuthorized_WithValidPartner_ReturnsTrue(string partnerKey, string encodedPassword)
        {
            Assert.True(_partnerService.IsAuthorized(partnerKey, encodedPassword));
        }

        [Theory]
        [InlineData("UNKNOWN", "RkFLRVBBU1NXT1JEMTIzNA==")]
        [InlineData("FAKEGOOGLE", "RkFLRVBBU1NXT1JENDU3OA==")]
        [InlineData("fakegoogle", "RkFLRVBBU1NXT1JEMTIzNA==")]
        [InlineData("FAKEGOOGLE", "FAKEPASSWORD1234")]
        [InlineData("FAKEGOOGLE", "not-base64!!")]
        [InlineData(null, "RkFLRVBBU1NXT1JEMTIzNA==")]
        [InlineData("FAKEGOOGLE", null)]

        public void IsAuthorized_WithInvalidPartner_ReturnsFalse(string? partnerKey, string? encodedPassword)
        {
            Assert.False(_partnerService.IsAuthorized(partnerKey, encodedPassword));
        }

    
    }
}
