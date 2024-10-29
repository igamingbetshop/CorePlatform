using Newtonsoft.Json;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                

namespace IqSoft.CP.PaymentGateway.Models.Spayz
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "order")]
        public OrderModel Order { get; set; }
    }

    public class OrderModel
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "moid")]
        public string Moid { get; set; }


        [JsonProperty(PropertyName = "initialAmount")]
        public AmountModel initialAmount { get; set; }


        [JsonProperty(PropertyName = "actualAmount")]
        public AmountModel ActualAmount { get; set; }

        [JsonProperty(PropertyName = "createdDateTime")]
        public string CreatedDateTime { get; set; }

        [JsonProperty(PropertyName = "authorizationDateTime")]
        public string AuthorizationDateTime { get; set; }
    }

    public class AmountModel
    {
        [JsonProperty(PropertyName = "value")]
        public decimal PValue { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }
    }
}