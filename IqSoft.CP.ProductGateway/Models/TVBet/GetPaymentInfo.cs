﻿using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.TVBet
{
	public class GetPaymentInfo
	{
		[JsonProperty(PropertyName = "ti")]
		public long UnixTime { get; set; }

		[JsonProperty(PropertyName = "si")]
		public string Signature { get; set; }

		[JsonProperty(PropertyName = "tid")]
		public string TransactionId { get; set; }

		[JsonProperty(PropertyName = "to")]
		public string Token { get; set; }
	}
}