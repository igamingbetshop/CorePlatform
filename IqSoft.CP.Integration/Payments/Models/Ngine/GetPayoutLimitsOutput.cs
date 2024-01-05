using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Models.Ngine
{
    public class LimitsAuthentication
    {
        [JsonProperty(PropertyName = "IconURL")]
        public string IconURL { get; set; }

        [JsonProperty(PropertyName = "ImageName")]
        public string ImageName { get; set; }

        [JsonProperty(PropertyName = "PayoutName")]
        public string PayoutName { get; set; }

        [JsonProperty(PropertyName = "PayoutID")]
        public string PayoutID { get; set; }

        [JsonProperty(PropertyName = "PaymentType")]
        public string PaymentType { get; set; }

        [JsonProperty(PropertyName = "MaxPayout")]
        public decimal MaxPayout { get; set; }

        [JsonProperty(PropertyName = "MinPayout")]
        public decimal MinPayout { get; set; }

        [JsonProperty(PropertyName = "ProcessorID")]
        public int ProcessorID { get; set; }
    }

    public class GetPayoutLimitsOutput
    {
        [JsonProperty(PropertyName = "Authentication")]
        public List<LimitsAuthentication> Authentication { get; set; }
    }
}
