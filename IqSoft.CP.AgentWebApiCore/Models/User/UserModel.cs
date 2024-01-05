using IqSoft.CP.Common.Models.AgentModels;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.AgentWebApi.Models.User
{
    public class UserModel
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? Gender { get; set; }
        public string LanguageId { get; set; }
        public string UserName { get; set; }
        public string UserNamePrefix { get; set; }
        public string CloningUserName { get; set; }
        public string NickName { get; set; }
        public string MobileNumber { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public bool IsTwoFactorEnabled { get; set; }
        public int State { get; set; }
        public int? ParentState { get; set; }
        public bool? Closed { get; set; }
        public int Type { get; set; }
        public string CurrencyId { get; set; }
        public string Email { get; set; }
        public int? ParentId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastLogin { get; set; }
        public string LoginIp { get; set; }
        public int? ClientCount { get; set; }
        public int? DirectClientCount { get; set; }
        public decimal? Balance { get; set; }
        public int? Level { get; set; }
        public int? Count { get; set; }
        // public ProductSetting ProductsSettings { get; set; }
        public decimal TotalBetAmount { get; set; }
        public decimal DirectBetAmount { get; set; }
        public decimal TotalWinAmount { get; set; }
        public decimal DirectWinAmount { get; set; }
        public decimal TotalGGR { get; set; }
        public decimal DirectGGR { get; set; }
        public decimal TotalTurnoverProfit { get; set; }
        public decimal DirectTurnoverProfit { get; set; }
        public decimal TotalGGRProfit { get; set; }
        public decimal DirectGGRProfit { get; set; }
        public bool? ViewBetsAndForecast { get; set; }
        public bool? ViewReport { get; set; }
        public bool? ViewBetsLists { get; set; }
        public bool? ViewTransfer { get; set; }
        public bool? ViewLog { get; set; }
        public int? MemberInformationPermission { get; set; }
        public List<int> CalculationPeriod { get; set; }
        public bool? AllowAutoPT { get; set; }
        public bool? AllowParentAutoPT { get; set; }
        public bool? AllowOutright { get; set; }
        public bool? AllowParentOutright { get; set; }
        public bool? AllowDoubleCommission { get; set; }
        public bool? AllowParentDoubleCommission { get; set; }
        public decimal? MaxCredit { get; set; }
        public List<LevelLimit> LevelLimits { get; set; }
        public List<CountLimit> CountLimits { get; set; }
        public List<decimal> Commissions { get; set; }
        public List<PositionTaking> PositionTakings { get; set; }
        public AsianCommissionPlan CommissionPlan { get; set; }       
    }
}