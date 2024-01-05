using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetAvailableLanguagesOutput : ApiResponseBase
    {
        public List<Language> Languages { get; set; }
    }
}