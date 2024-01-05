using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Integration.Platforms.Models.UKSanctionsService;
using log4net;
using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Helpers
{
    public static class UKSanctionsCustomService
    {
        public static AMLStatus GetAMLStatus(BllClient client, ILog log)
        {
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.UKSanctionsServiceUrl).StringValue;
            var requestInput = new
            {
                Name = string.Format("{0} {1}", client.FirstName, client.LastName),
                client.Email,
                Phone = client.MobileNumber,
                PassportNumber = client.DocumentNumber
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                PostData = JsonConvert.SerializeObject(requestInput),
                Url = url,
                Log = log
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _, timeout: 15000);
            var apiResponseBase = JsonConvert.DeserializeObject<ApiResponseBase>(response);
            if (apiResponseBase.ResponseCode == 0)
            {
                var clientAMLInfo = apiResponseBase.ResponseObject == null ? null : (AMLInfoOutput)apiResponseBase.ResponseObject;
                return new AMLStatus
                {
                    IsVerified = clientAMLInfo != null,
                    Status = clientAMLInfo != null ? AMLStatuses.BLOCK : AMLStatuses.NA,
                    Percentage = clientAMLInfo != null ? 100 : 0
                };
            }
            return new AMLStatus
            {
                Error = apiResponseBase.Description
            };
        }
    }
}
