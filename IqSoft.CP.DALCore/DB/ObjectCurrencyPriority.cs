﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class ObjectCurrencyPriority
    {
        public long Id { get; set; }
        public int ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public string CurrencyId { get; set; }
        public int Priority { get; set; }

        public virtual Currency Currency { get; set; }
        public virtual ObjectType ObjectType { get; set; }
    }
}