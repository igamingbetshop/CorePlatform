using System;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.ContentModels
{
    public class ApiAnnouncement
    {
        public long? Id { get; set; }
        public int PartnerId { get; set; }
        public int? UserId { get; set; }
        public int? ReceiverId { get; set; }
        public int? ReceiverTypeId { get; set; }
        public List<int> Receivers { get; set; }
        public string NickName { get; set; }
        public int Type { get; set; }
        public int State { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}