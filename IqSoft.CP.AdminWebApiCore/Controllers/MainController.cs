using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.AdminWebApiCore;
using Microsoft.AspNetCore.Hosting;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.AdminWebApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly IWebHostEnvironment Environment;
        public MainController(IWebHostEnvironment _environment)
        {
            Environment = _environment;
        }
        [HttpPost]
        public Session LoginUser([FromQuery]RequestInfo requestInfo, EncryptedData inp)
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
                action.Source = Request.Headers["User-Agent"].ToString();
                var ipCountry = Request.Headers.ContainsKey("CF-IPCountry") ? Request.Headers["CF-IPCountry"].ToString() : string.Empty;
                string userIp = Request.Headers.ContainsKey("CF-Connecting-IP") ? Request.Headers["CF-Connecting-IP"] : HttpContext.Connection.RemoteIpAddress.ToString();
                var siteUrl = Request.Headers["Origin"].ToString().Replace("http://", string.Empty).Replace("https://", string.Empty);


                var input = JsonConvert.DeserializeObject<LoginDetails>(CommonFunctions.RSADecrypt(inp.Data));
                action.Domain = siteUrl;
                action.Country = ipCountry;
                action.Ip = userIp;
                action.ActionId = CacheManager.GetAction(MethodBase.GetCurrentMethod().Name).Id;

                using (var userBl = new UserBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var partnerBl = new PartnerBll(userBl))
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
                Program.DbLogger.Error(loginResult);
            }
            catch (Exception e)
            {
                Program.DbLogger.Error(e);
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
                    Program.DbLogger.Error("Action log: " + e.Message);
                }
            }
            return loginResult;
        }

        [HttpPost]
        public ApiResponseBase ApiRequest([FromQuery]RequestInfo requestInfo, [FromBody] RequestBase request)
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
                actionLog.Source = Request.Headers["User-Agent"].ToString();
                actionLog.Ip = Request.Headers.ContainsKey("CF-Connecting-IP") ? Request.Headers["CF-Connecting-IP"] : HttpContext.Connection.RemoteIpAddress.ToString();
                actionLog.Domain = Request.Headers.ContainsKey("Origin") ? Request.Headers["Origin"] : Request.Host.ToString();
                actionLog.Country = Request.Headers.ContainsKey("CF-IPCountry") ? Request.Headers["CF-IPCountry"] : string.Empty;
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
                    using var partnerBl = new PartnerBll(new SessionIdentity(), Program.DbLogger);
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
                Program.DbLogger.Error(m);
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
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
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
                    Program.DbLogger.Error("Action log: " + e.Message);
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
                    return BaseController.CallFunction(request, identity, Program.DbLogger);
                case Enums.Controllers.User:
                    return UserController.CallFunction(request, identity, Program.DbLogger, Environment);
                case Enums.Controllers.Client:
                    return ClientController.CallFunction(request, identity, Program.DbLogger, Environment, Request);
                case Enums.Controllers.BetShop:
                    return BetShopController.CallFunction(request, identity, Program.DbLogger, Environment);
                case Enums.Controllers.Document:
                    return DocumentController.CallFunction(request, identity, Program.DbLogger);
                case Enums.Controllers.Partner:
                    return PartnerController.CallFunction(request, identity, Program.DbLogger, Environment);
                case Enums.Controllers.PaymentSystem:
                    return PaymentSystemController.CallFunction(request, identity, Program.DbLogger, Environment);
                case Enums.Controllers.Report:
                    return ReportController.CallFunction(request, identity, Program.DbLogger, Environment);
                case Enums.Controllers.EnumerationModel:
                    return EnumerationModelController.CallFunction(request, identity, Program.DbLogger);
                case Enums.Controllers.Permission:
                    return PermissionController.CallFunction(request, identity, Program.DbLogger);
                case Enums.Controllers.Product:
                    return ProductController.CallFunction(request, identity, Program.DbLogger, Environment);
                case Enums.Controllers.Dashboard:
                    return DashboardController.CallFunction(request, identity, Program.DbLogger, Environment);
                case Enums.Controllers.Region:
                    return RegionController.CallFunction(request, identity, Program.DbLogger);
                case Enums.Controllers.Currency:
                    return CurrencyController.CallFunction(request, identity, Program.DbLogger);
                case Enums.Controllers.Util:
                    return UtilController.CallFunction(request, identity, Program.DbLogger);
                case Enums.Controllers.Bonus:
                    return BonusController.CallFunction(request, identity, Program.DbLogger);
                case Enums.Controllers.Language:
                    return LanguageController.CallFunction(request, identity, Program.DbLogger);
                case Enums.Controllers.Content:
                    return ContentController.CallFunction(request, identity, Program.DbLogger);
            }
            return new ApiResponseBase();
        }

        [HttpPost]
        public List<Language> GetAvailableLanguages([FromQuery]RequestInfo requestInfo)
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
                action.Source = Request.Headers["User-Agent"].ToString();
                var ipCountry = Request.Headers["CF-IPCountry"];
                var ip = Request.Headers["CF-Connecting-IP"];
                var siteUrl = Request.Headers["Origin"];
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
        public ResponseBase ValidateToken([FromQuery]RequestInfo requestInfo, string token)
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
                action.Source = Request.Headers["User-Agent"].ToString();
                var ipCountry = Request.Headers.ContainsKey("CF-IPCountry") ? Request.Headers["CF-IPCountry"].ToString() : string.Empty;
                var siteUrl = Request.Headers.ContainsKey("Origin") ? Request.Headers["Origin"].ToString() : Request.Host.ToString();
                action.Ip = Request.Headers.ContainsKey("CF-Connecting-IP") ? Request.Headers["CF-Connecting-IP"] : HttpContext.Connection.RemoteIpAddress.ToString();
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
        public ApiResponseBase GetUserBalance([FromQuery]RequestInfo requestInfo, string token)
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
                action.Source = Request.Headers["User-Agent"].ToString();
                var ipCountry = Request.Headers.ContainsKey("CF-IPCountry") ? Request.Headers["CF-IPCountry"].ToString() : string.Empty;
                var siteUrl = Request.Headers.ContainsKey("Origin") ? Request.Headers["Origin"].ToString() : Request.Host.ToString();
                action.Ip = Request.Headers.ContainsKey("CF-Connecting-IP") ? Request.Headers["CF-Connecting-IP"] : HttpContext.Connection.RemoteIpAddress.ToString();


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
            using (var userBl = new UserBll(new SessionIdentity(), Program.DbLogger))
            {
                var session = userBl.GetUserSession(token, extendSession: extend);
                var user = userBl.GetUserById(session.UserId);
                if (user.Type >= (int)UserTypes.MasterAgent)
                    throw BaseBll.CreateException(requestInfo.LanguageId, Constants.Errors.NotAllowed);
                var userIdentity = new SessionIdentity
                {
                    LanguageId = requestInfo.LanguageId,
                    LoginIp = session.Ip,
                    PartnerId = user.PartnerId,
                    SessionId = session.Id,
                    Token = session.Token,
                    Id = session.UserId,
                    TimeZone = requestInfo.TimeZone,
                    CurrencyId = user.CurrencyId,
                    IsAdminUser = user.Type == (int)UserTypes.AdminUser
                };
                return userIdentity;
            }
        }
    }
}