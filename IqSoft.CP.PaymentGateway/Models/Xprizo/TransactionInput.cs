using System;
using System.Collections.Generic;

namespace IqSoft.CP.PaymentGateway.Models.Xprizo
{
	public class TransactionInput
	{
		public int statusType { get; set; }
		public string status { get; set; }
		public object description { get; set; }
		public int actionedById { get; set; }
		public List<int> affectedContactIds { get; set; }
		public Transaction transaction { get; set; }
	}
	public class Transaction
	{
		public int id { get; set; }
		public int createdById { get; set; }
		public object type { get; set; }
		public object description { get; set; }
		public DateTime date { get; set; }
		public string reference { get; set; }
		public string currencyCode { get; set; }
		public decimal amount { get; set; }
	}
}