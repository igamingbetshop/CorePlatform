using System;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class ApiCloseShiftOutput : ApiResponseBase
    {
        public int Id { get; set; }

        public string CashierFirstName { get; set; }

        public string CashierLastName { get; set; }

        public int BetShopId { get; set; }

        public int CashDeskId { get; set; }

        public string BetShopAddress { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public decimal StartAmount { get; set; }

        public decimal? EndAmount { get; set; }

        public decimal? BetAmount { get; set; }
        
        public decimal? PayedWin { get; set; }
        
        public decimal? DepositToInternetClient { get; set; }
        
        public decimal? WithdrawFromInternetClient { get; set; }
        
        public decimal? DebitCorrectionOnCashDesk { get; set; }
        
        public decimal? CreditCorrectionOnCashDesk { get; set; }

        public decimal Balance { get; set; }

        public decimal BonusAmount { get; set; }
    }
}