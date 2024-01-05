using System;

namespace IqSoft.CP.Integration.Products.Models.EveryMatrix
{
    public class ApiBonusModule
    {
        public string Id { get; set; }
        public string BonusName { get; set; }
       // public decimal Amount { get; set; }
        public decimal RemainingWagerRequirement { get; set; }
        public decimal InitialWagerRequirement { get; set; }
        public string BonusType { get; set; }
        public DateTime GrantedTime { get; set; }
        public string Status { get; set; }
        public int StatusId { get; set; }
        public int TypeId { get; set; }
    }
}
