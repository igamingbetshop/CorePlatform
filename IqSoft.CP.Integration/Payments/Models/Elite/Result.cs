using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Elite
{
	public class Result
	{
		[JsonProperty(PropertyName = "status")]
		public int Status { get; set; }

		[JsonProperty(PropertyName = "statusText")]
		public string StatusText { get; set; }

		[JsonProperty(PropertyName = "resultData")]
		public string ResultData { get; set; }

		[JsonProperty(PropertyName = "resultError")]
		public string ResultError { get; set; }
	}
}
