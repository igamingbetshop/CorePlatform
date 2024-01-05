namespace IqSoft.CP.DAL.Models.Report
{
   public class InternetBetsByPartner
    {
        public int PartnerId { get; set; }

        public string CurrencyId { get; set; }

        public int ProductId { get; set; }

        public decimal? TotalBetAmount { get; set; }

        public decimal? TotalGGR { get; set; }

    }
}
