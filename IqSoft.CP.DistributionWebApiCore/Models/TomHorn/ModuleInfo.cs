using System.Collections.Generic;

namespace IqSoft.CP.DistributionWebApi.Models.TomHorn
{
    public class ModuleInfo
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public List<Parameter> Parameters { get; set; }
    }

    public class Parameter
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
