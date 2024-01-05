using IqSoft.CP.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
    public class ApiFilterPartnerPaymentsSummary : ApiFilterBase
    {
        public int PartnerId { get; set; }

        public int Type { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }
}