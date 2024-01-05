namespace IqSoft.CP.DAL.Models.Report
{
    public class InternetGame
    {
        public int GameId { get; set; }

        public string GameName { get; set; }

        public decimal BetAmount { get; set; }

        public decimal WinAmount { get; set; }

        public string CurrencyId { get; set; }

        public int Count { get; set; }

        public int? ProviderId { get; set; }

        public string ProviderName { get; set; }

        public int? SubproviderId { get; set; }

        public string SubproviderName { get; set; }

        public decimal SupplierPercent { get; set; }

        public decimal SupplierFee { get; set; }

        public decimal OriginalBetAmount { get; set; }

        public decimal OriginalWinAmount { get; set; }
    }
}
