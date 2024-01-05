using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Models.Stripe
{
	public class PaymentLinkInput
	{
		public List<line_items> LineItem { get; set; }
	}

	public class line_items
	{
		public int quantity { get; set; }
		public string price { get; set; }
	}
}
