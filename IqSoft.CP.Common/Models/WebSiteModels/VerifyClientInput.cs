using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class VerifyClientInput
    {
        public int ClientId { get; set; }
        public string Key { get; set; }
        public List<SecurityQuestion> SecurityQuestions { get; set; }
    }
}