using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL.Models.Bonuses
{
    public class BonusWinnerInfo
    {
        public int ClientId { get; set; }
        public string Name { get; set; }
        public DateTime? CalculationTime { get; set; }
    }
}
