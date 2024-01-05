using System;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class BetModel
    {
        public long BetDocumentId {get; set;}
        
        public int State { get; set; }
        
        public DateTime BetDate { get; set; }
        
        public DateTime? WinDate { get; set; }
        
        public int ClientId { get; set; }
        
        public decimal BetAmount { get; set; }
        
        public decimal WinAmount { get; set; }
        
        public int? BetTypeId { get; set; }
        
        public decimal? PossibleWin { get; set; }

        public int ProductId { get; set; }
        
        public string ProductName { get; set; }

        public decimal Profit { get; set; }

		public int ProviderId { get; set; }

        public int? SelectionsCount { get; set; }
    }
}