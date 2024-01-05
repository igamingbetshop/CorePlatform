using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.TimelessTech
{
	public class BaseOutput
	{
		[JsonProperty(PropertyName = "request")]
		public Request Request { get; set; }

		[JsonProperty(PropertyName = "response")]
		public Response Response { get; set; }
	}

	public class ResponseData
	{
		[JsonProperty(PropertyName = "user_id")]
		public string UserId { get; set; }

		[JsonProperty(PropertyName = "user_name")]
		public string UserName { get; set; }

		[JsonProperty(PropertyName = "user_country")]
		public string UserCountry { get; set; }

		[JsonProperty(PropertyName = "balance")]
		public decimal Balance { get; set; }

		[JsonProperty(PropertyName = "currency_code")]
		public string CurrencyCode { get; set; }
	}

	public class Request
	{

		[JsonProperty(PropertyName = "command")]
		public string Command { get; set; }

		[JsonProperty(PropertyName = "request_timestamp")]
		public string RequestTimestamp { get; set; }

		[JsonProperty(PropertyName = "hash")]
		public string Hash { get; set; }

		[JsonProperty(PropertyName = "data")]
		public object RequestData { get; set; }
	}

	public class RequestData
	{
		[JsonProperty(PropertyName = "token")]
		public string Token { get; set; } = "OK";
	}

	public class Response
	{
		[JsonProperty(PropertyName = "status")]
		public string Status { get; set; } = "OK";

		[JsonProperty(PropertyName = "response_timestamp")]
		public string ResponseTimestamp { get; set; }

		[JsonProperty(PropertyName = "hash")]
		public string Hash { get; set; }

		[JsonProperty(PropertyName = "data")]
		public object ResponseData { get; set; }
	}

	public class ErrorOutput
	{
		[JsonProperty(PropertyName = "error_code")]
		public string ErrorCode { get; set; }

		[JsonProperty(PropertyName = "error_message")]
		public string ErrorMessage { get; set; }
	}
}