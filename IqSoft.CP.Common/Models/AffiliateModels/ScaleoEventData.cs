using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.AffiliateModels
{
	public class ScaleoEventData
	{
		public string status { get; set; }
		public int code { get; set; }
		public Data data { get; set; }
	}
	public class Data
	{
		public List<Event> events { get; set; }
	}

	public class Event
	{
		public string timestamp { get; set; }
		public string type { get; set; }
		public string click_id { get; set; }
		public string player_id { get; set; }
		public decimal amount { get; set; }
		public string currency { get; set; }
	}
}