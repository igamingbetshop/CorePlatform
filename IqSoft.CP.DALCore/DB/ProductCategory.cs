﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class ProductCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public long TranslationId { get; set; }

        public virtual Translation Translation { get; set; }
    }
}