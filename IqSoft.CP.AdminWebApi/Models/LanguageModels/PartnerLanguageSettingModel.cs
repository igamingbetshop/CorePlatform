using System;

namespace IqSoft.CP.AdminWebApi.Models.LanguageModels
{
    public class PartnerLanguageSettingModel
    {
        public int Id { get; set; }

        public int PartnerId { get; set; }

        public string LanguageId { get; set; }

        public int State { get; set; }
        public int? Order { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastUpdateTime { get; set; }
    }
}