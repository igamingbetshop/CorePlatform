using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnAgentTurnoverProfit
    {
        [NotMapped]
        public decimal FinalPercent { get; set; }

        [NotMapped]
        public decimal TotalProfit { get; set; }
    }
}
