using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Mpesa
{
    public class PayoutOutput
    {
        [JsonProperty(PropertyName = "statusCode")]
        public string StatusCode { get; set; }

        [JsonProperty(PropertyName = "statusMessage")]
        public string StatusMessage { get; set; }

        [JsonProperty(PropertyName = "statusDescription")]
        public string StatusDescription { get; set; }

        [JsonProperty(PropertyName = "merchantID")]
        public string MerchantID { get; set; }

        [JsonProperty(PropertyName = "retrievalRefNumber")]
        public string RetrievalRefNumber { get; set; }
    }
}