using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.Common.Models.WebSiteModels.Products
{
	public class ApiGetPartnerSpecialProductsInput : ApiRequestBase
	{
		public int Type { get; set; }
	}
}