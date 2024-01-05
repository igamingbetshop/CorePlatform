using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class RollbackDepositToInternetClient : PlatformRequestBase
	{
		public int ClientId { get; set; }
		public string TransactionId { get; set; }
	}
}