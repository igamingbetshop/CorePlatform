using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Flexepin
{
    public class SwapOutput
    {
        [JsonProperty(PropertyName = "quote_trans_no")]
        public string TransNo { get; set; }

        [JsonProperty(PropertyName = "result")]
        public int Result { get; set; }

        [JsonProperty(PropertyName = "result_description")]
        public string ResultDescription { get; set; }

        [JsonProperty(PropertyName = "source_product_code")]
        public string SourceProductCode { get; set; }

        [JsonProperty(PropertyName = "source_product_currency")]
        public string SourceProductCurrency { get; set; }

        [JsonProperty(PropertyName = "source_product_value")]
        public string SourceProductValue { get; set; }

        [JsonProperty(PropertyName = "source_product_quantity")]
        public string SourceProductQuantity { get; set; }

        [JsonProperty(PropertyName = "destination_product_code")]
        public string DestinationProductcode { get; set; }

        [JsonProperty(PropertyName = "destination_product_currency")]
        public string DestinationProductCurrency { get; set; }

        [JsonProperty(PropertyName = "destination_product_value")]
        public string DestinationProductValue { get; set; }

        [JsonProperty(PropertyName = "destination_product_quantity")]
        public string DestinationProductQuantity { get; set; }

        [JsonProperty(PropertyName = "expiry_time")]
        public string ExpiryTime { get; set; }
    }
}
