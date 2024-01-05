﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class PartnerKey
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

        public virtual GameProvider GameProvider { get; set; }
        public virtual Partner Partner { get; set; }
        public virtual PaymentSystem PaymentSystem { get; set; }
    }
}