using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;

namespace IqSoft.CP.AgentWebApi.Models.User
{
    public class Session : ResponseBase
    {
        public int? UserId { get; set; }
        public int? AffiliateId { get; set; }
        public string LoginIp { get; set; }
        public string LanguageId { get; set; }
        public long SessionId { get; set; }
        public string Token { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string UserName { get; set; }
        public string CurrencyId { get; set; }
        public int State { get; set; }
        public int Type { get; set; }
        public string ImageData { get; set; }
        public int? Level { get; set; }
        public int ParentLevel { get; set; }
        public bool? IsTwoFactorEnabled { get; set; }
        public object RequiredParameters { get; set; }
        public bool? AllowAutoPT { get; set; }
        public bool? AllowOutright { get; set; }
        public bool? AllowDoubleCommission { get; set; }
        public bool? IsCalculationPeriodBlocked { get; set; }
        public decimal? AgentMaxCredit { get; set; }
        public Session()
        {
        }

        public Session(SessionIdentity s, BllUser u)
        {
            UserId = s.Id;
            LoginIp = s.LoginIp;
            LanguageId = s.LanguageId;
            SessionId = s.SessionId;
            Token = s.Token;
            CurrencyId = s.CurrencyId;
            State = s.State;
            Type = u.Type;
            RequiredParameters = s.RequiredParameters;
            UserName = u.UserName;
            NickName = u.NickName;
            FirstName = u.FirstName;
            LastName = u.LastName;
            IsTwoFactorEnabled = u.IsTwoFactorEnabled;
            Level = u.Level;
        }
    }
}