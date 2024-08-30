namespace IqSoft.CP.PaymentGateway.Models.MPAy
{
	public class PaymentInput
	{
		public Data data { get; set; }
	}

	public class AdditionalParams
	{
	}

	public class Data
	{
		public string method { get; set; }
		public string type { get; set; }
		public string sid { get; set; }
		public string username { get; set; }
		public string trx { get; set; }
		public string userID { get; set; }
		public string amount { get; set; }
		public string currency { get; set; }
		public string transaction_id { get; set; }
		public string status { get; set; }
		public int timestamp { get; set; }
		public string checksum { get; set; }
		public AdditionalParams additional_params { get; set; }
	}
}