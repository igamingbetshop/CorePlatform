using IqSoft.CP.Common.Attributes;
using IqSoft.CP.Common.Models.AffiliateModels;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.ClientModels.Models
{
    public class ApiFnAffiliateModel
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string UserName { get; set; }
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
        public FixedFeeCommission FixedFeeCommission { get; set; }
        public DepositCommission DepositCommission { get; set; }
        public List<BetCommission> TurnoverCommission { get; set; }
        public List<BetCommission> GGRCommission { get; set; }
		public int CommunicationType { get; set; }
		public string CommunicationTypeValue { get; set; }
        public int? ClientId { get; set; }
    }
}