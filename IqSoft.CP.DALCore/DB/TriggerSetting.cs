﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class TriggerSetting
    {
        public TriggerSetting()
        {
            BonusPaymentSystemSettings = new HashSet<BonusPaymentSystemSetting>();
            ClientBonus = new HashSet<ClientBonu>();
            ClientBonusTriggers = new HashSet<ClientBonusTrigger>();
            TriggerGroupSettings = new HashSet<TriggerGroupSetting>();
            TriggerProductSettings = new HashSet<TriggerProductSetting>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long TranslationId { get; set; }
        public int Type { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public int Percent { get; set; }
        public string BonusSettingCodes { get; set; }
        public int PartnerId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string Condition { get; set; }
        public int? MinBetCount { get; set; }
        public int? SegmentId { get; set; }
        public int? DayOfWeek { get; set; }
        public decimal? UpToAmount { get; set; }

        public virtual Partner Partner { get; set; }
        public virtual Segment Segment { get; set; }
        public virtual Translation Translation { get; set; }
        public virtual ICollection<BonusPaymentSystemSetting> BonusPaymentSystemSettings { get; set; }
        public virtual ICollection<ClientBonu> ClientBonus { get; set; }
        public virtual ICollection<ClientBonusTrigger> ClientBonusTriggers { get; set; }
        public virtual ICollection<TriggerGroupSetting> TriggerGroupSettings { get; set; }
        public virtual ICollection<TriggerProductSetting> TriggerProductSettings { get; set; }
    }
}