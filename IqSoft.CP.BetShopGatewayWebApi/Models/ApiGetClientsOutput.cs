using IqSoft.CP.DAL;
using System.Collections.Generic;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
	public class ApiGetClientsOutput : ApiResponseBase
	{
		public List<fnShopWalletClient> Entities { get; set; }

		public long Count { get; set; }
	}
}