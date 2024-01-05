namespace IqSoft.NGGP.DAL.Models.Integration.InternalGamesIntegration
{
    public class FinOperationInputItem
    {
        public int ClientId { get; set; }
        public int CashDeskId { get; set; }
        public string Token { get; set; }
        public decimal Amount { get; set; }
        public int Type { get; set; }
    }
}
