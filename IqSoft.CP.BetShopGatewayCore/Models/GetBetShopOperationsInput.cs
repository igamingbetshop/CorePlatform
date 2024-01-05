using System;
namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetBetShopOperationsInput : RequestBase
    {
        public int CashierId { get; set; }

        public int CashDeskId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }
}