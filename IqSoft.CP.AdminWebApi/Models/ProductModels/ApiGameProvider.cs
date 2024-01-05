using IqSoft.CP.Common.Models.AdminModels;

namespace IqSoft.CP.AdminWebApi.Models.ProductModels
{
    public class ApiGameProvider
    {
        public int Id { get; set; }
        public int? Type { get; set; }
        public int? SessionExpireTime { get; set; }
        public string Name { get; set; }
        public string GameLaunchUrl { get; set; }
        public ApiSetting CurrencySetting { get; set; }
    }
}