using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Flexepin
{
    public class BaseData
    {
        [JsonProperty(PropertyName = "resultCode")]
        public int ResultCode { get; set; } = 0;

        [JsonProperty(PropertyName = "resultDescription")]
        public string ResultDescription { get; set; } = "Success";
    }
}