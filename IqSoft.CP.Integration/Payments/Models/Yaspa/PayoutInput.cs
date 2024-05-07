namespace IqSoft.CP.Integration.Payments.Models.Yaspa
{
	public class PayoutInput
	{
		public string merchantId { get; set; }
		public string customerIdentifier { get; set; }
		public string[] scopes { get; set; }
	}
}
