using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels.Clients
{
    public class ApiClientData
    {
        public int ClientId { get; set; }
        public int DepCount { get; set; }
        public string Token { get; set; }
        public List<int> Segments { get; set; }
    }
}
