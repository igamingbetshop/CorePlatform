using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Habanero
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "playerdetailresponse", NullValueHandling = NullValueHandling.Ignore)]
        public PlayerDetailOutput PlayerDetails { get; set; }

        [JsonProperty(PropertyName = "fundtransferresponse", NullValueHandling = NullValueHandling.Ignore)]
        public FundTransferOutput FundTransferDetails { get; set; }
    }

    public class FundTransferOutput
    {
        [JsonProperty(PropertyName = "status", NullValueHandling = NullValueHandling.Ignore)]
        public StatusOutput StatusDetails { get; set; }

        [JsonProperty(PropertyName = "accountid", NullValueHandling = NullValueHandling.Ignore)]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "accountname", NullValueHandling = NullValueHandling.Ignore)]
        public string AccountName { get; set; }

        [JsonProperty(PropertyName = "balance", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Balance { get; set; }

        [JsonProperty(PropertyName = "currencycode", NullValueHandling = NullValueHandling.Ignore)]
        public string CurrencyCode { get; set; }
    }

    public class PlayerDetailOutput
    {
        [JsonProperty(PropertyName = "status", NullValueHandling = NullValueHandling.Ignore)]
        public StatusOutput StatusDetails { get; set; }

        [JsonProperty(PropertyName = "accountid", NullValueHandling = NullValueHandling.Ignore)]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "accountname", NullValueHandling = NullValueHandling.Ignore)]
        public string AccountName { get; set; }

        [JsonProperty(PropertyName = "balance", NullValueHandling = NullValueHandling.Ignore)]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "currencycode", NullValueHandling = NullValueHandling.Ignore)]
        public string CurrencyCode { get; set; }
    }
    public class StatusOutput
    {
        [JsonProperty(PropertyName = "success", NullValueHandling = NullValueHandling.Ignore)]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "autherror", NullValueHandling = NullValueHandling.Ignore)]
        public bool Autherror { get; set; }

        [JsonProperty(PropertyName = "nofunds", NullValueHandling = NullValueHandling.Ignore)]
        public bool? NoFunds { get; set; }

        [JsonProperty(PropertyName = "refundstatus", NullValueHandling = NullValueHandling.Ignore)]
        public int? RefundStatus { get; set; }

        [JsonProperty(PropertyName = "successdebit", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SuccessDebit { get; set; }

        [JsonProperty(PropertyName = "successcredit", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SuccessCredit { get; set; }

        [JsonProperty(PropertyName = "message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }
    }
}