﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class ObjectAction
    {
        public int Id { get; set; }
        public int ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public int Type { get; set; }
        public int State { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }

        public virtual ObjectType ObjectType { get; set; }
    }
}