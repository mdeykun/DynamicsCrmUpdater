using Cwru.Common.Extensions;
using Cwru.Common.Services;
using Newtonsoft.Json;
using System;
using System.Security;

namespace Cwru.Common.JsonConverters
{
    public class SecureStringConverter : JsonConverter<SecureString>
    {
        private readonly CryptoService cryptoService;
        private readonly string version;

        public SecureStringConverter(string version)
        {
            this.cryptoService = new CryptoService();
            this.version = version;
        }

        public SecureStringConverter(CryptoService cryptoService, string version) : this(version)
        {
            this.cryptoService = cryptoService;
        }

        public override void WriteJson(JsonWriter writer, SecureString secureValue, JsonSerializer serializer)
        {
            if (secureValue == null)
            {
                return;
            }

            var value = secureValue.GetString();
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var encryptedValue = cryptoService.Encrypt(value, version);
            writer.WriteValue(encryptedValue);
        }

        public override SecureString ReadJson(JsonReader reader, Type objectType, SecureString existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var encryptedValue = (string)reader.Value;
            if (string.IsNullOrEmpty(encryptedValue))
            {
                return null;
            }

            var stringValue = cryptoService.Decrypt(encryptedValue, version);
            var result = stringValue.ToSecureString();

            return result;
        }
    }
}
