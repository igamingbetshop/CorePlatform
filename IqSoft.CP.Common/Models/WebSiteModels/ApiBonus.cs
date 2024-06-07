using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ApiBonus
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PartnerId { get; set; }
        public int Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public int Type { get; set; }
        public string Info { get; set; }
        public int? Sequence { get; set; }

    }
}
