using System;

namespace IqSoft.NGGP.DAL.Models.Integration.InternalGamesIntegration
{
    public class AuthorizationOutput : OutputBase
    {
        public string Token { get; set; }
        public int ClientId { get; set; }
        public string NickName { get; set; }
        public string CurrencyId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UInt64 TimeStamp { get; set; }
        public int Gender { get; set; }
        public decimal AvailableBalance { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
