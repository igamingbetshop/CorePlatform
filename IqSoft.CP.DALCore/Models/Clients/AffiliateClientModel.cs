using System;

namespace IqSoft.CP.DAL.Models.Clients
{
    public class AffiliateClientModel
    {
        public int Id { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }

        public string Email { get; set; }
        
        public DateTime CreationTime { get; set; }
        
        public decimal BonusAmount { get; set; }
        
        public int Status { get; set; }
    }
}