using System;

namespace IqSoft.CP.AdminWebApi.Models.BonusModels
{
    public class ApiBonusProducts
    {
        public int? Id { get; set; }
        public int ProductId { get; set; }
        public decimal? Percent { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}