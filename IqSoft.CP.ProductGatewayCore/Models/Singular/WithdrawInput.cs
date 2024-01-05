using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Singular
{
    public class WithdrawInput : BaseInput
    {
        [JsonProperty(PropertyName = "userID")]
        public long UserId { get; set; }

        [JsonProperty(PropertyName = "currencyID")]
        public string CurrencyId { get; set; }

        [JsonProperty(PropertyName = "Amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "shouldWaitForApproval")]
        public bool ShouldWaitForApproval { get; set; }

        [JsonProperty(PropertyName = "providerUserID")]
        public string ProviderUserId { get; set; }      //Always NULL

        [JsonProperty(PropertyName = "providerServiceID")]
        public int? ProviderServiceId { get; set; }      //Always NULL

        [JsonProperty(PropertyName = "providerOppCode")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "additionalData")]
        public string AdditionalData { get; set; }

        [JsonProperty(PropertyName = "providerStatusCode")]
        public string ProviderStatusCode { get; set; }

        [JsonProperty(PropertyName = "statusNote")]
        public string StatusNote { get; set; }          //Always NULL
    }
}