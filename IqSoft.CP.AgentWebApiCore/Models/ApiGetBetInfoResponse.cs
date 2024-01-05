using System.Collections.Generic;

namespace IqSoft.CP.AgentWebApi.Models
{
	public class ApiGetBetInfoResponse : ApiResponseBase
	{
		public List<ApiBetInfoItem> Documents { get; set; }
	}
}