using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllAccountBalance
    {
        public long Id { get; set; }
        
        public long AccountId { get; set; }
        
        public decimal Balance { get; set; }
        
        public DateTime Date { get; set; }
    }
}
