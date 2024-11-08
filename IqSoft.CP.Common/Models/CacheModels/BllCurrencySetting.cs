﻿using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.CacheModels
{
    [Serializable]
    public class BllCurrencySetting
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public List<string> CurrencyIds { get; set; }
    }
}
