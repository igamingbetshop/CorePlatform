using System;

namespace IqSoft.CP.AdminWebApi.Models.ReportModels.BetShop
{
	public class ApiBetInfoItem
	{
		public long Id { get; set; }

		public string RoundId { get; set; }

		public int OperationTypeId { get; set; }

		public DateTime CreationTime { get; set; }
	}
}