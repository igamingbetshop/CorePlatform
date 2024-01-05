using IqSoft.CP.Common.Attributes;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.UserModels
{
    public class UserModel
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        [NotExcelProperty]
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Gender { get; set; }
        public string LanguageId { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string MobileNumber { get; set; }
        public int State { get; set; }
        public int Type { get; set; }
        public string CurrencyId { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int? ParentId { get; set; }
        [NotExcelProperty]
        public DateTime CreationTime { get; set; }
        public int? ClientCount { get; set; }
        public List<DAL.Models.ObjectAccount> Accounts { get; set; }
        public int? Level { get; set; }
        public int? OddsType { get; set; }
        public string UserRoles { get; set; }
        public decimal TotalBetAmount { get; set; }
        public decimal TotalWinAmount { get; set; }
        public decimal TotalGGR { get; set; }
        public decimal TotalTurnoverProfit { get; set; }
        public decimal TotalGGRProfit { get; set; }

    }
}