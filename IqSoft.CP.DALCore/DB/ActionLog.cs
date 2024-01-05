﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class ActionLog
    {
        public long Id { get; set; }
        public int ActionId { get; set; }
        public string Domain { get; set; }
        public string Source { get; set; }
        public string Ip { get; set; }
        public string Country { get; set; }
        public long? SessionId { get; set; }
        public string Page { get; set; }
        public long? ObjectId { get; set; }
        public int? ObjectTypeId { get; set; }
        public string Language { get; set; }
        public int? ResultCode { get; set; }
        public string Description { get; set; }
        public string Info { get; set; }
        public DateTime CreationTime { get; set; }
        public long Date { get; set; }

        public virtual UserSession Session { get; set; }
    }
}