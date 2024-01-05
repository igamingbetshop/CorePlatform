namespace IqSoft.NGGP.DAL.Models.Integration.InternalGamesIntegration
{
    public class GetBalanceOutput : OutputBase
    {
        public decimal AvailableBalance { get; set; }
        public string CurrencyId { get; set; }
    }
}