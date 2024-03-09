using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.BGGames
{
	public class BaseInput
	{
		[JsonProperty(PropertyName = "action")]
		public string Action { get; set; }

		[JsonProperty(PropertyName = "userID")]
		public string UserID { get; set; }

		[JsonProperty(PropertyName = "transID")]
		public string TransID { get; set; }

		[JsonProperty(PropertyName = "refTransID")]
		public string RefTransID { get; set; }

		[JsonProperty(PropertyName = "cid")]
		public string CId { get; set; }

		[JsonProperty(PropertyName = "roundID")]
		public string RoundID { get; set; }

		[JsonProperty(PropertyName = "remoteID")]
		public string RemoteID { get; set; }

		[JsonProperty(PropertyName = "gameID")]
		public string GameID { get; set; }

		[JsonProperty(PropertyName = "provID")]
		public string ProvID { get; set; }

		[JsonProperty(PropertyName = "provName")]
		public string ProvName { get; set; }

		[JsonProperty(PropertyName = "vendID")]
		public string VendID { get; set; }

		[JsonProperty(PropertyName = "vendName")]
		public string VendName { get; set; }

		[JsonProperty(PropertyName = "betAmount")]
		public string BetAmount { get; set; }

		[JsonProperty(PropertyName = "winAmount")]
		public string WinAmount { get; set; }

		[JsonProperty(PropertyName = "signature")]
		public string Signature { get; set; }

		[JsonProperty(PropertyName = "rollBackAmount")]
		public string RollBackAmount { get; set; }

		[JsonProperty(PropertyName = "extra")]
		public object Extra { get; set; }

		[JsonProperty(PropertyName = "betslipID")]
		public string BetslipID { get; set; }

		[JsonProperty(PropertyName = "amount")]
		public string Amount { get; set; }

		[JsonProperty(PropertyName = "data")]
		public object Data { get; set; }

		[JsonProperty(PropertyName = "status")]
		public string Status { get; set; }

		[JsonProperty(PropertyName = "betID")]
		public string BetID { get; set; }
	}
	public class Extra
	{
		[JsonProperty(PropertyName = "token")]
		public string Token { get; set; }

		[JsonProperty(PropertyName = "customer")]
		public string Customer { get; set; }
	}
}