using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.VisionaryiGaming
{
    public class BaseInput 
    {
        [JsonProperty(PropertyName = "Method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "ArgumentList")]
        public List<Argument> ArgumentList { get; set; }

        [JsonProperty(PropertyName = "TS")]
        public long TS { get; set; }        
    }

    public class Argument
    {
        [JsonProperty(PropertyName = "Viguser")]
        public string VigUser { get; set; }

        [JsonProperty(PropertyName = "OTP")]
        public string OTP { get; set; }

        [JsonProperty(PropertyName = "siteID")]
        public string SiteID { get; set; }

        [JsonProperty(PropertyName = "Username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "Amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "TransferID")]
        public string TransferID { get; set; }

        [JsonProperty(PropertyName = "TransactionType")]
        public string TransactionType { get; set; }

        [JsonProperty(PropertyName = "GameID")]
        public string GameID { get; set; }

        [JsonProperty(PropertyName = "GameName")]
        public string GameName { get; set; }

        [JsonProperty(PropertyName = "Currency")]
        public string Currency { get; set; } 
        
        [JsonProperty(PropertyName = "tableID")]
        public string TableID { get; set; }
    }
}