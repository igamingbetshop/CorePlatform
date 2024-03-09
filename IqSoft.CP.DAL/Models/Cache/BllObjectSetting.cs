using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllObjectSetting
    {
        public int? Type { get; set; }
        public List<int> Ids { get; set; }
        public List<string> Names { get; set; }
    }
}