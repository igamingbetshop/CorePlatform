using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.PaymentGateway.Models.Ngine
{
    public class LimitsAuthentication
    {
        [JsonProperty(PropertyName = "IconURL")]
        public string IconURL { get; set; }

        [JsonProperty(PropertyName = "ImageName")]
        public string ImageName { get; set; }

        [JsonProperty(PropertyName = "DepositName")]
        public string DepositName { get; set; }

        [JsonProperty(PropertyName = "DepositID")]
        public string DepositID { get; set; }

        [JsonProperty(PropertyName = "PaymentType")]
        public string PaymentType { get; set; }

        [JsonProperty(PropertyName = "MaxDeposit")]
        public decimal MaxDeposit { get; set; }

        [JsonProperty(PropertyName = "MinDeposit")]
        public decimal MinDeposit { get; set; }

        [JsonProperty(PropertyName = "CreditCardTypeID")]
        public string CreditCardTypeID { get; set; }

        [JsonProperty(PropertyName = "ProcessorID")]
        public int ProcessorID { get; set; }
    }

    public class GetDepositLimitsOutput
    {
        [JsonProperty(PropertyName = "Authentication")]
        public List<LimitsAuthentication> Authentication { get; set; }
    }
}
