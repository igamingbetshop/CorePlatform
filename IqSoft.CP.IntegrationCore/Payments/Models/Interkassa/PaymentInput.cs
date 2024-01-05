using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Interkassa
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "ik_co_id")]
        public string ChackoutId { get; set; }

        [JsonProperty(PropertyName = "ik_cur")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "ik_am")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "ik_pm_no")]
        public string PaymentNumber { get; set; }

        [JsonProperty(PropertyName = "ik_desc")]
        public string Descriotion { get; set; }

        [JsonProperty(PropertyName = "ik_payment_method")]
        public string PaymentMethod { get; set; }
        
        [JsonProperty(PropertyName = "ik_payment_currency")]
        public string PaymentCurrency { get; set; }

        [JsonProperty(PropertyName = "ik_sign")]
        public string Sign{ get; set; }

        [JsonProperty(PropertyName = "ik_act")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "ik_int")]
        public string Interface { get; set; }

        [JsonProperty(PropertyName = "ik_mode")]
        public string Mode { get; set; }

        [JsonProperty(PropertyName = "ik_ia_u")]
        public string CallbackUrl { get; set; }

        [JsonProperty(PropertyName = "ik_ia_m")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "ik_suc_u")]
        public string SuccessUrl { get; set; }

        [JsonProperty(PropertyName = "ik_fal_u")]
        public string FailUrl { get; set; }
    }
}
