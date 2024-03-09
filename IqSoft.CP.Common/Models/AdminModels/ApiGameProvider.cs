using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.AdminModels
{
    public class ApiGameProvider
    {
        public int? Id { get; set; }
        public List<int> Ids { get; set; }
        public int? Type { get; set; }
        public int? SessionExpireTime { get; set; }
        public string Name { get; set; }
        public string GameLaunchUrl { get; set; }
        public bool? IsActive { get; set; }
        public ApiSetting CurrencySetting { get; set; }
        public List<ApiGameProviderCurrencySetting> GameProviderCurrencySettings { get; set; }
    }
    public class ApiGameProviderCurrencySetting
    {
        public int Id { get; set; }
        public int GameProviderId { get; set; }
        public Nullable<int> PartnerId { get; set; }
        public string CurrencyId { get; set; }
        public int Type { get; set; }
    }
}