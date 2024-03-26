using IqSoft.CP.BetShopWebApi.Models.Common;
using System.Collections.Generic;

namespace IqSoft.CP.BetShopWebApi.Models.Reports
{
    public class GetResultsReportOutput : ApiResponseBase
    {
        public int Count { get; set; }
        public List<ApiGameInfo> Entities { get; set; }
    }
}