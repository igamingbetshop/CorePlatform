namespace IqSoft.CP.DAL.Models.Report
{
    public class PartnerBetshopsSalesReport
    {
        public int PartnerId { get; set; }

        public string CurrencyId { get; set; }

        public decimal TotalBetAmount { get; set; }

        public decimal TotalWinAmount { get; set; }
    }
}
