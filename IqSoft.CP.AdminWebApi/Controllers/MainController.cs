﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.AdminWebApi.ControllerClasses;
using Newtonsoft.Json;
using Language = IqSoft.CP.AdminWebApi.Models.CommonModels.Language;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.AdminWebApi.Models.UserModels;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Enums;
using System.Reflection;
using System.Data.Entity.Validation;
using IqSoft.CP.AdminWebApi.Models.AdminModels;
using IqSoft.CP.Common.Models.UserModels;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.AdminWebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "GET,POST")]
    public class MainController : ApiController
    {
        [HttpPost]
        public Session LoginUser([FromUri] RequestInfo requestInfo, EncryptedData inp)
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
                action.Source = Request.Headers.UserAgent.ToString();
                var ipCountry = HttpContext.Current?.Request?.Headers?.Get("CF-IPCountry") ?? string.Empty;
                string ip = HttpContext.Current?.Request?.Headers?.Get("CF-Connecting-IP") ?? HttpContext.Current?.Request?.UserHostAddress;
                var siteUrl = HttpContext.Current?.Request?.Headers?.Get("Origin");
                if(!string.IsNullOrEmpty(siteUrl))
                    siteUrl = siteUrl.Replace("http://", string.Empty).Replace("https://", string.Empty);
                var userIp = string.IsNullOrEmpty(ip) ? "127.0.0.1" : ip;
                var input = JsonConvert.DeserializeObject<LoginDetails>(CommonFunctions.RSADecrypt(inp.Data));
                action.Domain = siteUrl;
                action.Country = ipCountry;
                action.Ip = userIp;
                action.ActionId = CacheManager.GetAction(MethodBase.GetCurrentMethod().Name).Id;
                using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var partnerBl = new PartnerBll(userBl))
                    {
                        using (var contentBl = new ContentBll(partnerBl))
                        {
                            var partner = partnerBl.GetPartners(new FilterPartner { AdminSiteUrl = siteUrl }, false).FirstOrDefault();
                            if (partner == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerNotFound);
                            var blockedIps = CacheManager.GetConfigParameters(partner.Id, "AdminBlockedIps").Select(x => x.Key).ToList();
                            if (blockedIps.Contains(userIp))
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.DontHavePermission);
                            var whitelistedIps = CacheManager.GetConfigParameters(partner.Id, "AdminWhitelistedIps").Select(x => x.Key).ToList();
                            var whitelistedCountries = CacheManager.GetConfigParameters(partner.Id, "AdminWhitelistedCountries").Select(x => x.Key).ToList();
                            if (!whitelistedIps.Any(x => x.IsIpEqual(userIp)) && !whitelistedCountries.Contains(ipCountry))
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.DontHavePermission);
                            var loginInput = new LoginUserInput
                            {
                                PartnerId = partner.Id,
                                UserName = input.UserName,
                                Password = input.Password,
                                Ip = userIp,
                                LanguageId = requestInfo.LanguageId,
                                UserType = (int)UserTypes.AdminUser
                            };
                            var userIdentity = userBl.LoginUser(loginInput, out string imageData);
                            loginResult = new Session(userIdentity);
                            var user = userBl.GetUserById(userIdentity.Id);
                            loginResult.UserLogin = user.UserName;
                            loginResult.UserName = string.Format("{0} {1}", user.FirstName, user.LastName);
                            var userPermissions = CacheManager.GetUserPermissions(userIdentity.Id);
                            loginResult.AdminMenu = contentBl.GetAdminMenus(userPermissions.Select(x => x.PermissionId).ToList(), userPermissions.Any(x => x.IsAdmin));
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> e)
            {
                loginResult = new Session
                {
                    ResponseCode = e.Detail.Id,
                    Description = e.Detail.Message
                };
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(loginResult));
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
                try
                {
                    action.ResultCode = loginResult.ResponseCode;
                    action.Description = loginResult.Description;
                    BaseBll.LogAction(action);
                }
                catch (Exception e)
                {
                    WebApiApplication.DbLogger.Error("Action log: " + e.Message);
                }
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

                var user = CacheManager.GetUserById(input.UserId ?? 0) ??
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
        public ApiResponseBase ApiRequest([FromUri]RequestInfo requestInfo, [FromBody]RequestBase request)
        {
            var startTime = DateTime.UtcNow;
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
                actionLog.Ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP") ?? HttpContext.Current.Request.UserHostAddress;
                actionLog.Domain = HttpContext.Current.Request.Headers.Get("Origin") ?? HttpContext.Current.Request.Url.Host;
                actionLog.Country = HttpContext.Current.Request.Headers.Get("CF-IPCountry") ?? string.Empty;
                var action = CacheManager.GetAction(request.Method);
                if (action == null)
                    throw BaseBll.CreateException(requestInfo.LanguageId, Constants.Errors.ActionNotFound);
                actionLog.ActionId = action.Id;
                actionLog.Info = action.Type == (int)ActionTypes.Info ? request.RequestData : string.Empty;
                var identity = new SessionIdentity();
                if (!string.IsNullOrEmpty(request.Token))
                    identity = CheckToken(requestInfo, request.Token);
                else if (!string.IsNullOrEmpty(request.ApiKey) && request.UserId.HasValue)
                {
                    using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
                    {
                        var user = CacheManager.GetUserById(request.UserId.Value);
                        if (user == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.UserNotFound);
                        var partner = partnerBl.GetPartnerById(user.PartnerId);
                        if (partner == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerNotFound);
                        var blockedIps = CacheManager.GetConfigParameters(partner.Id, "AdminBlockedIps").Select(x => x.Key).ToList();
                        if (blockedIps.Contains(actionLog.Ip))
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.DontHavePermission);
                        var whitelistedIps = CacheManager.GetConfigParameters(partner.Id, "AdminWhitelistedIps").Select(x => x.Key).ToList();
                        var whitelistedCountries = CacheManager.GetConfigParameters(partner.Id, "AdminWhitelistedCountries").Select(x => x.Key).ToList();
                        if (!whitelistedIps.Any(x => x.IsIpEqual(actionLog.Ip)) &&
                            !whitelistedCountries.Contains(actionLog.Country))
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.DontHavePermission);
                        identity = CheckApiAuthorization(request.UserId.Value, request.ApiKey, requestInfo.LanguageId, requestInfo.TimeZone);
                    }
                }
                else
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.SessionNotFound);
                actionLog.ObjectId = identity.Id;
                actionLog.SessionId = identity.SessionId == 0 ? (long?)null : identity.SessionId;

                response = GetResponse(request, identity);
            }
            catch (DbEntityValidationException e)
            {
                var m = string.Empty;
                foreach (var eve in e.EntityValidationErrors)
                {
                    m += string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        m += string.Format("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                WebApiApplication.DbLogger.Error(m);
                response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = m
                };
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
                if(response.ResponseCode == Constants.Errors.ActionNotFound)
                    WebApiApplication.DbLogger.Error("ActionNotFound: " + request?.Method);

                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response) + "_" + JsonConvert.SerializeObject(request));
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
                    actionLog.Info = (DateTime.UtcNow - startTime).TotalSeconds + "_" + actionLog.Info;
                    BaseBll.LogAction(actionLog);
                }
                catch(Exception e)
                {
                    WebApiApplication.DbLogger.Error("Action log: " + e.Message);
                }
            }
            return response;
        }

        private SessionIdentity CheckApiAuthorization(int userId, string secureCode, string languageId, double timeZone)
        {
            var user = CacheManager.GetUserById(userId);
            if (user == null)
                throw BaseBll.CreateException(languageId, Constants.Errors.UserNotFound);
            if (user.SecurityCode != secureCode)
                throw BaseBll.CreateException(languageId, Constants.Errors.WrongApiCredentials);

            return new SessionIdentity
            {
                LanguageId = languageId,
                PartnerId = user.PartnerId,
                Token = secureCode,
                Id = userId,
                TimeZone = timeZone,
                CurrencyId = user.CurrencyId,
                IsAdminUser = user.Type == (int)UserTypes.AdminUser
            };
        }

        private ApiResponseBase GetResponse(RequestBase request, SessionIdentity identity)
        {
            switch (request.Controller)
            {
                case Enums.Controllers.Base:
                    return BaseController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.User:
                    return UserController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Client:
                    return ClientController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Affiliate:
                    return AffiliateController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.BetShop:
                    return BetShopController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Document:
                    return DocumentController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Partner:
                    return PartnerController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.PaymentSystem:
                    return PaymentSystemController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Report:
                    return ReportController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.EnumerationModel:
                    return EnumerationModelController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Permission:
                    return PermissionController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Product:
                    return ProductController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Dashboard:
                    return DashboardController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Region:
                    return RegionController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Currency:
                    return CurrencyController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Util:
                    return UtilController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Bonus:
                    return BonusController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Language:
                    return LanguageController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Content:
                    return ContentController.CallFunction(request, identity, WebApiApplication.DbLogger);
                case Enums.Controllers.Provider:
                    return ProviderController.CallFunction(request, identity, WebApiApplication.DbLogger);
            }
            return new ApiResponseBase();
        }

        [HttpPost]
        public List<Language> GetAvailableLanguages([FromUri] RequestInfo requestInfo)
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
        public ResponseBase ValidateToken([FromUri] RequestInfo requestInfo, string token)
        {
            var result = new ResponseBase
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
                action.SessionId = CheckToken(requestInfo, token).SessionId;
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

        [HttpGet]
        public ApiResponseBase GetUserBalance([FromUri]RequestInfo requestInfo, string token)
        {
            var result = new ApiResponseBase();
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
                var identity = CheckToken(requestInfo, token, false);
                action.SessionId = identity.SessionId;
                result = UserController.GetUserBalance(identity);
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

        private SessionIdentity CheckToken(RequestInfo requestInfo, string token, bool extend = true)
        {
            using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                var session = userBl.GetUserSession(token, extendSession: extend);
                var user = userBl.GetUserById(session.UserId.Value);
                if (user.Type >= (int)UserTypes.MasterAgent)
                    throw BaseBll.CreateException(requestInfo.LanguageId, Constants.Errors.NotAllowed);
                var userIdentity = new SessionIdentity
                {
                    LanguageId = requestInfo.LanguageId,
                    LoginIp = session.Ip,
                    PartnerId = user.PartnerId,
                    SessionId = session.Id,
                    Token = session.Token,
                    Id = session.UserId.Value,
                    TimeZone = requestInfo.TimeZone,
                    CurrencyId = user.CurrencyId,
                    IsAdminUser = user.Type == (int)UserTypes.AdminUser
                };
                return userIdentity;
            }
        }
    }
}