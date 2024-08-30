using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Models.Katarun
{
	public class PaymentInput
	{
		public Client client { get; set; }
		public string reference { get; set; }
		public string success_redirect { get; set; }
		public string failure_redirect { get; set; }
		public string cancel_redirect { get; set; }
		public Purchase purchase { get; set; }
		public string brand_id { get; set; }
	}

	public class Client
	{
		public string email { get; set; }
	}

	public class Product
	{
		public string name { get; set; }
		public int price { get; set; }
	}

	public class Purchase
	{
		public List<Product> products { get; set; }
	}	
}