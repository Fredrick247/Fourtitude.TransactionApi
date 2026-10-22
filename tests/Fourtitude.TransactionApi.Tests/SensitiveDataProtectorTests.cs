using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using Fourtitude.TransactionApi.Api.Logging;
using Microsoft.Extensions.Options;

namespace Fourtitude.TransactionApi.Tests
{
    public class SensitiveDataProtectorTests
    {
        private const string TestKey = "AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8=";

        private readonly AesLogEncryptor _encryptor;
        private readonly SensitiveDataProtector _protector;

        public SensitiveDataProtectorTests()
        {
            _encryptor = new AesLogEncryptor(Options.Create(new LogEncryptionOptions { Key = TestKey }));
            _protector = new SensitiveDataProtector(_encryptor);
        }

        [Fact]
        public void Protect_WithPassword_EncryptsPasswordAndKeepsOtherFields()
        {
            var body = """{"partnerkey":"FAKEGOOGLE","partnerpassword":"RkFLRVBBU1NXT1JEMTIzNA=="}""";
            var result = JsonNode.Parse(_protector.Protect(body))!;

            Assert.Equal("FAKEGOOGLE", result["partnerkey"]!.ToString());
            Assert.NotEqual("RkFLRVBBU1NXT1JEMTIzNA==", result["partnerpassword"]!.ToString());
            Assert.Equal("RkFLRVBBU1NXT1JEMTIzNA==", _encryptor.Decrypt(result["partnerpassword"]!.ToString()));
        }

        [Fact]
        public void Protect_WithInvalidJson_DoesNotLogBody()
        {
            var result = _protector.Protect("""{"partnerpassword":"secret" """);

            Assert.DoesNotContain("secret", result);
        }

        [Fact]
        public void Encrypt_SameValueTwice_ProducesDifferentCipherText()
        {
            Assert.NotEqual(_encryptor.Encrypt("secret"), _encryptor.Encrypt("secret"));
        }
    }
}

