namespace IqSoft.CP.Integration.Payments.Models.MPay
{
	public class Data
	{
		public string trx { get; set; }
		public decimal amount { get; set; }
		public string return_url { get; set; }
		public string bank_id { get; set; }
		public string account_holders_name { get; set; }
		public string account_no { get; set; }
		public string iban { get; set; }
		public string account_number { get; set; }
		public string wallet_id { get; set; }
		public string wallet_holders_name { get; set; }
		public string popyIBAN { get; set; }
	}

	public class PaymentInput
	{
		public Data data { get; set; }
		public User user { get; set; }
	}

	public class User
	{
		public string username { get; set; }
		public string userID { get; set; }
		public string fullname { get; set; }
		public string yearofbirth { get; set; }
		public string tckn { get; set; }
	}
}
