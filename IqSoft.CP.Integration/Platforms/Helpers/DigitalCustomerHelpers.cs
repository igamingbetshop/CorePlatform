using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Integration.Platforms.Models.DigitalCustomer;
using log4net;
using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Platforms.Helpers
{
    public static class DigitalCustomerHelpers
    {
        public static int GetJCJStatus(int partnerId, int type, string documentNumber, string languageId, ILog log)
        {
            var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.DigitalCustomerJCJUrl).StringValue;
            if (type != (int)KYCDocumentTypes.Passport && type != (int)KYCDocumentTypes.IDCard)
                throw BaseBll.CreateException(languageId, Constants.Errors.WrongInputParameters);

            var requestInput = new
            {
                documentType = type == (int)KYCDocumentTypes.Passport ? "PASSPORT" : "CEDULA",
                documentNumber
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                PostData = CommonFunctions.GetUriDataFromObject(requestInput),
                Url = url,
                Log = log
            };
            var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _, timeout: 15000);
            var respObj = JsonConvert.DeserializeObject<JCJOutput>(resp);
            if (!string.IsNullOrEmpty(respObj.ErrorResponse))
                throw new Exception(respObj.ErrorResponse);
            return respObj.Excluded.ToLower() == "yes" ? 1 : 0;
        }

        public static AMLStatus GetAMLStatus(BllClient client, int countryId, ILog log)
        {
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DigitalCustomerAMLUrl).StringValue;
            var nickName = CacheManager.GetRegionById(countryId, Constants.DefaultLanguageId).NickName;
            var requestInput = new
            {
                name = client.FirstName,
                surname = client.LastName,
                dateOfBirth = client.BirthDate,
                playerId = client.Id,
                username = client.UserName,
                countryOfResidence = nickName,
                nationality = nickName
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                PostData = CommonFunctions.GetUriDataFromObject(requestInput),
                Url = url,
                Log = log
            };
            log.Info(httpRequestInput.PostData);
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _, timeout: 15000);
            log.Info(response);

            var resp = JsonConvert.DeserializeObject<AMLOutput>(response);
            if (string.IsNullOrEmpty(resp.ErrorDescription))
                return new AMLStatus
                {
                    IsVerified = resp.Status.ToLower() == "yes",
                    Status = (AMLStatuses)Enum.Parse(typeof(AMLStatuses), resp.Status.Replace("/", string.Empty), false),
                    Percentage = resp.Percentage ?? 0
                };

            return new AMLStatus
            {
                Error = resp.ErrorDescription
            };
        }
    }
}
