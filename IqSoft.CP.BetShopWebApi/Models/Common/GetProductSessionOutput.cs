using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetProductSessionOutput : ClientRequestResponseBase
	{
		public string ProductToken { get; set; }

		public string ProductId { get; set; }
	}
}