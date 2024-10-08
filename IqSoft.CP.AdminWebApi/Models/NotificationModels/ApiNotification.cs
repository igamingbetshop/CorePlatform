using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IqSoft.CP.AdminWebApi.Models.NotificationModels
{
    public class ApiNotification
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public int TypeId { get; set; }
        public int? ClientId { get; set; }
        public long? PaymentRequestId { get; set; }
        public int? BonusId { get; set; }
        public int Status { get; set; }
        public DateTime CreationTime { get; set; }
    }
}