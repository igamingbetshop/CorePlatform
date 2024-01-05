using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class RequestInfo
	{
		public int TimeZone { get; set; }
		public string LanguageId { get; set; }
		public int PartnerId { get; set; }
	}
}