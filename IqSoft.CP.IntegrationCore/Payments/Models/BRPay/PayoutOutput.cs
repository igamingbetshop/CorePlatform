using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.BRPay
{
    public class PayoutOutput
    {
        [JsonProperty(PropertyName = "sp_salt")]
        public string Salt { get; set; }

        [JsonProperty(PropertyName = "sp_status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "sp_transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "sp_sig")]
        public string Signature { get; set; }

        [JsonProperty(PropertyName = "sp_error_code")]
        public int ErrorCode { get; set; }

        [JsonProperty(PropertyName = "sp_error_message")]
        public string ErrorMessage { get; set; } 
        
        [JsonProperty(PropertyName = "sp_message ")]
        public string Message { get; set; }
    }
}
