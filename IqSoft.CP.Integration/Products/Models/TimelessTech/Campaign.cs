using IqSoft.CP.Integration.Products.Models.BGGames;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.TimelessTech
{
	public class Campaign
	{
		public string vendor { get; set; }
		public string campaign_code { get; set; }
		public int freespins_per_player { get; set; }
		public long begins_at { get; set; }
		public long expires_at { get; set; }
		public string currency_code { get; set; }
		public List<GameModel> games { get; set; }
		public string players { get; set; }
	}
	public class GameModel
	{
		public int game_id { get; set; }
		public decimal total_bet { get; set; }
	}

	public class GameLimits
	{
		public string status { get; set; }
		public object data { get; set; }
	}

	public class Datum
	{
		public string currency_code { get; set; }
		public int game_id { get; set; }
		public string vendor { get; set; }
		public List<double> limits { get; set; }
	}
}
