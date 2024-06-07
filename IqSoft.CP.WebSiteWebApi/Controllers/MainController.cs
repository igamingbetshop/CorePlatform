using System.Linq;
using System.Reflection;
using Serilog;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Common.Models.WebSiteModels.Products;
using IqSoft.CP.Common.Models.WebSiteModels.Filters;
using IqSoft.CP.Common.Models.WebSiteModels.Clients;
using IqSoft.CP.Common.Models.WebSiteModels.Bonuses;
using IqSoft.CP.WebSiteWebApi.Common;
using IqSoft.CP.WebSiteWebApi.Helpers;
using IqSoft.CP.WebSiteWebApi.Models;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using System.Collections.Generic;
using IqSoft.CP.Common.Enums;
using System;
using IqSoft.CP.Common.Models;
using IqSoft.CP.CommonCore.Models.WebSiteModels.Clients;
using IqSoft.CP.CommonCore.Models.WebSiteModels;
namespace IqSoft.CP.WebSiteWebApi.Controllers
{
    [Route("{partnerId}/api/[controller]/[action]")]
    [ApiController]
    public class MainController : ControllerBase
    {
        #region Registration

        [HttpPost]
        public IActionResult RegisterClient(int partnerId, ClientModel request)
        {
            var resp = CheckRequestState(partnerId, request, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return Ok(resp);

            var apiRestrictions = SlaveCache.GetApiRestrictions(partnerId);

            if (apiRestrictions.RegistrationLimitPerDay.HasValue && SlaveCache.GetRegistrationsCount(partnerId, request.Ip) >= apiRestrictions.RegistrationLimitPerDay.Value)
            {
                resp.ResponseCode = Constants.Errors.MaxLimitExceeded;
                resp.Description = SlaveCache.GetErrorTypeById(partnerId, resp.ResponseCode, request.LanguageId).Message;
                return Ok(resp);
            }
            var masterCacheResponse = MasterCacheIntegration.SendMasterCacheRequest<ApiLoginClientOutput>(partnerId, MethodBase.GetCurrentMethod().Name, request);
            if (masterCacheResponse.ResponseCode == Constants.SuccessResponseCode)
                SlaveCache.IncrementRegistrationsCount(partnerId, request.Ip);
            return Ok(masterCacheResponse);
        }

        [HttpPost]
        public IActionResult QuickEmailRegistration(int partnerId, QuickClientModel request)
        {
            var resp = CheckRequestState(partnerId, request, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return Ok(resp);
            var apiRestrictions = SlaveCache.GetApiRestrictions(partnerId);

            if (apiRestrictions.RegistrationLimitPerDay.HasValue && SlaveCache.GetRegistrationsCount(partnerId, request.Ip) >= apiRestrictions.RegistrationLimitPerDay.Value)
            {
                resp.ResponseCode = Constants.Errors.MaxLimitExceeded;
                resp.Description = SlaveCache.GetErrorTypeById(partnerId, resp.ResponseCode, request.LanguageId).Message;
                return Ok(resp);
            }

            var response = MasterCacheIntegration.SendMasterCacheRequest<ApiLoginClientOutput>(partnerId, MethodBase.GetCurrentMethod().Name, request);
            if (response.ResponseCode == Constants.SuccessResponseCode)
                SlaveCache.IncrementRegistrationsCount(partnerId, request.Ip);

            return Ok(response);
        }

        [HttpPost]
        public IActionResult QuickSmsRegistration(int partnerId, QuickClientModel request)
        {
            var resp = CheckRequestState(partnerId, request, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return Ok(resp);
            var apiRestrictions = SlaveCache.GetApiRestrictions(partnerId);

            if (apiRestrictions.RegistrationLimitPerDay.HasValue && SlaveCache.GetRegistrationsCount(partnerId, request.Ip) >= apiRestrictions.RegistrationLimitPerDay.Value)
            {
                resp.ResponseCode = Constants.Errors.MaxLimitExceeded;
                resp.Description = SlaveCache.GetErrorTypeById(partnerId, resp.ResponseCode, request.LanguageId).Message;
                return Ok(resp);
            }
            var masterCacheResponse = MasterCacheIntegration.SendMasterCacheRequest<ApiLoginClientOutput>(partnerId, MethodBase.GetCurrentMethod().Name, request);
            if (masterCacheResponse.ResponseCode == Constants.SuccessResponseCode)
                SlaveCache.IncrementRegistrationsCount(partnerId, request.Ip);

            return Ok(masterCacheResponse);
        }

        [HttpPost]
        public IActionResult RegisterAffiliate(int partnerId, ClientModel request)
        {
            var resp = CheckRequestState(partnerId, request, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return Ok(resp);

            var apiRestrictions = SlaveCache.GetApiRestrictions(partnerId);

            if (apiRestrictions.RegistrationLimitPerDay.HasValue && SlaveCache.GetRegistrationsCount(partnerId, request.Ip) >= apiRestrictions.RegistrationLimitPerDay.Value)
            {
                resp.ResponseCode = Constants.Errors.MaxLimitExceeded;
                resp.Description = SlaveCache.GetErrorTypeById(partnerId, resp.ResponseCode, request.LanguageId).Message;
                return Ok(resp);
            }
            var masterCacheResponse = MasterCacheIntegration.SendMasterCacheRequest<ApiLoginClientOutput>(partnerId, MethodBase.GetCurrentMethod().Name, request);
            if (masterCacheResponse.ResponseCode == Constants.SuccessResponseCode)
                SlaveCache.IncrementRegistrationsCount(partnerId, request.Ip);
            return Ok(masterCacheResponse);
        }

        #endregion

        #region Recovery

        [HttpPost]
        public ApiResponseBase SendRecoveryToken(int partnerId, ApiSendRecoveryTokenInput request)
        {
            var resp = CheckRequestState(partnerId, request, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        [HttpPost]
        public ApiResponseBase RecoverPassword(int partnerId, ClientPasswordRecovery request)
        {
            var resp = CheckRequestState(partnerId, request, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<RecoverPasswordOutput>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        #endregion

        #region Verification

        [HttpPost]
        public ApiResponseBase SendSMSCode(int partnerId, ApiNotificationInput request)
        {
            var resp = CheckRequestState(partnerId, request, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        [HttpPost]
        public ApiResponseBase VerifySMSCode(int partnerId, ApiNotificationInput request)
        {
            var resp = CheckRequestState(partnerId, request, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        [HttpPost]
        public ApiResponseBase SendEmailCode(int partnerId, ApiNotificationInput request)
        {
            var resp = CheckRequestState(partnerId, request, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        [HttpPost]
        public ApiResponseBase VerifyEmailCode(int partnerId, ApiNotificationInput request)
        {
            var resp = CheckRequestState(partnerId, request, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        #endregion

        #region Product

        [HttpGet, HttpPost]
        public ApiResponseBase GetProductUrl(int partnerId, GetProductUrlInput request)
        {
            var resp = CheckRequestState(partnerId, request);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            if (request.IsForDemo || string.IsNullOrEmpty(request.Token))
            {
                var product = SlaveCache.GetProductFromCache(partnerId, request.ProductId);
                if (product == null)
                    return new ApiResponseBase { ResponseCode = Constants.Errors.ProductNotFound };
                var provider = SlaveCache.GetGameProviderFromCache(partnerId, product.GameProviderId.Value);
                if (provider.Name.ToLower() == Constants.GameProviders.Internal.ToLower())
                {
                    if (!string.IsNullOrEmpty(request.Position))
                        request.Position = request.Position.Replace(" ", string.Empty);
                    return new ApiResponseBase { ResponseObject = SlaveCache.GetInternalProductUrl(partnerId, request) + "&timezone=" + request.TimeZone };
                }
            }
            var response = MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, request);
            return response;
        }

        [HttpGet, HttpPost]
        public ApiResponseBase GetProductInfo(int partnerId, GetProductUrlInput request)
        {
            var resp = CheckRequestState(partnerId, request);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;

            var setting = SlaveCache.GetPartnerProductSettingByProductId(partnerId, request.ProductId);
            var product = SlaveCache.GetProductById(partnerId, request.ProductId, request.LanguageId);
            return new ApiResponseBase
            {
                ResponseObject = new { Rating = (setting == null || setting.Id == 0 ? 0 : setting.Rating), Name = product.Name, BackgroundImageUrl = product.BackgroundImageUrl ?? string.Empty }
            };
        }

        [HttpPost]
        public ApiResponseBase GetPartnerProductInfo(int partnerId, ApiProductInfoInput request)
        {
            var resp = CheckRequestState(partnerId, request);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        [HttpPost]
        public ApiResponseBase GetGames(int partnerId, ApiGetGamesInput input)
        {
            var resp = CheckRequestState(partnerId, input, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, input);
        }

        [HttpPost]
        public ApiResponseBase GetJackpotFeed(int partnerId, ApiRequestBase request)
        {
            var resp = CheckRequestState(partnerId, request, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        [HttpPost]
        public ApiResponseBase GetGameProviders(int partnerId, ApiGetGamesInput input)
        {
            var resp = CheckRequestState(partnerId, input, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, input);
        }

        [HttpPost]
        public ApiResponseBase SearchContentInfo(int partnerId, ApiGetGamesInput input)
        {
            var resp = CheckRequestState(partnerId, input, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, input);
        }

        #endregion

        #region Partner

        [HttpGet, HttpPost]
        public ApiResponseBase GetPartnerBetShops(int partnerId, RequestBase request)
        {
            var resp = CheckRequestState(partnerId, request);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<GetPartnerBetShopsOutput>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        [HttpGet, HttpPost]
        public ApiResponseBase GetPartnerPaymentSystems(int partnerId, ApiFilterPartnerPaymentSetting request)
        {
            var resp = CheckRequestState(partnerId, request);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            var response = MasterCacheIntegration.SendMasterCacheRequest<GetPartnerPaymentSystemsOutput>(partnerId, MethodBase.GetCurrentMethod().Name, request);
            return response.ToApiPartnerPaymentSystemsOutput();
        }

        [HttpPost]
        public ApiResponseBase SendEmailToPartner(int partnerId, ApiOpenTicketInput request)
        {
            var resp = CheckRequestState(partnerId, request, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

		[HttpPost]
		public ApiResponseBase GetCharacters(int partnerId, ApiRequestBase input)
		{
			var resp = CheckRequestState(partnerId, input, MethodBase.GetCurrentMethod().Name);
			if (resp.ResponseCode != Constants.SuccessResponseCode)
				return resp;
			return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, input);
		}

		[HttpPost]
		public ApiResponseBase GetCharacterHierarchy(int partnerId, GetCharactersInput input)
		{
			var resp = CheckRequestState(partnerId, input, MethodBase.GetCurrentMethod().Name);
			if (resp.ResponseCode != Constants.SuccessResponseCode)
				return resp;
			return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, input);
		}

        #endregion

        #region Client

        [HttpPost]
        public IActionResult LoginClient(int partnerId, EncryptedData input)
        {
            var request = JsonConvert.DeserializeObject<LoginDetails>(CommonFunctions.RSADecrypt(input.Data));
            var resp = CheckRequestState(partnerId, request, MethodBase.GetCurrentMethod().Name);
            Request.Headers.TryGetValue("User-Agent", out StringValues userAgent);
            request.Source = userAgent;
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return Ok(resp);

            var masterCacheResponse = MasterCacheIntegration.SendMasterCacheRequest<ApiLoginClientOutput>(partnerId, MethodBase.GetCurrentMethod().Name, request);
            return Ok(masterCacheResponse);
        }

        [HttpPost]
        public IActionResult GetRefreshToken(int partnerId, EncryptedData input)
        {
            var request = JsonConvert.DeserializeObject<LoginDetails>(CommonFunctions.RSADecrypt(input.Data));
            var resp = CheckRequestState(partnerId, request, MethodBase.GetCurrentMethod().Name);
            Request.Headers.TryGetValue("User-Agent", out StringValues userAgent);
            request.Source = userAgent;
            
            if (request.OSType == (int)OSTypes.IPad || request.OSType == (int)OSTypes.IPhone || request.OSType == (int)OSTypes.Android)
                request.DeviceType = (int)DeviceTypes.Application;

            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return Ok(resp);

            var masterCacheResponse = MasterCacheIntegration.SendMasterCacheRequest<ApiLoginClientOutput>(partnerId, MethodBase.GetCurrentMethod().Name, request);

            return Ok(masterCacheResponse);
        }

        [HttpPost]
        public ApiResponseBase ValidateTwoFactorPIN(int partnerId, Api2FAInput input)
        {
            var resp = CheckRequestState(partnerId, input);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, input);
        }

        [HttpPost]
        public IActionResult CreateToken(int partnerId, ApiNotificationInput input)
        {
            var resp = CheckRequestState(partnerId, input, MethodBase.GetCurrentMethod().Name);
            Request.Headers.TryGetValue("User-Agent", out StringValues userAgent);
            input.Source = userAgent;
            if (input.OSType == (int)OSTypes.IPad || input.OSType == (int)OSTypes.IPhone || input.OSType == (int)OSTypes.Android)
                input.DeviceType = (int)DeviceTypes.Application;

            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return Ok(resp);

            var masterCacheResponse = MasterCacheIntegration.SendMasterCacheRequest<ApiLoginClientOutput>(partnerId, MethodBase.GetCurrentMethod().Name, input);

            return Ok(masterCacheResponse);
        }

        [HttpGet, HttpPost]
        public ApiResponseBase GetClientInfo(int partnerId, RequestBase request)
        {
            var resp = CheckRequestState(partnerId, request, null, false);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiClientInfo>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        [HttpGet, HttpPost]
        public ApiResponseBase GetClientBalance(int partnerId, RequestBase request)
        {
            var resp = CheckRequestState(partnerId, request);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;

            return MasterCacheIntegration.SendMasterCacheRequest<GetBalanceOutput>(partnerId, "getclientbalance", request);
        }

        [HttpGet, HttpPost]
        public ApiResponseBase GetClientByToken(int partnerId, RequestBase request)
        {
            var resp = CheckRequestState(partnerId, request, null, false);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiLoginClientOutput>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        [HttpGet, HttpPost]
        public ApiResponseBase GetClientPaymentInfoTypesEnum(int partnerId, ApiRequestBase request)//old
        {
            var resp = CheckRequestState(partnerId, request);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        #endregion

        #region Common

        [HttpPost]
        public ApiResponseBase ApiRequest(int partnerId, RequestBase request)
        {
            var resp = CheckRequestState(partnerId, request, request.Method);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            switch (request.Method)
            {
                case "CancelPaymentRequest":
                    {
                        var cprResp = MasterCacheIntegration.SendMasterCacheRequest<CancelPaymentRequestOutput>(partnerId, request.Method, request);
                        if (cprResp.ResponseCode == Constants.SuccessResponseCode)
                            BroadcastService.BroadcastBalance(request.ClientId, cprResp.ApiBalance);
                        return cprResp;
                    }
                case "CreatePaymentRequest":
                    {
                        var cprResp = MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, "ApiRequest", request);
                        if (cprResp.ResponseCode == Constants.SuccessResponseCode)
                        {
                            try
                            {
                                var model = JsonConvert.DeserializeObject<PaymentRequestModel>(JsonConvert.SerializeObject(cprResp.ResponseObject));
                                BroadcastService.BroadcastBalance(request.ClientId, model.ApiBalance);
                            }
                            catch (Exception e)
                            {
                                Log.Error(JsonConvert.SerializeObject(cprResp), e);
                            }
                        }
                        return cprResp;
                    }
                default:
                    return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, "ApiRequest", request);
            }
        }
        // optimise enum code here
        [HttpGet, HttpPost]
        public ApiResponseBase GetBonusStatusesEnum(int partnerId, ApiRequestBase request)
        {
            var resp = CheckRequestState(partnerId, request);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        [HttpGet, HttpPost]
        public ApiResponseBase GetBetStatesEnum(int partnerId, ApiRequestBase request)
        {
            var resp = CheckRequestState(partnerId, request);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        [HttpGet, HttpPost]
        public ApiResponseBase GetClientTitles(int partnerId, ApiRequestBase request)
        {
            var resp = CheckRequestState(partnerId, request);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        [HttpGet, HttpPost]
        public ApiResponseBase GetRegions(int partnerId, ApiFilterRegion request)
        {
            var resp = CheckRequestState(partnerId, request);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        [HttpGet, HttpPost]
        public ApiResponseBase GetJobAreas(int partnerId, ApiRequestBase request)
        {
            var resp = CheckRequestState(partnerId, request);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return new ApiResponseBase { ResponseObject = SlaveCache.GetJobAreasFromCache(partnerId, request).Select(x => new { x.Id, Value = x.Name }).ToList() };
        }

        [HttpGet, HttpPost]
        public ApiResponseBase GetTicker(int partnerId, RequestBase request)
        {
            var resp = CheckRequestState(partnerId, request);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        [HttpGet, HttpPost]
        public ApiResponseBase GetReferralTypes(int partnerId, ApiRequestBase request)
        {
            var resp = CheckRequestState(partnerId, request);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return new ApiResponseBase { ResponseObject = SlaveCache.GetReferralTypesFromCache(partnerId, request).Select(x => new { x.Name, x.Value }).ToList() };
        }

        [HttpGet, HttpPost]
        public ApiResponseBase GetExclusionReasons(int partnerId, ApiRequestBase request)
        {
            var resp = CheckRequestState(partnerId, request);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return new ApiResponseBase { ResponseObject = SlaveCache.GetExclusionReasonsFromCache(partnerId, request).Select(x => new { x.Name, x.Value }).ToList() };
        }

        [HttpPost]
        public ApiResponseBase GeolocationData(int partnerId, ApiGeolocationDataInput request)
        {
            Request.Headers.TryGetValue("Origin", out StringValues originValues);
            request.Domain = originValues.ToString().Replace("https://", string.Empty).Replace("http://", string.Empty).Replace("www.", string.Empty);
            Log.ForContext("FileName", request.Domain).Information(JsonConvert.SerializeObject(request));
            
            var input = new ApiRequestBase();
            var output = new ApiResponseBase();

            input.Domain = request.Domain;
            var partner = SlaveCache.GetPartnerByDomain(request.Domain);
            var ip = string.Empty;
            
            if (partner != null && partner.Id > 0)
            {
                ip = GetRequestIp(partner.Id, out string ipCountry);
                input.CountryCode = ipCountry;
                output.ResponseObject = SlaveCache.GetGeolocationDataFromCache(partner.Id, input).ToApiGeolocationData();
            }
            output.ResponseCode = string.IsNullOrEmpty(ip) ? Constants.Errors.RestrictedDestination : Constants.SuccessResponseCode;
            return output;
        }

        [HttpGet, HttpPost]
        public ApiResponseBase GetWelcomeBonus(int partnerId, ApiRegBonusInput request)
        {
            var resp = CheckRequestState(partnerId, request);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        [HttpGet, HttpPost]
        public ApiResponseBase GetImages(int partnerId, ApiBannerInput request)
        {
            var resp = CheckRequestState(partnerId, request);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            var banners = SlaveCache.GetImagesFromCache(partnerId, request)?.Where(x => x.Languages == null ||
                x.Languages.Names == null || x.Languages.Names.Count == 0 ||
                (x.Languages.Type == (int)BonusSettingConditionTypes.InSet && x.Languages.Names.Contains(request.LanguageId)) ||
                (x.Languages.Type == (int)BonusSettingConditionTypes.OutOfSet && !x.Languages.Names.Contains(request.LanguageId))).ToList();

            Request.Headers.TryGetValue("User-Agent", out StringValues userAgent);
            var agentInfo = userAgent.ToString().ToLower();

            var visibility = new List<int> { (int)BannerVisibility.LoggedOut };
            var clientSegments = new List<int>();
            if (request.ClientId > 0 && !string.IsNullOrEmpty(request.Token))
            {
                var clientResp = MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, "GetClientData", request);
                if (clientResp.ResponseCode == 0)
                {
                    var clientData = JsonConvert.DeserializeObject<ApiClientData>(JsonConvert.SerializeObject(clientResp.ResponseObject));

                    visibility = new List<int> { (int)BannerVisibility.LoggedIn };
                    switch (clientData.DepCount)
                    {
                        case 0:
                            visibility.Add((int)BannerVisibility.NoDeposit);
                            break;
                        case 1:
                            visibility.Add((int)BannerVisibility.OneDepositOnly);
                            break;
                        default:
                            visibility.Add((int)BannerVisibility.TwoOrMoreDeposits);
                            break;
                    }

                    if (clientData.Segments != null && clientData.Segments.Any())
                        clientSegments = clientData.Segments;
                }
                banners = banners.Where(x => x.Segments == null || x.Segments.Ids.Count == 0 ||
                    (x.Segments.Type == (int)BonusSettingConditionTypes.InSet && clientSegments.Any(y => x.Segments.Ids.Contains(y))) ||
                    (x.Segments.Type == (int)BonusSettingConditionTypes.OutOfSet && clientSegments.All(y => !x.Segments.Ids.Contains(y)))
                    ).ToList();
            }
            banners = banners.Where(x => x.Visibility == null || x.Visibility.Count == 0 || visibility.Any(y => x.Visibility.Contains(y))).ToList();

            if (!string.IsNullOrEmpty(userAgent) && (agentInfo.Contains("iphone") || agentInfo.Contains("ipad")))
            {
                foreach (var b in banners)
                {
                    b.Image = b.Image.Replace(".webp", ".jp2");
                }
            }
            return new ApiResponseBase { ResponseObject = banners.Select(x => x.ToApiBannerOutput()).ToList() };
        }

        [HttpPost]
        public ApiResponseBase GetPromotions(int partnerId, RequestBase request)
        {
            var response = new ApiResponseBase();

            var resp = CheckRequestState(partnerId, request);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;

            var deviceType = (int)DeviceTypes.Desktop;
            if (request.OSType == (int)OSTypes.IPad || request.OSType == (int)OSTypes.IPhone || request.OSType == (int)OSTypes.Android)
                deviceType = (int)DeviceTypes.Mobile;
            var promotions = SlaveCache.GetPromotionsFromCache(partnerId, request)?.Where(x => (x.Languages == null ||
                x.Languages.Names == null || x.Languages.Names.Count == 0 ||
                (x.Languages.Type == (int)BonusSettingConditionTypes.InSet && x.Languages.Names.Contains(request.LanguageId)) ||
                (x.Languages.Type == (int)BonusSettingConditionTypes.OutOfSet && !x.Languages.Names.Contains(request.LanguageId))) && 
                (x.DeviceType == null || x.DeviceType == deviceType)).GroupBy(x => x.ParentId ?? 0).ToList();

            var parents = promotions?.FirstOrDefault(x => x.Key == 0).ToList();
            var output = new List<ApiPromotionGroup>();

            if (parents != null)
            {
                foreach (var p in promotions)
                {
                    if (p.Key != 0)
                    {
                        var parent = parents.FirstOrDefault(x => x.Id == p.Key);
                        if (parent != null)
                        {
                            var item = output.FirstOrDefault(x => x.Id == parent.Id);
                            if (item == null)
                            {
                                item = new ApiPromotionGroup
                                {
                                    Id = parent.Id,
                                    Title = parent.Title,
                                    ImageName = parent.ImageName,
                                    Order = parent.Order,
                                    StyleType = parent.StyleType,
                                    Promotions = new List<ApiPromotion>()
                                };
                                output.Add(item);
                            }
                            item.Promotions.AddRange(p.Select(x => new ApiPromotion
                            {
                                Id = x.Id,
                                Title = x.Title,
                                Description = x.Description,
                                Type = x.Type,
                                ImageName = x.ImageName,
                                Order = x.Order,
                                StyleType = x.StyleType
                            }).OrderBy(x => x.Order).ToList());
                        }
                    }
                }
            }

            var clientSegments = new List<int>();

            if (request.ClientId > 0 && !string.IsNullOrEmpty(request.Token))
            {
                var clientResp = MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, "GetClientData", request);
                if (clientResp.ResponseCode == 0)
                {
                    var clientData = JsonConvert.DeserializeObject<ApiClientData>(JsonConvert.SerializeObject(clientResp.ResponseObject));
                    if (clientData.Segments != null && clientData.Segments.Any())
                    {
                        clientSegments = clientData.Segments;
                        foreach (var p in output)
                        {
                            p.Promotions = p.Promotions.Where(x => x.Segments?.Ids == null || 
                                (x.Segments.Type == (int)BonusSettingConditionTypes.InSet && clientSegments.Any(y => x.Segments.Ids.Contains(y))) ||
                                (x.Segments.Type == (int)BonusSettingConditionTypes.OutOfSet && clientSegments.All(y => !x.Segments.Ids.Contains(y))) ||
                                (x.Segments.Type != (int)BonusSettingConditionTypes.InSet && x.Segments.Type != (int)BonusSettingConditionTypes.OutOfSet)).ToList();
                        }
                    }
                }
            }

            response.ResponseObject = output;
            return response;
        }

        [HttpPost]
        public ApiResponseBase GetNews(int partnerId, RequestBase request)
        {
            var response = new ApiResponseBase();

            var resp = CheckRequestState(partnerId, request);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;

            var news = SlaveCache.GetNewsFromCache(partnerId, request)?.Where(x => x.Languages == null ||
                x.Languages.Names == null || x.Languages.Names.Count == 0 ||
                (x.Languages.Type == (int)BonusSettingConditionTypes.InSet && x.Languages.Names.Contains(request.LanguageId)) ||
                (x.Languages.Type == (int)BonusSettingConditionTypes.OutOfSet && !x.Languages.Names.Contains(request.LanguageId))).
                GroupBy(x => x.ParentId ?? 0).ToList();

            var parents = news?.FirstOrDefault(x => x.Key == 0).ToList();
            var output = new List<ApiNewsGroup>();

            if (parents != null)
            {
                foreach (var n in news)
                {
                    if (n.Key != 0)
                    {
                        var parent = parents.First(x => x.Id == n.Key);
                        var item = output.FirstOrDefault(x => x.Id == parent.Id);
                        if (item == null)
                        {
                            item = new ApiNewsGroup
                            {
                                Id = parent.Id,
                                Title = parent.Title,
                                ImageName = parent.ImageName,
                                Order = parent.Order,
                                StyleType = parent.StyleType,
                                News = new List<ApiNews>()
                            };
                            output.Add(item);
                        }
                        item.News.AddRange(n.Select(x => new ApiNews
                        {
                            Id = x.Id,
                            Title = x.Title,
                            Description = x.Description,
                            Type = x.Type,
                            ImageName = x.ImageName,
                            Order = x.Order,
                            StyleType = x.StyleType,
                            StartDate = x.StartDate
                        }).OrderBy(x => x.Order).ToList());
                    }
                }
            }

            var clientSegments = new List<int>();

            if (request.ClientId > 0 && !string.IsNullOrEmpty(request.Token))
            {
                var clientResp = MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, "GetClientData", request);
                if (clientResp.ResponseCode == 0)
                {
                    var clientData = JsonConvert.DeserializeObject<ApiClientData>(JsonConvert.SerializeObject(clientResp.ResponseObject));
                    if (clientData.Segments != null && clientData.Segments.Any())
                    {
                        clientSegments = clientData.Segments;
                        foreach (var p in output)
                        {
                            p.News = p.News.Where(x => x.Segments?.Ids == null ||
                                (x.Segments.Type == (int)BonusSettingConditionTypes.InSet && clientSegments.Any(y => x.Segments.Ids.Contains(y))) ||
                                (x.Segments.Type == (int)BonusSettingConditionTypes.OutOfSet && clientSegments.All(y => !x.Segments.Ids.Contains(y))) ||
                                (x.Segments.Type != (int)BonusSettingConditionTypes.InSet && x.Segments.Type != (int)BonusSettingConditionTypes.OutOfSet)).ToList();
                        }
                    }
                }
            }
            response.ResponseObject = output;
            return response;
        }

        [HttpPost]
        public ApiResponseBase GetTicketSubjects(int partnerId, ApiRequestBase input)
        {
            var resp = CheckRequestState(partnerId, input, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, input);
        }

        [HttpPost]
        public ApiResponseBase GetSecurityQuestions(int partnerId, ApiGetGamesInput input)
        {
            var resp = CheckRequestState(partnerId, input, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, input);
        }

		[HttpPost]
		public ApiResponseBase GetProviderData(int partnerId, ApiProviderData input)
		{
			var resp = CheckRequestState(partnerId, input, MethodBase.GetCurrentMethod().Name);
			if (resp.ResponseCode != Constants.SuccessResponseCode)
				return resp;
			return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, input);
		}

		[HttpPost]
        public ApiResponseBase CheckTelegramAuthorization(int partnerId, RequestBase request)
        {
            var resp = CheckRequestState(partnerId, request, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            if (string.IsNullOrEmpty(Program.AppSetting.TelegramBotToken))
                resp.ResponseCode = Constants.Errors.NotAllowed;
            var userData = JsonConvert.DeserializeObject<TelegramUserData>(request.RequestData);
            var hash = userData.hash;
            var genHash = CommonFunctions.GetSortedValuesAsString(userData, "\n");
            if (CommonFunctions.ComputeHMACSha256(genHash, Program.AppSetting.TelegramBotToken) != hash)
                resp.ResponseCode = Constants.Errors.WrongHash;
            var currentDate = Convert.ToInt64(DateTime.UtcNow.ToString("yyyyMMddHHmm"));
            if (currentDate - userData.auth_date > 86400)
                resp.ResponseCode = Constants.Errors.RequestExpired;
            return resp;
        }

        [HttpGet, HttpPost]
        public ApiResponseBase GetExternalModuleUrl(int partnerId, ApiExternalApiInput input)
        {
            var resp = CheckRequestState(partnerId, input, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, input);
        }

        [HttpGet]
        public ApiResponseBase GetActiveTournaments(int partnerId, [FromQuery]ApiRequestBase request)
        {
            var resp = CheckRequestState(partnerId, request, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        [HttpGet]
        public ApiResponseBase GetTournamentLeaderboard(int partnerId, [FromQuery]RequestBase request)
        {
            var resp = CheckRequestState(partnerId, request, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != Constants.SuccessResponseCode)
                return resp;
            return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, request);
        }

        #endregion

        private ApiResponseBase CheckRequestState(int partnerId, ApiRequestBase request, string methodName = null, bool setLanguage = true)
        {
            var response = new ApiResponseBase();
            var ip = GetRequestIp(partnerId, out string ipCountry);
            if (string.IsNullOrEmpty(ip))
            {
                response.ResponseCode = Constants.Errors.RestrictedDestination;
                Log.ForContext("FileName", request.Domain).Information("EmptyIp_" + methodName + "_" + partnerId + "_" + ipCountry);
                return response;
            }
            else
            {
                request.Ip = ip;
                request.PartnerId = partnerId;
                request.CountryCode = ipCountry;
                request.OSType = CustomMappers.GetOperationSystemType(Request.Headers.UserAgent.ToString());
                request.Source = Request.Headers.UserAgent.ToString();
            }
            var apiRestrictions = SlaveCache.GetApiRestrictions(partnerId);
            if (!string.IsNullOrEmpty(methodName) && !apiRestrictions.WhitelistedIps.Any(x => x.IsIpEqual(ip)) && 
                SlaveCache.GetIpCount(partnerId, methodName, ip).Count > GetMaxRequestsCount(methodName))
            {
                response.ResponseCode = Constants.Errors.RestrictedDestination;
                response.Description = SlaveCache.GetErrorTypeById(partnerId, response.ResponseCode, request.LanguageId)?.Message;
                Log.ForContext("FileName", request.Domain).Information("BlockedIp_" + methodName + "_" + ip + "_" + JsonConvert.SerializeObject(SlaveCache.GetIpCount(partnerId, methodName, ip)));
                return response;
            }
            if (string.IsNullOrEmpty(request.LanguageId) && setLanguage)
            {
                request.LanguageId = Constants.DefaultLanguageId;
            }
            if (!string.IsNullOrEmpty(methodName))
                SlaveCache.UpdateIpCount(partnerId, methodName, ip);

            StringValues originValues = string.Empty;
            Request.Headers.TryGetValue("Origin", out originValues);
            var domain = originValues.ToString().Replace("https://", string.Empty).Replace("http://", string.Empty).Replace("www.", string.Empty).Replace("distribution.", string.Empty);
            request.Domain = domain;
            return response;
        }

        private string GetRequestIp(int partnerId, out string ipCountry)
        {
            ipCountry = string.Empty;
            var apiRestrictions = SlaveCache.GetApiRestrictions(partnerId);
            
            try
            {
                var ip = Constants.DefaultIp;
                if (Request.Headers.TryGetValue(string.IsNullOrEmpty(apiRestrictions.ConnectingIPHeader) ? "CF-Connecting-IP" :
                    apiRestrictions.ConnectingIPHeader, out StringValues header))
                    ip = header.ToString();
                if (ip == Constants.DefaultIp && !string.IsNullOrEmpty(Request.HttpContext.Connection.RemoteIpAddress?.ToString()))
                    ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString();

                if (Request.Headers.TryGetValue(string.IsNullOrEmpty(apiRestrictions.IPCountryHeader) ? "CF-IPCountry" : apiRestrictions.IPCountryHeader, out header))
                    ipCountry = header.ToString();

                if (apiRestrictions.BlockedIps.Contains(ip))
                {
                    Log.ForContext("FileName", "BlockedIps").Information("GetRequestIp_" + partnerId + "_" + ip);
                    return string.Empty;
                }
                if (apiRestrictions.WhitelistedIps.Any(x => x.IsIpEqual(ip)))
                    return ip;

                if (apiRestrictions.WhitelistedCountries.Any() && !apiRestrictions.WhitelistedCountries.Contains(ipCountry))
                {
                    Log.ForContext("FileName", "WhitelistedCountries").Information("GetRequestIp_" + partnerId + "_" + ipCountry);
                    return string.Empty;
                }
                if (!apiRestrictions.WhitelistedCountries.Any() && apiRestrictions.BlockedCountries.Contains(ipCountry))
                {
                    Log.ForContext("FileName", "BlockedCountries").Information("GetRequestIp_" + partnerId + "_" + ipCountry);
                    return string.Empty;
                }
                return ip;
            }
            catch(Exception e)
            {
                Log.Error(e.Message + "_" + e.StackTrace + "_" + partnerId + "_" + JsonConvert.SerializeObject(apiRestrictions));
                return string.Empty;
            }
        }

        private int GetMaxRequestsCount(string methodName)
        {
            switch(methodName)
            {
                case "GetGames":
                case "GetClientStates":
                    return 500;
                default:
                    return 50;
            }
        }
    }
}