using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Neteller
{
    public class PayoutInput
    {
        [JsonProperty("payeeProfile")]
        public Profile PayeeProfile { get; set; }

        [JsonProperty("transaction")]
        public Transaction TransactionData { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class Transaction
    {
        [JsonProperty("merchantRefId")]
        public string MerchantRefId { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("currency")]
        public string CurrencyId { get; set; }

        [JsonProperty("id")]
        public string ExternalTransactionId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }


    }

    public class Profile
    {
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
