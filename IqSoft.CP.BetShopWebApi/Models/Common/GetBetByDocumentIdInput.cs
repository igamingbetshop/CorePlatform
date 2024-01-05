using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetBetByDocumentIdInput : PlatformRequestBase
	{
		public long DocumentId { get; set; }

		public bool IsForPrint { get; set; }

		public int CashDeskId { get; set; }
	}
}