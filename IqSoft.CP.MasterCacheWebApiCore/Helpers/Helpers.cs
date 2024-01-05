using System;
using IqSoft.CP.Common;
using System.Net;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Common.Enums;
using System.Threading.Tasks;
using IqSoft.CP.MasterCacheWebApiCore;
using System.Net.Http;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class Helpers
    {
        public static SessionIdentity CheckToken(string token, int clientId, double timeZone)
        {
            var session = ClientBll.GetClientPlatformSession(clientId);
            if (session.Token != token)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.SessionNotFound);
            if (session.State == (int)SessionStates.Inactive)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.SessionExpired);

            return new SessionIdentity
            {
                Id = session.ClientId,
                LoginIp = session.Ip,
                LanguageId = session.LanguageId,
                SessionId = session.Id,
                Token = session.Token,
                ProductId = session.ProductId,
                Country = session.Country,
                DeviceType = session.DeviceType,
                StartTime = session.StartTime.Value,
                LastUpdateTime = session.LastUpdateTime,
                State = session.State,
                CurrentPage = session.CurrentPage,
                ParentId = session.ParentId,
                CurrencyId = session.CurrencyId,
                TimeZone = timeZone
            };
        }

        //public static bool CheckPartnerSecretKey(string secretKey, int partnerId) //???
        //{
        //    try
        //    {
        //        using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
        //        {
        //            var partnerKey =
        //                partnerBl.GetPartnerKey(new FilterPartnerKey
        //                {
        //                    Name = Constants.PartnerKeys.PartnerWebSiteWebApiSecretKey,
        //                    PartnerId = partnerId
        //                }, false);
        //            if (partnerKey == null)
        //                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerKeyNotFound);
        //            if (partnerKey.StringValue == secretKey)
        //                return true;
        //            return false;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        public static string GetLoginCountry(string ip)
        {
            var result = "";
            try
            {
                var url = new Uri(String.Format("http://freegeoip.net/json/{0}", ip));
                using var httpClient = new HttpClient();
                var httpResponseMessage = httpClient.GetAsync(url).Result;
                var response = httpResponseMessage.Content.ReadAsStringAsync().Result;
                var responseDetails = JsonConvert.DeserializeObject<LoginInfo>(response);
                result = responseDetails.country_name;
            }
            catch (Exception)
            {
                //ignored
            }
            return result;
        }

        public static void InvokeMessage(string messageName, params object[] obj)
        {
            Startup.JobConnection.InvokeCoreAsync(messageName, typeof(string), obj);
        }
    }
}