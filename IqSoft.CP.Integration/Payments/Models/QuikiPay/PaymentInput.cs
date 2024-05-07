namespace IqSoft.CP.Integration.Payments.Models.QuikiPay
{
	public class PaymentInput
	{
		public string merchant { get; set; }
		public string customer_name { get; set; }
		public string customer_email { get; set; }
		public string currency { get; set; }
		public string order_id { get; set; }
		public string code { get; set; }
		public string amount { get; set; }
		public string success_url { get; set; }
		public string cancel_url { get; set; }
		public string callback_url { get; set; }
		public string country_code { get; set; }
		public string signature { get; set; }
		public string products_data { get; set; }
	}
}
