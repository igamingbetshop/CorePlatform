using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.WalletOne
{
    public class WalletOneFormFields
    {
        [JsonProperty(PropertyName = "MinLength")]
        public int MinLength { get; set; }

        [JsonProperty(PropertyName = "MaxLength")]
        public int MaxLength { get; set; }

        [JsonProperty(PropertyName = "RegEx")]
        public string RegEx { get; set; }

        [JsonProperty(PropertyName = "Title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "TabOrder")]
        public int TabOrder { get; set; }

        [JsonProperty(PropertyName = "FieldType")]
        public string FieldType { get; set; }

        [JsonProperty(PropertyName = "InputMask")]
        public string InputMask { get; set; }

        [JsonProperty(PropertyName = "Description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "IsRequired")]
        public string IsRequired { get; set; }

        [JsonProperty(PropertyName = "IsHidden")]
        public string IsHidden { get; set; }

        [JsonProperty(PropertyName = "DefaultValue")]
        public string DefaultValue { get; set; }

        [JsonProperty(PropertyName = "Example")]
        public string Example { get; set; }

        [JsonProperty(PropertyName = "FieldId")]
        public string FieldId { get; set; }

        [JsonProperty(PropertyName = "Value")]
        public string Value { get; set; }
    }
}
