using IqSoft.CP.BLL.Models;
using IqSoft.CP.DAL.Models;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.UserModels
{
    public class Session : ResponseBase
    {
        public int UserId { get; set; }

        public string LoginIp { get; set; }

        public string LanguageId { get; set; }

        public long SessionId { get; set; }

        public string Token { get; set; }

        public string UserName { get; set; }

        public string UserLogin { get; set; }

        public string CurrencyId { get; set; }
        
        public int? OddsType { get; set; }
        
        public object RequiredParameters { get; set; }
        
        public bool IsTwoFactorEnabled { get; set; }

        public int VipLevel { get; set; }
        
        public List<ApiAdminMenu> AdminMenu { get; set; }

        public Session()
        {
        
        }

        public Session(SessionIdentity s)
        {
            UserId = s.Id;
            LoginIp = s.LoginIp;
            LanguageId = s.LanguageId;
            SessionId = s.SessionId;
            Token = s.Token;
            CurrencyId = s.CurrencyId;
            OddsType = s.OddsType;
            RequiredParameters = s.RequiredParameters;
            IsTwoFactorEnabled = s.IsTwoFactorEnabled ?? false;
        }
    }
}