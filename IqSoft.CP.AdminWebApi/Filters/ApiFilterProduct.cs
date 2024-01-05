using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterProduct : ApiFilterBase
    {
        public int? Id { get; set; }

        public int? GameProviderId { get; set; }

        public int? PaymentSystemId { get; set; }

        public int? ParentId { get; set; }

        public string Description { get; set; }

        public string ExternalId { get; set; }
    }
}
