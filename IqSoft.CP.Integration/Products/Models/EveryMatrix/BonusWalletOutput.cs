using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.EveryMatrix
{
    public class BonusWalletOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "lockedAmount")]
        public decimal LockedAmount { get; set; }

        [JsonProperty(PropertyName = "bonusWalletType")]
        public Dictionary<string, BonusWallet> BonusWallets { get; set; }
    }

    public class BonusWallet
    {
        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "grantedBonusAmount")]
        public decimal GrantedBonusAmount { get; set; }
    }
}
