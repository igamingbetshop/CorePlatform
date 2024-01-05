using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Habanero
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "playerdetailresponse")]
        public PlayerDetailOutput PlayerDetails { get; set; }

        [JsonProperty(PropertyName = "fundtransferresponse")]
        public FundTransferOutput FundTransferDetails { get; set; }
    }

    public class FundTransferOutput
    {
        [JsonProperty(PropertyName = "status")]
        public StatusOutput StatusDetails { get; set; }

        [JsonProperty(PropertyName = "accountid")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "accountname")]
        public string AccountName { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public decimal? Balance { get; set; }

        [JsonProperty(PropertyName = "currencycode")]
        public string CurrencyCode { get; set; }
    }

    public class PlayerDetailOutput
    {
        [JsonProperty(PropertyName = "status")]
        public StatusOutput StatusDetails { get; set; }

        [JsonProperty(PropertyName = "accountid")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "accountname")]
        public string AccountName { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "currencycode")]
        public string CurrencyCode { get; set; }
    }
    public class StatusOutput
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "autherror")]
        public bool Autherror { get; set; }

        [JsonProperty(PropertyName = "nofunds")]
        public bool? NoFunds { get; set; }

        [JsonProperty(PropertyName = "refundstatus")]
        public int? RefundStatus { get; set; }

        [JsonProperty(PropertyName = "successdebit")]
        public bool? SuccessDebit { get; set; }

        [JsonProperty(PropertyName = "successcredit")]
        public bool? SuccessCredit { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}