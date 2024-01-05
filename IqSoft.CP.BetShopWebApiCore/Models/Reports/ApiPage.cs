using IqSoft.CP.BetShopWebApi.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Reports
{
    public class ApiPage : PlatformRequestBase
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}