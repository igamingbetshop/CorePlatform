using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Payments.Models.Pay3000
{
	public class ConsentOutput
    {
        [JsonProperty(PropertyName = "consentId")]
        public string ConsentId { get; set; }

        [JsonProperty(PropertyName = "createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty(PropertyName = "updatedDate")]
        public DateTime UpdatedDate { get; set; }

        [JsonProperty(PropertyName = "customerAccountNumber")]
        public string CustomerAccountNumber { get; set; }

        [JsonProperty(PropertyName = "merchantTitle")]
        public string MerchantTitle { get; set; }

        [JsonProperty(PropertyName = "merchantAccountNumber")]
        public string MerchantAccountNumber { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "expires")]
        public DateTime Expires { get; set; }

        [JsonProperty(PropertyName = "acceptUrl")]
        public string AcceptUrl { get; set; }

        [JsonProperty(PropertyName = "rejectUrl")]
        public string RejectUrl { get; set; }

        [JsonProperty(PropertyName = "failureUrl")]
        public string FailureUrl { get; set; }
    }
}
