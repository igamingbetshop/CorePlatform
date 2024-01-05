using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetBetShopOperationsOutput : ClientRequestResponseBase
	{
		public List<PaymentRequest> Operations { get; set; }
	}
}