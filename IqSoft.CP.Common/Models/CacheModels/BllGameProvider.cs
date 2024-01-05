using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.CacheModels
{
    [Serializable]
    public class BllGameProvider
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int Type { get; set; }

        public int? SessionExpireTime { get; set; }

        public string GameLaunchUrl { get; set; }
        public List<BllCurrencySetting> CurrencySetting { get; set; }
    }
}
