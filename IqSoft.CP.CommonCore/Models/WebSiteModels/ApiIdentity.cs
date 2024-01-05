using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
	public class ApiIdentity
	{
		public int PartnerId { get; set; }
		
		public string LanguageId { get; set; }

		public int ClientId { get; set; }

		public double TimeZone { get; set; }

		public string Token { get; set; }
	}
}
