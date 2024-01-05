using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial class fnTransaction : IBase
    {
        public decimal BalanceBefore { get; set; }

        public decimal BalanceAfter { get; set; }
    }
}
