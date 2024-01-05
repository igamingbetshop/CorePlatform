using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Ezugi
{
    public class RollbackInput : GeneralInput
    {
        [JsonProperty(PropertyName = "rollbackAmount")]
        public decimal RollbackAmount { get; set; }
    }
}