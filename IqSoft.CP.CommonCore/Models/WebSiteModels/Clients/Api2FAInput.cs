using IqSoft.CP.Common.Models.WebSiteModels;

namespace IqSoft.CP.CommonCore.Models.WebSiteModels.Clients
{
    public class Api2FAInput : RequestBase
    {
        public string Pin { get; set; }
    }
}
