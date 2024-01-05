using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class ProductCategory
	{
		public string Name { get; set; }

		public List<Product> Products { get; set; }
	}
}