using Cwru.Common.Model;
using Newtonsoft.Json;
using System;

namespace Cwru.Common.JsonConverters
{
    internal class AuthenticationTypeConverter : JsonConverter<AuthenticationType?>
    {
        public override void WriteJson(JsonWriter writer, AuthenticationType? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                return;
            }

            var stringValue = Enum.GetName(typeof(AuthenticationType), value.Value);
            writer.WriteValue(stringValue);
        }

        public override AuthenticationType? ReadJson(JsonReader reader, Type objectType, AuthenticationType? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var stringValue = (string)reader.Value;
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            return (AuthenticationType)Enum.Parse(typeof(AuthenticationType), stringValue);
        }
    }
}
