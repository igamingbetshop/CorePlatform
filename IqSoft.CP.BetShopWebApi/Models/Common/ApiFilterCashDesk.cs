using IqSoft.CP.BetShopGatewayWebApi.Models;
using System;

namespace IqSoft.CP.BetShopGatewayWebApi.Common
{
	public class ApiFilterCashDesk : ApiFilterBase
	{
		public int? Id { get; set; }

		public int? BetShopId { get; set; }

		public int CashDeskId { get; set; }

		public string Name { get; set; }

		public int? State { get; set; }

		public DateTime? CreatedFrom { get; set; }

		public DateTime? CreatedBefore { get; set; }
	}
}