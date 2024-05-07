using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL.Models.Dashboard
{
    public class CountryClientsInfo
    {
        public int? CountryId { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public int TotalCount { get; set; }
        public double Percent { get; set; }
    }
}
