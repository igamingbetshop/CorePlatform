using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.EveryMatrix
{
    public class BonusListOutput
    {
        [JsonProperty(PropertyName = "totalRecords")]
        public int TotalRecords { get; set; }

        [JsonProperty(PropertyName = "wallets")]
        public List<WalletModel> Wallets { get; set; }
    }

    public class WalletModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "bonusID")]
        public string BonusID { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "lockedAmount")]
        public decimal LockedAmount { get; set; }

        [JsonProperty(PropertyName = "grantedBonusAmount")]
        public decimal GrantedBonusAmount { get; set; }

        [JsonProperty(PropertyName = "fulfilledWR")]
        public decimal FulfilledWR { get; set; }

        [JsonProperty(PropertyName = "ordinal")]
        public int Ordinal { get; set; }

        [JsonProperty(PropertyName = "incompleteBets")]
        public int IncompleteBets { get; set; }

        [JsonProperty(PropertyName = "extension")]
        public ExtensionModel Extension { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "ins")]
        public DateTime Ins { get; set; }
    }

    public class ExtensionModel
    {
        [JsonProperty(PropertyName = "bonusWalletID")]
        public string BonusWalletID { get; set; }

        [JsonProperty(PropertyName = "domainID")]
        public int DomainID { get; set; }

        [JsonProperty(PropertyName = "realm")]
        public string Realm { get; set; }

        [JsonProperty(PropertyName = "bonusID")]
        public string BonusID { get; set; }

        [JsonProperty(PropertyName = "userID")]
        public string UserID { get; set; }

        [JsonProperty(PropertyName = "bonus")]
        public Bonus Bonus { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "totalWR")]
        public decimal TotalWR { get; set; }

        [JsonProperty(PropertyName = "initialLockedAmount")]
        public decimal InitialLockedAmount { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "ins")]
        public DateTime Ins { get; set; }
    }

    public class Bonus
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "ins")]
        public DateTime Ins { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }
    }
}