using System;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetCashDeskOperationsInput : RequestBase
    {
        public int? LastShiftsNumber { get; set; }

        public int CashierId { get; set; }

        public int CashDeskId { get; set; }

        public DateTime EndTime { get; set; }
        
        public DateTime StartTime { get; set; }
    }
}