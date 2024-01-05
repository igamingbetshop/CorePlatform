using System;

namespace IqSoft.CP.AdminWebApi.Models.BetShopModels
{
    public class CashDeskModel
    {
        public int Id { get; set; }
        public int BetShopId { get; set; }
        public string Name { get; set; }
        public int State { get; set; }
        public string StateName { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string MacAddress { get; set; }
        public string EncryptPassword { get; set; }
        public string EncryptSalt { get; set; }
        public string EncryptIv { get; set; }
        public decimal Balance { get; set; }
        public int Type { get; set; }
    }
}