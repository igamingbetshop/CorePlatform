namespace IqSoft.CP.ProductGateway.Models.LuckyStreak
{
	public class ValidateInput
	{
		public Validate data { get; set; }
	}
	public class Validate
	{
		public string AuthorizationCode { get; set; }
		public string OperatorName { get; set; }
		public string AdditionalParams { get; set; }
	}
}