using System;
using System.Collections.Generic;

namespace IqSoft.CP.AgentWebApi.Models
{
    public class ApiAnnouncement
    {
        public long? Id { get; set; }
        public int PartnerId { get; set; }
        public int? UserId { get; set; }
        public int? ReceiverId { get; set; }
        public List<int> Receivers { get; set; }
        public string Message { get; set; }
        public int Type { get; set; }
        public int State { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}