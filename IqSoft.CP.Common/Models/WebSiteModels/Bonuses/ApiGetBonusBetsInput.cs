using IqSoft.CP.Common.Enums;
using System;

namespace IqSoft.CP.Common.Models.WebSiteModels.Bonuses
{
	public class ApiGetBonusBetsInput
	{
		public int PlatformId { get; set; } = (int)ExternalPlatformTypes.IQSoft;

		public int ProductId { get; set; }

		public int? Status { get; set; }

		public string BonusId { get; set; }

		public int BetId { get; set; }

		public DateTime? FromDate { get; set; }
	}
}