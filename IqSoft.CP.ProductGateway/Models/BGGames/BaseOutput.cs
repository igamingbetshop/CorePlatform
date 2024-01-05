using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.BGGames
{
	public class BaseOutput
	{
		[JsonProperty(PropertyName = "data")]
		public Data Data { get; set; }

		[JsonProperty(PropertyName = "error")]
		public int Error { get; set; } = 0;

		[JsonProperty(PropertyName = "desc")]
		public string Desc { get; set; } = "Success";
	}
	public class Data
	{
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }

		[JsonProperty(PropertyName = "userID")]
		public string UserID { get; set; }

		[JsonProperty(PropertyName = "username")]
		public string Username { get; set; }

		[JsonProperty(PropertyName = "balance")]
		public string Balance { get; set; }

		[JsonProperty(PropertyName = "old_balance")]
		public string OldBalance { get; set; }

		[JsonProperty(PropertyName = "signature")]
		public string Signature { get; set; }
	}
}