using System.Collections.Generic;

namespace IqSoft.CP.PaymentGateway.Models.Katarun
{
	public class Product
	{
		public string name { get; set; }
		public int price { get; set; }
	}

	public class Purchase
	{
		public List<Product> products { get; set; }
	}

	public class PaymentInput
	{
		public string id { get; set; }
		public string reference { get; set; }
		public string status { get; set; }
		public Purchase purchase { get; set; }
	}
}