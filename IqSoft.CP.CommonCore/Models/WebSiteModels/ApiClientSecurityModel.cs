using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ApiClientSecurityModel : ApiRequestBase
    {
        public List<SecurityQuestion> SecurityQuestions { get; set; }
        public string SMSCode { get; set; }
        public string EmailCode { get; set; }
    }
}
