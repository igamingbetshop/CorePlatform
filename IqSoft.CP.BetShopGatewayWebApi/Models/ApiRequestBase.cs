using System;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class ApiRequestBase : RequestBase
    {
        public int CashDeskId { get; set; }
        public string RequestObject { get; set; }
    }
}