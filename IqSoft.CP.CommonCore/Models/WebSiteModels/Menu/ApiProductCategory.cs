using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Common.Models.WebSiteModels.Menu
{
	public class ApiProductCategory
	{
		public string Type { get; set; }

		public List<int> Products { get; set; }
	}
}
