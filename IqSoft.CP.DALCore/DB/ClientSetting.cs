﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class ClientSetting
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string Name { get; set; }
        public decimal? NumericValue { get; set; }
        public string StringValue { get; set; }
        public DateTime? DateValue { get; set; }
        public int? UserId { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? LastUpdateTime { get; set; }

        public virtual Client Client { get; set; }
        public virtual User User { get; set; }
    }
}