using System.Collections.Generic;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class ApiGetClientsOutput : ClientRequestResponseBase
	{
		public List<GetClientOutput> Entities { get; set; }

		public long Count { get; set; }
	}
}