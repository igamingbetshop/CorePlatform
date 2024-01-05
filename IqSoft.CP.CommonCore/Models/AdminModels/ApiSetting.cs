using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.AdminModels
{
    public class ApiSetting
    {
        public int? Type { get; set; }
        public List<int> Ids { get; set; }
        public List<string> Names { get; set; }
    }
}
