using IqSoft.CP.BetShopWebApi.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Reports
{
    public class GetUnitResultInfoOutput : ClientRequestResponseBase
    {
        public string State { get; set; }

        public string Outcome { get; set; }

        public object Selections { get; set; }
    }
}