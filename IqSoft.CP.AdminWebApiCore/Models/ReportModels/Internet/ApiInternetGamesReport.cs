using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.ReportModels.Internet
{
    public class ApiInternetGamesReport
    {
        public decimal TotalBetAmount { get; set; }

        public decimal TotalWinAmount { get; set; }

        public decimal TotalProfit { get; set; }

        public int TotalBetCount { get; set; }

        public decimal TotalSupplierFee { get; set; }

        public List<ApiInternetGame> Entities { get; set; }
    }
}