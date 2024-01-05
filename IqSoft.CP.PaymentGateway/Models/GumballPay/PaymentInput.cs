using System.Runtime.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.GumballPay
{
	public class PaymentInput
	{
		[DataMember(Name = "amount")]
		public decimal Amount { get; set; }

		[DataMember(Name = "initial-amount")]
		public decimal Initial_Amount { get; set; }

		[DataMember(Name = "control")]
		public string Control { get; set; }

		[DataMember(Name = "status")]
		public string Status { get; set; }

		[DataMember(Name = "orderid")]
		public string Orderid { get; set; }

		[DataMember(Name = "merchant_order")]
		public string Merchant_Order { get; set; }

		[DataMember(Name = "error_code")]
		public string Error_Code { get; set; }

		[DataMember(Name = "error_message")]
		public string Error_Message { get; set; }
    }
}