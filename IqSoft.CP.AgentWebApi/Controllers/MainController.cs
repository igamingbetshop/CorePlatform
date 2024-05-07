using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.AgentWebApi.ControllerClasses;
using IqSoft.CP.AgentWebApi.Models;
using IqSoft.CP.AgentWebApi.Models.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.UserModels;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.AgentWebApi.Models.Affiliate;

namespace IqSoft.CP.AgentWebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "GET,POST")]
    public class MainController : ApiController
    {
        [HttpPost]
        public Session LoginUser([FromUri]RequestInfo requestInfo, LoginInput input)
        {
            var loginResult = new Session();
            var action = new DAL.ActionLog
            {
                Page = string.Empty,
                ObjectTypeId = (int)ObjectTypes.User,
                ResultCode = 0,
                Language = requestInfo.LanguageId,
                Info = string.Empty
            };
            try
            {
                var isAffiliate = false;
                action.Source = (Request != null && Request.Headers != null && Request.Headers.UserAgent != null) ? 
                    Request.Headers.UserAgent.ToString() : string.Empty;
                var ip = string.Empty;
                var ipCountry = string.Empty;
                string siteUrl = string.Empty;
                if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.Headers != null)
                {
                    ipCountry = HttpContext.Current.Request.Headers.Get("CF-IPCountry") ?? string.Empty;
                    ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP");
                    var origin = HttpContext.Current.Request.Headers.Get("Origin");
                    WebApiApplication.DbLogger.Debug("Origin: " + origin);
                    if (!string.IsNullOrEmpty(origin))
                    {
                        siteUrl = origin.Replace("http://", string.Empty).Replace("https://", string.Empty).Replace("agent", "admin").Replace("affiliate", "admin");
                        if (origin.Contains("affiliate") || origin == "http://localhost:4201" || origin== "http://10.50.17.10:10006") // development 
                            isAffiliate = true;
                    }
                }
                if (string.IsNullOrEmpty(ip))
                    ip = "127.0.0.1";

                action.Domain = siteUrl;
                action.Country = ipCountry;
                action.Ip = ip;
                action.ActionId = CacheManager.GetAction(MethodBase.GetCurrentMethod().Name).Id;
                action.ObjectTypeId = isAffiliate ? (int)ObjectTypes.Affiliate : (int)ObjectTypes.User;
                using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var contentBl = new ContentBll(partnerBl))
                    {
                        var partner = partnerBl.GetPartners(new FilterPartner { AdminSiteUrl = siteUrl }, false).FirstOrDefault() ??
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerNotFound);
                        PartnerBll.CheckApiRestrictions(partner.Id, isAffiliate ? Constants.SystemModuleTypes.AffilliateSystem : Constants.SystemModuleTypes.AgentSystem);
                        var loginInput = new LoginUserInput
                        {
                            PartnerId = partner.Id,
                            UserName = input.UserName,
                            Password = input.Password,
                            Ip = ip,
                            LanguageId = requestInfo.LanguageId,
                            UserType = (int)UserTypes.DownlineAgent,
                            ReCaptcha = input.ReCaptcha
                        };

                        if (!isAffiliate)
                        {
                            using (var userBl = new UserBll(partnerBl))
                            {
                                var userIdentity = userBl.LoginUser(loginInput, out string imageData);
                                var user = CacheManager.GetUserById(userIdentity.Id);
                                var parentLevel = user.ParentId.HasValue ? CacheManager.GetUserById(user.ParentId.Value)?.Level : 0;
                                BllUserSetting userSetting;
                                if (user.Type == (int)UserTypes.AdminUser)
                                    userSetting = new BllUserSetting
                                    {
                                        UserId = user.Id,
                                        AllowAutoPT = true,
                                        AllowOutright = true,
                                        AllowDoubleCommission = true,
                                        AgentMaxCredit = 0
                                    };
                                else
                                    userSetting = CacheManager.GetUserSetting(user.Id);

                                loginResult = new Session(userIdentity, user)
                                {
                                    ImageData = imageData,
                                    AllowAutoPT = userSetting?.AllowAutoPT,
                                    AllowOutright = userSetting?.AllowOutright,
                                    AllowDoubleCommission = userSetting?.AllowDoubleCommission,
                                    IsCalculationPeriodBlocked = userSetting?.IsCalculationPeriodBlocked,
                                    AgentMaxCredit = userSetting?.AgentMaxCredit,
                                    ParentLevel = parentLevel ?? 0
                                };
                                loginResult.AdminMenu = contentBl.GetAdminMenus(new List<string>(), true, (int)InterfaceTypes.Agent).ToList();
                            }
                        }
                        else
                        {
                            using (var affiliateBl = new AffiliateService(partnerBl))
                            {
                                WebApiApplication.DbLogger.Debug("LoginAffiliate_" + JsonConvert.SerializeObject(loginInput));
                                var userIdentity = affiliateBl.LoginAffiliate(loginInput);
                                var affiliate = affiliateBl.GetAffiliateById(userIdentity.Id, false);
                                loginResult = new Session
                                {
                                    AffiliateId = userIdentity.Id,
                                    LoginIp = userIdentity.LoginIp,
                                    LanguageId = userIdentity.LanguageId,
                                    SessionId = userIdentity.SessionId,
                                    Token = userIdentity.Token,
                                    State = userIdentity.State,
                                    UserName = affiliate.UserName,
                                    NickName = affiliate.NickName,
                                    FirstName = affiliate.FirstName,
                                    LastName = affiliate.LastName,
                                    CurrencyId = partner.CurrencyId
                                };
                                loginResult.AdminMenu = contentBl.GetAdminMenus(new List<string>(), true, (int)InterfaceTypes.Agent).ToList();
                            }
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> e)
            {
                WebApiApplication.DbLogger.Error(e);
                if (e.Detail != null && e.Detail.Id == Constants.Errors.DontHavePermission)
                    WebApiApplication.DbLogger.Error($"Ip: {HttpContext.Current.Request.Headers.Get("CF-Connecting-IP")}, " +
                                                     $"Country: {HttpContext.Current.Request.Headers.Get("CF-IPCountry")} " + e);
                loginResult = new Session
                {
                    ResponseCode = e.Detail.Id,
                    Description = e.Detail.Message
                };
            }
            catch (Exception e)
            {
                WebApiApplication.DbLogger.Error(e);
                loginResult = new Session
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = e.Message
                };
            }
            finally
            {
                action.ResultCode = loginResult.ResponseCode;
                action.Description = loginResult.Description;
                BaseBll.LogAction(action);
            }
            return loginResult;
        }

        [HttpPost]
        public ApiResponseBase ValidateTwoFactorPIN([FromUri] RequestInfo requestInfo, Api2FAInput input)
        {
            var actionLog = new DAL.ActionLog
            {
                Page = string.Empty,
                ObjectTypeId = (int)ObjectTypes.User,
                ResultCode = 0,
                Language = requestInfo.LanguageId,
                Info = string.Empty
            };
            ApiResponseBase response = null;
            try
            {
                actionLog.Source = Request.Headers.UserAgent.ToString();
                actionLog.Ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP") ?? HttpContext.Current.Request.UserHostAddress;
                actionLog.Domain = HttpContext.Current.Request.Headers.Get("Origin") ?? HttpContext.Current.Request.Url.Host;
                actionLog.Country = HttpContext.Current.Request.Headers.Get("CF-IPCountry") ?? string.Empty;
                actionLog.ActionId = CacheManager.GetAction(MethodBase.GetCurrentMethod().Name).Id;

                var user = CacheManager.GetUserById(input.AgentId) ??
                    throw BaseBll.CreateException(requestInfo.LanguageId, Constants.Errors.UserNotFound);
                using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    userBl.CheckUserTwoFactorPin(user, input.Token, input.Pin);
                    userBl.UpdateUserSessionStatus(user.Id, input.Token, SessionStates.Active);
                    return new ApiResponseBase();
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null)
                    response = new ApiResponseBase
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                else
                    response = new ApiResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    };
                if (response.ResponseCode == Constants.Errors.ActionNotFound)
                    WebApiApplication.DbLogger.Error("ActionNotFound: " + MethodBase.GetCurrentMethod().Name);
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
            }
            finally
            {
                try
                {
                    actionLog.ResultCode = response?.ResponseCode;
                    actionLog.Description = response?.Description;
                    BaseBll.LogAction(actionLog);
                }
                catch (Exception e)
                {
                    WebApiApplication.DbLogger.Error("Action log: " + e.Message);
                }
            }
            return response;
        }

        [HttpPost]
        public ApiResponseBase ApiRequest([FromUri] RequestInfo requestInfo, RequestBase request)
        {
            var actionLog = new DAL.ActionLog
            {
                Page = string.Empty,
                ObjectTypeId = (int)ObjectTypes.User,
                ResultCode = 0,
                Language = requestInfo.LanguageId
            };
            ApiResponseBase response = null;
            try
            {
                actionLog.Source = Request.Headers.UserAgent.ToString();
                actionLog.Ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP") ?? "127.0.0.1";
                actionLog.Domain = HttpContext.Current.Request.Headers.Get("Origin");
                actionLog.Country = HttpContext.Current.Request.Headers.Get("CF-IPCountry");
                var action = CacheManager.GetAction(request.Method);
                if (action == null)
                    throw BaseBll.CreateException(requestInfo.LanguageId, Constants.Errors.ActionNotFound);
                actionLog.ActionId = action.Id;
                actionLog.Info = action.Type == (int)ActionTypes.Info ? request.RequestData : string.Empty;
                var identity = CheckToken(requestInfo, request.Token);
                identity.Domain = HttpContext.Current.Request.Url.Authority;
                identity.Country = actionLog.Country;
                actionLog.ObjectId = identity.Id;
                actionLog.SessionId = identity.SessionId;
                actionLog.ObjectTypeId = identity.IsAffiliate ? (int)ObjectTypes.Affiliate : (int)ObjectTypes.User;
                PartnerBll.CheckApiRestrictions(identity.PartnerId, identity.IsAffiliate ? Constants.SystemModuleTypes.AffilliateSystem : Constants.SystemModuleTypes.AgentSystem);

                if (!identity.IsAffiliate)
                {
                    var user = CacheManager.GetUserById(identity.Id);
                    var userState = CacheManager.GetUserSetting(user.Id)?.ParentState;
                    if (userState.HasValue && CustomHelper.Greater((UserStates)userState.Value, (UserStates)user.State))
                        user.State = userState.Value;
                    if ((user.State == (int)UserStates.Suspended &&
                        !(request.Method.StartsWith("Get") || request.Method.StartsWith("Find") || request.Method.StartsWith("Check") || request.Method.StartsWith("Generate") ||
                        request.Method == "ChangePassword" || request.Method == "ChangeNickName" || request.Method == "ResetSecurityCode"))
                        || (user.State != (int)UserStates.Active && user.State != (int)UserStates.Suspended))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserBlocked);
                }
                response = GetResponse(request, identity);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null)
                {
                    response = new ApiResponseBase
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                    if (ex.Detail.Id == Constants.Errors.SessionExpired || ex.Detail.Id == Constants.Errors.SessionNotFound)
                    {
                        response.ResponseObject = ex.Detail.IntegerInfo;
                    }
                }
                else
                    response = new ApiResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    };
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
            }
            finally
            {
                actionLog.ResultCode = response.ResponseCode;
                actionLog.Description = response.Description;
                BaseBll.LogAction(actionLog, WebApiApplication.DbLogger);
            }
            return response;
        }

        [HttpPost]
        public List<Language> GetAvailableLanguages([FromUri]RequestInfo requestInfo)
        {
            var action = new DAL.ActionLog
            {
                Page = string.Empty,
                ObjectTypeId = (int)ObjectTypes.User,
                ResultCode = 0,
                Language = requestInfo.LanguageId,
                Info = string.Empty
            };
            try
            {
                action.Source = Request.Headers.UserAgent.ToString();
                var ipCountry = HttpContext.Current.Request.Headers.Get("CF-IPCountry");
                var ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP");
                var siteUrl = HttpContext.Current.Request.Headers.Get("Origin");
                action.Ip = string.IsNullOrEmpty(ip) ? "127.0.0.1" : ip;
                action.Domain = siteUrl;
                action.Country = ipCountry;
                action.ActionId = CacheManager.GetAction(MethodBase.GetCurrentMethod().Name).Id;

                return CacheManager.GetAvailableLanguages().Select(x => new Language { Id = x.Id, Name = x.Name }).ToList();
            }
            catch (FaultException<BllFnErrorType> e)
            {
                action.ResultCode = e.Detail.Id;
                action.Description = e.Detail.Message;
            }
            catch (Exception e)
            {
                action.ResultCode = Constants.Errors.GeneralException;
                action.Description = e.Message;
            }
            finally
            {
                BaseBll.LogAction(action);
            }
            return new List<Language>();
        }
        
        [HttpGet]
        public ApiResponseBase ValidateToken([FromUri]RequestInfo requestInfo, string token)
        {
            var result = new ApiResponseBase
            {
                ResponseCode = 0,
                Description = "Success"
            };
            DAL.ActionLog action = new DAL.ActionLog
            {
                Page = string.Empty,
                ObjectTypeId = (int)ObjectTypes.User,
                ResultCode = 0,
                Language = requestInfo.LanguageId,
                Info = string.Empty,
            };
            try
            {
                action.Source = Request.Headers.UserAgent.ToString();
                var ipCountry = HttpContext.Current.Request.Headers.Get("CF-IPCountry");
                var ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP");
                var siteUrl = HttpContext.Current.Request.Headers.Get("Origin");
                action.Ip = string.IsNullOrEmpty(ip) ? "127.0.0.1" : ip;
                action.Domain = siteUrl;
                action.Country = ipCountry;

                action.ActionId = CacheManager.GetAction(MethodBase.GetCurrentMethod().Name).Id;
                var sessionIdentity = CheckToken(requestInfo, token);
                action.ObjectTypeId = sessionIdentity.IsAffiliate ? (int)ObjectTypes.Affiliate : (int)ObjectTypes.User;
                action.SessionId = sessionIdentity.SessionId;
                if (!sessionIdentity.IsAffiliate)
                {
                    var user = CacheManager.GetUserById(sessionIdentity.Id);
                    PartnerBll.CheckApiRestrictions(user.PartnerId, Constants.SystemModuleTypes.AgentSystem);
                    var parentLevel = user.ParentId.HasValue ? CacheManager.GetUserById(user.ParentId.Value)?.Level : 0;
                    BllUserSetting userSetting;
                    if (user.Type == (int)UserTypes.AdminUser)
                        userSetting = new BllUserSetting
                        {
                            UserId = user.Id,
                            AllowAutoPT = true,
                            AllowOutright = true,
                            AllowDoubleCommission = true,
                            AgentMaxCredit = 0,
                            IsCalculationPeriodBlocked = false
                        };
                    else
                        userSetting = CacheManager.GetUserSetting(user.Id);

                    result.ResponseObject = new Session(sessionIdentity, user)
                    {
                        // ImageData = imageData,
                        AllowAutoPT = userSetting?.AllowAutoPT,
                        AllowOutright = userSetting?.AllowOutright,
                        AllowDoubleCommission = userSetting?.AllowDoubleCommission,
                        AgentMaxCredit = userSetting?.AgentMaxCredit,
                        IsCalculationPeriodBlocked = userSetting?.IsCalculationPeriodBlocked,
                        ParentLevel = parentLevel ?? 0,
                        State = user.State
                    };
                }
                else
                {
                    using (var affiliateBl = new AffiliateService(sessionIdentity, WebApiApplication.DbLogger))
                    {
                        var affiliate = affiliateBl.GetAffiliateById(sessionIdentity.Id, false);
                        PartnerBll.CheckApiRestrictions(affiliate.PartnerId, Constants.SystemModuleTypes.AffilliateSystem);
                        result.ResponseObject = new Session
                        {
                            AffiliateId = sessionIdentity.Id,
                            LoginIp = sessionIdentity.LoginIp,
                            LanguageId = sessionIdentity.LanguageId,
                            SessionId = sessionIdentity.SessionId,
                            Token = sessionIdentity.Token,
                            State = sessionIdentity.State,
                            UserName = affiliate.UserName,
                            NickName = affiliate.NickName,
                            FirstName = affiliate.FirstName,
                            LastName = affiliate.LastName
                        };
                    }
                }
            }
            catch (FaultException<BllFnErrorType> e)
            {
                result.ResponseCode = e.Detail.Id;
                result.Description = e.Detail.Message;
            }
            catch (Exception e)
            {
                result.ResponseCode = Constants.Errors.GeneralException;
                result.Description = e.Message;
            }
            finally
            {
                action.ResultCode = result.ResponseCode;
                action.Description = result.Description;
                BaseBll.LogAction(action);
            }
            return result;
        }

        [HttpPost]
        public ApiResponseBase RecoverPassword(PasswordRecovery input) 
        {
            var actionLog = new DAL.ActionLog
            {
                Page = string.Empty,
                ObjectTypeId = (int)ObjectTypes.User,
                ResultCode = 0,
                Language = input.LanguageId
            };
            try
            {
                var isAffiliate = false;
                var origin = HttpContext.Current.Request.Headers.Get("Origin");
                actionLog.Source = Request.Headers.UserAgent.ToString();
                actionLog.Ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP") ?? "127.0.0.1";
                actionLog.Domain = origin;
                actionLog.Country = HttpContext.Current.Request.Headers.Get("CF-IPCountry");
                var action = CacheManager.GetAction(MethodBase.GetCurrentMethod().Name) ??
                    throw BaseBll.CreateException(input.LanguageId, Constants.Errors.ActionNotFound);
                if (origin.Contains("affiliate") || origin == "http://ag.com" || origin == "http://localhost:4201" || origin == "http://localhost:4200" || origin== "http://10.50.17.10:10006") // development 
                    isAffiliate = true;
                var siteUrl = origin.Replace("http://", string.Empty).Replace("https://", string.Empty).Replace("agent", "admin").Replace("affiliate", "admin");
                using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var notificationBll = new NotificationBll(partnerBl))
                {
                    var partner = partnerBl.GetPartners(new FilterPartner { AdminSiteUrl = siteUrl }, false).FirstOrDefault() ??
                      throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerNotFound);
                    PartnerBll.CheckApiRestrictions(partner.Id, isAffiliate ? Constants.SystemModuleTypes.AffilliateSystem : Constants.SystemModuleTypes.AgentSystem);
                    if (isAffiliate)
                        using (var affiliateService = new AffiliateService(partnerBl))
                            affiliateService.RecoverPassword(partner.Id, input.Token, input.NewPassword);
                    else
                        using (var userBll = new UserBll(partnerBl))userBll.RecoverPassword(partner.Id, input.Token, input.NewPassword);
                }
                return new ApiResponseBase();
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
               return new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
            }
        }

        [HttpPost]
        public ApiResponseBase SendRecoveryToken(RecoveryTokenInput input)
        {
            var actionLog = new DAL.ActionLog
            {
                Page = string.Empty,
                ObjectTypeId = (int)ObjectTypes.User,
                ResultCode = 0,
                Language = input.LanguageId
            };
            try
            {
                var isAffiliate = false;
                var origin = HttpContext.Current.Request.Headers.Get("Origin");
                actionLog.Source = Request.Headers.UserAgent.ToString();
                actionLog.Ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP") ?? "127.0.0.1";
                actionLog.Domain = origin;
                actionLog.Country = HttpContext.Current.Request.Headers.Get("CF-IPCountry");
                var action = CacheManager.GetAction(MethodBase.GetCurrentMethod().Name);
                if (action == null)
                    throw BaseBll.CreateException(input.LanguageId, Constants.Errors.ActionNotFound);
                if (origin.Contains("affiliate") || origin == "http://ag.com" || origin == "http://localhost:4201" || origin == "http://localhost:4200" || origin== "http://10.50.17.10:10006") // development 
                    isAffiliate = true;
                actionLog.ObjectTypeId = isAffiliate ? (int)ObjectTypes.Affiliate : (int)ObjectTypes.User;
                var siteUrl = origin.Replace("http://", string.Empty).Replace("https://", string.Empty).Replace("agent", "admin").Replace("affiliate", "admin");
                int activePeriodInMinutes = 0;
                using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var notificationBll = new NotificationBll(partnerBl))
                {
                    var partner = partnerBl.GetPartners(new FilterPartner { AdminSiteUrl = siteUrl }, false).FirstOrDefault() ??
                      throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerNotFound);
                    PartnerBll.CheckApiRestrictions(partner.Id, isAffiliate ? Constants.SystemModuleTypes.AffilliateSystem : Constants.SystemModuleTypes.AgentSystem);                 
                    if (isAffiliate)
                    {
                        using (var affiliateService = new AffiliateService(partnerBl))
                        {
                            var affiliate = affiliateService.GetAffiliateByIdentifier(partner.Id, input.Identifier);
                            if (affiliate == null)
                                activePeriodInMinutes = CacheManager.GetPartnerById(partner.Id).VerificationKeyActiveMinutes;
                            else activePeriodInMinutes = notificationBll.SendRecoveryEmail(partner.Id, affiliate.Id, (int)ObjectTypes.Affiliate, affiliate.Email);
                        }
                    }
                    else
                    {
                        using (var userBll = new UserBll(partnerBl))
                        {
                            var agent = userBll.GetUserByIdentifier(partner.Id, input.Identifier, true);
                            if (agent == null)
                                activePeriodInMinutes = CacheManager.GetPartnerById(partner.Id).VerificationKeyActiveMinutes;
                            else activePeriodInMinutes = notificationBll.SendRecoveryEmail(partner.Id, agent.Id, (int)ObjectTypes.User, agent.Email);
                        }
                    }
                }
                return new ApiResponseBase
                {
                    ResponseObject = new { ActivePeriodInMinutes = activePeriodInMinutes }
                };
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
            }
        }

        private ApiResponseBase GetResponse(RequestBase request, SessionIdentity identity)
        {
            switch (request.Controller)
            {
                case Enums.Controllers.Agent:
                    return AgentController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Affiliate:
                    return AffiliateController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Client:
                    return ClientController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Commission:
                    return CommissionController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Payment:
                    return PaymentController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Report:
                    return ReportController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.EnumerationModel:
                    return EnumerationModelController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Product:
                    return ProductController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Dashboard:
                    return DashboardController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.BetShop:
                    return BetShopController.CallFunction(request, identity, WebApiApplication.DbLogger);
            }
            return new ApiResponseBase();
        }

        private  SessionIdentity CheckToken(RequestInfo requestInfo, string token)
        {
            using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                var session = userBl.GetUserSession(token);
                var userIdentity = new SessionIdentity
                {
                    LanguageId = requestInfo.LanguageId,
                    LoginIp = session.Ip,
                    SessionId = session.Id,
                    Token = session.Token,
                    Id = session.UserId ?? session.AffiliateId ?? 0,
                    IsAffiliate = session.AffiliateId != null && session.AffiliateId > 0,
                    TimeZone = requestInfo.TimeZone
                };
                if (session.UserId != null)
                {
                    var user = userBl.GetUserById(session.UserId.Value);
                    if (user.Type < (int)UserTypes.CompanyAgent && user.Type != (int)UserTypes.AdminUser)
                        throw BaseBll.CreateException(requestInfo.LanguageId, Constants.Errors.NotAllowed);
                    userIdentity.PartnerId = user.PartnerId;
                    userIdentity.CurrencyId = user.CurrencyId;
                }
                else if (session.AffiliateId != null)
                {
                    using (var affiliateBl = new AffiliateService(userBl))
                    {
                        var affiliate = affiliateBl.GetAffiliateById(session.AffiliateId.Value, false);
                        userIdentity.PartnerId = affiliate.PartnerId;
                    }
                }
                return userIdentity;
            }
        }
    }
}
