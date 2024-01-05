using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Integration.Products.Models.ZuesPlay;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class ZeusPlayHelpers
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.ZeusPlay).Id;
        public static string GetUrl(int partnerId, int productId, int clientId, string token, bool isForDemo)
        {
            var providerUrl = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.ZeusPlayCasinoUrl);
            var product = CacheManager.GetProductById(productId);

            if (isForDemo)
                return string.Format("{0}/demo/partner_get_game_args_for_demo/{1}", providerUrl, product.ExternalId);

            var client = CacheManager.GetClientById(clientId);
            var input = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Get,
                Url = string.Format("{0}/partner_get_game_args/{1}/{2}/{3}/{4}", providerUrl, token, product.ExternalId, client.CurrencyId, clientId)
            };

            using (var stream = CommonFunctions.SendHttpRequestForStream(input, out _, SecurityProtocolType.Tls12))
            {
                var deserializer = new XmlSerializer(typeof(BaseOutput), new XmlRootAttribute("flash_game"));
                var output = (BaseOutput)deserializer.Deserialize(stream);
                if (output.success)
                {
                    var regEx = new Regex("src=\"(.*)\"");
                    var matches = regEx.Matches(output.game_html);
                    return matches[0].Groups[1].Value;
                }
                return output.error_id;
            }
        }
    }
}