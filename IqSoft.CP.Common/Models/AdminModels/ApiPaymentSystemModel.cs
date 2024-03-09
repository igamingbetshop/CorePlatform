using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.AdminModels
{
    public class ApiPaymentSystemModel
    {
        public int? Id { get; set; }
        public List<int> Ids { get; set; }
        public string Name { get; set; }      
        public int? PeriodicityOfRequest { get; set; }
        public int? PaymentRequestSendCount { get; set; }
        public int? Type { get; set; }
        public bool? IsActive { get; set; }
        public int? ContentType { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}