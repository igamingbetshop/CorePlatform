﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class BonusCurrencySetting
    {
        public int Id { get; set; }
        public int BonusId { get; set; }
        public string CurrencyId { get; set; }
        public int Type { get; set; }

        public virtual Bonu Bonus { get; set; }
        public virtual Currency Currency { get; set; }
    }
}