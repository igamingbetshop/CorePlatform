using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using Newtonsoft.Json;
using IqSoft.CP.Integration.Platforms.Models.MGTCompliance;
using IqSoft.CP.DAL.Models.Cache;
using System.Collections.Generic;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using log4net;
using System.Linq;
using IqSoft.CP.Common.Enums;

namespace IqSoft.CP.Integration.Platforms.Helpers
{
    public static class MGTComplianceHelpers
    {
        private static string GetAPIToken(int partnerId)
        {
            var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.MGTComplianceApiUrl).StringValue;
            var apiClient = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.MGTComplianceClient).StringValue;
            var apiSecret = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.MGTComplianceApiSecret).StringValue;

            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = $"{url}/auth",
                PostData = JsonConvert.SerializeObject(new { client = apiClient, apiSecret })
            };
            var tokenOutput = JsonConvert.DeserializeObject<TokenOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (tokenOutput.Code != 0)
                throw new System.Exception(tokenOutput.Message);
            return tokenOutput.Token;
        }

        public static string Register(BllClient client, SessionIdentity session, ILog log)
        {
            if (string.IsNullOrEmpty(client.FirstName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            if (string.IsNullOrWhiteSpace(client.ZipCode))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ZipCodeCantBeEmpty);
            if (string.IsNullOrWhiteSpace(client.Address))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);
            if (string.IsNullOrEmpty(client.Email))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailOrMobileMustBeFilled);
            var regionPath = CacheManager.GetRegionPathById(client.RegionId);
            var country = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country);
            var city = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.City);
            if (country == null || city == null)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.MGTComplianceApiUrl).StringValue;
            var services = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.MGTComplianceService).StringValue.Split(',');
            var productGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ProductGateway).StringValue;
            var autToken = GetAPIToken(client.PartnerId);
            var registerInput = new
            {
                collection = "Identify",
                services = services,
                gender = ((Gender)client.Gender).ToString(),
                firstName = client.FirstName,
                lastName = client.LastName,
                birthdate = client.BirthDate.ToString("yyyy-MM-dd"),
                email = client.Email,
                street = client.Address,
                postcode = client.ZipCode,
                city,
                country,
                redirectUrl = $"https://{session.Domain}/",
                callbackUrl = $"{productGatewayUrl}/{client.PartnerId}/api/MGTCompliance/ApiRequest" // check method name
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + autToken } },
                Url =  $"{url}/registration",
                PostData = JsonConvert.SerializeObject(registerInput)
            };
            var registerOutput = JsonConvert.DeserializeObject<RegisterOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (string.IsNullOrEmpty(registerOutput.Message))
                throw new System.Exception(registerOutput.Message);
            return registerOutput.IdentUrl;
        }
    }
}