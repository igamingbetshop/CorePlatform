using IqSoft.CP.AdminWebApi.Models.BonusModels;
using IqSoft.CP.Common.Attributes;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.ClientModels
{
    public class ApiClientBonuses
    {
        public long Count { get; set; }

        public List<ApiClientBonus> Entities { get; set; }
    }

    public class ApiClientBonus
    {
		public int Id { get; set; }
        public int PartnerId { get; set; }
        public int ClientId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CurrencyId { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public int BonusId { get; set; }
        public string BonusName { get; set; }
        public int BonusType { get; set; }
        public int Status { get; set; }
        public decimal BonusPrize { get; set; }
        public decimal? SpinsCount { get; set; }
        public decimal? TurnoverAmountLeft { get; set; }
        public decimal? FinalAmount { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? AwardingTime { get; set; }
        public DateTime? CalculationTime { get; set; }
        public DateTime? ValidUntil { get; set; }
        public long? ReuseNumber { get; set; }
        public decimal? RemainingCredit { get; set; }

        [NotExcelProperty]
        public decimal? WageringTarget { get; set; }
        [NotExcelProperty]
        public int? TriggerId { get; set; }
        [NotExcelProperty]
        public int? BetCount { get; set; }
        [NotExcelProperty]
        public List<ApiTriggerSetting> TriggerSettings { get; set; }

	}
}