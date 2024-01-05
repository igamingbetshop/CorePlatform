using System;

namespace IqSoft.CP.AdminWebApi.Models.NotificationModels
{
    public class ClientMessageModel
    {
        public long Id { get; set; }

        public int PartnerId { get; set; }

        public int ClientId { get; set; }

        public string UserName { get; set; }

        public string Message { get; set; }

        public int Type { get; set; }
        
        public int? Status { get; set; }

        public DateTime CreationTime { get; set; }
        
        public string Subject { get; set; }
        public string MobileOrEmail { get; set; }
    }
}