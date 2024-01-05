namespace IqSoft.CP.Common.Models.WebSiteModels.Bonuses
{
	public class ApiGetBonusBetsInput
	{
		public int ProductId { get; set; }

		public int? Status { get; set; }

		public int BonusId { get; set; }

		public int BetId { get; set; }
	}
}