using System;

namespace IqSoft.CP.Common.Models.WebSiteModels.Clients
{
    public class ApiExclusionInput
    {
        public DateTime Date { get; set; }
        public string Credentials { get; set; }
        public int Reason { get; set; }
    }
}