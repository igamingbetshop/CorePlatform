using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.RiseUp
{
	public class BaseInput
	{
		[JsonProperty(PropertyName = "client")]
		public string OperatorId { get; set; }

		[JsonProperty(PropertyName = "pid")]
		public string Clientid { get; set; }

		[JsonProperty(PropertyName = "product")]
		public string Provider { get; set; }

		[JsonProperty(PropertyName = "token")]
		public string Token { get; set; }

		[JsonProperty(PropertyName = "gameid")]
		public string GameId { get; set; }
	}
}