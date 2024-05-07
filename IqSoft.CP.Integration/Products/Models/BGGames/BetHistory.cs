using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.BGGames
{
	public class BetHistory
	{
		public string status { get; set; }
		public int total { get; set; }
		public object data { get; set; }
	}

	public class Datum
	{
		public Id _id { get; set; }
		public string betslipId { get; set; }
		public int ID { get; set; }
		public int site { get; set; }
		public string user { get; set; }
		public string currency { get; set; }
		public string timestamp { get; set; }
		public int unix { get; set; }
		public string date { get; set; }
		public string device { get; set; }
		public string product { get; set; }
		public string type { get; set; }
		public int group { get; set; }
		public decimal amount { get; set; }
		public int columns { get; set; }
		public double pWins { get; set; }
		public double wins { get; set; }
		public int betTax { get; set; }
		public int winTax { get; set; }
		public string status { get; set; }
		public string changes { get; set; }
		public string sent { get; set; }
		public string lastUpdate { get; set; }
		public List<Event> events { get; set; }
	}

	public class Event
	{
		public string id { get; set; }
		public string sid { get; set; }
		public string lid { get; set; }
		public string number { get; set; }
		public string name { get; set; }
		public string start { get; set; }
		public string type { get; set; }
		public string selID { get; set; }
		public string selPrefix { get; set; }
		public string oddID { get; set; }
		public string oddType { get; set; }
		public string spv { get; set; }
		public string oldSpv { get; set; }
		public string selName { get; set; }
		public string oddName { get; set; }
		public string oddValue { get; set; }
		public string oddLine { get; set; }
		public string status { get; set; }
		public int lastChange { get; set; }
		public Extra extra { get; set; }
	}

	public class Extra
	{
		public string eventTime { get; set; }
		public string eventScore { get; set; }
		public string timeAddedToBetslip { get; set; }
	}

	public class Id
	{
		[JsonProperty("$oid")]
		public string oid { get; set; }
	}

	public class Report
	{
		public string betslipId { get; set; }
		public List<Info> events { get; set; }
	}

	public class Info
	{
		public string name { get; set; }
		public string selName { get; set; }
		public string oddName { get; set; }
		public string oddValue { get; set; }
		public string status { get; set; }
		public Extra extra { get; set; }
	}


}
