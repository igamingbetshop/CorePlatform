﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class CRMSetting
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string NickeName { get; set; }
        public int State { get; set; }
        public int Type { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public int? Sequence { get; set; }
        public string Condition { get; set; }

        public virtual Partner Partner { get; set; }
    }
}