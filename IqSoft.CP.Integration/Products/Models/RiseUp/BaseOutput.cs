using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.RiseUp
{
	public class BaseOutput
	{
		[JsonProperty(PropertyName = "status")]
		public bool Status { get; set; }

		[JsonProperty(PropertyName = "error")]
		public string Error { get; set; }

		[JsonProperty(PropertyName = "data")]
		public object Data { get; set; }
	}
	
	public class Games
	{
		[JsonProperty(PropertyName = "games")]
		public List<Game> GameList { get; set; }

		[JsonProperty(PropertyName = "provider")]
		public string Provider { get; set; }
	}

	public class Game
	{
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "provider")]
		public string Provider { get; set; }

		[JsonProperty(PropertyName = "description")]
		public string Description { get; set; }

		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "device")]
		public string Device { get; set; }

		[JsonProperty(PropertyName = "imgUrl")]
		public string ImgUrl { get; set; }
	}

	public class Product
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Provider { get; set; }
		public string ExternalId  { get; set; }
		public string Description { get; set; }
		public string Type { get; set; }
		public bool DesktopSupport { get; set; }
		public bool MobileSupport { get; set; }
		public string MobileImageUrl { get; set; }
		public string WebImageUrl { get; set; }
	}
}
