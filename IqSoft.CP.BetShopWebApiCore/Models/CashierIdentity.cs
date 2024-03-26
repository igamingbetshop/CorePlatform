using System.Collections.Generic;

namespace IqSoft.CP.BetShopWebApi.Models
{
	public class CashierIdentity
	{
		public double TimeZone { get; set; }
		public string LanguageId { get; set; }
		public int PartnerId { get; set; }
		public int CashierId { get; set; }
		public int CashDeskId { get; set; }
		public int BetShopId { get; set; }
		public List<string> ConnectionIds { get; set; }
	}
}