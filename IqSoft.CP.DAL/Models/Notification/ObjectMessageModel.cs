using System;

namespace IqSoft.CP.DAL.Models.Notification
{
    public class ObjectMessageModel
    {
        public int PartnerId { get; set; }
        public long MessageId { get; set; }
        public int? Id { get; set; }
        public string UserName { get; set; }
        public string MobileOrEmail { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public int MessageType { get; set; }        
        public int? Status { get; set; }
        public DateTime CreationTime { get; set; }        
    }
}