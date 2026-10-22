using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
namespace Fourtitude.TransactionApi.Api.Logging
{
    public class SensitiveDataProtector
    {
        private const string PasswordFieldName = "partnerpassword";
        private const string UnreadableBodyMessage = "[body not logged: invalid JSON]";

        private static readonly JsonSerializerOptions LogJsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private readonly ILogEncryptor _encryptor;

        public SensitiveDataProtector(ILogEncryptor encryptor)
        {
            _encryptor = encryptor;
        }

        public string Protect(string jsonBody)
        {
            if (string.IsNullOrWhiteSpace(jsonBody))
            {
                return jsonBody;
            }

            try
            {
                if (JsonNode.Parse(jsonBody) is not JsonObject json)
                {
                    return jsonBody;
                }
                if (json[PasswordFieldName] is JsonValue password)
                {
                    json[PasswordFieldName] =
                        _encryptor.Encrypt(password.ToString());
                }

                return json.ToJsonString(LogJsonOptions);
            }
            catch(JsonException)
            {
                return UnreadableBodyMessage;
            }
        }
    }
}
