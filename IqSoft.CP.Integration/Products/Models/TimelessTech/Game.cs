using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.TimelessTech
{
	public class Data
	{
		public List<Game> games { get; set; }
	}
	public class Game
	{
		public int id { get; set; }
		public string title { get; set; }
		public string platform { get; set; }
		public string type { get; set; }
		public string subtype { get; set; }
		public int enabled { get; set; }
		public int fun_mode { get; set; }
		public int? campaigns { get; set; }
		public string vendor { get; set; }
		public List<string> vendorGroups { get; set; }
		public Details details { get; set; }
	}
	public class Details
	{
		public Thumbnails thumbnails { get; set; }
		public List<string> tags { get; set; }
		public decimal rtp { get; set; }
		public string volatility { get; set; }
	}
	public class Thumbnails
	{
		[JsonProperty("300x300")]
		public string _300x300 { get; set; }

		[JsonProperty("300x300-gif")]
		public string _300x300gif { get; set; }

		[JsonProperty("450x345-jpg")]
		public string _450x345jpg { get; set; }

		[JsonProperty("450x345-gif")]
		public string _450x345gif { get; set; }
	}
}
