﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class ClientLog
    {
        public long Id { get; set; }
        public int Action { get; set; }
        public int ClientId { get; set; }
        public int? UserId { get; set; }
        public string Ip { get; set; }
        public string Page { get; set; }
        public DateTime CreationTime { get; set; }
        public long? ClientSessionId { get; set; }

        public virtual Client Client { get; set; }
        public virtual ClientSession ClientSession { get; set; }
    }
}