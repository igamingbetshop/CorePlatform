using System;
using System.Collections.Generic;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetTicketInfoOutput
    {
        public long TicketId { get; set; }
        public int BetShopId { get; set; }
        public string BetShopAddress { get; set; }
        public int CashDeskId { get; set; }
        public string CashierName { get; set; }
        public int CashierId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreationTime { get; set; }
        public decimal TotalCoefficient { get; set; }
        public decimal PossibleWin { get; set; }
        public long BarCode { get; set; }
        public List<Selection> Selections { get; set; }
    }

    public class Selection
    {
        public long RoundId { get; set; }
        public decimal Coefficient { get; set; }
        public string GameName { get; set; }
        public string SelectionName { get; set; }
    }
}