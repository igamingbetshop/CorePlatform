namespace IqSoft.CP.Integration.Payments.Models.Jmitsolutions
{
	public class PaymentOutput
	{
		public bool success { get; set; }
		public int result { get; set; }
		public int status { get; set; }
		public string token { get; set; }
		public string processingUrl { get; set; }
		public Payment payment { get; set; }
	}
	public class Payment
	{
		public int amount { get; set; }
		public int gateway_amount { get; set; }
		public string currency { get; set; }
		public string status { get; set; }
		public bool two_stage_mode { get; set; }
	}
}
