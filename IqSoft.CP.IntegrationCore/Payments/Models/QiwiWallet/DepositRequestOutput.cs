using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.QiwiWallet
{
    public class DepositRequestOutput
    {
        [JsonProperty(PropertyName = "response")]
        public ResponseData response { get; set; }
    }

    public class ResponseData
    {
        [JsonProperty(PropertyName = "result_code")]
        public int result_code { get; set; }

        [JsonProperty(PropertyName = "bill")]
        public Bill bill { get; set; }
    }

    public class Bill : RequestBase
    {
        [JsonProperty(PropertyName = "bill_id")]
        public string bill_id { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string status { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string error { get; set; }
    }
}
