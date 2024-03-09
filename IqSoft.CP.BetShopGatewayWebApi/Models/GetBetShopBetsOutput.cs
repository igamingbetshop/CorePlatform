using System;
using System.Collections.Generic;


namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetBetShopBetsOutput
    {
        public List<BetShopBet> Bets { get; set; }
    }

    public class BetShopBet
    {
        public long BetDocumentId { get; set; }
        public long? TicketNumber { get; set; }
        public int State { get; set; }
        public string BetInfo { get; set; }
        public string WinInfo { get; set; }
        public int? CashDeskId { get; set; }
        public decimal BetAmount { get; set; }
        public decimal WinAmount { get; set; }
        public string CurrencyId { get; set; }
        public int ProductId { get; set; }
        public int? GameProviderId { get; set; }
        public long? Barcode { get; set; }
        public int? CashierId { get; set; }
        public DateTime BetDate { get; set; }
        public DateTime? WinDate { get; set; }
        public DateTime? PayDate { get; set; }
        public int BetShopId { get; set; }
        public string BetShopName { get; set; }
        public int PartnerId { get; set; }
        public string ProductName { get; set; }
        public decimal Profit { get; set; }
        public int? BetType  { get; set; }
    }
}