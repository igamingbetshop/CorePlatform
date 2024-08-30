using System;

namespace IqSoft.CP.DAL.Models.PlayersDashboard
{
    public class DashboardSessionInfo
    {
        public int ClientId { get; set; }
        public string UserName { get; set; }
        public int PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long TotalSessionTime { get; set; }
    }
}
