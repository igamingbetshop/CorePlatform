using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.AdminWebApi.Models.PartnerModels
{
    public class ApiPartnerKey
    {
        public int Id { get; set; }
        public int? PartnerId { get; set; }
        public int? GameProviderId { get; set; }
        public int? PaymentSystemId { get; set; }
        public string Name { get; set; }
        public string StringValue { get; set; }
        public DateTime? DateValue { get; set; }
        public long? NumericValue { get; set; }
        public int? NotificationServiceId { get; set; }
    }
}