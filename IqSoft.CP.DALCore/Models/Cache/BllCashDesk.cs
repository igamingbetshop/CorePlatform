using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllCashDesk
    {
        public int Id { get; set; }

        public int BetShopId { get; set; }

        public string Name { get; set; }

        public int State { get; set; }

        public long SessionId { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public string MacAddress { get; set; }

        public string EncryptPassword { get; set; }

        public string EncryptSalt { get; set; }

        public string EncryptIv { get; set; }

        public int CurrentCasherId { get; set; }

        public int Type { get; set; }
    }
}
