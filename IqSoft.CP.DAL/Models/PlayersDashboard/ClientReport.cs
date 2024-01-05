using IqSoft.CP.Common.Models;
namespace IqSoft.CP.DAL.Models.PlayersDashboard
{
    public class ClientReport
    {
        public PagedModel<ApiClientInfo> Clients { get; set; }

        public ClientReportTotal Totals { get; set; }
    }
}
