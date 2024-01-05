using IqSoft.CP.BetShopWebApi.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Reports
{
    public class GetResultsReportOutput : ClientRequestResponseBase
    {
        public int Count { get; set; }
        public List<ApiGameInfo> Entities { get; set; }
    }
}