using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.Fiverscool
{
	public class GameList
	{
		[JsonProperty(PropertyName = "status")]
		public int status { get; set; }

		[JsonProperty(PropertyName = "msg")]
		public string Message { get; set; }

		[JsonProperty(PropertyName = "games")]
		public List<Game> Games { get; set; }
	}
	public class Game
	{
		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		[JsonProperty(PropertyName = "game_code")]
		public string GameCode { get; set; }

		[JsonProperty(PropertyName = "game_name")]
		public string GameName { get; set; }

		[JsonProperty(PropertyName = "banner")]
		public string Banner { get; set; }

		[JsonProperty(PropertyName = "status")]
		public int Status { get; set; }
	}

	public class GamesWithProvider
	{
		public Provider Provider { get; set; }
		public List<Game> Games { get; set; }
	}
}
