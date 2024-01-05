using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.AdminModels
{
    public class ApiAnnouncement
    {
        public long? Id { get; set; }
        public int PartnerId { get; set; }
        public string NickName { get; set; }
        public int Type { get; set; }
        public int ReceiverType { get; set; }
        public int State { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public List<int> ClientIds { get; set; }
        public List<int> UserIds { get; set; }
        public List<int> SegmentIds { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}