using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.GatewayPay
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "responseCode")]
        public int ResponseCode { get; set; }

        [JsonProperty(PropertyName = "responseMessage")]
        public string ResponseMessage { get; set; }

        [JsonProperty(PropertyName = "3dsUrl")]
        public string TreeDsUrl { get; set; }

        [JsonProperty(PropertyName = "data")]
        public DataModel Data { get; set; }
    }

    public class DataModel
    {
        [JsonProperty(PropertyName = "transaction")]
        public TransactionModel Transaction { get; set; }
    }

    public class TransactionModel
    {
        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "customer_order_id")]
        public string CustomerOrderId { get; set; }
    }
}
