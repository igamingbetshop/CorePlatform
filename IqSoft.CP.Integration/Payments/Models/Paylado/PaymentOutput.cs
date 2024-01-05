using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Payments.Models.Paylado
{
	public class PaymentOutput
	{
		[JsonProperty(PropertyName = "ResultStatus")]
		public string ResultStatus { get; set; }

		[JsonProperty(PropertyName = "ResultCode")]
		public string ResultCode { get; set; }

		[JsonProperty(PropertyName = "ResultMessage")]
		public string ResultMessage { get; set; }

		[JsonProperty(PropertyName = "Token")]
		public string Token { get; set; }

		[JsonProperty(PropertyName = "RedirectUrl")]
		public string RedirectUrl { get; set; }
	}
}
