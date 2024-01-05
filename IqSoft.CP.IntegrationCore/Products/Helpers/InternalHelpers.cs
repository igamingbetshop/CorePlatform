using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.IntegrationCore.Products.Models.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class InternalHelpers
    {
        private static AppSettingModel AppSettings;
        static InternalHelpers()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configurationRoot = builder.Build();
            AppSettings = configurationRoot.GetSection("AppConfiguration").Get<AppSettingModel>();
        }
        public static HttpRequestInput GetBetInfo(BllProduct product, string externalTransactionId, string languageId, string productId)
        {
            var request = new
            {
                Method = "GetBetInfo",
                LanguageId = languageId,
                Credentials = AppSettings.WebSiteCredentials,
                RequestObject = externalTransactionId,
                ProductId = productId
            };
            var input = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                PostData = JsonConvert.SerializeObject(request)
            };

            switch (product.NickName)
            {
                case Constants.ProductDescriptions.Sportsbook:
                    input.Url = AppSettings.SportsbookConnectionUrl;
                    break;
                default:
                    input.Url = AppSettings.VirtualGamesConnectionUrl;
                    break;
            }
            return input;
        }

        public static HttpRequestInput GetBonusBalance(int playerId, BllProduct product, SessionIdentity sessionIdentity)
        {
            var request = new
            {
                Method = "GetBonusBalance",
                LanguageId = sessionIdentity.LanguageId,
                Credentials = AppSettings.WebSiteCredentials,
                RequestObject = JsonConvert.SerializeObject(new { PartnerId = sessionIdentity.PartnerId, PlayerId = playerId })
            };
            var input = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                PostData = JsonConvert.SerializeObject(request)
            };

            switch (product.NickName)
            {
                case Constants.ProductDescriptions.Sportsbook:
                    input.Url = AppSettings.SportsbookConnectionUrl;
                    break;
                default:
                    break;
            }
            return input;
        }

        public static HttpRequestInput GetBonusBets(BllProduct product, int bonusId, string languageId, double timeZone)
        {
            var request = new
            {
                Method = "GetBonusBets",
                LanguageId = languageId,
                Credentials = AppSettings.WebSiteCredentials,
                RequestObject = JsonConvert.SerializeObject(new { PlayerBonusId = bonusId, TimeZone = timeZone })
            };
            var input = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                PostData = JsonConvert.SerializeObject(request)
            };

            switch (product.NickName)
            {
                case Constants.ProductDescriptions.Sportsbook:
                    input.Url = AppSettings.SportsbookConnectionUrl;
                    break;
                default:
                    break;
            }
            return input;
        }

        public static string GetReportPerRound(int productId, string roundId, string languageId)
        {
            var product = CacheManager.GetProductById(productId);
            if (product == null)
                throw BaseBll.CreateException(languageId, Constants.Errors.ProductNotFound);

            var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);

            switch (provider.Name)
            {
                case Constants.GameProviders.Ezugi:
                    return JsonConvert.SerializeObject(EzugiHelpers.GetReport(roundId, DateTime.UtcNow));
                default:
                    throw BaseBll.CreateException(languageId, Constants.Errors.WrongProductId);
            }
        }

        public static class BetTypes
        {
            public const int Single = 1;
            public const int Multiple = 2;
            public const int System = 3;
            public const int Chain = 4;
        }

        public enum BetStatus
        {
            Uncalculated = 1,
            Won = 2,
            Lost = 3,
            Canceled = 4,
            Cashouted = 5,
            Returned = 6
        };

        public static int GetMappedStatus(int productStatus)
        {
            switch (productStatus)
            {
                case (int)BetStatus.Uncalculated:
                    return (int)BetDocumentStates.Uncalculated;
                case (int)BetStatus.Won:
                    return (int)BetDocumentStates.Won;
                case (int)BetStatus.Lost:
                    return (int)BetDocumentStates.Lost;
                case (int)BetStatus.Canceled:
                    return (int)BetDocumentStates.Deleted;
                case (int)BetStatus.Cashouted:
                    return (int)BetDocumentStates.Cashouted;
                case (int)BetStatus.Returned:
                    return (int)BetDocumentStates.Returned;
                default:
                    return (int)BetDocumentStates.Uncalculated;
            }
        }
    }
}
