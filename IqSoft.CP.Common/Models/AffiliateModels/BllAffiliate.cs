using System;

namespace IqSoft.CP.Common.Models.AffiliateModels
{
    public class BllAffiliate : ApiAffiliateCommission
    {        
        public int PartnerId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public int Gender { get; set; }
        public int RegionId { get; set; }
        public string LanguageId { get; set; }
        public string PasswordHash { get; set; }
        public int Salt { get; set; }
        public int State { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public int CommunicationType { get; set; }
        public string CommunicationTypeValue { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public int? ClientId { get; set; }
    }
}
