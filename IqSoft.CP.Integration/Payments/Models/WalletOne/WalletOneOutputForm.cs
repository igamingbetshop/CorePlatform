using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.WalletOne
{
    public class WalletOneOutputForm
    {
        [JsonProperty(PropertyName = "FormId")]
        public string FormId { get; set; }

        [JsonProperty(PropertyName = "Title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "Description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "Fields")]
        public WalletOneFormFields[] Fields { get; set; }

        [JsonProperty(PropertyName = "MasterFieldId")]
        public string MasterFieldId { get; set; }
    }
}
