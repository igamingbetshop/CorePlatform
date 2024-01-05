using Newtonsoft.Json;
using System;

namespace IqSoft.CP.ProductGateway.Models.ISoftBet
{
    public class BaseInput
    {

        [JsonProperty(PropertyName = "allow_open_rounds")]
        public bool? AllowOpenRounds { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string CurrencyId { get; set; }

        [JsonProperty(PropertyName = "sessionid")]
        public string SessionId { get; set; }

        /// <summary>
        /// iSoftBet unique game id, optional for method depositmoney.
        /// </summary>
        [JsonProperty(PropertyName = "skinid")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "playerid")]
        public string PlayerId { get; set; }

        [JsonProperty(PropertyName = "operator")]
        public string Operator { get; set; }


        [JsonProperty(PropertyName = "multiplier")]
        public string Multiplier { get; set; }

        [JsonProperty(PropertyName = "action")]
        public ActionType Action { get; set; }

        [JsonProperty(PropertyName = "actions")]
        public ActionType[] Actions { get; set; }
        /// <summary>
        /// Possible value: "single", "multi"
        /// </summary>
        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }
    }

    public class ActionType
    {
        [JsonProperty(PropertyName = "command")]
        public string Command { get; set; }

        [JsonProperty(PropertyName = "parameters")]
        public object Parameters { get; set; }
    }

    public class Parameter
    {
        [JsonProperty(PropertyName = "transactionid")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "roundid")]
        public string RoundId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        [JsonConverter(typeof(DecimalJsonConverter))]
        public decimal? Amount { get; set; }
        /// <summary>
        /// with 10 decimals, in cents
        /// </summary>
        [JsonProperty(PropertyName = "jpc")]
        [JsonConverter(typeof(DecimalJsonConverter))]
        public decimal? JackpotContribution { get; set; }

        [JsonProperty(PropertyName = "jpw")]
        [JsonConverter(typeof(DecimalJsonConverter))]
        public decimal? WinJackpotContribution { get; set; }

        [JsonProperty(PropertyName = "jpw_from_jpc")]
        [JsonConverter(typeof(DecimalJsonConverter))]
        public decimal? JpwFromJpc { get; set; }

        [JsonProperty(PropertyName = "closeround")]
        public bool? CloseRound { get; set; }

        [JsonProperty(PropertyName = "froundid")]
        public int? FroundId { get; set; }

		[JsonProperty(PropertyName = "fround_coin_value")]
		public int? FroundCoinValue { get; set; }

		[JsonProperty(PropertyName = "fround_lines")]
		public int? FroundLines { get; set; }

		[JsonProperty(PropertyName = "fround_line_bet")]
		public int? FroundLineBet { get; set; }

		[JsonProperty(PropertyName = "fround_campaignid")]
		public int? FroundCampaignId { get; set; }
		
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "game_type")]
        public string GameType { get; set; }

        [JsonProperty(PropertyName = "sessionstatus")]
        public string SessionStatus { get; set; }
    }

    public class DecimalJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(decimal));
        }

        public override object ReadJson(JsonReader reader, Type objectType,
                                        object existingValue, JsonSerializer serializer)
        {
            if(reader.TokenType == JsonToken.Null)
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
                return Convert.ToDecimal(reader.Value);
            }

            throw new JsonSerializationException("Unexpected token type: " +
                                                 reader.TokenType.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value,
                                       JsonSerializer serializer)
        {
            decimal dec = (decimal)value;
            if (dec == decimal.MinValue)
            {
                writer.WriteValue(string.Empty);
            }
            else
            {
                long iValue = (long)dec;
                if (dec > iValue)
                    writer.WriteValue(dec);
                else
					writer.WriteValue(iValue);
            }
        }
    }
}