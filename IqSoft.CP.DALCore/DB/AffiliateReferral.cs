﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class AffiliateReferral
    {
        public AffiliateReferral()
        {
            Clients = new HashSet<Client>();
        }

        public int Id { get; set; }
        public int AffiliatePlatformId { get; set; }
        public string AffiliateId { get; set; }
        public string RefId { get; set; }
        public int Type { get; set; }
        public DateTime? LastProcessedBonusTime { get; set; }

        public virtual AffiliatePlatform AffiliatePlatform { get; set; }
        public virtual ICollection<Client> Clients { get; set; }
    }
}