using IqSoft.CP.Common.Models;
namespace IqSoft.CP.DAL.Models.PlayersDashboard
{
    public class ClientReport
    {
        public PagedModel<DashboardClientInfo> Clients { get; set; }

        public ClientReportTotal Totals { get; set; }
    }
}
