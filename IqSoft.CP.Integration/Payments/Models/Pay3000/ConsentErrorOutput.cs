using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Payments.Models.Pay3000
{
	public class ConsentErrorOutput
	{
		[JsonProperty(PropertyName = "error")]
		public string Error { get; set; }

		[JsonProperty(PropertyName = "errorCode")]
		public string ErrorCode { get; set; }

		[JsonProperty(PropertyName = "timestamp")]
		public DateTime Timestamp { get; set; }

		[JsonProperty(PropertyName = "status")]
		public int Status { get; set; }

		[JsonProperty(PropertyName = "path")]
		public string Path { get; set; }
	}
}
