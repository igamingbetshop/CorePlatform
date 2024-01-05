using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.PaySec
{
    class PayoutRequestInput
    {
        [JsonProperty(PropertyName = "merchantCode")]
        public string MerchantCode { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "bankCode")]
        public string BankCode { get; set; }

        [JsonProperty(PropertyName = "bankName")]
        public string BankName { get; set; }

        [JsonProperty(PropertyName = "bankBranch")]
        public string BankBranch { get; set; }

        /// <summary>
        /// Account Holder Name
        /// For CNY transaction, must be in Simplified Chinese.
        /// For other currency, must be in English Language.
        /// </summary>
        [JsonProperty(PropertyName = "customerName")]
        public string CustomerName { get; set; }

        /// <summary>
        /// Bank Account Name 
        /// For CNY transaction, must be in Simplified Chinese.
        /// For other currency, must be in English Language.
        /// </summary>
        [JsonProperty(PropertyName = "bankAccountName")]
        public string BankAccountName { get; set; }

        [JsonProperty(PropertyName = "bankAccountNumber")]
        public string BankAccountNumber { get; set; }

        [JsonProperty(PropertyName = "cartId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "province")]
        public string Province { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "notifyURL")]
        public string NotifyURL { get; set; }

        [JsonProperty(PropertyName = "signature")]
        public string Signature { get; set; }
    }
}
