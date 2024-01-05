using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Platforms.Models.TicketingSystem;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace IqSoft.CP.Integration.Platforms.Helpers
{
    public static class TicketingSystem
    {
        private enum SiteTypes
        {
            Agent = 1,
            Player = 2
        }
        public static string CallTicketSystemApi(int clientId, SessionIdentity session)
        {
            var client = CacheManager.GetClientById(clientId);

            var ticketSystemInput = new
            {
                idIQUser = client.Id,
                userName = client.UserName,
                idSite = client.PartnerId.ToString(),
                siteURL = string.Format("https://{0}", session.Domain),
                idSiteType = (int)SiteTypes.Player,
                email = client.Email
            };
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.TicketingSystemApiUrl).StringValue;
            var apiToken = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.TicketingSystemApiToken).StringValue;
            var requestHeaders = new Dictionary<string, string> { { "Authorization", "bearer " + apiToken } };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = HttpMethod.Post,
                Url =  url + "/InsertNewIQUserToken",
                RequestHeaders = requestHeaders,
                PostData = JsonConvert.SerializeObject(ticketSystemInput)
            };
            var resp = JsonConvert.DeserializeObject<GenerateTokenOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (resp.Code != 0)
                throw new Exception(resp.Message);
            url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.TicketingSystemOpenUrl).StringValue;
            return string.Format("{0}?access={1}", url, resp.Token);
        }
    }
}