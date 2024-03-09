using System;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
	public class CashDesk 
	{
		public int Id { get; set; }
		public int BetShopId { get; set; }
		public string Name { get; set; }
		public int State { get; set; }
		public DateTime CreationTime { get; set; }
		public DateTime LastUpdateTime { get; set; }
		public decimal Balance { get; set; }
		public decimal TerminalBalance { get; set; }
		public int Type { get; set; }
	}
}