using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.WalletOne
{
     public class WalletOneFormBaseFields
    {
        [JsonProperty(PropertyName = "FieldId")]
        public string FieldId { get; set; }

        [JsonProperty(PropertyName = "Value")]
        public string Value { get; set; }
    }
}
