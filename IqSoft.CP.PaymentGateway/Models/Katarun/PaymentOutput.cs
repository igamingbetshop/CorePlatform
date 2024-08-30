using System.Collections.Generic;

namespace IqSoft.CP.PaymentGateway.Models.Katarun
{
	public class Attempt
	{
		public object error { get; set; }
	}

	public class PaymentOutput
	{
		public string id { get; set; }
		public string status { get; set; }
		public string reference { get; set; }
		public TransactionData transaction_data { get; set; }
	}

	public class TransactionData
	{
		public List<Attempt> attempts { get; set; }
	}

	public class Error
	{
		public string code { get; set; }
		public string message { get; set; }
	}
}