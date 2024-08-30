namespace IqSoft.CP.PaymentGateway.Models.Huch
{
	public class PaymentInput
	{
		public string payment_id { get; set; }
		public string type { get; set; }
		public string status { get; set; }
		public string currency { get; set; }
		public int amount { get; set; }
		public string order_ref { get; set; }
		public string payment_status { get; set; }
		public string status_info { get; set; }
	}
}