using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Ngine
{
    public class CustomerInfoInput
    {
        [JsonProperty(PropertyName = "GetCustomerInfo")]
        public GetCustomerInfo GetCustomerInfo { get; set; }        
    }

    public class GetCustomerInfo
    {
        [JsonProperty(PropertyName = "CustPIN")]
        public string RequestId { get; set; }

        [JsonProperty(PropertyName = "CustPassword")]
        public string ClientId { get; set; }
    }
}