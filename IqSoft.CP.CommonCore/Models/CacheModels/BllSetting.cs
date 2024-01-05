using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.CacheModels
{
    [Serializable]
    public class BllSetting
    {
        public int? Type { get; set; }
        public List<int> Ids { get; set; }
        public List<string> Names { get; set; }
    }
}
