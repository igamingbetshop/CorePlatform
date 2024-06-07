using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllLeaderboardItem
    {
        public int Id { get; set; }
        public string CurrencyId { get; set; }
        public decimal CurrencyRate { get; set; }
        public string Name { get; set; }
        public decimal Points { get; set; }
    }
}
