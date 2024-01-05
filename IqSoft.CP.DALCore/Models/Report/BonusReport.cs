namespace IqSoft.CP.DAL.Models.Report
{
    public class BonusReport : PagedModel<fnClientBonus>
    {
        public decimal TotalBonusPrize { get; set; }
        public decimal TotalFinalAmount { get; set; }
    }
}