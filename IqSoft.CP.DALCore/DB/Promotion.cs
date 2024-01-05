﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class Promotion
    {
        public Promotion()
        {
            PromotionSegmentSettings = new HashSet<PromotionSegmentSetting>();
        }

        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string NickName { get; set; }
        public long TitleTranslationId { get; set; }
        public long DescriptionTranslationId { get; set; }
        public long ContentTranslationId { get; set; }
        public int Type { get; set; }
        public string ImageName { get; set; }
        public int State { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public int Order { get; set; }

        public virtual Translation ContentTranslation { get; set; }
        public virtual Translation DescriptionTranslation { get; set; }
        public virtual Partner Partner { get; set; }
        public virtual Translation TitleTranslation { get; set; }
        public virtual ICollection<PromotionSegmentSetting> PromotionSegmentSettings { get; set; }
        public virtual ICollection<PromotionLanguageSetting> PromotionLanguageSettings { get; set; }
    }
}