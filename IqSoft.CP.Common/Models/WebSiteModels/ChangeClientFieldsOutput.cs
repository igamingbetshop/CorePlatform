using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ChangeClientFieldsOutput : ApiResponseBase
    {
        public List<ClientModel> Clients { get; set; }
    }
}