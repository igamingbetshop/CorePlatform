using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.CashLib
{
    public class VoucherOutput
    {
        [JsonProperty(PropertyName = "transaction_id")]
        public string MerchantTransactionId { get; set; }

        [JsonProperty(PropertyName = "transaction_reference")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "iframe_url")]
        public string RedirectUrl { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "error_message")]
        public string ErrorMessage { get; set; }
    }
}
