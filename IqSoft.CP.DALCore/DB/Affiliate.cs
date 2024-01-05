using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL
{
    public class Affiliate
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Gender { get; set; }
        public int RegionId { get; set; }
        public string LanguageId { get; set; }
        public string NickName { get; set; }
        public string PasswordHash { get; set; }
        public int Salt { get; set; }
        public int State { get; set; }
        public string CurrencyId { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public long SessionId { get; set; }

        public virtual Currency Currency { get; set; }
        public virtual Language Language { get; set; }
        public virtual Partner Partner { get; set; }
        public virtual UserSession UserSession { get; set; }
    }
}
