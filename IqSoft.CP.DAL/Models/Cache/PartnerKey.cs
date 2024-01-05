using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class PartnerKey
    {
        public int Id { get; set; }
        public int? PartnerId { get; set; }
        public int? GameProviderId { get; set; }
        public int? PaymentSystemId { get; set; }
        public string Name { get; set; }
        public string StringValue { get; set; }
        public DateTime? DateValue { get; set; }
        public decimal? NumericValue { get; set; }
        public int? NotificationServiceId { get; set; }
    }
}
