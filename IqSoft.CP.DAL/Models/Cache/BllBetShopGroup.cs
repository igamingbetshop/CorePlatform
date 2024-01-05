using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllBetShopGroup
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int PartnerId { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public int State { get; set; }
        public int? MaxCopyCount { get; set; }
        public decimal? MaxWinAmount { get; set; }
        public decimal? MinBetAmount { get; set; }
        public int? MaxEventCountPerTicket { get; set; }
        public int? CommissionType { get; set; }
        public decimal? CommissionRate { get; set; }
        public bool? AnonymousBet { get; set; }
        public bool? AllowCashout { get; set; }
        public bool? AllowLive { get; set; }
        public bool? UsePin { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}
