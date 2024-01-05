using IqSoft.CP.BetShopWebApi.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Reports
{
    public class GetResultsReportInput : ApiPage
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? GameId { get; set; }
    }
}