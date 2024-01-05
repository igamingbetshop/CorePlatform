using System;

namespace IqSoft.CP.AdminWebApi.Models.BetShopModels
{
    public class BetShopReconingModel
    {
        public long Id { get; set; }

        public int BetShopId { get; set; }

        public int UserId { get; set; }

        public decimal Amount { get; set; }

        public decimal BetShopAvailiableBalance { get; set; }

        public string CurrencyId { get; set; }

        public DateTime CreationTime { get; set; }
    }
}