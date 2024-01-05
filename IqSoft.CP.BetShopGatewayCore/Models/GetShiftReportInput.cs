using System;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetShiftReportInput : RequestBase
    {
        public int CashDeskId { get; set; }

        public int? CashierId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}