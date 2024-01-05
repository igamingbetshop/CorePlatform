using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.Ezugi
{
    public class InitializeSessionInput
    {
        public string MessageType { get; set; }

        [JsonProperty(PropertyName = "OperatorID")]
        public int OperatorId { get; set; }

        [JsonProperty(PropertyName= "vipLevel")]
        public int VipLevel { get; set; }

        public string SessionCurrency { get; set; }
    }
}
