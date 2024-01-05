using System;

namespace IqSoft.CP.BetShopGatewayWebApi.Models.Reports
{
    public class ApiShift
    {
        public long Id { get; set; }

        public string CashierFirstName { get; set; }

        public string CashierLastName { get; set; }

        public int BetShopId { get; set; }

        public int CashDeskId { get; set; }

        public string BetShopAddress { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public decimal StartAmount { get; set; }

        public decimal BetAmounts { get; set; }

        public decimal PayedWins { get; set; }

        public decimal DepositToInternetClients { get; set; }

        public decimal WithdrawFromInternetClients { get; set; }

        public decimal DebitCorrectionOnCashDesk { get; set; }

        public decimal CreditCorrectionOnCashDesk { get; set; }

        public decimal Balance { get; set; }

        public decimal BonusAmount { get; set; }

        public decimal? EndAmount { get; set; }
    }
}