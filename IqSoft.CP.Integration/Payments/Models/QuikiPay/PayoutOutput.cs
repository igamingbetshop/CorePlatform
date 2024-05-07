namespace IqSoft.CP.Integration.Payments.Models.QuikiPay
{
	public class PayoutOutput
	{
		public bool success { get; set; }
		public string message { get; set; }
		public string withdrawal_amount { get; set; }
		public string withdrawal_currency { get; set; }
		public int? code { get; set; }
	}
}
