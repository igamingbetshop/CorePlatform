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
		public string id { get; set; }
		public string name { get; set; }
		public string provider { get; set; }
		public string externalId  { get; set; }
		public string description { get; set; }
		public string type { get; set; }
		public string device { get; set; }
		public string mobileImgUrl { get; set; }
		public string webImageUrl { get; set; }
	}
}
