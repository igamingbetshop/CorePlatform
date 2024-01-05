using IqSoft.CP.AdminWebApi.Models.CommonModels;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.ReportModels.BetShop
{
	public class ApiGetBetInfoResponse : ApiResponseBase
	{
		public List<ApiBetInfoItem> Documents { get; set; }
	}
}