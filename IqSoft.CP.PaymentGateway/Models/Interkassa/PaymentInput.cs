using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Interkassa
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "ik_co_id")]
        public string ik_co_id { get; set; }

        [JsonProperty(PropertyName = "ik_co_prs_id")]
        public string ik_co_prs_id { get; set; }

        [JsonProperty(PropertyName = "ik_inv_id")]
        public string ik_inv_id { get; set; } 

        [JsonProperty(PropertyName = "ik_inv_st")]
        public string ik_inv_st { get; set; }

        [JsonProperty(PropertyName = "ik_inv_crt")]
        public string ik_inv_crt { get; set; }

        [JsonProperty(PropertyName = "ik_inv_prc")]
        public string ik_inv_prc { get; set; }

        [JsonProperty(PropertyName = "ik_trn_id")]
        public string ik_trn_id { get; set; }

        [JsonProperty(PropertyName = "ik_pm_no")]
        public string ik_pm_no { get; set; }

        [JsonProperty(PropertyName = "ik_payment_method")]
        public string ik_payment_method { get; set; }

        [JsonProperty(PropertyName = "ik_payment_currency")]
        public string ik_payment_currency { get; set; }

        [JsonProperty(PropertyName = "ik_pw_via")]
        public string ik_pw_via { get; set; }

        [JsonProperty(PropertyName = "ik_am")]
        public string ik_am { get; set; }

        [JsonProperty(PropertyName = "ik_co_rfn")]
        public string ik_co_rfn { get; set; }

        [JsonProperty(PropertyName = "ik_ps_price")]
        public string ik_ps_price { get; set; }

        [JsonProperty(PropertyName = "ik_cur")]
        public string ik_cur { get; set; }

        [JsonProperty(PropertyName = "ik_desc")]
        public string ik_desc { get; set; }

        [JsonProperty(PropertyName = "ik_p_gw_order_id")]
        public string ik_p_gw_order_id { get; set; }

        [JsonProperty(PropertyName = "ik_p_eci")]
        public string ik_p_eci { get; set; }

        [JsonProperty(PropertyName = "ik_p_card_mask")]
        public string ik_p_card_mask { get; set; }

        [JsonProperty(PropertyName = "ik_p_card_token")]
        public string ik_p_card_token { get; set; }

        [JsonProperty(PropertyName = "ik_customer_first_name")]
        public string ik_customer_first_name { get; set; }

        [JsonProperty(PropertyName = "ik_customer_last_name")]
        public string ik_customer_last_name { get; set; }

        [JsonProperty(PropertyName = "ik_customer_email")]
        public string ik_customer_email { get; set; }

        [JsonProperty(PropertyName = "ik_sign")]
        public string ik_sign { get; set; }
    }
}