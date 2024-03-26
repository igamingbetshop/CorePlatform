using System;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
    public class ApiRequestBase : PlatformRequestBase
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}