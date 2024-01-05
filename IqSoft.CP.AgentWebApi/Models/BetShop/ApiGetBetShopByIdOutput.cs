using System;
using System.Collections.Generic;

namespace IqSoft.CP.AgentWebApi.Models
{
    public class ApiGetBetShopByIdOutput
	{
        public int Id { get; set; }
        public string GroupName { get; set; }
        public string PartnerName { get; set; }
        public string CurrencyId { get; set; }
        public string Address { get; set; }
        public string StateName { get; set; }
        public decimal CurrentLimit { get; set; }
        public string Name { get; set; }
        public int GroupId { get; set; }
        public long SessionId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public int PartnerId { get; set; }
        public int State { get; set; }
        public decimal DefaultLimit { get; set; }
        public int DailyTicketNumber { get; set; }
        public List<CashDeskModel> CashDeskModels { get; set; }
        public int RegionId { get; set; }
        public decimal Balance { get; set; }
		public decimal BonusPercent { get; set; }
		public bool PrintLogo { get; set; }
        public int Type { get; set; }
        public int? AgentId { get; set; }
    }
}