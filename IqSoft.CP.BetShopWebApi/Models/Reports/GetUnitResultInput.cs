using IqSoft.CP.BetShopWebApi.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Reports
{
    public class GetUnitResultInput : PlatformRequestBase
    {
        public int Id { get; set; }
    }
}