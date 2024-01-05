using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Qiwi
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

    public class Bill 
    {
        [JsonProperty(PropertyName = "bill_id")]
        public string bill_id { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string status { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string error { get; set; }

        public string amount { get; set; }
        /// <summary>
        /// currensy id
        /// </summary>
        public string ccy { get; set; }

        public string user { get; set; }

        public string comment { get; set; }
    }
}
