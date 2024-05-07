namespace IqSoft.CP.PaymentGateway.Models.QuikiPay
{
	public class PayoutInput
	{
		public int id { get; set; }
		public string withdrawal_id { get; set; }
		public string customer_name { get; set; }
		public string customer_email { get; set; }
		public string status { get; set; }
		public string source { get; set; }
		public int amount { get; set; }
		public string currency { get; set; }
		public string fees { get; set; }
		public string type { get; set; }
		public object description { get; set; }
		public int auto { get; set; }
		public string c_type { get; set; }
		public string crypto_currency { get; set; }
		public double quantity { get; set; }
		public string crypto_address { get; set; }
		public object tag { get; set; }
		public object proof_signature { get; set; }
		public string signature { get; set; }
	}
}