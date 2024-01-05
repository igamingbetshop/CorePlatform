using IqSoft.CP.BetShopWebApi.Models.Common;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
	public class CashDesksOutput : ClientRequestResponseBase
	{
		public List<CashDesk> Entities { get; set; }

		public long Count { get; set; }
	}
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