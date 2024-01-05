using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Platforms.Models.CashCenter
{
    public class WalletResult
    {
        [JsonProperty(PropertyName = "errors")]
        public List<string> Errors { get; set; }

        [JsonProperty(PropertyName = "result")]
        public UserWallet UserWalletInfo { get; set; }
    }

    public class UserWallet
    {
        [JsonProperty(PropertyName = "refId")]
        public string RefId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currencyId")]
        public string CurrencyId { get; set; }

        [JsonProperty(PropertyName = "available")]
        public decimal AvailableBalance { get; set; }

        [JsonProperty(PropertyName = "balanceBefore")]
        public decimal BalanceBefore { get; set; }

        [JsonProperty(PropertyName = "balanceAfter")]
        public decimal BalanceAfter { get; set; }

        [JsonProperty(PropertyName = "isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonProperty(PropertyName = "previouslyProcessed")]
        public bool PreviouslyProcessed { get; set; }
    }
}
