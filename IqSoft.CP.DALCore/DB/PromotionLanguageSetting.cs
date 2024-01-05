using IqSoft.CP.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL
{
    public partial class PromotionLanguageSetting
    {
        public int Id { get; set; }
        public int PromotionId { get; set; }
        public string LanguageId { get; set; }
        public int Type { get; set; }

        public virtual Language Language { get; set; }
        public virtual Promotion Promotion { get; set; }
    }
}
