namespace IqSoft.CP.AdminWebApi.Filters.Bonuses
{
	public class ApiFilterBonus : ApiFilterBase
	{
		public int? PartnerId { get; set; }
		public int? BonusId { get; set; }
        public int? ClientId { get; set; }
		public int? Type { get; set; }
		public bool? IsActive { get; set; }
	}
}