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
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.UserModels;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.AgentWebApiCore;
using IqSoft.CP.Common.Models.CacheModels;
using Microsoft.AspNetCore.Hosting;

namespace IqSoft.CP.AgentWebApi.Controllers
{
    public class MainController : ControllerBase
    {
        private IWebHostEnvironment HostEnvironment;
        public MainController(IWebHostEnvironment _environment)
        {
            HostEnvironment = _environment;
        }

        [HttpPost]
        public Session LoginUser([FromQuery]RequestInfo requestInfo, LoginInput input)
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
                action.Source = Request.Headers.ContainsKey("User-Agent") ? Request.Headers["User-Agent"].ToString() : string.Empty;
                   var ipCountry = Request.Headers.ContainsKey("CF-IPCountry") ? Request.Headers["CF-IPCountry"].ToString() : string.Empty;
                    var ip = Request.Headers.ContainsKey("CF-Connecting-IP") ? Request.Headers["CF-Connecting-IP"].ToString() : "127.0.0.1";
                    var origin = Request.Headers.ContainsKey("Origin") ? Request.Headers["Origin"].ToString() : string.Empty;
                    if (!string.IsNullOrEmpty(origin))
                    origin =  origin.Replace("http://", string.Empty).Replace("https://", string.Empty).Replace("agent", "admin").Replace("affiliate", "admin");                
                if (string.IsNullOrEmpty(ip))
                    ip = "127.0.0.1";

                action.Domain = origin;
                action.Country = ipCountry;
                action.Ip = ip;
                action.ActionId = CacheManager.GetAction(MethodBase.GetCurrentMethod().Name).Id;

                using (var userBl = new UserBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var partnerBl = new PartnerBll(userBl))
                    {
                        var partner = partnerBl.GetPartners(new FilterPartner { AdminSiteUrl = origin }, false).FirstOrDefault();
                        if (partner == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerNotFound);
                        var blockedIps = CacheManager.GetConfigParameters(partner.Id, "AgentBlockedIps").Select(x => x.Key).ToList();
                        if (blockedIps.Contains(ip))
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.DontHavePermission);

                        var whitelistedIps = CacheManager.GetConfigParameters(partner.Id, "AgentWhitelistedIps").Select(x => x.Key).ToList();
                        var whitelistedCountries = CacheManager.GetConfigParameters(partner.Id, "AgentWhitelistedCountries").Select(x => x.Key).ToList();
                        
                        if (!whitelistedIps.Any(x => x.IsIpEqual(ip)) && (whitelistedCountries.Any() && !whitelistedCountries.Contains(ipCountry)))
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.DontHavePermission);
                        var loginInput = new LoginUserInput
                        {
                            PartnerId = partner.Id,
                            UserName = input.UserName,
                            Password = input.Password,
                            Ip = ip,
                            LanguageId = requestInfo.LanguageId,
                            UserType = (int)UserTypes.Agent,
                            ReCaptcha = input.ReCaptcha
                        };

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
                    }
                }
            }
            catch (FaultException<BllFnErrorType> e)
            {
                Program.DbLogger.Error(e);
                loginResult = new Session
                {
                    ResponseCode = e.Detail.Id,
                    Description = e.Detail.Message
                };
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
                action.ResultCode = loginResult.ResponseCode;
                action.Description = loginResult.Description;
                BaseBll.LogAction(action);
            }
            return loginResult;
        }
      
        [HttpPost]
        public ApiResponseBase ApiRequest([FromQuery]RequestInfo requestInfo, RequestBase request)
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
                actionLog.Source = Request.Headers.ContainsKey("User-Agent") ? Request.Headers["User-Agent"].ToString() : string.Empty;
                actionLog.Ip = Request.Headers.ContainsKey("CF-Connecting-IP") ? Request.Headers["CF-Connecting-IP"].ToString() : "127.0.0.1";
                actionLog.Domain = Request.Headers.ContainsKey("Origin") ? Request.Headers["Origin"].ToString() : string.Empty;
                actionLog.Country = Request.Headers.ContainsKey("CF-IPCountry") ? Request.Headers["CF-IPCountry"].ToString() : string.Empty;
                var action = CacheManager.GetAction(request.Method);
                if (action == null)
                    throw BaseBll.CreateException(requestInfo.LanguageId, Constants.Errors.ActionNotFound);
                actionLog.ActionId = action.Id;
                actionLog.Info = action.Type == (int)ActionTypes.Info ? request.RequestData : string.Empty;
                var identity = CheckToken(requestInfo, request.Token);
                identity.Domain = Request.Host.ToString();
                actionLog.ObjectId = identity.Id;
                actionLog.SessionId = identity.SessionId;
                var user = CacheManager.GetUserById(identity.Id);
                var userState = CacheManager.GetUserSetting(user.Id)?.ParentState;
                if (userState.HasValue && CustomHelper.Greater((UserStates)userState.Value, (UserStates)user.State))
                    user.State = userState.Value;
                if ((user.State == (int)UserStates.Suspended &&
                    !(request.Method.StartsWith("Get") || request.Method.StartsWith("Find") || request.Method.StartsWith("Check") || request.Method.StartsWith("Generate") || 
                    request.Method == "ChangePassword" || request.Method == "ChangeNickName" || request.Method == "ResetSecurityCode")) 
                    || ( user.State != (int)UserStates.Active && user.State != (int)UserStates.Suspended))
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserBlocked);
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
                actionLog.ResultCode = response?.ResponseCode;
                actionLog.Description = response?.Description;
                BaseBll.LogAction(actionLog);
            }
            return response;
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
                Program.DbLogger.Info("Enter_L1");
                action.Source = Request.Headers.ContainsKey("User-Agent") ? Request.Headers["User-Agent"].ToString() : string.Empty;
                var ipCountry = Request.Headers.ContainsKey("CF-IPCountry") ? Request.Headers["CF-IPCountry"].ToString() : string.Empty;
                var ip = Request.Headers.ContainsKey("CF-Connecting-IP") ? Request.Headers["CF-Connecting-IP"].ToString() : "127.0.0.1";
                var origin = Request.Headers.ContainsKey("Origin") ? Request.Headers["Origin"].ToString() : string.Empty;
                action.Ip = string.IsNullOrEmpty(ip) ? "127.0.0.1" : ip;
                action.Domain = origin;
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
        public ApiResponseBase ValidateToken([FromQuery]RequestInfo requestInfo, string token)
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
                action.Source = Request.Headers.ContainsKey("User-Agent") ? Request.Headers["User-Agent"].ToString() : string.Empty;
                var ipCountry = Request.Headers.ContainsKey("CF-IPCountry") ? Request.Headers["CF-IPCountry"].ToString() : string.Empty;
                var ip = Request.Headers.ContainsKey("CF-Connecting-IP") ? Request.Headers["CF-Connecting-IP"].ToString() : "127.0.0.1";
                action.Ip = string.IsNullOrEmpty(ip) ? "127.0.0.1" : ip;
                action.Domain = Request.Headers.ContainsKey("Origin") ? Request.Headers["Origin"].ToString() : string.Empty; ;
                action.Country = ipCountry;

                action.ActionId = CacheManager.GetAction(MethodBase.GetCurrentMethod().Name).Id;
                var sessionIdentity = CheckToken(requestInfo, token);
                action.SessionId = sessionIdentity.SessionId;
                var user = CacheManager.GetUserById(sessionIdentity.Id);
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

        private ApiResponseBase GetResponse(RequestBase request, SessionIdentity identity)
        {
            switch (request.Controller)
            {
                case Enums.Controllers.Agent:
                    return AgentController.CallFunction(request, identity, Program.DbLogger);
                case Enums.Controllers.Client:
                    return ClientController.CallFunction(request, identity, Program.DbLogger, Request, HostEnvironment);
                case Enums.Controllers.Commission:
                    return CommissionController.CallFunction(request, identity, Program.DbLogger);
                case Enums.Controllers.Payment:
                    return PaymentController.CallFunction(request, identity, Program.DbLogger);
                case Enums.Controllers.Report:
                    return ReportController.CallFunction(request, identity, Program.DbLogger);
                case Enums.Controllers.EnumerationModel:
                    return EnumerationModelController.CallFunction(request, identity, Program.DbLogger);
                case Enums.Controllers.Product:
                    return ProductController.CallFunction(request, identity, Program.DbLogger);
                case Enums.Controllers.Dashboard:
                    return DashboardController.CallFunction(request, identity, Program.DbLogger);
            }
            return new ApiResponseBase();
        }

        private  SessionIdentity CheckToken(RequestInfo requestInfo, string token)
        {
            using (var userBl = new UserBll(new SessionIdentity(), Program.DbLogger))
            {
                var session = userBl.GetUserSession(token);
                var user = userBl.GetUserById(session.UserId);
                if (user.Type < (int)UserTypes.MasterAgent && user.Type!= (int)UserTypes.AdminUser)
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
                    CurrencyId = user.CurrencyId
                };
                return userIdentity;
            }
        }
    }
}
