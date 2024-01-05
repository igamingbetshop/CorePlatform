namespace IqSoft.CP.Integration.Platforms.Models.CashCenter
{
    public class CashCenterClient
    {
        public int ClientId { get; set; }
        public string UserName { get; set; }
        public string CurrencyId { get; set; }
        public string Token { get; set; }
        public decimal Balance { get; set; }
    }
}
