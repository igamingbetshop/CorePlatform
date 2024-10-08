using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.AgentWebApi.Filters;
using IqSoft.CP.AgentWebApi.Helpers;
using IqSoft.CP.AgentWebApi.Models;
using IqSoft.CP.AgentWebApi.Models.User;
using log4net;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System;
using IqSoft.CP.Common.Models.UserModels;
using IqSoft.CP.Common.Helpers;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.AgentWebApi.ClientModels;
using static IqSoft.CP.Common.Constants;
using IqSoft.CP.BLL.Models;
using IqSoft.CP.AgentWebApi.Filter;
using IqSoft.CP.Common.Models.Commission;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.AdminModels;

namespace IqSoft.CP.AgentWebApi.ControllerClasses
{
    public static class AgentController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "Logout":
                    return Logout(request.Token, identity, log);
                case "GenerateQRCode":
                    return GenerateQRCode(identity, log);
                case "DisableTwoFactor":
                    return DisableTwoFactor(JsonConvert.DeserializeObject<ApiQRCodeInput>(request.RequestData), identity, log);
                case "EnableTwoFactor":
                    return EnableTwoFactor(JsonConvert.DeserializeObject<ApiQRCodeInput>(request.RequestData), identity, log);
                case "GetBalance":
                    return GetBalance(identity);
                case "GetAgentData":
                    var inp = (string.IsNullOrEmpty(request.RequestData) || request.RequestData == "{}") ? string.Empty :
                        JsonConvert.DeserializeObject<string>(request.RequestData);
                    return GetAgentData(inp, identity, log);
                case "ChangePassword":
                    return ChangePassword(JsonConvert.DeserializeObject<ChangePasswordInput>(request.RequestData), identity, log);
                case "ResetPassword":
                    return ResetPassword(JsonConvert.DeserializeObject<ChangePasswordInput>(request.RequestData), request.SecurityCode, identity, log);
                case "ResetSecurityCode":
                    return ResetSecurityCode(JsonConvert.DeserializeObject<ChangePasswordInput>(request.RequestData), request.SecurityCode, identity, log);
                case "ChangeNickName":
                    return ChangeNickName(JsonConvert.DeserializeObject<NickNameInput>(request.RequestData), identity, log);
                case "CreateAgent":
                    return CreateAgent(JsonConvert.DeserializeObject<UserModel>(request.RequestData), request.SecurityCode, identity, log);
                case "CloneAgent":
                    return CloneAgent(JsonConvert.DeserializeObject<UserModel>(request.RequestData), request.SecurityCode, identity, log);
                case "UpdateAgent":
                    return UpdateAgent(JsonConvert.DeserializeObject<ChangeObjectStateInput>(request.RequestData), identity, log);
                case "UpdateAgentSetting":
                    return UpdateAgentSetting(JsonConvert.DeserializeObject<UserModel>(request.RequestData), request.SecurityCode, identity, log);
                case "GetAgents":
                    return GetAgents(JsonConvert.DeserializeObject<ApiAgentInput>(request.RequestData), identity, log);
                case "GetSubAgents":
                    return GetSubAgents(identity, log);
                case "GetSubAccounts":
                    return GetSubAccounts(Convert.ToInt32(request.RequestData), identity, log);
                case "SaveSubAccount":
                    return SaveSubAccount(JsonConvert.DeserializeObject<UserModel>(request.RequestData), identity, log);
                case "IsUserNameAvailable":
                    return IsUserNameAvailable(JsonConvert.DeserializeObject<ApiUserNameInput>(request.RequestData), identity, log);
                case "CreateDebitCorrection":
                    return CreateDebitCorrection(JsonConvert.DeserializeObject<TransferInput>(request.RequestData), identity, log);
                case "ChangeAgentMaxCredit":
                    return ChangeAgentMaxCredit(JsonConvert.DeserializeObject<TransferInput>(request.RequestData), identity, log);
                case "CreateCreditCorrection":
                    return CreateCreditCorrection(JsonConvert.DeserializeObject<TransferInput>(request.RequestData), identity, log);
                case "GetCorrections":
                    return GetCorrections(JsonConvert.DeserializeObject<ApiFilterUserCorrection>(request.RequestData), identity, log);
                case "GetAgentsCreditInfo":
                    return GetAgentsCreditInfo(JsonConvert.DeserializeObject<string>(request.RequestData), identity, log);
                case "CheckSecurityCode":
                    return CheckSecurityCode(JsonConvert.DeserializeObject<string>(request.RequestData), identity, log);

                //case "GetRoles":
                //    return PermissionController.GetRoles(JsonConvert.DeserializeObject<ApiFilterRole>(request.RequestData), identity, log);
                //case "SaveUserRoles":
                //    return SaveUserRoles(JsonConvert.DeserializeObject<SaveUserRoleModel>(request.RequestData),
                //        identity, log);
                //case "GetRolePermissions":
                //    return PermissionController.GetRolePermissions(
                //        JsonConvert.DeserializeObject<ApiRolePermissionModel>(request.RequestData), identity, log);

                case "GetAnnouncements":
                    return GetAnnouncements(JsonConvert.DeserializeObject<ApiFilterAnnouncement>(request.RequestData), identity, log);
                case "SaveAnnouncement":
                    return SaveAnnouncement(JsonConvert.DeserializeObject<ApiAnnouncement>(request.RequestData), identity, log);

                case "GetTicker":
                    return GetTicker(identity);
                case "GetExistingLevels":
                    return GetAgentDownline(Convert.ToInt32(request.RequestData), identity, log);
                case "GetAgentStatusesInfo":
                    return GetAgentStatusesInfo(Convert.ToInt32(request.RequestData), identity, log);
                case "GetAvailableLevels":
                    return GetAvailableLevels(identity, log);
                case "FindAgentAvailableUserName":
                    return FindAvailableUserName((int)UserTypes.DownlineAgent, JsonConvert.DeserializeObject<UserNameModel>(request.RequestData), identity, log);
                case "FindSubAccountAvailableUserName":
                    return FindAvailableUserName((int)UserTypes.AgentEmployee, new UserNameModel { Level = 0, StartsWith = '\0' }, identity, log);
                case "GenerateUserNamePrefix":
                    return GenerateUserNamePrefix((int)UserTypes.DownlineAgent, Convert.ToInt32(request.RequestData), identity, log);
                case "GenerateSubAccountUserNamePrefix":
                    return GenerateUserNamePrefix((int)UserTypes.AgentEmployee, 0, identity, log);
                case "UploadProfileImage":
                    return UploadProfileImage(JsonConvert.DeserializeObject<string>(request.RequestData), identity, log);
                case "GetAgentTransferCondition":
                    return GetAgentTransferCondition(Convert.ToInt32(request.RequestData), identity, log);
                case "UpdateAgentTransferCondition":
                    return UpdateAgentTransferCondition(JsonConvert.DeserializeObject<ApiAgentSettings>(request.RequestData), identity, log);
                case "ChangeAgentsDoubleCommissionState":
                    return ChangeAgentsDoubleCommissionState(JsonConvert.DeserializeObject<List<ApiAgentSettings>>(request.RequestData), identity, log);
                case "ChangeAgentSettingState":
                    return ChangeAgentSettingState(JsonConvert.DeserializeObject<ApiAgentSettings>(request.RequestData), identity, log);
                case "GetPartnerCurrencies":
                    return GetPartnerCurrencies(identity, log);
                case "GetLevelLimits":
                    return GetLevelLimits(JsonConvert.DeserializeObject<ApiAgentInput>(request.RequestData), identity, log);
                case "FindAgents":
                    return FindAgents(JsonConvert.DeserializeObject<string>(request.RequestData), identity, log);
                case "FindDownlineNames":
                    return FindDownlineNames(JsonConvert.DeserializeObject<ApiAgentInput>(request.RequestData), identity, log);
                case "GetAgentPath":
                    return GetAgentPath(JsonConvert.DeserializeObject<ApiAgentInput>(request.RequestData).Id.Value, identity, log);
                case "SaveNote":
                    return SaveNote(JsonConvert.DeserializeObject<Note>(request.RequestData), identity, log);
                case "GetNotes":
                    return GetNotes(JsonConvert.DeserializeObject<ApiFilterNote>(request.RequestData), identity, log);
                case "GetAgentStates":
                    return GetAgentStates(identity, log);
                case "UpdateUserState":
                    return UpdateUserState(JsonConvert.DeserializeObject<ApiUserState>(request.RequestData), identity, log);
                case "GetUserState":
                    return GetUserState(JsonConvert.DeserializeObject<ApiUserState>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase Logout(string token, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                userBl.LogoutUser(token);
                return new ApiResponseBase();
            }
        }
      
        private static ApiResponseBase DisableTwoFactor(ApiQRCodeInput input, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                userBl.DisableTwoFactor(input.Pin);
                CacheManager.RemoveUserFromCache(identity.Id);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.User, identity.Id));
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase EnableTwoFactor(ApiQRCodeInput input, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                userBl.EnableTwoFactor(input.QRCode, input.Pin);
                CacheManager.RemoveUserFromCache(identity.Id);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.User, identity.Id));
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase GetBalance(SessionIdentity identity)
        {
            if (identity.IsAffiliate)
            {
                return new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.UserNotFound
                };
            }
            else
            {
                var agent = CacheManager.GetUserById(identity.Id);
                var agentBalance = BaseBll.GetObjectBalance((int)ObjectTypes.User, agent.Type == (int)UserTypes.AgentEmployee ? agent.ParentId.Value : identity.Id);
                agentBalance.AvailableBalance = Math.Floor(agentBalance.Balances.Sum(x => BaseBll.ConvertCurrency(x.CurrencyId, agent.CurrencyId, x.Balance)) * 100) / 100;
                agentBalance.CurrencyId = agentBalance.CurrencyId;
                return new ApiResponseBase
                {
                    ResponseObject = agentBalance.AvailableBalance
                };
            }
        }
        private static ApiResponseBase GenerateQRCode(SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            var partner = CacheManager.GetPartnerById(user.PartnerId);
            var key = CommonFunctions.GenerateQRCode();
            return new ApiResponseBase
            {
                ResponseObject = new
                {
                    Key = key,
                    Data = "otpauth://totp/"+Uri.EscapeDataString($"{partner.Name}:{user.UserName}?secret={key}&issuer={partner.Name}")
                }
            };
        }
        private static ApiResponseBase GetAgentData(string userName, SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            using (var userBl = new UserBll(identity, log))
            {
                var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                if (isAgentEmploye)
                    userBl.CheckPermission(Constants.Permissions.ViewUser);
                fnAgent agent = null;
                if (!string.IsNullOrEmpty(userName))
                {
                    var subAgent = CacheManager.GetUserByUserName(user.PartnerId, userName);
                    if (subAgent == null || !subAgent.Path.Contains("/" + user.Id + "/"))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);

                    agent = userBl.GetSubAgents(user.Id, null, null, true, string.Empty).
                        FirstOrDefault(x => x.Id == subAgent.Id);
                    agent.ParentId = subAgent.ParentId;
                }
                else
                {
                    agent = userBl.GetfnAgent(identity.Id);
                    agent.ParentId = user.ParentId;
                }

                return new ApiResponseBase
                {
                    ResponseCode = Constants.SuccessResponseCode,
                    ResponseObject = agent?.MapToUserModel(identity.TimeZone, new List<AgentCommission>(), user.Id, log)
                };
            }
        }

        private static ApiResponseBase FindAgents(string userIdentity, SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            using (var userBl = new UserBll(identity, log))
            {
                if (string.IsNullOrEmpty(userIdentity))
                    return new ApiResponseBase();
                var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                if (isAgentEmploye)
                    userBl.CheckPermission(Constants.Permissions.ViewUser);
                var res = new List<UserModel>();
                return new ApiResponseBase
                {
                    ResponseObject = userBl.FindUsers(userIdentity, null, null, true).Select(x => x.MapToUserModel(identity.TimeZone)).ToList()
                };
            }
        }

        private static ApiResponseBase FindDownlineNames(ApiAgentInput input, SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            using (var userBl = new UserBll(identity, log))
            {
                using (var clientBl = new ClientBll(identity, log))
                {
                    if (string.IsNullOrEmpty(input.AgentIdentifier))
                        return new ApiResponseBase();
                    var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                    if (isAgentEmploye)
                        userBl.CheckPermission(Constants.Permissions.ViewUser);

                    var resp = new List<object>();
                    if (input.Level != (int)AgentLevels.Member)
                        resp.AddRange(userBl.FindUsers(input.AgentIdentifier, input.Level, input.ParentId, input.WithDownlines).Select(x => new { x.UserName, x.NickName, x.FirstName, x.LastName }).ToList());
                    if (!input.Level.HasValue || input.Level == (int)AgentLevels.Member)
                        resp.AddRange(clientBl.FindClients(input.AgentIdentifier, input.WithDownlines, input.ParentId).Select(x => new { x.UserName, x.NickName, x.FirstName, x.LastName }).ToList());
                    return new ApiResponseBase
                    {
                        ResponseObject = resp
                    };
                }
            }
        }

        private static ApiResponseBase GetAgentPath(int agentId, SessionIdentity identity, ILog log)
        {
            var resp = new ApiResponseBase { ResponseObject = new List<object>() };
            var agent = CacheManager.GetUserById(agentId);
            if(agent == null || agent.Id == 0 || string.IsNullOrEmpty(agent.Path))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongAgentLevel);
            var path = agent.Path.Split('/').ToList();
            var data = new List<object>();
            var startIndex = path.IndexOf(identity.Id.ToString());

            for (int i = startIndex; i < path.Count - 1; i++)
            {
                var child = CacheManager.GetUserById(Convert.ToInt32(path[i]));
                data.Add(new { Id = child.Id, UserName = child.UserName, Level = child.Level });
            }
            resp.ResponseObject = data;
            return resp;
        }

        private static ApiResponseBase ChangePassword(ChangePasswordInput changePasswordInput, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var user = CacheManager.GetUserById(identity.Id);
                userBl.ChangeUserPassword(user.Id, changePasswordInput.OldPassword, changePasswordInput.NewPassword);
                CacheManager.RemoveUserFromCache(identity.Id);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.User, identity.Id));
                return new ApiResponseBase();
            }
        }

        private static void CheckInputData(UserModel request, BllUser user, UserBll userBl, string languageId, bool isAsian, bool isNew = true)
        {
            var levels = Enum.GetValues(typeof(AgentLevels)).Cast<int>().Where(x => x >= request.Level).ToList();
            if (request.LevelLimits != null && !request.LevelLimits.Select(x => x.Level).SequenceEqual(levels))
                throw BaseBll.CreateException(languageId, Constants.Errors.WrongAgentLevel);
            if (request.CountLimits != null && !request.CountLimits.Select(x => x.Level).SequenceEqual(levels.Where(x => x != request.Level)))
                throw BaseBll.CreateException(languageId, Constants.Errors.WrongInputParameters);
            var countLimitByLevel = -1;
            var parentUserSettings = CacheManager.GetUserSetting(user.Id);
            if (user.Type == (int)UserTypes.AdminUser)
            {
                if (request.Level != (int)AgentLevels.Company)
                    throw BaseBll.CreateException(languageId, Constants.Errors.WrongAgentLevel);
                if (string.IsNullOrEmpty(request.CurrencyId))
                    throw BaseBll.CreateException(languageId, Constants.Errors.WrongCurrencyId);

                var currencySetting = CacheManager.GetPartnerCurrencies(user.PartnerId).
                           FirstOrDefault(x => x.CurrencyId == request.CurrencyId);
                if (currencySetting != null && currencySetting.UserMinLimit.HasValue)
                {
                    var creditMinLimit = currencySetting.UserMinLimit.Value * 100000000;
                    var i = 1;
                    if (request.LevelLimits != null && request.LevelLimits.Any(x => { var res = x.Limit > creditMinLimit / i; i *= 2; return res; }))
                        throw BaseBll.CreateException(languageId, Constants.Errors.MaxLimitExceeded);
                }
                var partnerSetting = CacheManager.GetPartnerSettingByKey(user.PartnerId, Constants.PartnerKeys.AgentAccountLimits);
                if (partnerSetting != null)
                {
                    var partnerLimits = JsonConvert.DeserializeObject<List<CountLimit>>(partnerSetting.StringValue);
                    if (request.CountLimits != null && request.CountLimits.Any(x => partnerLimits.First(y => y.Level == x.Level).Count < x.Count || x.Count < 0))
                        throw BaseBll.CreateException(languageId, Constants.Errors.MaxLimitExceeded);
                }
            }
            else
            {
                if (user.Type == (int)UserTypes.AgentEmployee)
                {
                    userBl.CheckPermission(Constants.Permissions.CreateUser);
                    user = CacheManager.GetUserById(user.ParentId.Value);
                }
                if ((parentUserSettings != null && parentUserSettings.Id > 0 && !parentUserSettings.AllowDoubleCommission) 
                    || !request.AllowDoubleCommission.HasValue)
                    request.AllowDoubleCommission = false;
                if ((parentUserSettings != null && parentUserSettings.Id > 0 && !parentUserSettings.AllowOutright) 
                    || !request.AllowOutright.HasValue)
                    request.AllowOutright = false;
                var parentLimits = parentUserSettings == null || parentUserSettings.Id == 0 ? new List<CountLimit>() : 
                    JsonConvert.DeserializeObject<List<CountLimit>>(parentUserSettings.CountLimits);
                if (request.CountLimits != null && isNew && request.CountLimits.Any(x => parentLimits.FirstOrDefault(y => y.Level == x.Level)?.Count < x.Count))
                    throw BaseBll.CreateException(languageId, Constants.Errors.MaxLimitExceeded);
                var pl = parentLimits.FirstOrDefault(x => x.Level == request.Level);
                if (pl != null)
                    countLimitByLevel = pl.Count;
            }

            if (isAsian)
            {
                if (request.CalculationPeriod == null || (request.CalculationPeriod.Count > 1 && (request.CalculationPeriod.Contains(1) || request.CalculationPeriod.Contains(-1))) ||
                    request.CalculationPeriod.Any(x => !Enum.IsDefined(typeof(AgentTransferCalculationPeriods), x)) ||
                    (parentUserSettings != null && parentUserSettings.IsCalculationPeriodBlocked.HasValue && parentUserSettings.IsCalculationPeriodBlocked.Value && request.CalculationPeriod[0] != -1))
                    throw BaseBll.CreateException(languageId, Constants.Errors.WrongCalculationPeriod);

                request.MaxCredit = request.LevelLimits.FirstOrDefault(x => x.Level == request.Level).Limit;
            }
            //if (request.LevelLimits.Any(x => x.Limit > request.MaxCredit))
            //    throw BaseBll.CreateException(languageId, Constants.Errors.WrongInputParameters); should be clarified

            var existingAgentCount = userBl.GetSubAgents(user.Id, request.Level, request.Level == (int)AgentLevels.Company ? (int)UserTypes.CompanyAgent : (int)UserTypes.DownlineAgent, true, string.Empty).Count;
            var requestedAgentsCount = !request.Count.HasValue || request.Count.Value == 0 ? 1 : request.Count.Value;
            if (isNew && countLimitByLevel != -1 && existingAgentCount + requestedAgentsCount > countLimitByLevel)
                throw BaseBll.CreateException(languageId, Constants.Errors.MaxLimitExceeded);
        }

        private static ApiResponseBase CreateAgent(UserModel request, string securityCode, SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            using (var userBl = new UserBll(identity, log))
            {
                using (var documentBl = new DocumentBll(userBl))
                {
                    if (user.Type == (int)UserTypes.AgentEmployee)
                    {
                        userBl.CheckPermission(Constants.Permissions.CreateUser);
                        user = CacheManager.GetUserById(user.ParentId.Value);
                    }
                    bool isAsian = false;
                    var partnerSetting = CacheManager.GetPartnerSettingByKey(user.PartnerId, Constants.PartnerKeys.IsUserNameGeneratable);
                    if (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue != 0)
                    {
                        isAsian = true;
                        userBl.CheckSecurityCode(securityCode);
                    }
                    CheckInputData(request, user, userBl, identity.LanguageId, isAsian);
                    var resultList = new List<UserModel>();
                    if (!request.Level.HasValue)
                        request.Level = ++user.Level;
                    if(!Enum.IsDefined(typeof(AgentLevels), request.Level) || request.Level == (int)AgentLevels.Member)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);

                    var input = request.MapToUser();
                    input.ParentId = user.Id;
                    input.State = (request.Closed.HasValue && request.Closed.Value) ? (int)UserStates.Closed : (int)UserStates.Active;
                    input.PartnerId = user.PartnerId;
                    input.CurrencyId = user.CurrencyId;
                    if (user.Type == (int)UserTypes.AdminUser)
                    {
                        if (string.IsNullOrEmpty(request.CurrencyId))
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
                        input.Type = (int)UserTypes.CompanyAgent;
                        input.CurrencyId = request.CurrencyId;
                        request.Count = 1;
                        request.CalculationPeriod = null;
                    }
                    else
                    {
                        var userBalance = userBl.GetUserBalance(user.Id);
                        if (userBalance.Balance < request.MaxCredit * request.Count)
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.LowBalance);
                    }
                    if (input.Type != (int)UserTypes.AgentEmployee)
                        input.Type = (int)UserTypes.DownlineAgent;
                    if(request.LevelLimits != null)
                    {
                        for (int i = 1; i < request.LevelLimits.Count; i++)
                        {
                            if (request.LevelLimits[0].Limit < request.LevelLimits[i].Limit)
                                request.LevelLimits[i].Limit = request.LevelLimits[0].Limit;
                        }
                    }

                    var userSettings = new UserSetting
                    {
                        AllowAutoPT = request.AllowAutoPT,
                        CalculationPeriod = request.CalculationPeriod != null && request.CalculationPeriod.Count != 0 ? JsonConvert.SerializeObject(request.CalculationPeriod) : "[2]",
                        AllowOutright = request.AllowOutright ?? false,
                        AllowDoubleCommission = request.AllowDoubleCommission ?? false,
                        LevelLimits = request.LevelLimits != null ? JsonConvert.SerializeObject(request.LevelLimits) : "[]",
                        CountLimits = request.CountLimits != null ? JsonConvert.SerializeObject(request.CountLimits) : "[]",
                        AgentMaxCredit = request.MaxCredit
                    };

                    var commissionSettings = string.Empty;
                    if (request.CommissionPlan != null)
                    {
                        if(request.CommissionPlan.PositionTaking != null)
                        {
                            foreach(var pt in request.CommissionPlan.PositionTaking)
                            {
                                if (pt.MarketTypes != null)
                                {
                                    foreach (var mt in pt.MarketTypes)
                                    {
                                        mt.AgentPercent = mt.OwnerPercent;
                                        mt.OwnerPercent = 0;
                                    }
                                }
                            }
                        }
                        commissionSettings = JsonConvert.SerializeObject(request.CommissionPlan);
                    }
                    var userTransferInput = new TransferInput
                    {
                        FromUserId = user.Id,
                        Amount = request.MaxCredit ?? 0,
                        CurrencyId = input.CurrencyId
                    };
                    using (var transactionScope = CommonFunctions.CreateTransactionScope())
                    {
                        if (request.Count.HasValue && request.Count.Value > 1)
                        {
                            if (request.Count.Value > 50)
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);

                            for (int i = 0; i < request.Count; ++i)
                            {
                                var userName = request.UserNamePrefix + new string(userBl.FindAvailableUserName(request.Type, request.Level.Value, !string.IsNullOrEmpty(request.UserNamePrefix) ?
                                    request.UserNamePrefix[0] : '\0').ToArray());
                                var newInput = new User
                                {
                                    PartnerId = input.PartnerId,
                                    FirstName = input.FirstName,
                                    LastName = input.LastName,
                                    UserName = userName,
                                    State = input.State,
                                    CurrencyId = input.CurrencyId,
                                    Email = input.Email,
                                    MobileNumber = input.MobileNumber,
                                    Type = input.Type,
                                    ParentId = user.Id,
                                    Level = input.Level,
                                    Password = input.Password
                                };
                                var userItem = userBl.AddUser(newInput, user.Type == (int)UserTypes.AdminUser);
                                userSettings.UserId = userItem.Id;
                                userBl.SaveUserSettings(userSettings, out List<int> cIds);
                                resultList.Add(userItem.MapToUserModel(identity.TimeZone));
                                var commissionPlan = new AgentCommission
                                {
                                    AgentId = userItem.Id,
                                    ProductId = Constants.PlatformProductId,
                                    TurnoverPercent = commissionSettings
                                };
                                userBl.UpdateAgentCommission(commissionPlan);

                                if (userTransferInput.Amount != 0)
                                {
                                    userTransferInput.UserId = userItem.Id;
                                    userBl.CreateDebitOnUser(userTransferInput, documentBl);
                                }
                            }
                        }
                        else
                        {
                            input.UserName = request.UserNamePrefix + input.UserName;
                            var newUser = userBl.AddUser(input, false).MapToUserModel(identity.TimeZone);
                            userSettings.UserId = newUser.Id;
                            userBl.SaveUserSettings(userSettings, out List<int> cIds);
                            resultList.Add(newUser);
                            var commissionPlan = new AgentCommission
                            {
                                AgentId = newUser.Id,
                                ProductId = Constants.PlatformProductId,
                                TurnoverPercent = commissionSettings
                            };
                            var ps = CacheManager.GetPartnerSettingByKey(user.PartnerId, Constants.PartnerKeys.IsUserNameGeneratable);
                            if (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue == 1)
                                userBl.UpdateAgentCommission(commissionPlan);

                            if (userTransferInput.Amount != 0)
                            {
                                userTransferInput.UserId = newUser.Id;
                                userBl.CreateDebitOnUser(userTransferInput, documentBl);
                            }
                        }
                        transactionScope.Complete();
                    }
                    return new ApiResponseBase
                    {
                        ResponseObject = resultList
                    };
                }
            }
        }

        private static ApiResponseBase CloneAgent(UserModel request, string securityCode, SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            using (var userBl = new UserBll(identity, log))
            {
                using (var documentBl = new DocumentBll(userBl))
                {
                    if (user.Type == (int)UserTypes.AgentEmployee)
                    {
                        userBl.CheckPermission(Constants.Permissions.CreateUser);
                        user = CacheManager.GetUserById(user.ParentId.Value);
                    }
                    var cloningUser = CacheManager.GetUserByUserName(user.PartnerId, request.CloningUserName);
                    if (cloningUser == null || !cloningUser.Path.Contains("/" + user.Id + "/"))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                    var cloningUserSettings = CacheManager.GetUserSetting(cloningUser.Id);
                    request.Level = cloningUser.Level;
                    request.CalculationPeriod = JsonConvert.DeserializeObject<List<int>>(cloningUserSettings.CalculationPeriod);
                    userBl.CheckSecurityCode(securityCode);
                    CheckInputData(request, user, userBl, identity.LanguageId, true);
                    var levels = Enum.GetValues(typeof(AgentLevels)).Cast<int>().Where(x => x >= request.Level).ToList();
                    if (!request.LevelLimits.Select(x => x.Level).SequenceEqual(levels))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongAgentLevel);
                    request.MaxCredit = request.LevelLimits.FirstOrDefault(x => x.Level == request.Level).Limit;
                    if (request.LevelLimits.Any(x => x.Limit > request.MaxCredit))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.LevelLimitExceeded);

                    using (var transactionScope = CommonFunctions.CreateTransactionScope())
                    {
                        var newUserInput = new User
                        {
                            PartnerId = cloningUser.PartnerId,
                            FirstName = request.FirstName ?? string.Empty,
                            LastName = request.LastName ?? string.Empty,
                            Gender = cloningUser.Gender,
                            LanguageId = identity.LanguageId,
                            UserName = request.UserNamePrefix + request.UserName,
                            Password = request.Password,
                            CurrencyId = cloningUser.CurrencyId,
                            MobileNumber = request.MobileNumber,
                            Type = cloningUser.Type,
                            ParentId = user.Id,
                            Level = cloningUser.Level,
                            State = (request.Closed.HasValue && request.Closed.Value) ? (int)UserStates.Closed : (int)UserStates.Active
                        };
                        var newUser = userBl.AddUser(newUserInput, false).MapToUserModel(identity.TimeZone);
                        var userSettings = new UserSetting
                        {
                            UserId = newUser.Id,
                            AllowAutoPT = request.AllowAutoPT ?? false,
                            CalculationPeriod = cloningUserSettings.CalculationPeriod,
                            AgentMaxCredit = request.MaxCredit ?? 0,
                            AllowOutright = request.AllowOutright ?? false,
                            AllowDoubleCommission = request.AllowDoubleCommission ?? false,
                            LevelLimits = JsonConvert.SerializeObject(request.LevelLimits),
                            CountLimits = JsonConvert.SerializeObject(request.CountLimits)
                        };
                        userBl.SaveUserSettings(userSettings, out List<int> cIds);
                        var commissionSettings = userBl.GetAgentCommissionPlan(cloningUser.PartnerId, cloningUser.Id, null, 
                            Constants.PlatformProductId, false).FirstOrDefault().TurnoverPercent;
                        var commissionPlan = new AgentCommission
                        {
                            AgentId = newUser.Id,
                            ProductId = Constants.PlatformProductId,
                            TurnoverPercent = commissionSettings
                        };
                        userBl.UpdateAgentCommission(commissionPlan);
                        if (request.MaxCredit.Value != 0)
                        {
                            var userTransferInput = new TransferInput
                            {
                                UserId = newUser.Id,
                                FromUserId = user.Id,
                                Amount = request.MaxCredit.Value,
                                CurrencyId = newUser.CurrencyId
                            };
                            userBl.CreateDebitOnUser(userTransferInput, documentBl);
                        }
                        transactionScope.Complete();
                        return new ApiResponseBase
                        {
                            ResponseObject = newUser
                        };
                    }
                }
            }
        }

        private static ApiResponseBase UpdateAgent(ChangeObjectStateInput changeObjectStateInput, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var user = CacheManager.GetUserById(identity.Id);
                if (user.Type == (int)UserTypes.AgentEmployee)
                {
                    user = CacheManager.GetUserById(user.ParentId.Value);
                    userBl.CheckPermission(AgentEmployeePermissions.FullControl.ToString());
                }
                if (changeObjectStateInput.ObjectTypeId.HasValue && changeObjectStateInput.ObjectTypeId == (int)ObjectTypes.Client)
                {
                    using (var clientBl = new ClientBll(userBl))
                    {
                        using (var documentBl = new DocumentBll(userBl))
                        {
                            fnClientModel res = null;
                            using (var transactionScope = CommonFunctions.CreateTransactionScope())
                            {
                                res = clientBl.UpdateAgentClient(user.Id, changeObjectStateInput.ObjectId,
                                CustomHelper.MapUserStateToClient[changeObjectStateInput.State], null, documentBl).MapTofnClientModel(identity.TimeZone);
                                transactionScope.Complete();
                            }
                            var adc = CacheManager.GetClientSettingByName(changeObjectStateInput.ObjectId, nameof(res.AllowDoubleCommission));
                            res.AllowDoubleCommission = Convert.ToBoolean(adc == null || adc.Id == 0 ? 0 : (adc.NumericValue ?? 0));
                            var ao = CacheManager.GetClientSettingByName(changeObjectStateInput.ObjectId, nameof(res.AllowOutright));
                            res.AllowOutright = Convert.ToBoolean(ao == null || ao.Id == 0 ? 0 : (ao.NumericValue ?? 0));
                            CacheManager.RemoveClientFromCache(changeObjectStateInput.ObjectId);
                            Helpers.Helpers.InvokeMessage("RemoveClient", changeObjectStateInput.ObjectId);
                            return new ApiResponseBase
                            {
                                ResponseObject = res
                            };
                        }
                    }
                }
                else
                {
                    using (var transactionScope = CommonFunctions.CreateTransactionScope())
                    {
                        var res = userBl.UpdateAgent(changeObjectStateInput.ObjectId, changeObjectStateInput.State,
                                                     changeObjectStateInput.Password, out List<int> clientIds);
                        res.ParentId = user.ParentId;
                        var resp = res.MapToUserModel(identity.TimeZone, new List<AgentCommission>(), user.Id, log);
                        transactionScope.Complete();

                        foreach (var c in clientIds)
                            Helpers.Helpers.InvokeMessage("RemoveClient", string.Format("{0}_{1}", Constants.CacheItems.Clients, c));

                        return new ApiResponseBase
                        {
                            ResponseObject = resp
                        };
                    }
                }
            }
        }

        private static ApiResponseBase UpdateAgentSetting(UserModel userModel, string securityCode, SessionIdentity identity, ILog log)
        {
            ApiResponseBase response;
            using (var userBl = new UserBll(identity, log))
            {
                using (var documentBl = new DocumentBll(userBl))
                {
                    using (var transactionScope = CommonFunctions.CreateTransactionScope())
                    {
                        var user = CacheManager.GetUserById(identity.Id);
                        var subAgent = CacheManager.GetUserById(userModel.Id);
                        if (subAgent == null)
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                        if (!subAgent.Path.Contains("/" + user.Id + "/"))
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
                        var userSetting = CacheManager.GetUserSetting(subAgent.Id);
                        userModel.PartnerId = user.PartnerId;
                        userModel.Level = subAgent.Level;
                        userModel.State = subAgent.State;
                        userModel.CurrencyId = subAgent.CurrencyId;

                        if (user.Type == (int)UserTypes.AgentEmployee)
                        {
                            userBl.CheckPermission(Constants.Permissions.CreateUser);
                            user = CacheManager.GetUserById(user.ParentId.Value);
                        }
                        bool isAsian = false;
                        var partnerSetting = CacheManager.GetPartnerSettingByKey(user.PartnerId, Constants.PartnerKeys.IsUserNameGeneratable);
                        if (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue != 0)
                        {
                            isAsian = true;
                            userBl.CheckSecurityCode(securityCode);
                        }
                        CheckInputData(userModel, user, userBl, identity.LanguageId, isAsian, false);
                        var input = userModel.MapToUser();

                        var userSettings = new UserSetting
                        {
                            AllowAutoPT = userModel.AllowAutoPT,
                            CalculationPeriod = userModel.CalculationPeriod != null && userModel.CalculationPeriod.Count != 0 ? JsonConvert.SerializeObject(userModel.CalculationPeriod) : "[2]",
                            AllowOutright = userModel.AllowOutright ?? false,
                            AllowDoubleCommission = userModel.AllowDoubleCommission ?? false,
                            LevelLimits = userModel.LevelLimits != null ? JsonConvert.SerializeObject(userModel.LevelLimits) : "[]",
                            CountLimits = userModel.CountLimits != null ? JsonConvert.SerializeObject(userModel.CountLimits) : "[]",
                            AgentMaxCredit = userModel.MaxCredit
                        };
                        var parentBalance = userBl.GetUserBalance(user.Id);
                        var childBalance = userBl.GetUserBalance(subAgent.Id);

                        var userTransferInput = new TransferInput
                        {
                            FromUserId = user.Id,
                            UserId = input.Id,
                            Amount = userModel.MaxCredit.Value - userSetting.AgentMaxCredit.Value,
                            CurrencyId = user.CurrencyId
                        };
                        if (user.Type != (int)UserTypes.AdminUser && userTransferInput.Amount > parentBalance.Balance)
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.LowBalance);

                        userBl.EditUser(input, false);
                        userSettings.UserId = input.Id;
                        userBl.SaveUserSettings(userSettings, out List<int> cIds);
                        var ps = CacheManager.GetPartnerSettingByKey(user.PartnerId, Constants.PartnerKeys.IsUserNameGeneratable);
                        if (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue == 1)
                        {

                            if (userModel.CommissionPlan != null && userModel.CommissionPlan.PositionTaking != null)
                            {
                                foreach (var pt in userModel.CommissionPlan.PositionTaking)
                                {
                                    if (pt.MarketTypes != null)
                                    {
                                        foreach (var mt in pt.MarketTypes)
                                        {
                                            mt.AgentPercent = mt.OwnerPercent;
                                            mt.OwnerPercent = 0;
                                        }
                                    }
                                }
                            }
                            var commissionSettings = userModel.CommissionPlan == null ? string.Empty : JsonConvert.SerializeObject(userModel.CommissionPlan);

                            var commissionPlan = new AgentCommission
                            {
                                AgentId = input.Id,
                                ProductId = Constants.PlatformProductId,
                                TurnoverPercent = commissionSettings
                            };
                            userBl.UpdateAgentCommission(commissionPlan);
                        }
                        if (userTransferInput.Amount > 0)
                            userBl.CreateDebitOnUser(userTransferInput, documentBl);
                        else if (userTransferInput.Amount < 0)
                        {
                            userTransferInput.Amount = Math.Abs(userTransferInput.Amount);
                            if (userTransferInput.Amount > childBalance.Balance)
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.LowBalance);

                            userBl.CreateCreditOnUser(userTransferInput, documentBl);
                        }
                        var commission = userBl.GetAgentCommissions(new List<int> { subAgent.Id });
                        var res = userBl.GetfnAgent(subAgent.Id);
                        res.ParentId = subAgent.ParentId;
                        response = new ApiResponseBase
                        {
                            ResponseObject = res.MapToUserModel(identity.TimeZone, commission, user.Id, log)
                        };
                        transactionScope.Complete();
                    }
                    return response;
                }
            }
        }

        public static ApiResponseBase GetAgents(ApiAgentInput input, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                using (var clientBl = new ClientBll(userBl))
                {
                    var user = CacheManager.GetUserById(identity.Id);
                    var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                    if (isAgentEmploye)
                    {
                        userBl.CheckPermission(Constants.Permissions.ViewUser);
                        user = CacheManager.GetUserById(identity.Id);
                    }
                    var resp = userBl.GetSubAgents(input.ParentId ?? user.Id, input?.Level, input?.Type, !input.WithDownlines, 
                        input.AgentIdentifier, input.Id, input.IsFromSuspend );
                    if (input.State.HasValue)
                    {
                        var states = new List<int>();
                        if (input.State == (int)UserStates.ForceBlock || input.State == (int)UserStates.ForceBlockBySecurityCode)
                        {
                            states.Add((int)UserStates.ForceBlock);
                            states.Add((int)UserStates.ForceBlockBySecurityCode);
                        }
                        else
                            states.Add(input.State.Value);

                        resp = resp.Where(x => states.Contains(x.State)).ToList();
                    }
                    if (input.AllowDoubleCommission.HasValue)
                        resp = resp.Where(x => x.AllowDoubleCommission == input.AllowDoubleCommission).ToList();
                    var commissions = userBl.GetAgentCommissions(resp.Select(x => x.Id).ToList());
                    var agents = resp.Select(x => x.MapToUserModel(identity.TimeZone, commissions, user.Id, log)).ToList();
                    if (input.IsFromSuspend.HasValue && (!input.Level.HasValue || input.Level.Value == (int)AgentLevels.Member))
                    {
                        var clients = clientBl.GetAgentClients(new DAL.Filters.FilterClientModel(), input.ParentId.Value, !input.WithDownlines, null);
                        foreach (var c in clients)
                        {

                            var clientSetting = CacheManager.GetClientSettingByName(c.Id, "ParentState");
                            if (clientSetting != null && clientSetting.NumericValue.HasValue)
                            {
                                var parents = c.User.Path.Split('/');
                                foreach (var sPId in parents)
                                {
                                    if (int.TryParse(sPId, out int pId) && pId != input.ParentId)
                                    {
                                        var p = CacheManager.GetUserById(Convert.ToInt32(pId));
                                        var st = CustomHelper.MapUserStateToClient[p.State];
                                        if (CustomHelper.Greater((ClientStates)st, (ClientStates)clientSetting.NumericValue))
                                            c.State = st;
                                    }
                                }
                            }

                            if ((input.IsFromSuspend.Value && clientSetting != null && clientSetting.NumericValue == (int)ClientStates.Suspended && c.State != (int)ClientStates.Suspended &&
                                 CustomHelper.Greater((ClientStates)clientSetting.NumericValue, (ClientStates)c.State)) ||
                                (!input.IsFromSuspend.Value && (clientSetting == null || (clientSetting.NumericValue != (int)ClientStates.Suspended || c.State == (int)ClientStates.Suspended))))
                            {
                                var clientState = c.State;
                                if (clientSetting != null && clientSetting.NumericValue.HasValue && CustomHelper.Greater((ClientStates)clientSetting.NumericValue.Value, (ClientStates)clientState))
                                    clientState = Convert.ToInt32(clientSetting.NumericValue.Value);

                                agents.Add(new UserModel
                                {

                                    Id = c.Id,
                                    UserName = c.UserName,
                                    NickName = c.NickName,
                                    State = CustomHelper.MapUserStateToClient.First(x => x.Value == clientState).Key,
                                    FirstName = c.FirstName,
                                    LastName = c.LastName,
                                    CreationTime = c.CreationTime,
                                    Level = (int)AgentLevels.Member,
                                    //AllowDoubleCommission = Convert.ToBoolean(adc == null || adc.Id == 0 ? 0 : (adc.NumericValue ?? 0)),
                                    //AllowOutright = Convert.ToBoolean(ao == null || ao.Id == 0 ? 0 : (ao.NumericValue ?? 0)),
                                });
                            }
                        }
                    }
                    return new ApiResponseBase
                    {
                        ResponseObject = new
                        {
                            Agents = new
                            {
                                agents.Count,
                                Entities = agents.OrderBy(x => x.State == (int)UserStates.Disabled).ThenBy(x=>x.Level).ThenByDescending(x => x.UserName).Skip(input.SkipCount * input.TakeCount).Take(input.TakeCount)
                            }
                        }
                    };
                }
            }
        }

        public static ApiResponseBase GetSubAgents(SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var user = CacheManager.GetUserById(identity.Id);
                var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                if (isAgentEmploye)
                {
                    userBl.CheckPermission(Constants.Permissions.ViewUser);
                    user = CacheManager.GetUserById(user.ParentId.Value);
                }
                var source = userBl.GetSubAgents(user.Id, null, null, false, string.Empty).Where(x => x.Type != (int)UserTypes.AgentEmployee).OrderBy(x => x.Level).
                    Select(x => new ApiSubAgent { Id = x.Id, UserName = x.UserName, Level = x.Level, ParentId = x.ParentId }).ToList();

                return new ApiResponseBase
                {
                    ResponseObject = source.OrderBy(x => x.Level).ThenBy(x => x.UserName).ToList()
                };
            }
        }

        public static ApiResponseBase GetSubAccounts(int parentId, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var parent = CacheManager.GetUserById(parentId);
                var user = CacheManager.GetUserById(identity.Id);
                if (!parent.Path.Contains("/" + identity.Id + "/"))
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);

                var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                if (isAgentEmploye)
                    return new ApiResponseBase();
                return new ApiResponseBase
                {
                    ResponseObject = userBl.GetSubAccounts(parentId)
                };
            }
        }

        public static ApiResponseBase SaveSubAccount(UserModel request, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var user = CacheManager.GetUserById(identity.Id);
                var userState = CacheManager.GetUserSetting(user.Id)?.ParentState;
                if (userState.HasValue && CustomHelper.Greater((UserStates)userState.Value, (UserStates)user.State))
                    user.State = userState.Value;

                if (user.State != (int)UserStates.Active || user.Type == (int)UserTypes.AdminUser || user.Type == (int)UserTypes.AgentEmployee)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);

                var input = request.MapToUser();
                input.PartnerId = identity.PartnerId;
                input.ParentId = user.Id;
                input.CurrencyId = user.CurrencyId;
                input.Level = user.Level;
                input.Type = (int)UserTypes.AgentEmployee;
                var partnerSetting = CacheManager.GetPartnerSettingByKey(user.PartnerId, Constants.PartnerKeys.SubAccountLimits);

                if (partnerSetting != null && partnerSetting.Id > 0)
                {
                    var limits = JsonConvert.DeserializeObject<List<CountLimit>>(partnerSetting.StringValue);
                    var limit = limits.FirstOrDefault(x => x.Level == user.Level.Value);
                    var existingSubAccountsCount = userBl.GetSubAgents(user.Id, request.Level, (int)UserTypes.AgentEmployee, true, string.Empty).Count;
                    if (limit == null || existingSubAccountsCount + 1 > limit.Count)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.MaxLimitExceeded);
                }
                var agentSubAccount = new AgentSubAccount
                {
                    ViewBetsAndForecast = request.ViewBetsAndForecast ?? false,
                    ViewReport = request.ViewReport ?? false,
                    ViewBetsLists = request.ViewBetsLists ?? false,
                    ViewTransfer = request.ViewTransfer ?? false,
                    ViewLog = request.ViewLog ?? false,
                    MemberInformationPermission = request.MemberInformationPermission ?? (int)AgentEmployeePermissions.None
                };
                if (!Enum.IsDefined(typeof(AgentEmployeePermissions), agentSubAccount.MemberInformationPermission))
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
                var res = input.Id == 0 ? userBl.AddUser(input, false, agentSubAccount) :
                                                      userBl.EditUser(input, false, agentSubAccount);
                agentSubAccount.Id = res.Id;
                agentSubAccount.Username = res.UserName;
                agentSubAccount.Nickname = res.NickName;
                agentSubAccount.FirstName = res.FirstName;
                agentSubAccount.LastName = res.LastName;
                agentSubAccount.CreationTime = res.CreationTime.GetGMTDateFromUTC(identity.TimeZone);
                return new ApiResponseBase
                {
                    ResponseObject = agentSubAccount
                };
            }
        }

        public static ApiResponseBase IsUserNameAvailable(ApiUserNameInput usernameInput, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var user = CacheManager.GetUserById(identity.Id);
                return new ApiResponseBase
                {
                    ResponseObject = !userBl.IsUserNameExists(user.PartnerId, usernameInput.Username, usernameInput.Level, usernameInput.StartsWith, user.Type == (int)UserTypes.AgentEmployee)
                };
            }
        }

        private static ApiResponseBase CreateDebitCorrection(TransferInput userCorrectionInput, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                using (var documentBl = new DocumentBll(userBl))
                {
                    userCorrectionInput.CurrencyId = identity.CurrencyId;
                    var user = userBl.GetUserById(userCorrectionInput.UserId.Value);
                    var userState = CacheManager.GetUserSetting(user.Id)?.ParentState;
                    if (userState.HasValue && CustomHelper.Greater((UserStates)userState.Value, (UserStates)user.State))
                        user.State = userState.Value;
                    if (user == null || user.Type != (int)UserTypes.DownlineAgent || user.ParentId != identity.Id || user.State != (int)UserStates.Active)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);

                    return new ApiResponseBase
                    {
                        ResponseObject = userBl.CreateDebitOnUser(userCorrectionInput, documentBl).MapToDocumentModel(identity.TimeZone)
                    };
                }
            }
        }

        private static ApiResponseBase ChangeAgentMaxCredit(TransferInput userCorrectionInput, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                using (var documentBl = new DocumentBll(userBl))
                {
                    using (var clientBl = new ClientBll(userBl))
                    {
                        var user = CacheManager.GetUserById(identity.Id);
                        if (user.Type == (int)UserTypes.AgentEmployee)
                        {
                            //Check Permission
                            user = CacheManager.GetUserById(user.ParentId.Value);
                            identity.Id = user.Id;
                        }

                        var parentBalance = userBl.GetUserBalance(user.Id);
                        if (userCorrectionInput.ObjectTypeId == (int)ObjectTypes.User)
                        {
                            var userState = CacheManager.GetUserSetting(user.Id)?.ParentState;
                            if (userState.HasValue && CustomHelper.Greater((UserStates)userState.Value, (UserStates)user.State))
                                user.State = userState.Value;

                            var subAgent = userBl.GetUserById(userCorrectionInput.ObjectId);
                            int? parentState = null;
                            var ps = CacheManager.GetUserSetting(subAgent.Id)?.ParentState;
                            if (ps.HasValue)
                                parentState = ps.Value;

                            if (subAgent == null || subAgent.ParentId != identity.Id || user.State != (int)UserStates.Active || 
                                subAgent.State == (int)UserStates.Disabled || parentState == (int)UserStates.Disabled)
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);

                            var subAgentBalance = userBl.GetUserBalance(subAgent.Id);
                            if (user.Type != (int)UserTypes.AdminUser && userCorrectionInput.Amount > parentBalance.Balance + subAgentBalance.Credit)
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.LowBalance);
                            if (subAgentBalance.Credit - subAgentBalance.Balance > userCorrectionInput.Amount)
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongOperationAmount);
                            userCorrectionInput.UserId = userCorrectionInput.ObjectId;
                            userCorrectionInput.CurrencyId = subAgentBalance.CurrencyId;
                            userCorrectionInput.FromUserId = user.Id;
                            var credit = userCorrectionInput.Amount;
                            if (!subAgentBalance.Credit.HasValue)
                                subAgentBalance.Credit = 0;
                            userCorrectionInput.Amount = subAgentBalance.Credit.Value - userCorrectionInput.Amount;

                            var subAgentSetting = CacheManager.GetUserSetting(subAgent.Id);
                            var levelLimits = JsonConvert.DeserializeObject<List<LevelLimit>>(subAgentSetting.LevelLimits);
                            levelLimits.ForEach(x =>
                                x.Limit = subAgent.Level == x.Level ? credit :
                                          Math.Min(x.Limit.Value, credit)
                            );
                            var userSettings = new UserSetting
                            {
                                UserId = subAgent.Id,
                                AllowAutoPT = subAgentSetting.AllowAutoPT,
                                CalculationPeriod = subAgentSetting.CalculationPeriod,
                                AllowOutright = subAgentSetting.AllowOutright,
                                AllowDoubleCommission = subAgentSetting.AllowDoubleCommission,
                                LevelLimits = JsonConvert.SerializeObject(levelLimits),
                                CountLimits = subAgentSetting.CountLimits,
                                AgentMaxCredit = credit
                            };
                            userBl.SaveUserSettings(userSettings, out List<int> cIds);
                            if (userCorrectionInput.Amount > 0)
                            {
                                userBl.CreateCreditOnUser(userCorrectionInput, documentBl);
                            }
                            else
                            {
                                userCorrectionInput.Amount = Math.Abs(userCorrectionInput.Amount);
                                userBl.CreateDebitOnUser(userCorrectionInput, documentBl);
                            }
                            CacheManager.RemoveUserSetting(subAgent.Id);
                        }
                        else if (userCorrectionInput.ObjectTypeId == (int)ObjectTypes.Client)
                        {
                            var client = CacheManager.GetClientById(userCorrectionInput.ObjectId);
                            if (client == null)
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);
                            var ss = CacheManager.GetClientSettingByName(client.Id, ClientSettings.ParentState);
                            var state = client.State;
                            if (ss.NumericValue.HasValue && CustomHelper.Greater((ClientStates)ss.NumericValue, (ClientStates)state))
                                state = Convert.ToInt32(ss.NumericValue.Value);
                            if(state == (int)ClientStates.Disabled)
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientBlocked);
                            var clientSetting = new ClientCustomSettings();
                            var mc = CacheManager.GetClientSettingByName(client.Id, nameof(clientSetting.MaxCredit));
                            clientSetting.MaxCredit = Convert.ToDecimal(mc == null || mc.Id == 0 ? 0 : (mc.NumericValue ?? 0));
                            var clientBalance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;

                            if (user.Type != (int)UserTypes.AdminUser && userCorrectionInput.Amount > parentBalance.Balance + clientSetting.MaxCredit)
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.LowBalance);
                            if (clientSetting.MaxCredit - clientBalance > userCorrectionInput.Amount)
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongOperationAmount);
                            var credit = userCorrectionInput.Amount;
                            if (!clientSetting.MaxCredit.HasValue)
                                clientSetting.MaxCredit = 0;

                            userCorrectionInput.Amount = clientSetting.MaxCredit.Value - userCorrectionInput.Amount;
                            clientSetting.MaxCredit = credit;
                            clientSetting.ClientId = client.Id;
                            var clientCorrectionInput = new ClientCorrectionInput
                            {
                                Amount = Math.Abs(userCorrectionInput.Amount),
                                CurrencyId = client.CurrencyId,
                                ClientId = client.Id
                            };

                            if (userCorrectionInput.Amount > 0)
                                clientBl.CreateCreditCorrectionOnClient(clientCorrectionInput, documentBl, false);
                            else
                                clientBl.CreateDebitCorrectionOnClient(clientCorrectionInput, documentBl, false);

                            var res = clientBl.SaveClientSetting(clientSetting);
                            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}",
                                Constants.CacheItems.ClientSettings, client.Id, nameof(clientSetting.MaxCredit)));
                        }
                        return new ApiResponseBase();
                    }
                }
            }
        }

        private static ApiResponseBase CreateCreditCorrection(TransferInput userCorrectionInput, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                using (var documentBl = new DocumentBll(userBl))
                {
                    userCorrectionInput.CurrencyId = identity.CurrencyId;
                    var user = userBl.GetUserById(userCorrectionInput.UserId.Value);
                    var userState = CacheManager.GetUserSetting(user.Id)?.ParentState;
                    if (userState.HasValue && CustomHelper.Greater((UserStates)userState.Value, (UserStates)user.State))
                        user.State = userState.Value;
                    if (user == null || user.Type != (int)UserTypes.DownlineAgent || user.ParentId != identity.Id || user.State != (int)UserStates.Active)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);

                    return new ApiResponseBase
                    {
                        ResponseObject = userBl.CreateCreditOnUser(userCorrectionInput, documentBl).MapToDocumentModel(identity.TimeZone)
                    };
                }
            }
        }

        private static ApiResponseBase GetCorrections(ApiFilterUserCorrection filter, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var user = CacheManager.GetUserById(identity.Id);
                if (user == null)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                if (isAgentEmploye)
                    userBl.CheckPermission(Constants.Permissions.ViewReportByCorrection);

                var subUser = CacheManager.GetUserById(filter.UserId ?? 0);
                if (subUser == null || subUser.Type != (int)UserTypes.DownlineAgent || subUser.ParentId != (isAgentEmploye ? user.ParentId.Value : identity.Id))
                    return new ApiResponseBase();

                return new ApiResponseBase
                {
                    ResponseObject = userBl.GetUserCorrections(filter.MapToFilterCorrection()).MapToApiUserCorrections(identity.TimeZone)
                };
            }
        }

        private static ApiResponseBase GetAgentsCreditInfo(string userName, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                using (var clientBl = new ClientBll(identity, log))
                {
                    var user = CacheManager.GetUserById(identity.Id);
                    if (user == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                    var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                    if (isAgentEmploye)
                    {
                        userBl.CheckPermission(Constants.Permissions.ViewReportByCorrection); // ?
                        user = CacheManager.GetUserById(user.ParentId.Value);
                    }
                    int userId = user.Id;
                    if (!string.IsNullOrEmpty(userName))
                    {
                        var childUser = userBl.GetUserByUserName(userName);
                        if (childUser == null || !childUser.Path.Contains(user.Id.ToString()))
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                        userId = childUser.Id;
                    }
                    var resultList = userBl.GetAgentAccountsInfo(userId);
                    resultList.AddRange(clientBl.GetClientsAccountsInfo(userId));

                    return new ApiResponseBase
                    {
                        ResponseObject = resultList
                    };
                }
            }
        }

        //private static ApiResponseBase SaveUserRoles(SaveUserRoleModel userRoleModel, SessionIdentity identity, ILog log)
        //{
        //    var user = CacheManager.GetUserById(identity.Id);
        //    var subUser = CacheManager.GetUserById(userRoleModel.UserId);
        //    if (user == null || subUser == null)
        //        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);

        //    if (user.Type == (int)UserTypes.AgentEmployee || subUser.Type != (int)UserTypes.AgentEmployee)
        //        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);

        //    return PermissionController.SaveUserRoles(userRoleModel, identity, log);
        //}

        public static ApiResponseBase GetTicker(SessionIdentity identity)
        {
            int partnerId = 0;
            if (identity.IsAffiliate)
            {
                using (var affiliateBl = new AffiliateService(identity, WebApiApplication.DbLogger))
                {
                    var affiliate = affiliateBl.GetAffiliateById(identity.Id, false);
                    partnerId = affiliate.PartnerId;
                }
            }
            else
            {
                var user = CacheManager.GetUserById(identity.Id);
                if (user == null)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                partnerId = user.PartnerId;
            }
            return new ApiResponseBase
            {
                ResponseObject = CacheManager.GetPartnerTicker(partnerId,(int)AnnouncementReceiverTypes.Agent, identity.LanguageId)
            };
        }

        public static ApiResponseBase GetAnnouncements(ApiFilterAnnouncement apiAnnouncement, SessionIdentity identity, ILog log)
        {
            var filter = apiAnnouncement.MapToFilterAnnouncement();
            //if (apiAnnouncement.Type == (int)AnnouncementTypes.Personal)
            //    filter.ReceiverId = identity.Id;
            using (var contentBl = new ContentBll(identity, log))
            {
                if (!identity.IsAffiliate)
                {
                    var user = CacheManager.GetUserById(identity.Id);
                    if (user == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound); using (var userBl = new UserBll(identity, log))
                    {
                        var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                        if (isAgentEmploye)
                        {
                            userBl.CheckPermission(Constants.Permissions.EditAnnouncement);
                            user = CacheManager.GetUserById(user.ParentId.Value);
                        }
                    }
                    filter.AgentId = user.Id;
                }

                var announcements = contentBl.GetAnnouncements(filter, false);
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        announcements.Count,
                        Entities = announcements.Entities.Select(x => x.MapToApiAnnouncement(identity.TimeZone)).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase SaveAnnouncement(ApiAnnouncement apiAnnouncement, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var user = CacheManager.GetUserById(identity.Id);
                if (user == null)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                if (isAgentEmploye)
                {
                    userBl.CheckPermission(Constants.Permissions.EditAnnouncement);
                    user = CacheManager.GetUserById(user.ParentId.Value);
                }
                apiAnnouncement.PartnerId = user.PartnerId;
                if (apiAnnouncement.ReceiverType != (int)AnnouncementReceiverTypes.Client &&apiAnnouncement.ReceiverType != (int)AnnouncementReceiverTypes.Agent)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
                using (var contentBl = new ContentBll(identity, log))
                {
                    var announcement = contentBl.SaveAnnouncement(apiAnnouncement, false, user);
                    var translations = new List<fnTranslation>{ new fnTranslation
                    {
                        TranslationId = announcement.TranslationId,
                        LanguageId = Constants.DefaultLanguageId,
                        PartnerId = announcement.PartnerId,
                        ObjectTypeId = (int)ObjectTypes.Announcement,
                        SessionId = identity.SessionId,
                        Text = apiAnnouncement.Message
                    } };
                    contentBl.SaveTranslationEntries(translations, false, out _); ;

                    if (apiAnnouncement.Type == (int)AnnouncementTypes.Ticker)
                    {
                        CacheManager.RemovePartnerTickerFromCache(apiAnnouncement.PartnerId, announcement.ReceiverType, identity.LanguageId);// to check with transaltion 
                        Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.Ticker, apiAnnouncement.PartnerId));
                    }
                    return new ApiResponseBase();
                }
            }
        }

        private static ApiResponseBase CheckSecurityCode(string code, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                userBl.CheckSecurityCode(code);
            }
            return new ApiResponseBase();
        }

        private static ApiResponseBase GetAgentDownline(int userId, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var user = CacheManager.GetUserById(identity.Id);
                var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                if (isAgentEmploye)
                    userBl.CheckPermission(Constants.Permissions.ViewUser);
                var agent = CacheManager.GetUserById(userId);
                if (agent == null)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                var levEnums = BaseBll.GetEnumerations(Constants.EnumerationTypes.AgentLevels, identity.LanguageId);

                var direct = userBl.GetAgentDownline(userId, true, true);
                var all = userBl.GetAgentDownline(userId, true, false);
                
                return new ApiResponseBase
                {
                    ResponseObject = direct.Select(x => new
                    {
                        Id = x.Level,
                        Name = levEnums.First(y => y.Value == x.Level).Text,
                        Count = x.Count,
                        TotalCount = all.FirstOrDefault(y => y.Level == x.Level)?.Count ?? 0
                    })
                };
            }
        }

        private static ApiResponseBase GetAgentStatusesInfo(int userId, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var user = CacheManager.GetUserById(identity.Id);
                var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                if (isAgentEmploye)
                    userBl.CheckPermission(Constants.Permissions.ViewUser);
                var agent = CacheManager.GetUserById(userId);
                if (agent == null)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                return new ApiResponseBase
                {
                    ResponseObject = userBl.GetAgentDownlineStatuses(userId, true)
                };
            }
        }

        public static ApiResponseBase GetAvailableLevels(SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            using (var userBl = new UserBll(identity, log))
            {
                if (user.Type == (int)UserTypes.AgentEmployee)
                    user = CacheManager.GetUserById(user.ParentId.Value);
                var partnerSetting = CacheManager.GetConfigParameters(user.PartnerId, Constants.PartnerKeys.BulkAccountsLimits);
                var levels = BaseBll.GetEnumerations(Constants.EnumerationTypes.AgentLevels, identity.LanguageId).
                    Where(x => (user.Type == (int)UserTypes.AdminUser && x.Value == 1) || (user.Type != (int)UserTypes.AdminUser && x.Value > user.Level)
                    && x.Value != (int)AgentLevels.Member).Select(x => new
                    {
                        Id = x.Value,
                        Name = x.Text,
                        BulkAccountsCount = partnerSetting.ContainsKey(x.Value.ToString()) ? partnerSetting[x.Value.ToString()] : "0"
                    }).OrderBy(x => x.Id);

                return new ApiResponseBase
                {
                    ResponseObject = levels.ToList()
                };
            }
        }

        public static ApiResponseBase ResetPassword(ChangePasswordInput input, string securityCode, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var agentUser = CacheManager.GetUserById(identity.Id);
                var isAgentEmploye = agentUser.Type == (int)UserTypes.AgentEmployee;
                if (isAgentEmploye)
                {
                    userBl.CheckPermission(Constants.Permissions.ViewUser);
                    agentUser = CacheManager.GetUserById(agentUser.ParentId.Value);
                }
                var partnerSetting = CacheManager.GetPartnerSettingByKey(agentUser.PartnerId, Constants.PartnerKeys.IsUserNameGeneratable);
                if (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue != 0)
                {
                    userBl.CheckSecurityCode(securityCode);
                }
                if (input.ClientIdentity.HasValue)
                {
                    var client = CacheManager.GetClientById(input.ClientIdentity.Value);
                    if (client == null || client.UserId == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);
                    var user = CacheManager.GetUserById(client.UserId.Value);
                    if (string.IsNullOrEmpty(user.Path) || !user.Path.Contains("/" + agentUser.Id + "/"))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);
                    
                    using (var clientBll = new ClientBll(userBl))
                    {
                        clientBll.ChangeClientPassword(new Common.Models.WebSiteModels.ChangeClientPasswordInput { ClientId = client.Id, NewPassword = input.NewPassword }, true);
                    }
                    CacheManager.RemoveClientFromCache(client.Id);
                    Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.Clients, client.Id));
                }
                else
                {
                    BllUser user;
                    user = CacheManager.GetUserById(input.UserIdentity ?? identity.Id);
                    if (user == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                    if (!user.Path.Contains(string.Format("/{0}/", agentUser.Id)))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                    userBl.ChangeUserPassword(user.Id, input.OldPassword, input.NewPassword, input.UserIdentity.HasValue);
                    CacheManager.RemoveUserFromCache(identity.Id);
                    Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.User, identity.Id));
                }

            }
            return new ApiResponseBase();
        }

        public static ApiResponseBase ResetSecurityCode(ChangePasswordInput input, string securityCode, SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            using (var userBl = new UserBll(identity, log))
            {
                if (input.UserIdentity.HasValue && input.UserIdentity.Value != identity.Id)
                {
                    var partnerSetting = CacheManager.GetPartnerSettingByKey(user.PartnerId, Constants.PartnerKeys.IsUserNameGeneratable);
                    if (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue != 0)
                        userBl.CheckSecurityCode(securityCode);
                    user = CacheManager.GetUserById(input.UserIdentity.Value);
                    if (user == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                    if (!user.Path.Contains(string.Format("/{0}/", identity.Id)))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                }
                else
                    input.UserIdentity = (int?)null;
                userBl.ChangeUserSecurityCode(user.Id, input.OldPassword, input.NewPassword, false, true, input.UserIdentity.HasValue);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.User, user.Id));
            }
            return new ApiResponseBase();
        }

        public static ApiResponseBase ChangeNickName(NickNameInput input, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                userBl.ChangeUserNickName(identity.Id, input.NickName, true);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.User, identity.Id));
            }
            return new ApiResponseBase();
        }

        public static ApiResponseBase FindAvailableUserName(int type, UserNameModel input, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var user = CacheManager.GetUserById(identity.Id);
                if (user.Type == (int)UserTypes.AdminUser)
                {
                    type = (int)UserTypes.CompanyAgent;
                    if (input.Level != (int)AgentLevels.Company)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
                }
                else if (user.Type == (int)UserTypes.AgentEmployee && type == (int)UserTypes.AgentEmployee)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
                if (input.Level == 0)
                    input.Level = user.Level.Value;
                return new ApiResponseBase
                {
                    ResponseObject = userBl.FindAvailableUserName(type, input.Level, input.StartsWith)
                };
            }
        }

        public static ApiResponseBase GenerateUserNamePrefix(int type, int level, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var agent = CacheManager.GetUserById(identity.Id);
                if (agent.Type == (int)UserTypes.AdminUser)
                {
                    type = (int)UserTypes.CompanyAgent;
                    if (level != (int)AgentLevels.Company)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
                }
                if (level == 0)
                    level = agent.Level.Value;
                return new ApiResponseBase
                {
                    ResponseObject = UserBll.GenerateUserNamePrefix(agent, level, type)
                };
            }
        }

        private static ApiResponseBase UploadProfileImage(string imageData, SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            if (user.Type == (int)UserTypes.AgentEmployee)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);

            using (var userBl = new UserBll(identity, log))
            {
                byte[] bytes = Convert.FromBase64String(imageData);
                Image image;
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    image = Image.FromStream(ms);
                    if (image.Width > 455 || image.Height > 455)
                        image = CommonFunctions.ResizeImage(image, new Size(Math.Min(image.Width, 455), Math.Min(image.Height, 455)));
                    using (var newMemoryStream = new MemoryStream())
                    {
                        image.Save(newMemoryStream, ImageFormat.Jpeg);
                        userBl.UpdateUserImage(identity.Id, newMemoryStream.ToArray());
                    }
                }
                return new ApiResponseBase();
            }
        }

        public static ApiResponseBase UpdateAgentTransferCondition(ApiAgentSettings input, SessionIdentity identity, ILog log)
        {
            var agent = CacheManager.GetUserById(identity.Id);
            using (var userBl = new UserBll(identity, log))
            {
                if (agent.Type == (int)UserTypes.AgentEmployee)
                {
                    userBl.CheckPermission(AgentEmployeePermissions.FullControl.ToString());
                    agent = CacheManager.GetUserById(agent.ParentId.Value);
                }
                var subAgent = CacheManager.GetUserById(input.ObjectId);
                if (subAgent == null || !agent.Path.Contains(string.Format("/{0}/", subAgent.Id)))
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                if (input.CalculationPeriod == null ||
               (input.CalculationPeriod.Count > 1 && (input.CalculationPeriod.Contains(1) || input.CalculationPeriod.Contains(-1))) ||
                input.CalculationPeriod.Any(x => !Enum.IsDefined(typeof(AgentTransferCalculationPeriods), x)))
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
                var userSetting = CacheManager.GetUserSetting(subAgent.Id);
                var userSettings = new UserSetting
                {
                    UserId = subAgent.Id,
                    AllowOutright = userSetting.AllowOutright,
                    AllowDoubleCommission = userSetting.AllowDoubleCommission,
                    CalculationPeriod = string.Join(",", input.CalculationPeriod)
                };
                userBl.SaveUserSettings(userSettings, out List<int> cIds);
                return new ApiResponseBase
                {
                    ResponseObject = input.CalculationPeriod
                };
            }
        }

        public static ApiResponseBase GetAgentTransferCondition(int agentId, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var agent = CacheManager.GetUserById(identity.Id);
                var subAgent = CacheManager.GetUserById(agentId);
                if (subAgent == null || !agent.Path.Contains(string.Format("/{0}/", subAgent.Id)))
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                var userSetting = CacheManager.GetUserSetting(subAgent.Id);

                return new ApiResponseBase
                {
                    ResponseObject = userSetting.CalculationPeriod.Split(',').Select(Int32.Parse).ToList()
                };
            }
        }

        public static ApiResponseBase ChangeAgentsDoubleCommissionState(List<ApiAgentSettings> inputList, SessionIdentity identity, ILog log)
        {
            var agent = CacheManager.GetUserById(identity.Id);
            using (var userBl = new UserBll(identity, log))
            {
                if (agent.Type == (int)UserTypes.AgentEmployee)
                {
                    userBl.CheckPermission(AgentEmployeePermissions.FullControl.ToString());
                    agent = CacheManager.GetUserById(agent.ParentId.Value);
                }
                else if (agent.Type != (int)UserTypes.AdminUser)
                {
                    var agentSetting = CacheManager.GetUserSetting(agent.Id);
                    if (!agentSetting.AllowDoubleCommission)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
                }
                var response = new List<ApiAgentSettings>();
                foreach (var input in inputList)
                {
                    var subAgent = CacheManager.GetUserById(input.ObjectId);
                    if (subAgent == null || !subAgent.Path.Contains(string.Format("/{0}/", agent.Id)))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                    var parentSetting = CacheManager.GetUserSetting(subAgent.ParentId ?? 0);
                    if (parentSetting != null && !parentSetting.AllowDoubleCommission || subAgent.ParentId != agent.Id)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);

                    var subAgentSetting = CacheManager.GetUserSetting(subAgent.Id);
                    var userSettings = new UserSetting
                    {
                        UserId = subAgent.Id,
                        AllowAutoPT = subAgentSetting.AllowAutoPT,
                        CalculationPeriod = subAgentSetting.CalculationPeriod,
                        AllowOutright = subAgentSetting.AllowOutright,
                        AllowDoubleCommission = input.AllowDoubleCommission.Value,
                        LevelLimits = subAgentSetting.LevelLimits,
                        CountLimits = subAgentSetting.CountLimits
                    };
                    var res = userBl.SaveUserSettings(userSettings, out List<int> cIds);
                    foreach (var id in cIds)
                    {
                        Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientSettings,
                                                                            id, "AllowDoubleCommission"));
                        Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientSettings,
                                                                            id, "AllowOutright"));
                    }
                    response.Add(new ApiAgentSettings { ObjectId = res.UserId, AllowDoubleCommission = res.AllowDoubleCommission });
                }
                return new ApiResponseBase { ResponseObject = response };
            }
        }

        public static ApiResponseBase ChangeAgentSettingState(ApiAgentSettings input, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var agent = CacheManager.GetUserById(identity.Id);
                var subAgent = CacheManager.GetUserById(input.ObjectId);
                if (subAgent == null || !subAgent.Path.Contains(string.Format("/{0}/", agent.Id)))
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                var subAgentSetting = CacheManager.GetUserSetting(subAgent.Id);
                if (agent.Type != (int)UserTypes.AdminUser)
                {
                    if (agent.Type == (int)UserTypes.AgentEmployee)
                    {
                        userBl.CheckPermission(AgentEmployeePermissions.FullControl.ToString());
                        agent = CacheManager.GetUserById(agent.ParentId.Value);
                    }
                    var agentSetting = CacheManager.GetUserSetting(agent.Id);
                    if ((input.AllowOutright.HasValue && (!agentSetting.AllowOutright || subAgentSetting.AllowOutright == input.AllowOutright.Value)) ||
                        (input.AllowAutoPT.HasValue && (!agentSetting.AllowAutoPT.HasValue || !agentSetting.AllowAutoPT.Value || subAgentSetting.AllowAutoPT == input.AllowAutoPT)))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
                }
                var userSettings = new UserSetting
                {
                    UserId = subAgent.Id,
                    AllowAutoPT = input.AllowAutoPT ?? subAgentSetting.AllowAutoPT,
                    CalculationPeriod = subAgentSetting.CalculationPeriod,
                    AllowOutright = input.AllowOutright ?? subAgentSetting.AllowOutright,
                    AllowDoubleCommission = subAgentSetting.AllowDoubleCommission,
                    LevelLimits = subAgentSetting.LevelLimits,
                    CountLimits = subAgentSetting.CountLimits,
                };
                var res = userBl.SaveUserSettings(userSettings, out List<int> cIds);
                foreach (var id in cIds)
                {
                    Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientSettings,
                                                                        id, "AllowDoubleCommission"));
                    Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientSettings,
                                                                        id, "AllowOutright"));
                }
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ObjectId = res.UserId,
                        res.AllowOutright,
                        res.AllowAutoPT
                    }
                };
            }
        }

        public static ApiResponseBase GetPartnerCurrencies(SessionIdentity identity, ILog log)
        {
            var agent = CacheManager.GetUserById(identity.Id);
            return new ApiResponseBase
            {
                ResponseObject = CacheManager.GetPartnerCurrencies(agent.PartnerId).OrderBy(x => x.Priority).Select(x => x.CurrencyId).ToList()
            };
        }

        public static ApiResponseBase GetLevelLimits(ApiAgentInput agentInput, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var user = CacheManager.GetUserById(identity.Id);
                if (user.Type == (int)UserTypes.AgentEmployee)
                    user = CacheManager.GetUserById(user.ParentId.Value);
                var isAdmin = user.Type == (int)UserTypes.AdminUser;
                var level = agentInput.Id.HasValue && !agentInput.Level.HasValue ? CacheManager.GetUserById(agentInput.Id.Value).Level : agentInput.Level;
                var downlineLimits = agentInput.Id.HasValue && level != (int)AgentLevels.Member ? userBl.GetAgentDownline(agentInput.Id.Value, false, false) : new List<AgentDownlineInfo>();
                var agentCurrentBalance = 0m;
                if (agentInput.Id.HasValue)
                    agentCurrentBalance = level != (int)AgentLevels.Member ? userBl.GetUserBalance(agentInput.Id.Value).Balance :
                                          BaseBll.GetObjectBalance((int)ObjectTypes.Client, agentInput.Id.Value).AvailableBalance;
                var parentUserSettings = CacheManager.GetUserSetting(user.Id);
                var currentUserSettings = parentUserSettings;
                if (agentInput.Id.HasValue)
                {
                    if (level != (int)AgentLevels.Member)
                        currentUserSettings = CacheManager.GetUserSetting(agentInput.Id.Value);
                    else
                    {
                        var memberSetting = CacheManager.GetClientSettingByName(agentInput.Id.Value, ClientSettings.MaxCredit);
                        currentUserSettings.AgentMaxCredit = memberSetting.NumericValue;
                    }
                }
                if (!isAdmin)
                {
                    var levelEnum = BaseBll.GetEnumerations(Constants.EnumerationTypes.AgentLevels, identity.LanguageId).Where(x => x.Value >= agentInput.Level).ToList();
                    var parentLevelLimits = JsonConvert.DeserializeObject<List<LevelLimit>>(parentUserSettings.LevelLimits);
                    var parentExistingLimits = userBl.GetSubAgents(user.Id, null, null, true, string.Empty);
                    var parentCountLimits = JsonConvert.DeserializeObject<List<CountLimit>>(parentUserSettings.CountLimits);
                    

                    var currentLevelLimits = JsonConvert.DeserializeObject<List<LevelLimit>>(currentUserSettings.LevelLimits);
                    var currentCountLimits = JsonConvert.DeserializeObject<List<CountLimit>>(currentUserSettings.CountLimits);

                    var userCurrentBalance = userBl.GetUserBalance(user.Id).Balance;

                    var levelLimits = new List<Models.Downline.LevelLimit>();
                    for(int i = 0; i < levelEnum.Count; i++)
                    {
                        var currentLimit = levelEnum[i].Value == level ?
                                    (agentInput.Id.HasValue ? currentUserSettings.AgentMaxCredit : parentLevelLimits.Where(y => y.Level == levelEnum[i].Value).Select(y => Math.Min(y.Limit.Value, userCurrentBalance)).FirstOrDefault()) :
                                    currentLevelLimits.Where(y => y.Level == levelEnum[i].Value).Select(y => Math.Min(y.Limit.Value, userCurrentBalance)).FirstOrDefault();
                        levelLimits.Add(new Models.Downline.LevelLimit
                        {
                            Id = levelEnum[i].Value,
                            Name = levelEnum[i].Text,
                            MinLimit = levelEnum[i].Value == level ? (agentInput.Id.HasValue ? currentUserSettings.AgentMaxCredit - agentCurrentBalance : 0) :
                                    (downlineLimits.Any(y => y.Level == levelEnum[i].Value) ? downlineLimits.First(y => y.Level == levelEnum[i].Value).MaxCredit : 0),
                            Limit = levelEnum[i].Value == level ?
                                    (parentLevelLimits.Where(y => y.Level == levelEnum[i].Value).Select(y => Math.Min(y.Limit.Value,
                                    (agentInput.Id.HasValue ? userCurrentBalance + currentUserSettings.AgentMaxCredit : currentUserSettings.AgentMaxCredit) ?? 0)).FirstOrDefault()) :
                                     parentLevelLimits.Where(y => y.Level == levelEnum[i].Value).Select(y => Math.Min(y.Limit.Value, userCurrentBalance)).FirstOrDefault(),
                            CurrentLimit = currentLimit
                        });

                        if(i > 0 && agentInput.Id.HasValue && levelLimits[i].Limit > levelLimits[0].CurrentLimit)
                            levelLimits[i].Limit = levelLimits[0].CurrentLimit;
                    }

                    return new ApiResponseBase
                    {
                        
                        ResponseObject = new
                        {
                            LevelLimits = levelLimits.OrderBy(x => x.Id).ToList(),
                            CountLimits = parentCountLimits.Where(x => x.Level > agentInput.Level).Select(x => new
                            {
                                Id = x.Level,
                                Name = levelEnum.FirstOrDefault(y => y.Value == x.Level).Text,
                                MinCount = downlineLimits.Any(y => y.Level == x.Level) ? downlineLimits.First(y => y.Level == x.Level).Count : 0,
                                Count = x.Count - parentExistingLimits.Where(y => y.Level == x.Level).Count(),
                                CurrentCount = agentInput.Id.HasValue ? currentCountLimits.FirstOrDefault(z => z.Level == x.Level).Count :
                                x.Count - parentExistingLimits.Where(y => y.Level == x.Level).Count(),
                            })
                        }
                    };
                }
                else
                {
                    var levelEnum = BaseBll.GetEnumerations(Constants.EnumerationTypes.AgentLevels, identity.LanguageId).Where(x => x.Value >= (int)AgentLevels.Company);
                    var partnerSetting = CacheManager.GetConfigParameters(user.PartnerId, Constants.PartnerKeys.AgentAccountLimits);
                    List<LevelLimit> currentLevelLimits = null;
                    List<CountLimit> currentCountLimits = null;
                    if (agentInput.Id.HasValue)
                    {
                        currentLevelLimits = JsonConvert.DeserializeObject<List<LevelLimit>>(currentUserSettings.LevelLimits);
                        currentCountLimits = JsonConvert.DeserializeObject<List<CountLimit>>(currentUserSettings.CountLimits);
                        var agent = CacheManager.GetUserById(agentInput.Id.Value);
                        if (agent == null)
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                        agentInput.CurrencyId = agent.CurrencyId;
                    }
                    if (string.IsNullOrEmpty(agentInput.CurrencyId))
                        agentInput.CurrencyId = user.CurrencyId;
                    var currencySetting = CacheManager.GetPartnerCurrencies(user.PartnerId).
                        FirstOrDefault(x => x.CurrencyId == agentInput.CurrencyId);
                    var creditMinLimit = 0m;
                    if (currencySetting != null && currencySetting.UserMinLimit.HasValue)
                    {
                        creditMinLimit = currencySetting.UserMinLimit.Value * 100000000;
                        var i = 1;
                        return new ApiResponseBase
                        {
                            ResponseObject = new
                            {
                                LevelLimits = levelEnum.Select(x =>
                                {
                                    var res = new
                                    {
                                        Id = x.Value,
                                        Name = x.Text,
                                        MinLimit = x.Value == level ? (agentInput.Id.HasValue ? currentUserSettings.AgentMaxCredit - agentCurrentBalance : 0) :
                                            (downlineLimits.Any(y => y.Level == x.Value) ? downlineLimits.First(y => y.Level == x.Value).MaxCredit : 0),
                                        Limit = creditMinLimit / i,
                                        CurrentLimit = currentLevelLimits != null ?
                                        currentLevelLimits.FirstOrDefault(y => y.Level == x.Value)?.Limit
                                        : creditMinLimit / i,
                                    }; i *= 2; return res;
                                }).OrderBy(x => x.Id),
                                CountLimits = partnerSetting != null ?
                                partnerSetting.Where(x => Convert.ToInt32(x.Key) > agentInput.Level).Select(x => new
                                {
                                    Id = x.Key,
                                    Name = levelEnum.FirstOrDefault(y => y.Value == Convert.ToInt32(x.Key)).Text,
                                    MinCount = downlineLimits.Any(y => y.Level == Convert.ToInt32(x.Key)) ? downlineLimits.First(y => y.Level == Convert.ToInt32(x.Key)).Count : 0,
                                    Count = Convert.ToInt32(x.Value),
                                    CurrentCount = currentLevelLimits != null ? currentCountLimits.FirstOrDefault(z => z.Level == Convert.ToInt32(x.Key))?.Count : Convert.ToInt32(x.Value)
                                }) : null
                            }
                        };
                    }
                    return new ApiResponseBase
                    {
                        ResponseObject = new
                        {
                            LevelLimits = levelEnum.Select(x => new
                            {
                                Id = x.Value,
                                Name = x.Text,
                                MinLimit = 0
                            }).OrderBy(x => x.Id),
                            CountLimits = partnerSetting != null ?
                            partnerSetting.Where(x => Convert.ToInt32(x.Key) > agentInput.Level).Select(x => new
                            {
                                Id = x.Key,
                                Name = levelEnum.FirstOrDefault(y => y.Value == Convert.ToInt32(x.Key)).Text,
                                MinCount = 0,
                                Count = x.Value
                            }) : null
                        }
                    };
                }
            }
        }
        private static ApiResponseBase SaveNote(Note note, SessionIdentity identity, ILog log)
        {
            if (note.ObjectTypeId == (int)ObjectTypes.Client)
            {
                using (var clientbl = new ClientBll(identity, log))
                {
                    using (var userBl = new UserBll(identity, log))
                    {
                        clientbl.SaveNote(note);
                        CacheManager.RemoveClientFromCache((int)note.ObjectId);
                        Helpers.Helpers.InvokeMessage("RemoveClient", (int)note.ObjectId);
                        return new ApiResponseBase
                        {
                            ResponseObject = note
                        };
                    }
                }
            }
            else if (note.ObjectTypeId == (int)ObjectTypes.Document)
            {
                using (var documentbl = new DocumentBll(identity, log))
                {
                    documentbl.SaveNote(note);

                    return new ApiResponseBase
                    {
                        ResponseObject = note
                    };
                }
            }
            using (var utilbl = new UtilBll(identity, log))
            {
                utilbl.SaveNote(note);

                return new ApiResponseBase
                {
                    ResponseObject = note
                };
            }
        }

        private static ApiResponseBase GetNotes(ApiFilterNote filter, SessionIdentity identity, ILog log)
        {
            using (var utilbl = new UtilBll(identity, log))
            {
                var notes = utilbl.GetNotes(filter.MapToFilterNote());

                return new ApiResponseBase
                {
                    ResponseObject = notes.MapToNoteModels(utilbl.GetUserIdentity().TimeZone)
                };
            }
        }

        public static ApiAgentStatesOutput GetAgentStates(SessionIdentity session, ILog log)
        {
            if (session.IsAffiliate)
                return new ApiAgentStatesOutput();

            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var userBl = new UserBll(paymentSystemBl))
                {
                    var agentId = session.Id;
                    var agent = CacheManager.GetUserById(session.Id);
                    if (agent.Type == (int)UserTypes.AgentEmployee)
                    {
                        agentId = agent.ParentId.Value;
                    }

                    var prCount = paymentSystemBl.GetPaymentRequestsCount(new List<int> { (int)PaymentRequestStates.Pending },
                        new List<int> {
                        CacheManager.GetPaymentSystemByName(PaymentSystems.BankTransferSwift).Id,
                        CacheManager.GetPaymentSystemByName(PaymentSystems.BankWire).Id,
                        CacheManager.GetPaymentSystemByName(PaymentSystems.BankTransferSepa).Id }, agentId);

                    var response = new ApiAgentStatesOutput
                    {
                        UnreadMessagesCount = userBl.GetUnreadTicketsCount(agent.PartnerId, agentId),
                        PaymentRequestsCount = prCount
                    };
                    return response;
                }
            }
        }

        private static ApiResponseBase UpdateUserState(ApiUserState userState, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                userBl.UpdateUserState(userState.AdminMenuId, userState.GridIndex, userState.State);
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase GetUserState(ApiUserState userState, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var state = userBl.GetUserState(userState.AdminMenuId, userState.GridIndex);
                return new ApiResponseBase
                {
                    ResponseObject = state
                };
            }
        }
    }
}