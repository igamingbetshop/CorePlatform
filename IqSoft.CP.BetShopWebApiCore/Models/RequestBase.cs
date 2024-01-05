using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models
{
	public class RequestBase
	{
		public bool IsFromServer { get; set; }

		public string ServerCredentials { get; set; }

		public string Method { get; set; }

		public string Token { get; set; }

		public string RequestObject { get; set; }
	}
}