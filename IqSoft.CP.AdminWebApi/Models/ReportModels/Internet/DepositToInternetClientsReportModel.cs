using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.AdminWebApi.Models.ReportModels.Internet
{
    public class DepositToInternetClientsReportModel : PagedModel<FnDepositToInternetClientModel>
    {
        public decimal? TotalDepositAmount { get; set; }
    }
}