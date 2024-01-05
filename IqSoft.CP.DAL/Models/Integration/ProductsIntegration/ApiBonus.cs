using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL.Models.Integration.ProductsIntegration
{
    public class ApiBonus
    {
        public int Id { get; set; }
        public int BonusId { get; set; }
        public int BonusType { get; set; }
        public string Condition { get; set; }
        public decimal Amount { get; set; }
        public string Name { get; set; }
        public bool AllowSplit { get; set; }
    }
}
