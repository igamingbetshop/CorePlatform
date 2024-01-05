using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Interkassa
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "ik_co_id")]
        public string CheckoutId { get; set; }

        [JsonProperty(PropertyName = "ik_co_prs_id")]
        public string CheckoutPurseId { get; set; }

        [JsonProperty(PropertyName = "ik_inv_id")]
        public string InvoiceId { get; set; } 

        [JsonProperty(PropertyName = "ik_inv_st")]
        public string InvoiceState { get; set; }

        [JsonProperty(PropertyName = "ik_inv_crt")]
        public string InvoiceCreated { get; set; }

        [JsonProperty(PropertyName = "ik_inv_prc")]
        public string InvoiceProcessed { get; set; }

        [JsonProperty(PropertyName = "ik_trn_id")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "ik_pm_no")]
        public string PaymentNumber { get; set; }

        [JsonProperty(PropertyName = "ik_payment_method")]
        public string PaymentMethod { get; set; }

        [JsonProperty(PropertyName = "ik_payment_currency")]
        public string PaymentCurrency { get; set; }

        [JsonProperty(PropertyName = "ik_pw_via")]
        public string PaywayVia { get; set; }

        [JsonProperty(PropertyName = "ik_am")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "ik_co_rfn")]
        public string CheckoutRefund { get; set; }

        [JsonProperty(PropertyName = "ik_ps_price")]
        public string PaysystemPrice { get; set; }

        [JsonProperty(PropertyName = "ik_cur")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "ik_desc")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "ik_customer_card_number")]
        public string CardNumber { get; set; }

        [JsonProperty(PropertyName = "ik_sign")]
        public string Signature { get; set; }
    }
}