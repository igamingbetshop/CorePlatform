using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models.Cache;

namespace IqSoft.CP.DAL.Models.Clients
{
    public class ClientLoginOut : RegionTree
    {
        public BllClientSession LastSession { get; set; }
        public string NewToken { get; set; }
        public bool ResetPassword { get; set; }
        public bool AcceptTermsConditions { get; set; }
        public int? DocumentExpirationStatus { get; set; }
    }
}
