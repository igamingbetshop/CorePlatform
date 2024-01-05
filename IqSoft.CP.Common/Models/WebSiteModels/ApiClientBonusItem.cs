using System;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
	public class ApiClientBonusItem
	{
		public int Id { get; set; }
        public int BonusId { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? AwardingTime { get; set; }
        public decimal? TurnoverAmountLeft { get; set; }
        public decimal? FinalAmount { get; set; }
        public string StateName { get; set; } 
        public DateTime? CalculationTime { get; set; }
        public long? ReuseNumber { get; set; }
        public int StatusId { get; set; }
        public int TypeId { get; set; }
        public int? TurnoverCount { get; set; }

        //
        //public int PlayerId { get; set; }

        //public int PartnerId { get; set; }

        //public int SettingId { get; set; }


        //	public int? ChannelId { get; set; }

        //	public int SelectionsMinCount { get; set; }

        //	public int? SelectionsMaxCount { get; set; }

        //	public decimal? BonusPercent { get; set; }


        //public int? TurnoverCount { get; set; }

        //public decimal? MinCoeff { get; set; }


        //public DateTime CreationTime { get; set; }




        //public DateTime? FinishTime { get; set; }
        //	public decimal? RemainingCredit { get; set; }
        //	public decimal? WageringTarget { get; set; }
    }
}
