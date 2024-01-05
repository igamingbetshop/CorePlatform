using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetClientMessagesOutput : ApiResponseBase
    {
        public List<ClientMessageModel> Messages { get; set; }
        public long Count { get; set; }
    }
}