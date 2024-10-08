using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
	public class ApiPartnerProduct
	{
		public string i { get; set; } //ImageUrl
		public string n { get; set; } //Name
		public string nn { get; set; } //NickName
		public int s { get; set; } //SubProviderId
		public int p { get; set; } //Id
		public decimal r { get; set; }
		public int o { get; set; }
		public int ss { get; set; }
		public List<int> c { get; set; }
		public string sp { get; set; }
		public bool hd { get; set; }
		public string jp { get; set; }
		public bool f { get; set; }
		public int pc { get; set; } //PlayersCount
	}
}