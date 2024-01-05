using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.Habanero
{
    public class TransferInput
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "partnermeta")]
        public string PartnerMeta { get; set; }

        [JsonProperty(PropertyName = "accountid")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "customplayertype")]
        public int CustomPlayerType { get; set; }

        [JsonProperty(PropertyName = "gameinstanceid")]
        public string GameInstanceId { get; set; }

        [JsonProperty(PropertyName = "friendlygameinstanceid")]
        public long FriendlyGameInstanceId { get; set; }

        [JsonProperty(PropertyName = "isretry")]
        public bool IsRetry { get; set; }

        [JsonProperty(PropertyName = "RetryCount")]
        public string RetryCount { get; set; }

        [JsonProperty(PropertyName = "isrefund")]
        public bool IsRefund { get; set; }

        [JsonProperty(PropertyName = "isrecredit")]
        public bool IsRecredit { get; set; }

        [JsonProperty(PropertyName = "funds")]
        public Fund Funds { get; set; }
    }

    public class Fund
    {
        [JsonProperty(PropertyName = "debitandcredit")]
        public bool DebitAndCredit { get; set; }

        [JsonProperty(PropertyName = "fundinfo")]
        public List<FundInfo> FundInfoDetails { get; set; }

        [JsonProperty(PropertyName = "refund")]
        public Refund RefundDetails { get; set; }
        
        [JsonProperty(PropertyName = "transferid")]
        public string TransferId { get; set; }

        [JsonProperty(PropertyName = "initialdebittransferid")]
        public string InitialDebitTransferId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "accountid")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "gamestatemode")]
        public int GameStatemode { get; set; }
    }

    public class FundInfo
    {
        [JsonProperty(PropertyName = "gamestatemode")]
        public int GameStatemode { get; set; }

        [JsonProperty(PropertyName = "transferid")]
        public string TransferId { get; set; }

        [JsonProperty(PropertyName = "currencycode")]
        public string CurrencyCode { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "bonusamount")]
        public decimal BonusAmount { get; set; }

        [JsonProperty(PropertyName = "jpwin")]
        public bool JpWin { get; set; }

        [JsonProperty(PropertyName = "jpcont")]
        public int JpCont { get; set; }

        [JsonProperty(PropertyName = "isbonus")]
        public bool IsBonus { get; set; }

        [JsonProperty(PropertyName = "initialdebittransferid")]
        public string InitialDebitTransferId { get; set; }

        [JsonProperty(PropertyName = "accounttransactiontype")]
        public int AccountTransactionType { get; set; }

        [JsonProperty(PropertyName = "gameinfeature")]
        public bool GameInFeature { get; set; }

        [JsonProperty(PropertyName = "lastbonusaction")]
        public bool LastBonusAction { get; set; }
    }

    public class Refund
    {
        [JsonProperty(PropertyName = "gamestatemode")]
        public int GameStatemode { get; set; }

        [JsonProperty(PropertyName = "originaltransferid")]
        public string OriginalTransferId { get; set; }

        [JsonProperty(PropertyName = "transferid")]
        public string TransferId { get; set; }

        [JsonProperty(PropertyName = "currencycode")]
        public string CurrencyCode { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "bonusamount")]
        public decimal BonusAmount { get; set; }

        [JsonProperty(PropertyName = "jpwin")]
        public bool JpWin { get; set; }

        [JsonProperty(PropertyName = "jpid")]
        public string JpId { get; set; }

        [JsonProperty(PropertyName = "jpcont")]
        public int JpCont { get; set; }

        [JsonProperty(PropertyName = "isbonus")]
        public bool IsBonus { get; set; }

        [JsonProperty(PropertyName = "initialdebittransferid")]
        public string InitialDebitTransferId { get; set; }

        [JsonProperty(PropertyName = "accounttransactiontype")]
        public int AccountTransactionType { get; set; }

        [JsonProperty(PropertyName = "gameinfeature")]
        public bool GameInFeature { get; set; }

        [JsonProperty(PropertyName = "lastbonusaction")]
        public bool LastBonusAction { get; set; }
    }
}