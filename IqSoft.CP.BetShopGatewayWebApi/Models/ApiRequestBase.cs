using System;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class ApiRequestBase :RequestBase
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}