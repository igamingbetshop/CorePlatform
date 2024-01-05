using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterPlacedBets
    {
        public int? PartnerId { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateBefore { get; set; }
    }
}