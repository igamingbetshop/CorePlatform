using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Models.Jmitsolutions
{
	public class PayoutOutput
	{
		public bool success { get; set; }
		public List<object> errors { get; set; }
		public Payout payout { get; set; }
	}

	public class Payout
	{
		public string token { get; set; }
		public string status { get; set; }
		public DateTime timestamp { get; set; }
	}
}
