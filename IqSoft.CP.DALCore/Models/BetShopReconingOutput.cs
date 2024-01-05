namespace IqSoft.CP.DAL.Models
{
	public class BetShopReconingOutput : PagedModel<fnBetShopReconing>
	{
		public decimal? TotalAmount { get; set; }

		public decimal? TotalBalance { get; set; }
	}
}
