using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.Bonus;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using System;


namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class EndorphinaHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.Endorphina);

        public static string GetLaunchUrl(int partnerId, string token, int productId, bool isForDemo, SessionIdentity session, ILog log)
        {
            var casinoPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CasinoPageUrl).StringValue;
            if (string.IsNullOrEmpty(casinoPageUrl))
                casinoPageUrl = string.Format("https://{0}/casino/all-games", session.Domain);
            else
                casinoPageUrl = string.Format(casinoPageUrl, session.Domain);
            var product = CacheManager.GetProductById(productId);
            if (isForDemo)
            {
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = Constants.HttpRequestMethods.Get,
                    Url = $"https://edemo.endorphina.com/api/link/accountId/9789/hash/{CommonFunctions.ComputeMd5(product.ExternalId)}" +
                          $"/returnURL/{Uri.EscapeDataString(Uri.EscapeDataString(casinoPageUrl))}"
                };
                return CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            }

            var salt = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EndorphinaSalt);
            var merchantId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EndorphinaMerchantId);
            var requestData = new 
            {
                exit = casinoPageUrl,
                nodeId = merchantId,
                token,
                sign = CommonFunctions.ComputeSha1($"{casinoPageUrl}{merchantId}{token}{salt}")
            };
            return $"{Provider.GameLaunchUrl}?{CommonFunctions.GetUriEndocingFromObject(requestData)}";
        }

        public static void AddFreeRound(FreeSpinModel freespinModel, ILog log)
        {
            var inputString = string.Empty;
            try
            {
                var client = CacheManager.GetClientById(freespinModel.ClientId);
                var product = CacheManager.GetProductByExternalId(Provider.Id, freespinModel.ProductExternalId);
                var salt = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.EndorphinaSalt);
                var merchantId = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.EndorphinaMerchantId);

                var requestInput = new 
                {
                    currency = client.CurrencyId,
                    expires = freespinModel.FinishTime.ToString("yyyy-MM-ddThh:mm:ssZ"),
                    game = product.ExternalId,
                    id = freespinModel.BonusId.ToString(),
                    name = "FreeSpin Bonus",
                    nodeId = merchantId,
                    player = client.Id.ToString()
                };
                var sign = CommonFunctions.ComputeSha1($"{CommonFunctions.GetSortedValuesAsString(requestInput)}{freespinModel.SpinCount}{(int)freespinModel.BetValueLevel}{salt}");

                var httpRequestInput = new HttpRequestInput
                {
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    Url = $"{Provider.GameLaunchUrl}/bonuses",
                    PostData = $"{CommonFunctions.GetUriEndocingFromObject(requestInput)}&spins.amount={freespinModel.SpinCount}&spins.totalBet={(int)freespinModel.BetValueLevel}&sign={sign}"
                };
                inputString = httpRequestInput.PostData;
                log.Info("Endorphina httpRequestInput: " + JsonConvert.SerializeObject(httpRequestInput));
                var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                log.Info("Endorphina res: " + res);
            }
            catch (Exception ex)
            {
                log.Error("Endorphina: " + inputString + " __Error: " + ex);
                throw;
            }
        }
    }
}
