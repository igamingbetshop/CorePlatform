namespace IqSoft.CP.DAL.Models.Job
{
    public class ClientProductBet
    {
        public int ClientId { get; set; }

        public string CurrencyId { get; set; }
        
        public string LanguageId { get; set; }
        
        public int? CountryId { get; set; }

        public int ProductId { get; set; }
        
        public decimal BetAmount { get; set; }

        public decimal GGRAmount { get; set; }

        public decimal Percent { get; set; } 
    }
}
