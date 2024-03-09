using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.XcoinsPay
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "data")]
        public DataModel Data { get; set; }

        [JsonProperty(PropertyName = "statusCode")]
        public int StatusCode { get; set; }
    }
    public class DataModel
    {
        [JsonProperty(PropertyName = "id")]
        public string OrderId { get; set; }
    }
}