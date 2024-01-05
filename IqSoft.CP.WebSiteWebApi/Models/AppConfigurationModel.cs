using System.Collections.Generic;

namespace IqSoft.CP.WebSiteWebApi.Models
{
    public class AppConfigurationModel
    {
        public List<string> MasterCacheConnectionUrl { get; set; }
        public string ProductGatewayHostAddress { get; set; }
        public string PaymentGatewayHostAddress { get; set; }
        public string AdminHostAddress { get; set; }
        public string TelegramBotToken { get; set; }
        public string[] AllowOrigins { get; set; }
    }
}