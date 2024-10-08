using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Cache
{
	[Serializable]
	public class BonusInfo
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int PartnerId { get; set; }
		public int? FinalAccountTypeId { get; set; }
		public int? WinAccountTypeId { get; set; }
		public int Status { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime FinishTime { get; set; }
		public int Period { get; set; }
		public int Type { get; set; }
		public int? TurnoverCount { get; set; }
		public string Info { get; set; }
		public decimal? MinAmount { get; set; }
		public decimal? MaxAmount { get; set; }
		public int? Priority { get; set; }
		public int? ValidForAwarding { get; set; }
		public int? ValidForSpending { get; set; }
		public int? Sequence { get; set; }
		public string Condition { get; set; }
		public List<int> PaymentSystems { get; set; }
		public bool HasPromo { get; set; }
		public int ReusingMaxCount { get; set; }
		public bool? FreezeBonusBalance { get; set; }
		public int? Regularity { get; set; }
		public int? DayOfWeek { get; set; }
		public int? ReusingMaxCountInPeriod { get; set; }
		public long TranslationId { get; set; }
        public string Color { get; set; }

        public List<TriggerGroupInfo> TriggerGroups { get; set; }

	}
}
