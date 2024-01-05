using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.BRPay
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "sp_outlet_id")]
        public long OutletId { get; set; }

        [JsonProperty(PropertyName = "sp_order_id")]
        public long OrderId { get; set; }

        [JsonProperty(PropertyName = "sp_amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "sp_payment_system")]
        public string PaymentSystem { get; set; }

        [JsonProperty(PropertyName = "sp_description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "sp_user_name")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "sp_user_phone")]
        public string UserPhone { get; set; }

        [JsonProperty(PropertyName = "sp_user_contact_email")]
        public string Email { get; set; }  
        
        [JsonProperty(PropertyName = "sp_user_ip")]
        public string UserIp { get; set; }

        [JsonProperty(PropertyName = "sp_failure_url")]
        public string FailureUrl { get; set; }

        [JsonProperty(PropertyName = "sp_success_url")]
        public string SuccessUrl { get; set; }

        [JsonProperty(PropertyName = "sp_result_url")]
        public string ResultUrl { get; set; }

        [JsonProperty(PropertyName = "sp_salt")]
        public string Salt { get; set; } 
        
        [JsonProperty(PropertyName = "sp_sig")]
        public string Signature { get; set; }
    }
}
