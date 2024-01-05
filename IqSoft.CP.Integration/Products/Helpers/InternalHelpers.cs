using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using Newtonsoft.Json;
using System;
using System.Configuration;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class InternalHelpers
    {
        public static HttpRequestInput GetBetInfo(BllProduct product, string externalTransactionId, string languageId, string productId, int partnerId)
        {
            var request = new
            {
                Method = "GetBetInfo",
                LanguageId = languageId,
                Credentials = ConfigurationManager.AppSettings["WebSiteCredentials"],
                RequestObject = externalTransactionId,
                ProductId = productId,
                PartnerId = partnerId
            };
            var input = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                PostData = JsonConvert.SerializeObject(request)
            };

            switch (product.NickName)
            {
                case ProductDescriptions.Sportsbook:
                    input.Url = ConfigurationManager.AppSettings["SportsbookConnectionUrl"];
                    break;
                case ProductDescriptions.Bingo37:
                case ProductDescriptions.Colors:
                case ProductDescriptions.Bingo48:
                case ProductDescriptions.Keno:
                case ProductDescriptions.BetOnPoker:
                case ProductDescriptions.BetOnRacing:
                case ProductDescriptions.HighLow:
                case ProductDescriptions.Crash:
                case ProductDescriptions.Lottery:
                case ProductDescriptions.Minesweeper:
                case ProductDescriptions.SpinAndWin:
                case ProductDescriptions.BlackJack:
                    input.Url = ConfigurationManager.AppSettings["VirtualGamesConnectionUrl"];
                    break;
                default:
                    return null;
            }
            return input;
        }

        public static HttpRequestInput GetBonusBalance(int playerId, BllProduct product, SessionIdentity sessionIdentity)
        {
            var request = new
            {
                Method = "GetBonusBalance",
                LanguageId = sessionIdentity.LanguageId,
                Credentials = ConfigurationManager.AppSettings["WebSiteCredentials"],
                RequestObject = JsonConvert.SerializeObject(new { PartnerId = sessionIdentity.PartnerId, PlayerId = playerId })
            };
            var input = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                PostData = JsonConvert.SerializeObject(request)
            };

            switch (product.NickName)
            {
                case Constants.ProductDescriptions.Sportsbook:
                    input.Url = ConfigurationManager.AppSettings["SportsbookConnectionUrl"];
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
                Credentials = ConfigurationManager.AppSettings["WebSiteCredentials"],
                RequestObject = JsonConvert.SerializeObject(new { PlayerBonusId = bonusId, TimeZone = timeZone })
            };
            var input = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                PostData = JsonConvert.SerializeObject(request)
            };

            switch (product.NickName)
            {
                case Constants.ProductDescriptions.Sportsbook:
                    input.Url = ConfigurationManager.AppSettings["SportsbookConnectionUrl"];
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
            switch(productStatus)
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

        public static class ProductDescriptions
        {
            public const string Sportsbook = "Sportsbook";
            public const string Comparison = "Comparison";
            public const string Bingo37 = "Bingo37";
            public const string Colors = "Colors";
            public const string Bingo48 = "Bingo48";
            public const string Keno = "Keno";
            public const string BetOnPoker = "BetOnPoker";
            public const string BetOnRacing = "BetOnRacing";
            public const string HighLow = "HighLow";
            public const string Crash = "Crash";
            public const string Lottery = "Lottery";
            public const string Minesweeper = "Minesweeper";
            public const string SpinAndWin = "SpinAndWin";
            public const string BlackJack = "BlackJack";
            public const string Okey101 = "Okey101";
            public const string Backgammon = "Backgammon";
            public const string Blockball = "Blockball";
        };
    }
}
