namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
	public class ApiFilterCashDesk : ApiFilterBase
	{
		public int? Id { get; set; }
		public int? BetShopId { get; set; }
		public string Name { get; set; }
		public int? State { get; set; }
	}
}