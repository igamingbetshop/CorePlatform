using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllProductCountrySetting
    {
        public int ProductId { get; set; }
        public List<string> Countries { get; set; }
    }
}
