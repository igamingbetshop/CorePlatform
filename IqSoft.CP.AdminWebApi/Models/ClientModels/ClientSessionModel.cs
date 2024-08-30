using System;

namespace IqSoft.CP.AdminWebApi.ClientModels.Models
{
    public class ClientSessionModel
    {
        public long Id { get; set; }

        public int PartnerId { get; set; }

        public int ClientId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
        
        public string UserName { get; set; }

        public string LanguageId { get; set; }

        public string Country { get; set; }

        public string Ip { get; set; }
        
        public int? ProductId { get; set; }
        public string ProductName { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? LastUpdateTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int State { get; set; }

        public int DeviceType { get; set; }
        public string Source { get; set; }
        public int? LogoutType { get; set; }
    }
}