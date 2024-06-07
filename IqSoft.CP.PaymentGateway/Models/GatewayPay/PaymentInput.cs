using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.GatewayPay
{
    public class PaymentInput
    {

        [JsonProperty(PropertyName = "responseCode")]
        public int ResponseCode { get; set; }

        [JsonProperty(PropertyName = "responseMessage")]
        public string ResponseMessage { get; set; }

        [JsonProperty(PropertyName = "data")]
        public DataModel Data { get; set; }
    }

    public class DataModel
    {
        [JsonProperty(PropertyName = "transaction")]
        public TransactionModel Transaction { get; set; }

        [JsonProperty(PropertyName = "card")]
        public CardModel Card { get; set; }
    }

    public class TransactionModel
    {
        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "customer_order_id")]
        public string CustomerOrderId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }
    }

    public class CardModel
    {
        [JsonProperty(PropertyName = "card_no")]
        public string CardNo { get; set; }

        [JsonProperty(PropertyName = "ccExpiryMonth")]
        public string ExpiryMonth { get; set; }

        [JsonProperty(PropertyName = "ccExpiryYear")]
        public string ExpiryYear { get; set; }
    }
}