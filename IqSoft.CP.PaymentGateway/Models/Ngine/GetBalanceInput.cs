using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Ngine
{
    public class GetBalance
    {
        [JsonProperty(PropertyName = "CustPIN")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "CustPassword")]
        public string ClientId { get; set; }
    }

    public class GetBalanceInput
    {
        [JsonProperty(PropertyName = "GetBalance")]
        public GetBalance GetBalance { get; set; }
    }
}