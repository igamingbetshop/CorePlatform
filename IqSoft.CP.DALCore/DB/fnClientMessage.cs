﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnClientMessage
    {
        public long Id { get; set; }
        public Nullable<int> ClientId { get; set; }
        public string UserName { get; set; }
        public string MobileOrEmail { get; set; }
        public int PartnerId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public int MessageType { get; set; }
        public Nullable<int> Status { get; set; }
        public System.DateTime CreationTime { get; set; }
        public Nullable<int> AffiliateReferralId { get; set; }
    }
}
