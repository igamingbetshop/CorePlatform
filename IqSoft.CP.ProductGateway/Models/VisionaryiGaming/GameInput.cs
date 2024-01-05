using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.VisionaryiGaming
{
	public class GameInput
	{
		[JsonProperty(PropertyName = "Method")]
		public string Method { get; set; }

		[JsonProperty(PropertyName = "ArgumentList")]
		public object ArgumentList { get; set; }

		[JsonProperty(PropertyName = "TS")]
		public long TS { get; set; }
	}

	public class RNGArgumentList
	{
		[JsonProperty(PropertyName = "baseURL")]
		public string BaseURL { get; set; }

		[JsonProperty(PropertyName = "tables")]
		public List<RNGTable> Tables { get; set; }

		[JsonProperty(PropertyName = "siteIDs")]
		public List<string> SiteIDs { get; set; }
	}

	public class RNGTable
	{
		[JsonProperty(PropertyName = "game")]
		public string Game { get; set; }

		[JsonProperty(PropertyName = "gameID")]
		public string GameId { get; set; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "icon")]
		public string Icon { get; set; }

		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "status")]
		public string Status { get; set; }
	}

	public class LobbyArgumentList
	{
		[JsonProperty(PropertyName = "tables")]
		public List<LobbyTable> Tables { get; set; }

		[JsonProperty(PropertyName = "siteIDs")]
		public List<string> SiteIDs { get; set; }
	}

	public class LobbyTable
	{
		[JsonProperty(PropertyName = "game")]
		public string Game { get; set; }

		[JsonProperty(PropertyName = "limits")]
		public List<Limit> Limits { get; set; }

		[JsonProperty(PropertyName = "table")]
		public string Table { get; set; }

		[JsonProperty(PropertyName = "dealer")]
		public Dealer Dealer { get; set; }

		[JsonProperty(PropertyName = "tableID")]
		public string TableID { get; set; }

		[JsonProperty(PropertyName = "status")]
		public string Status { get; set; }
	}
	public class Dealer
	{
		[JsonProperty(PropertyName = "start")]
		public string Start { get; set; }

		[JsonProperty(PropertyName = "photoURL")]
		public string PhotoURL { get; set; }

		[JsonProperty(PropertyName = "dealername")]
		public string Dealername { get; set; }
	}

	public class Limit
	{
		[JsonProperty(PropertyName = "currency")]
		public string Currency { get; set; }

		[JsonProperty(PropertyName = "limits")]
		public List<Limit> Limits { get; set; }

		[JsonProperty(PropertyName = "limitname")]
		public int Limitname { get; set; }

		[JsonProperty(PropertyName = "max")]
		public string Max { get; set; }

		[JsonProperty(PropertyName = "min")]
		public string Min { get; set; }
	}
}