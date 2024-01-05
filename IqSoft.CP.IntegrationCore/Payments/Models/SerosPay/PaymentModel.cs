using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.SerosPay
{
   public class PaymentModel
    {
		[JsonProperty(PropertyName = "balance", Order = 1)]
		public string Balance { get; set; }

		[JsonProperty(PropertyName = "bankName", Order = 2)]
		public string BankName { get; set; }

		[JsonProperty(PropertyName = "company", Order = 3)]
		public string Company { get; set; }

		[JsonProperty(PropertyName = "date", Order = 4)]
		public string Date { get; set; }

		[JsonProperty(PropertyName = "id", Order = 5)]
		public string Id { get; set; }

		[JsonProperty(PropertyName = "internalReference", Order = 6)]
		public string InternalReference { get; set; }

		[JsonProperty(PropertyName = "paymentAmount", Order = 7)]
		public string PaymentAmount { get; set; }

		[JsonProperty(PropertyName = "paymentCount", Order = 8)]
		public int PaymentCount { get; set; }

		[JsonProperty(PropertyName = "paymentFees", Order = 9)]
		public string PaymentFees { get; set; }

		[JsonProperty(PropertyName = "sortNum", Order = 10)]
		public int SortNum { get; set; }

		[JsonProperty(PropertyName = "status", Order = 11)]
		public string Status { get; set; }

		[JsonProperty(PropertyName = "transactionDetails", Order = 12)]
		public string TransactionDetails { get; set; }

		[JsonProperty(PropertyName = "transactionReference", Order = 13)]
		public string TransactionReference { get; set; }

		[JsonProperty(PropertyName = "type", Order = 14)]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "user", Order = 15)]
        public long User { get; set; }
	}
}