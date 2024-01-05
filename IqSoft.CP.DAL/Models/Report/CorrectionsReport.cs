using IqSoft.CP.Common.Models;
namespace IqSoft.CP.DAL.Models.Report
{
    public class CorrectionsReport : PagedModel<fnCorrection>
    {
        public decimal? TotalAmount { get; set; }
    }
}
