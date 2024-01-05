using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.Interkassa
{
    public class PayoutInput
    {
        [DataMember(Name = "amount")]
        public decimal amount { get; set; }

        [JsonProperty(PropertyName = "method")]
        public string method { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string currency { get; set; }

        [JsonProperty(PropertyName = "useShortAlias")]
        public bool useShortAlias { get; set; }

        //[JsonProperty(PropertyName = "details")]
        //public string details { get; set; }

        [JsonProperty(PropertyName = "purseId")]
        public string purseId { get; set; }

        [JsonProperty(PropertyName = "calcKey")]
        public string calcKey { get; set; }

        [JsonProperty(PropertyName = "action")]
        public string action { get; set; }

        [JsonProperty(PropertyName = "paymentNo")]
        public long paymentNo { get; set; }
    }

    public class Details
    {
        [JsonProperty(PropertyName = "card")]
        public string card { get; set; }

        [JsonProperty(PropertyName = "first_name")]
        public string first_name { get; set; }

        [JsonProperty(PropertyName = "last_name")]
        public string last_name { get; set; }

        [JsonProperty(PropertyName = "phone")]
        public string phone { get; set; }
    }
}
