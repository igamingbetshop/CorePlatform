using IqSoft.CP.DAL.Models.Cache;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Clients
{
    public class ClientTriggerInput
    {
        public int ClientId { get; set; }
        public int TriggerType { get; set; }
        public List<ClientBonusInfo> ClientBonuses { get; set; }
        public decimal? SourceAmount { get; set; }
        public string PromoCode { get; set; }
        public int? ProductId { get; set; }
        public int? PaymentSystemId { get; set; }
        public int? DepositsCount { get; set; }
        public string TicketInfo { get; set; }
        public string WinInfo { get; set; }
        public int? SegmentId { get; set; }
    }
}