using Newtonsoft.Json;
using System;
namespace IqSoft.CP.Common.Helpers
{
    public class DecimalJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(decimal));
        }

        public override void WriteJson(JsonWriter writer, object value,
                                       JsonSerializer serializer)
        {
            writer.WriteValue(decimal.Parse(string.Format("{0:N2}", value)));
        }

        public override object ReadJson(JsonReader reader, Type objectType,
                                     object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            { return null; }
            else if (reader.TokenType == JsonToken.String)
            {
                if ((string)reader.Value == string.Empty)
                {
                    return decimal.MinValue;
                }
            }
            else if (reader.TokenType == JsonToken.Float ||
                     reader.TokenType == JsonToken.Integer)
            {
                return decimal.Parse(string.Format("{0:N2}", reader.Value));
            }
            throw new NotImplementedException();
        }
    }
}
