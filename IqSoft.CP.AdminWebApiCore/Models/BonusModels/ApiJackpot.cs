using System;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.BonusModels
{
    public class ApiJackpot
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? PartnerId { get; set; }
        public int Type { get; set; }
        public decimal Amount { get; set; }
        public string SecondaryAmount { get; set; }
        public DateTime FinishTime { get; set; }
        public int? WinnedClient { get; set; }
        public List<ApiBonusProducts> Products { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}