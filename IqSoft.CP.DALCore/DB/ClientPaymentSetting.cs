﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class ClientPaymentSetting
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int PartnerPaymentSettingId { get; set; }
        public int State { get; set; }

        public virtual Client Client { get; set; }
        public virtual PartnerPaymentSetting PartnerPaymentSetting { get; set; }
    }
}