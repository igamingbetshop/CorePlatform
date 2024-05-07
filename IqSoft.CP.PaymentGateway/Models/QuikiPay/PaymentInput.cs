namespace IqSoft.CP.PaymentGateway.Models.QuikiPay
{
	public class PaymentInput
	{
		public string tx_id { get; set; }
		public string payment_method { get; set; }
		public string local_quantity { get; set; }
		public string local_currency { get; set; }
		public string local_paid_amount { get; set; }
		public string local_fees { get; set; }
		public string order_id { get; set; }
		public string currency_symbol { get; set; }
		public string quantity { get; set; }
		public string fees { get; set; }
		public string deposit_at { get; set; }
		public string status { get; set; }
		public string customer_email { get; set; }
		public string paid_amount { get; set; }
		public string paid_amount_usd { get; set; }
		public string fx_rate { get; set; }
		public string signature { get; set; }
	}
}