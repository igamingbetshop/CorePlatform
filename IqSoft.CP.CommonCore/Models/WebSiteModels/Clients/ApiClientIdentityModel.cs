using System;

namespace IqSoft.CP.Common.Models.WebSiteModels.Clients
{
    public class ApiClientIdentityModel
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public int DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; }
        public int Status { get; set; }
    }
}