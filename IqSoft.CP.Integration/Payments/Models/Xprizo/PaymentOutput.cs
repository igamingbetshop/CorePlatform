namespace IqSoft.CP.Integration.Payments.Models.Xprizo
{
	public class PaymentOutput
	{
		public string key { get; set; }
		public string statusCode { get; set; }
		public string status { get; set; }
		public string description { get; set; }
		public string value { get; set; }
		public string approveById { get; set; }
		public string canCancel { get; set; }
		public string ttl { get; set; }
		public string expiryDate { get; set; }
		public string error { get; set; }
	}
}
