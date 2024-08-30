using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Mpesa
{
    public class PayoutInput
    {
        [JsonProperty(PropertyName = "ftReference")]
        public string FtReference { get; set; }

        [JsonProperty(PropertyName = "transactionDate")]
        public string TransactionDate { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "transactionStatus")]
        public string TransactionStatus { get; set; }

        [JsonProperty(PropertyName = "transactionMessage")]
        public string TransactionMessage { get; set; }

        [JsonProperty(PropertyName = "beneficiaryAccountNumber")]
        public string BeneficiaryAccountNumber { get; set; }

        [JsonProperty(PropertyName = "beneficiaryName")]
        public string BeneficiaryName { get; set; }

        [JsonProperty(PropertyName = "transactionReference")]
        public string TransactionReference { get; set; }

        [JsonProperty(PropertyName = "merchantId")]
        public string MerchantId { get; set; }

        [JsonProperty(PropertyName = "debitAccountNumber")]
        public string DebitAccountNumber { get; set; }
    }
}