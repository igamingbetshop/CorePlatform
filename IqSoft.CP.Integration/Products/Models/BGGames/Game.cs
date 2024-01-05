using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.BGGames
{
	public class Data
	{
		public string status { get; set; }
		public List<object> data { get; set; }
	}
	public class Game
	{
		public string ID { get; set; }
		public string name { get; set; }
		public string gameRemoteID { get; set; }
		public string licenses { get; set; }
		public string category { get; set; }
		public string is_lobby { get; set; }
		public string version { get; set; }
		public string thumbnail { get; set; }
		public string providerID { get; set; }
		public string extra { get; set; }
		public string provider { get; set; }
		public string has_demo { get; set; }
		public string disp_order { get; set; }
		public string status { get; set; }
		public string vendor { get; set; }
		public bool mobile { get; set; }
		public bool desktop { get; set; }
	}

	public class Provider
	{
		public string provider { get; set; }
		public string disp_order { get; set; }
		public string open_as_lobby { get; set; }
		public string inSiteStatus { get; set; }
		public string name { get; set; }
		public string logo { get; set; }
		public string baseURL { get; set; }
		public string category { get; set; }
	}
}
