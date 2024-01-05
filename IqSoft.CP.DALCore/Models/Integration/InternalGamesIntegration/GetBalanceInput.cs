namespace IqSoft.NGGP.DAL.Models.Integration.InternalGamesIntegration
{
    public class GetBalanceInput : InputBase
    {
        public string Token { get; set; }
        public int ClientId { get; set; }
        public string CurrencyId { get; set; }
    }
}
