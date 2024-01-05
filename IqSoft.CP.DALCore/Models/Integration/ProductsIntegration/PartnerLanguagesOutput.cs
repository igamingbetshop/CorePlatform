using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL.Models.Integration.ProductsIntegration
{
    public class PartnerLanguagesOutput : ResponseBase
    {
        public List<ApiLanguage> Languages { get; set; }
    }
}
