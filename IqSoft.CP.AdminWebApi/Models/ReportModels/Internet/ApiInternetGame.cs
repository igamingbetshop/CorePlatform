namespace IqSoft.CP.AdminWebApi.Models.ReportModels.Internet
{
    public class ApiInternetGame
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public int? ProviderId { get; set; }

        public string ProviderName { get; set; }

        public int? SubproviderId { get; set; }

        public string SubproviderName { get; set; }
        public string CurrencyId { get; set; }

        public decimal BetAmount { get; set; }

        public decimal WinAmount { get; set; }

        public decimal OriginalBetAmount { get; set; }

        public decimal OriginalWinAmount { get; set; }
        public decimal Profit
        {
            get { return BetAmount - WinAmount; }
        }

        public decimal ProfitPercent
        {
            get { return BetAmount == 0 ? 0 : (BetAmount - WinAmount) * 100 / BetAmount; }
        }
        
        public decimal SupplierPercent { get; set; }
        
        public decimal SupplierFee { get; set; }
        public int Count { get; set; }

    }
}