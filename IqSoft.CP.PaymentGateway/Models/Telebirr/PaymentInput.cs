using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Telebirr
{
	public class PaymentInput
	{
		[JsonProperty(PropertyName = "msisdn")]
		public string Msisdn { get; set; }

		[JsonProperty(PropertyName = "outTradeNo")]
		public string OutTradeNo { get; set; }

		[JsonProperty(PropertyName = "totalAmount")]
		public string TotalAmount { get; set; }

		[JsonProperty(PropertyName = "tradeDate")]
		public long TradeDate { get; set; }

		[JsonProperty(PropertyName = "tradeNo")]
		public string TradeNo { get; set; }

		[JsonProperty(PropertyName = "tradeStatus")]
		public int TradeStatus { get; set; }

		[JsonProperty(PropertyName = "transactionNo")]
		public string TransactionNo { get; set; }
	}
}