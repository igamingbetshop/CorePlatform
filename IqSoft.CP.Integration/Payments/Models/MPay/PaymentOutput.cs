namespace IqSoft.CP.Integration.Payments.Models.MPay
{
	public class DataOutput
	{
		public string url { get; set; }
	}

	public class PaymentOutput
	{
		public string status { get; set; }
		public string message { get; set; }		
		public object data { get; set; }
	}
}
