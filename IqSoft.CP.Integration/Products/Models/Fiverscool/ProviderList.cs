using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.Fiverscool
{
	public class ProviderList
	{
		[JsonProperty(PropertyName = "status")]
		public int Status { get; set; }

		[JsonProperty(PropertyName = "msg")]
		public string Message { get; set; }

		[JsonProperty(PropertyName = "providers")]
		public List<Provider> Providers { get; set; }
	}
	public class Provider
	{
		[JsonProperty(PropertyName = "code")]
		public string Code { get; set; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "status")]
		public int Status { get; set; }
	}
}
