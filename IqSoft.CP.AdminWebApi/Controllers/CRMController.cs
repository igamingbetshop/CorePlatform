using IqSoft.CP.AdminWebApi.Helpers;
using IqSoft.CP.AdminWebApi.Models.AdminModels;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.AdminWebApi.Models.CRM;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Filters.Clients;
using IqSoft.CP.DAL.Filters.PaymentRequests;
using IqSoft.CP.DAL.Filters.Reporting;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Affiliates;
using IqSoft.CP.DataWarehouse.Filters;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.AdminWebApi.Controllers
{
    //[EnableCors(origins: "*", headers: "*", methods: "GET,POST")]
    public class CRMController : ApiController
    {
        private static readonly string logTemplate = "Code: {0}, Message: {1}";

        [HttpPost]
        [Route("api/CRM")]
        public HttpResponseMessage CRMData(ApiRequestInput request)
        {
            DAL.ActionLog actionLog = new DAL.ActionLog
            {
                Page = string.Empty,
                ObjectTypeId = (int)ObjectTypes.User,
                ResultCode = 0,
                Language = request.LanguageId,
                Info = string.Empty
            };
            var apiResponseBase = new ApiResponseBase();
            try
            {
                var user = CacheManager.GetUserById(request.UserId);
                if (user == null)
                    throw BaseBll.CreateException(request.LanguageId, Constants.Errors.UserNotFound);
                actionLog = CheckAuthentication(request, user.PartnerId);
                if (user.SecurityCode != request.ApiKey)
                    throw BaseBll.CreateException(request.LanguageId, Constants.Errors.WrongApiCredentials);
                var identity = new SessionIdentity
                {
                    LanguageId = request.LanguageId,
                    PartnerId = user.PartnerId,
                    Token = user.SecurityCode,
                    Id = user.Id,
                    TimeZone = request.TimeZone,
                    CurrencyId = user.CurrencyId,
                    IsAdminUser = user.Type == (int)UserTypes.AdminUser
                };
                apiResponseBase = CallFunction(request, identity, WebApiApplication.DbLogger);

            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(string.Format(logTemplate, fex.Detail.Id, fex.Detail.Message));
                apiResponseBase.ResponseCode = fex.Detail.Id;
                apiResponseBase.Description = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(string.Format(logTemplate, Constants.Errors.GeneralException, ex.Message));
                apiResponseBase.ResponseCode = Constants.Errors.GeneralException;
                apiResponseBase.Description = ex.Message;
            }
            finally
            {
                actionLog.ResultCode = apiResponseBase.ResponseCode;
                actionLog.Description = apiResponseBase.Description;
                BaseBll.LogAction(actionLog);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(apiResponseBase), Encoding.UTF8)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return resp;
        }

        private DAL.ActionLog CheckAuthentication(ApiRequestInput request, int partnerId)
        {
            var actionLog = new DAL.ActionLog
            {
                Page = string.Empty,
                ObjectTypeId = (int)ObjectTypes.User,
                ResultCode = 0,
                Language = request.LanguageId
            };
            var ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP") ?? HttpContext.Current.Request.UserHostAddress;
            if (string.IsNullOrEmpty(ip))
                ip = "127.0.0.1";
            var ipCountry = HttpContext.Current.Request.Headers.Get("CF-IPCountry") ?? string.Empty;
            actionLog.Source = Request.Headers.UserAgent.ToString();
            actionLog.Ip = ip;
            actionLog.Domain = HttpContext.Current.Request.Headers.Get("Origin") ?? HttpContext.Current.Request.Url.Host;
            actionLog.Country = ipCountry;
            var action = CacheManager.GetAction(request.Method);
            if (action == null)
                throw BaseBll.CreateException(request.LanguageId, Constants.Errors.ActionNotFound);

            var blockedIps = CacheManager.GetConfigParameters(partnerId, "AdminBlockedIps").Select(x => x.Key).ToList();
            if (blockedIps.Contains(actionLog.Ip))
                throw BaseBll.CreateException(string.Empty, Constants.Errors.DontHavePermission);
            var whitelistedIps = CacheManager.GetConfigParameters(partnerId, "AdminWhitelistedIps").Select(x => x.Key).ToList();
            var whitelistedCountries = CacheManager.GetConfigParameters(partnerId, "AdminWhitelistedCountries").Select(x => x.Key).ToList();
            if (!whitelistedIps.Any(x => x.IsIpEqual(ip)) &&
                !whitelistedCountries.Contains(ipCountry))
                throw BaseBll.CreateException(string.Empty, Constants.Errors.DontHavePermission);
            if (CacheManager.GetCRMApiRequestsCount(request.UserId, request.Method) > 30)
                throw BaseBll.CreateException(request.LanguageId, Constants.Errors.MaxLimitExceeded);
            return actionLog;
        }

        public static ApiResponseBase CallFunction(ApiRequestInput request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetClients":
                    return GetClients(JsonConvert.DeserializeObject<ApiFilterClient>(request.RequestData), identity, log);
                case "GetClientInfo":
                    {
                        if (int.TryParse(request.RequestObject.ToString(), out int clientId))
                            return GetClientInfo(clientId, identity, log);
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongClientId);
                    }
                case "GetClientReport":
                    return GetClientReport(JsonConvert.DeserializeObject<ApiFilterClient>(request.RequestData), identity, log);
                case "GetPaymentRequests":
                    return GetPaymentRequests(JsonConvert.DeserializeObject<ApiBaseFilter>(request.RequestData), identity, log);
                case "GetClientChangeHistory":
                    return GetClientChangeHistory(JsonConvert.DeserializeObject<ApiFilterClient>(request.RequestData), identity, log);
                case "GetClientSessions":
                    return GetClientSessions(JsonConvert.DeserializeObject<ApiFilterClient>(request.RequestData), identity, log);
                case "GetGamesSessions":
                    return GetGamesSessions(JsonConvert.DeserializeObject<ApiFilterClient>(request.RequestData), identity, log);
                case "GetBonuses":
                    return GetBonuses(JsonConvert.DeserializeObject<ApiFilterClient>(request.RequestData), identity, log);
                case "GetPartnerPaymentSettings":
                    return GetPartnerPaymentSettings(identity, log);
                case "GetGames":
                    return GetGames(identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        public static ApiResponseBase GetClients(ApiFilterClient apiFilter, SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                var filter = new FilterfnClient
                {
                    PartnerId = apiFilter.PartnerId,
                    CreatedFrom = apiFilter.StartDate ?? DateTime.UtcNow.Date,
                    CreatedBefore = apiFilter.EndDate,
                    SkipCount = apiFilter.SkipCount,
                    TakeCount = apiFilter.TakeCount,
                    OrderBy = apiFilter.OrderBy,
                    FieldNameToOrderBy = apiFilter.FieldNameToOrderBy
                };
                if (apiFilter.AffiliatePlatformId.HasValue)
                    filter.AffiliatePlatformIds = new FiltersOperation
                    {
                        IsAnd = true,
                        OperationTypeList = new List<FiltersOperationType>
                        {
                            new FiltersOperationType
                            {
                                OperationTypeId = (int)FilterOperations.IsEqualTo,
                                StringValue = apiFilter.AffiliatePlatformId.ToString(),
                                IntValue = apiFilter.AffiliatePlatformId.Value,
                                DecimalValue = apiFilter.AffiliatePlatformId.Value
                            }
                        }
                    };
                if (!string.IsNullOrEmpty(apiFilter.AffiliateId))
                    filter.AffiliateIds = new FiltersOperation
                    {
                        IsAnd = true,
                        OperationTypeList = new List<FiltersOperationType>
                        {
                            new FiltersOperationType
                            {
                                OperationTypeId = (int)FilterOperations.IsEqualTo,
                                StringValue = apiFilter.AffiliateId
                            }
                        }
                    };
                if (!string.IsNullOrEmpty(apiFilter.AffiliateReferralId))
                    filter.AffiliateReferralIds = new FiltersOperation
                    {
                        IsAnd = true,
                        OperationTypeList = new List<FiltersOperationType>
                        {
                            new FiltersOperationType
                            {
                                OperationTypeId = (int)FilterOperations.IsEqualTo,
                                StringValue = apiFilter.AffiliateReferralId.ToString()
                            }
                        }
                    };
                var resp = clientBl.GetfnClientsPagedModel(filter, true);
                return new ApiResponseBase
                {
                    ResponseObject = new { resp.Count, Entities = resp.Entities.MapToApiClientList(identity.TimeZone, identity.LanguageId) }
                };
            }
        }
        public static ApiResponseBase GetClientInfo(int clientId, SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                var hideClientContactInfo = false;
                try
                {
                    clientBl.CheckPermission(Constants.Permissions.ViewClientContactInfo);
                }
                catch (FaultException<BllFnErrorType> fex)
                {
                    if (fex.Detail.Id == Constants.Errors.DontHavePermission)
                        hideClientContactInfo = true;
                }
                return new ApiResponseBase
                {
                    ResponseObject = clientBl.GetClientInfo(clientId, true).MapToClientInfoModel(hideClientContactInfo, identity.TimeZone)
                };
            }
        }
        public static ApiResponseBase GetClientReport(ApiFilterClient apiFilter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var filter = new FilterfnClientDashboard
                {
                    PartnerId = apiFilter.PartnerId,
                    FromDate = apiFilter.StartDate ?? DateTime.UtcNow.Date,
                    ToDate = apiFilter.EndDate ?? DateTime.UtcNow.Date,
                    SkipCount = apiFilter.SkipCount,
                    TakeCount = apiFilter.TakeCount,
                    OrderBy = apiFilter.OrderBy,
                    FieldNameToOrderBy = apiFilter.FieldNameToOrderBy
                };
                if (apiFilter.AffiliatePlatformId.HasValue)
                    filter.AffiliatePlatformIds = new FiltersOperation
                    {
                        IsAnd = true,
                        OperationTypeList = new List<FiltersOperationType>
                        {
                            new FiltersOperationType
                            {
                                OperationTypeId = (int)FilterOperations.IsEqualTo,
                                StringValue = apiFilter.AffiliatePlatformId.ToString(),
                                IntValue = apiFilter.AffiliatePlatformId.Value,
                                DecimalValue = apiFilter.AffiliatePlatformId.Value
                            }
                        }
                    };
                if (!string.IsNullOrEmpty(apiFilter.AffiliateId))
                    filter.AffiliateIds = new FiltersOperation
                    {
                        IsAnd = true,
                        OperationTypeList = new List<FiltersOperationType>
                        {
                            new FiltersOperationType
                            {
                                OperationTypeId = (int)FilterOperations.IsEqualTo,
                                StringValue = apiFilter.AffiliateId
                            }
                        }
                    };
                if (!string.IsNullOrEmpty(apiFilter.AffiliateReferralId))
                    filter.AffiliateReferralIds = new FiltersOperation
                    {
                        IsAnd = true,
                        OperationTypeList = new List<FiltersOperationType>
                        {
                            new FiltersOperationType
                            {
                                OperationTypeId = (int)FilterOperations.IsEqualTo,
                                StringValue = apiFilter.AffiliateReferralId.ToString()
                            }
                        }
                    };
                return new ApiResponseBase
                {
                    ResponseObject = reportBl.GetClientsInfoList(filter)
                };
            }
        }

        public static ApiResponseBase GetClientChangeHistory(ApiFilterClient filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = reportBl.GetClientChangeHistory(filter.ClientId, filter.StartDate ?? DateTime.UtcNow, filter.EndDate, identity.TimeZone)
                };
            }
        }

        public static ApiResponseBase GetClientSessions(ApiFilterClient apiFilter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var filter = new FilterReportByfnClientSession
                {
                    FromDate = apiFilter.StartDate ?? DateTime.UtcNow.Date,
                    ToDate = apiFilter.EndDate ?? DateTime.UtcNow
                };
                if (apiFilter.ClientId.HasValue)
                    filter.ClientIds = new FiltersOperation
                    {
                        IsAnd = true,
                        OperationTypeList = new List<FiltersOperationType>
                        {
                            new FiltersOperationType
                            {
                                OperationTypeId = (int)FilterOperations.IsEqualTo,
                                IntValue = apiFilter.ClientId.Value
                            }
                        }
                    };
                var result = reportBl.GetClientSessions(filter, true);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        Entities = result.Entities.MapToClientSessionModels(identity.TimeZone)
                    }
                };
            }
        }

        public static ApiResponseBase GetGamesSessions(ApiFilterClient apiFilter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var filter = new FilterReportByfnClientSession
                {
                    FromDate = apiFilter.StartDate ?? DateTime.UtcNow.Date,
                    ToDate = apiFilter.EndDate ?? DateTime.UtcNow
                };
                if (apiFilter.ClientId.HasValue)
                    filter.ClientIds = new FiltersOperation
                    {
                        IsAnd = true,
                        OperationTypeList = new List<FiltersOperationType>
                        {
                            new FiltersOperationType
                            {
                                OperationTypeId = (int)FilterOperations.IsEqualTo,
                                IntValue = apiFilter.ClientId.Value
                            }
                        }
                    };
                var result = reportBl.GetClientSessions(filter, false);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        Entities = result.Entities.MapToClientSessionModels(identity.TimeZone)
                    }
                };
            }
        }

        public static ApiResponseBase GetPaymentRequests(ApiBaseFilter apiFilter, SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                if (!apiFilter.StartDate.HasValue)
                    apiFilter.StartDate = DateTime.UtcNow.Date;
                var startDate = (long)apiFilter.StartDate.Value.Year * 100000000 + (long)apiFilter.StartDate.Value.Month * 1000000 +
                                (long)apiFilter.StartDate.Value.Day * 10000 + (long)apiFilter.StartDate.Value.Hour * 100 + (long)apiFilter.StartDate.Value.Minute;
                var filter = new FilterfnPaymentRequest
                {
                    PartnerId = apiFilter.PartnerId,
                    FromDate = startDate
                };
                if (apiFilter.EndDate.HasValue)
                    filter.ToDate = (long)apiFilter.EndDate.Value.Year * 100000000 + (long)apiFilter.EndDate.Value.Month * 1000000 +
                                    (long)apiFilter.EndDate.Value.Day * 10000 + (long)apiFilter.EndDate.Value.Hour * 100 + (long)apiFilter.EndDate.Value.Minute;
                return new ApiResponseBase
                {
                    ResponseObject = paymentSystemBl.GetPaymentRequests(filter, true).Select(x => x.MapToApiPaymentRequest(identity.TimeZone))
                };
            }
        }

        private static ApiResponseBase GetBonuses(ApiFilterClient apiFilter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var filter = new FilterReportByBonus
                {
                    FromDate = apiFilter.StartDate ?? DateTime.UtcNow.Date,
                    ToDate = apiFilter.EndDate ?? DateTime.UtcNow
                };
                if (apiFilter.ClientId.HasValue)
                    filter.ClientIds = new FiltersOperation
                    {
                        IsAnd = true,
                        OperationTypeList = new List<FiltersOperationType>
                                {
                                    new FiltersOperationType
                                    {
                                        OperationTypeId = (int)FilterOperations.IsEqualTo,
                                        IntValue = apiFilter.ClientId.Value
                                    }
                                }
                    };

                var result = reportBl.GetReportByBonus(filter);
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        result.TotalBonusPrize,
                        result.TotalFinalAmount,
                        Entities = result.Entities.Select(x => x.MapToApiClientBonus(identity.TimeZone)).ToList()
                    }
                };
            }
        }

        public static ApiResponseBase GetPartnerPaymentSettings(SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                var paymentSettings = paymentSystemBl.GetfnPartnerPaymentSettings(new FilterfnPartnerPaymentSetting(), true, identity.PartnerId);
                return new ApiResponseBase
                {
                    ResponseObject = paymentSettings.Select(x => x.MapTofnPartnerPaymentSettingModel(identity.TimeZone)).ToList()
                };
            }
        }

        public static ApiResponseBase GetGames(SessionIdentity identity, ILog log)
        {
            using (var productBl = new ProductBll(identity, log))
            {
                var filter = new FilterfnProduct { Level = 4 };
                var products = productBl.GetFnProducts(filter, true);
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        products.Count,
                        Entities = products.Entities.Select(x => x.MapTofnProductModel(identity.TimeZone, productBl.HideAggregatorInfo())).ToList()
                    }
                };
            }
        }

        #region Wynta

        [HttpPost]
        [Route("Wynta/player-register-data")]
        public HttpResponseMessage GetRegisteredPlayers(string registrationDate, int whitelabelid)
        {
            var result = string.Empty;
            try
            {
                if (!Request.Headers.Contains("Authorization"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var headerApiKey = Request.Headers.GetValues("Authorization").FirstOrDefault();
                if (string.IsNullOrEmpty(headerApiKey))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(AffiliatePlatforms.Wynta);
                BaseBll.CheckIp(WhitelistedIps, WebApiApplication.DbLogger);
               
                var partner = CacheManager.GetPartnerById(whitelabelid) ??
                   throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerNotFound);
                var apiKey = CacheManager.GetPartnerSettingByKey(partner.Id, AffiliatePlatforms.Wynta + Constants.PartnerKeys.AffiliateApiKey).StringValue;
                if (headerApiKey.Replace("Basic ", string.Empty) != apiKey)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                using (var affiliateService = new AffiliateService(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var fromDate = Convert.ToDateTime(registrationDate);
                    var toDate = fromDate.AddDays(1);
                    var affiliate = affiliateService.GetAffiliatePlatform(partner.Id, AffiliatePlatforms.Wynta) ??
                     throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.AffiliateNotFound);

                    var clients = affiliateService.GetAffiliateClients(affiliate.Id, fromDate, toDate).Select(x => new
                    {
                        Alias = $"{x.FirstName} {x.LastName}",
                        CasinoName = partner.Name,
                        x.City,
                        Country = CacheManager.GetRegionById(x.CountryId ?? x.RegionId, x.LanguageId)?.IsoCode,
                        ClickID = affiliate.AffiliateReferrals.FirstOrDefault().RefId,
                        Currency = x.CurrencyId,
                        Gender = Enum.GetName(typeof(Gender), x.Gender ?? (int)Gender.Male),
                        PlayerID = x.Id,
                        RegisteredDate = x.CreationTime,
                        WhitelabelId = partner.Id
                    }).ToList();
                    result = JsonConvert.SerializeObject(clients);
                    SaveBroadcastHistory(partner.Id, result, affiliateService);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                result = $"Code: {fex.Detail.Id} Message: {fex.Detail.Message}";
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()}");
            }
            catch (Exception ex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                result = $"Error: {ex} Response: {ex}";
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {ex}");
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(result, Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("Wynta/player-activity-data")]
        public HttpResponseMessage GetPlayersActivity(string activityDate, int whitelabelid)
        {
            var result = string.Empty;
            try
            {
                List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(AffiliatePlatforms.Wynta);
                BaseBll.CheckIp(WhitelistedIps, WebApiApplication.DbLogger);               
                if (!Request.Headers.Contains("Authorization"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var headerApiKey = Request.Headers.GetValues("Authorization").FirstOrDefault();
                if (string.IsNullOrEmpty(headerApiKey))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var partner = CacheManager.GetPartnerById(whitelabelid) ??
                   throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerNotFound);
                var apiKey = CacheManager.GetPartnerSettingByKey(partner.Id, AffiliatePlatforms.Wynta + Constants.PartnerKeys.AffiliateApiKey).StringValue;
                if (headerApiKey.Replace("Basic ", string.Empty) != apiKey)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                using (var affiliateService = new AffiliateService(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var fromDate = Convert.ToDateTime(activityDate);
                    var toDate = fromDate.AddDays(1);
                    var affiliate = affiliateService.GetAffiliatePlatform(partner.Id, AffiliatePlatforms.Wynta) ??
                     throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.AffiliateNotFound);

                    var clients = affiliateService.GetAffiliateClients(affiliate.Id, null, null).Select(x => new AffiliatePlatformModel
                    {
                        PartnerId = x.PartnerId,
                        ClientId = x.Id,
                        CurrencyId = x.CurrencyId,
                        ClickId = x.AffiliateReferral.RefId,
                        FirstDepositDate = x.FirstDepositDate
                    }).ToList();
                    result = JsonConvert.SerializeObject(affiliateService.GetWyntaClientActivity(affiliate.Id, clients, fromDate, toDate));
                    SaveBroadcastHistory(partner.Id, result, affiliateService);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                result = $"Code: {fex.Detail.Id} Message: {fex.Detail.Message}";
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()}");
            }
            catch (Exception ex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                result = $"Error: {ex} Response: {ex}";
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {ex}");
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(result, Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        private static void SaveBroadcastHistory(int partnerId, string content, BaseBll baseBll)
        {
            var partner = CacheManager.GetPartnerById(partnerId);
            var mainFtpModel = new FtpModel
            {
                Url = CacheManager.GetPartnerSettingByKey(Constants.MainPartnerId, Constants.PartnerKeys.StatementFTPServer).StringValue,
                UserName = CacheManager.GetPartnerSettingByKey(Constants.MainPartnerId, Constants.PartnerKeys.StatementFTPUsername).StringValue,
                Password = CacheManager.GetPartnerSettingByKey(Constants.MainPartnerId, Constants.PartnerKeys.StatementFTPPassword).StringValue
            };
            var path = $"/AffiliateFiles/{AffiliatePlatforms.Wynta}/{partner.Name}";
            BaseBll.CreateFtpDirectory(mainFtpModel, $"ftp://{mainFtpModel.Url}{path}");
            baseBll.UploadFile(content, $"{path}/activity_{DateTime.UtcNow}.csv", mainFtpModel);
        }

        #endregion
    }
}