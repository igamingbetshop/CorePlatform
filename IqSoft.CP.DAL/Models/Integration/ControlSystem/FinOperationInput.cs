using System;

namespace IqSoft.CP.DAL.Models.Integration.ControlSystem
{
    public class FinOperationInput
    {
        public string Id { get; set; }
        
        public string ParentId { get; set; }
        
        public decimal BetAmount { get; set; }
        
        public decimal Coefficient { get; set; }
        
        public decimal WinAmount { get; set; }
        
        public string CurrencyId { get; set; }
        
        public int Type { get; set; }
        
        public int? ProductId { get; set; }
        
        public string ProductName { get; set; }
        
        public int BetShopId { get; set; }
        
        public string BetShopName { get; set; }
        
        public string BetInfo { get; set; }
        
        public string ResultInfo { get; set; }
        
        public long? BarCode { get; set; }
        
        public long? TicketNumber { get; set; }

        public DateTime OperationTime { get; set; }
    }
}
