using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ClientPasswordRecovery : ApiRequestBase
    {
        public string RecoveryToken { get; set; }

        public string NewPassword { get; set; }

        public List<SecurityQuestion> SecurityQuestions { get; set; }
    }
}