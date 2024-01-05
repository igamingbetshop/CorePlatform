using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Integration.Products.Models.EkkoSpin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class EkkoSpinHelpers
    {
        private const int operatorId = 29;

        private readonly static Dictionary<char, string> Hex = new Dictionary<char, string>
        {
            {'#', "%23" },
            {'/', "%2F" },
            {':', "%3A" }
            //{' ', "+" }, {'&', "%26" }, {'?', "%3F" }
        };

        public static string GetUrlFromProvider(int clientId, string urlBase, decimal balance, string externalId, string lang, string secretKey)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);
            string productGatewayUrl = string.Format(ConfigurationManager.AppSettings["LocalProductGatewayUrl"], client.PartnerId, "EkkoSpin", "Seamless");

            string callUrl = string.Format("{0}?id_user={1}&id_customer={2}&balance={3}&id_game={4}&language={5}&callback_url={6}",
                                urlBase, operatorId, clientId, balance, externalId, lang, productGatewayUrl);
            Dictionary<string, string> queryParams = new Dictionary<string, string>
            {
                {"id_user", operatorId.ToString() },
                {"id_customer", clientId.ToString() },
                {"balance", balance.ToString() },
                {"id_game", externalId },
                {"language", lang },
                {"callback_url", productGatewayUrl }
            };

            string request = BuildQuery(queryParams);
            string hash = CommonFunctions.ComputeHMACSha256(request, secretKey).ToLower();
            string base64Hash = Convert.ToBase64String(Encoding.UTF8.GetBytes(hash));

            string response = CommonFunctions.SendHttpRequest(
                new HttpRequestInput
                {
                    Url = callUrl,
                    RequestMethod = HttpMethod.Get,
                    RequestHeaders = new Dictionary<string, string>
                    {
                        {"ContentType", "application/json" },
                        {"Hash", base64Hash }
                    }
                }, out _);

            var responseObject = JsonConvert.DeserializeObject<GetUrlResponse>(response);
            if (responseObject.Done == 0)
                throw new ArgumentNullException(responseObject.Errors[0]);
            return responseObject.Url;
        }

        private static string BuildQuery(Dictionary<string, string> parameters)
        {
            char[] symbols = Hex.Keys.ToArray();
            var query = new StringBuilder();
            var temp = string.Empty;
            foreach (var param in parameters)
            {
                temp = parameters[param.Key];
                if (param.Value.IndexOfAny(symbols) != -1)
                {
                    foreach (var symbol in symbols)
                    {
                        if (param.Value.Contains(symbol))
                            temp = temp.Replace(symbol.ToString(), Hex[symbol]);
                    }
                }
                query.AppendFormat("{0}={1}&", param.Key, temp);
            }
            query = query.Remove(query.Length - 1, 1);
            return query.ToString();
        }
    }
}