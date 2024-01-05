using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterRole : ApiFilterBase
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public List<string> PermissionIds { get; set; }
    }
}