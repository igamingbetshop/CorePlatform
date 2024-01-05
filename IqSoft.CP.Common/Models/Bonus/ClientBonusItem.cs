using System;

namespace IqSoft.CP.Common.Models.Bonus
{
    public class ClientBonusItem
    {
        public int PartnerId { get; set; }
        public int BonusId { get; set; }
        public int Type { get; set; }
        public int ClientId { get; set; }
        public string ClientUserName { get; set; }
        public string ClientCurrencyId { get; set; }
        public int FinalAccountTypeId { get; set; }
        public decimal? TurnoverAmount { get; set; }
        public int? ReusingMaxCount { get; set; }
        public int? WinAccountTypeId { get; set; }
        public decimal BonusAmount { get; set; }
        public DateTime? ValidForAwarding { get; set; }
        public DateTime? ValidForSpending { get; set; }
        public int TriggerId { get; set; }
    }
}
