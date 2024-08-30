using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Models.Jmitsolutions
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "errors")]
        public List<object> Errors { get; set; }
    }

	public class PayoutOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "payout")]
        public Payout Payout { get; set; }
	}

	public class Payout
	{
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }
	}
}
