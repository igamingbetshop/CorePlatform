using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.AdminWebApi.Models.BonusModels
{
    public class ApiAmountSetting
    {
        public string CurrencyId { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public decimal? UpToAmount { get; set; }
        public decimal? Amount { get; set; }
    }
}
