namespace IqSoft.CP.Common.Models.WebSiteModels
{
	public class ApiWin
	{
		public string GameName { get; set; }
		public int ClientId { get; set; }
		public string ClientName { get; set; }
		public ApiBalance ApiBalance { get; set; }
        public decimal? BetAmount { get; set; }
        public decimal Amount { get; set; }
		public string CurrencyId { get; set; }
		public int PartnerId { get; set; }
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public string ImageUrl { get; set; }
	}
}
