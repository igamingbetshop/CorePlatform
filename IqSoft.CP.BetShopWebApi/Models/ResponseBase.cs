using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models
{
	public class ResponseBase
	{
		public string Method { get; set; }

		public int ResponseCode { get; set; }

		public string Description { get; set; }

		public object ResponseObject { get; set; }
	}
}