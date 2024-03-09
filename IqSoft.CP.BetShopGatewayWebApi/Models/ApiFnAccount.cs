namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
	public class ApiFnAccount
	{
		public long Id { get; set; }
		public long ObjectId { get; set; }
		public int ObjectTypeId { get; set; }
		public int TypeId { get; set; }
		public decimal Balance { get; set; }
		public string CurrencyId { get; set; }
		public long? SessionId { get; set; }
		public string AccountTypeName { get; set; }
		public int? BetShopId { get; set; }
		public int? PaymentSystemId { get; set; }
		public string PaymentSystemName { get; set; }
	}
}