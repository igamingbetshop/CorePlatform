using IqSoft.CP.DAL.Models.PlayersDashboard;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.ClientModels
{
    public class ApiClientReport
    {
        public long Count { get; set; }

        public List<ApifnClientReportModel> Entities { get; set; }
        
        public ClientReportTotal Total { get; set; }
    }
}