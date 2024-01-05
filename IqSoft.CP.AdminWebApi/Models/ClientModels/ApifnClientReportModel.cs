using IqSoft.CP.Common.Attributes;
using System;

namespace IqSoft.CP.AdminWebApi.Models.ClientModels
{
    public class ApifnClientReportModel
    {
        public int ClientId { get; set; }
        public int PartnerId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int CategoryId { get; set; }
        public string CurrencyId { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal DepositAmount { get; set; }
        public Nullable<int> DepositCount { get; set; }
        public decimal WithdrawAmount { get; set; }
        public Nullable<int> WithdrawCount { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PossibleWinAmount { get; set; }
        public Nullable<int> BetCount { get; set; }
        public decimal WinAmount { get; set; }
        public Nullable<int> WinCount { get; set; }
        public int Bonuses { get; set; }
        public int BonusesCount { get; set; }
        public decimal CorrectionAmount { get; set; }
        public decimal BalanceAfter { get; set; }

        [NotExcelProperty]
        public bool HasNote { get; set; }
    }
}