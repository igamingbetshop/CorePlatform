﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class PartnerLanguageSetting
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string LanguageId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public int State { get; set; }

        public virtual Language Language { get; set; }
        public virtual Partner Partner { get; set; }
    }
}