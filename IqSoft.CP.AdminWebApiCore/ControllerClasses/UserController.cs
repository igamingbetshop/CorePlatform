using Newtonsoft.Json;
using log4net;
using IqSoft.CP.Common;
using IqSoft.CP.AdminWebApi.Filters;
using IqSoft.CP.AdminWebApi.Helpers;
using IqSoft.CP.AdminWebApi.Models.UserModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.DAL.Models.User;
using IqSoft.CP.AdminWebApi.Models.AgentModels;
using IqSoft.CP.Common.Enums;
using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.AdminWebApiCore;
using Microsoft.AspNetCore.Hosting;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class UserController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            switch (request.Method)
            {
                case "Logout":
                    return Logout(request.Token, identity, log);
                case "GetUsers":
                    return GetUsers(JsonConvert.DeserializeObject<ApiFilterUser>(request.RequestData), identity, log);
                case "SaveUser":
                    return SaveUser(JsonConvert.DeserializeObject<UserModel>(request.RequestData), identity, log);
                case "IsUserNameExists":
                    return IsUserNameExists(JsonConvert.DeserializeObject<UserNameModel>(request.RequestData), identity, log);
                case "GetUserById":
                    return GetUserById(JsonConvert.DeserializeObject<int>(request.RequestData), identity, log);
                case "ChangePassword":
                    return ChangePassword(JsonConvert.DeserializeObject<ChangePasswordInput>(request.RequestData), identity, log);
                case "GetUserBalance":
                    return GetUserBalance(identity);
                case "ChangeUserCurrentPage":
                    return  new ApiResponseBase();
                case "ExportUsers":
                    return ExportUsers(JsonConvert.DeserializeObject<ApiFilterUser>(request.RequestData), identity, log, env);
                case "GetProjectToken":
                    return GetProjectToken(JsonConvert.DeserializeObject<int>(request.RequestData), identity, log);
				case "GetUserState":
					return GetUserState(identity, log);
				case "UpdateUserState":
					return UpdateUserState(request.RequestData, identity, log);
                case "CreateDebitCorrection":
                    return CreateDebitCorrection(JsonConvert.DeserializeObject<UserTransferInput>(request.RequestData), identity, log);
                case "CreateCreditCorrection":
                    return CreateCreditCorrection(JsonConvert.DeserializeObject<UserTransferInput>(request.RequestData), identity, log);
                case "GetCorrections":
                    return GetCorrections(JsonConvert.DeserializeObject<ApiFilterUserCorrection>(request.RequestData), identity, log);
                case "GetCommissionPlan":
                    return GetCommissionPlan(JsonConvert.DeserializeObject<ApiAgentCommission>(request.RequestData), identity, log);
				case "UpdateCommissionPlan":
					return UpdateCommissionPlan(JsonConvert.DeserializeObject<ApiAgentCommission>(request.RequestData), identity, log);
				case "SaveApiKey":
					return SaveApiKey(JsonConvert.DeserializeObject<UserIdentityInput>(request.RequestData), identity, log);
                case "FindAvailableUserName":
                    return FindAvailableUserName(JsonConvert.DeserializeObject<UserTypeInput>(request.RequestData), identity, log);
                case "GetUserAccountsBalanceHistoryPaging":
                    return
                        GetUserAccountsBalanceHistoryPaging(
                            JsonConvert.DeserializeObject<ApiFilterAccountsBalanceHistory>(request.RequestData),
                            identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        public static ApiResponseBase IsUserNameExists(UserNameModel input, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var response = new ApiResponseBase
                {
                    ResponseObject = userBl.IsUserNameExists(input.PartnerId, input.UserName, null, '\0', true)
                };
                return response;
            }
        }

        public static ApiResponseBase FindAvailableUserName(UserTypeInput input, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var defaultRegex = @"\w*";
                var availableUsername = string.Empty;
                if (input.Type == (int)UserTypes.MasterAgent)
                {
                    var partnerSetting = CacheManager.GetPartnerSettingByKey(input.PartnerId, Constants.PartnerKeys.IsUserNameGeneratable);
                    if (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue != 0)
                    {
                        defaultRegex = "^C[0-9]{3}$";
                        availableUsername = new string(userBl.FindAvailableUserName((int)UserTypes.MasterAgent, (int)AgentLevels.Company, input.StartWith).ToArray());
                    }
                }
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        Regex = defaultRegex,
                        AvailableUsername = availableUsername
                    }
                };
            }
        }
        private static ApiResponseBase Logout(string token, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var response = new ApiResponseBase();
                userBl.LogoutUser(token);
                return response;
            }
        }

        private static ApiResponseBase GetUserById(int id, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var resp = userBl.GetUserById(id).MapToUserModel(identity.TimeZone);
                if (identity.Id == id)
                    resp.OddsType = CacheManager.GetUserSetting(identity.Id)?.OddsType;

                resp.Accounts = BaseBll.GetObjectBalance((int)ObjectTypes.User, id).Balances.ToList();
                return new ApiResponseBase
                {
                    ResponseObject = resp
                };
            }
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

        public static ApiResponseBase GetUserBalance(SessionIdentity identity)
        {
            var balance = BaseBll.GetObjectBalance((int)ObjectTypes.User, identity.Id);
            var user = CacheManager.GetUserById(identity.Id);
            balance.AvailableBalance =Math.Floor( balance.Balances.Sum(x => BaseBll.ConvertCurrency(x.CurrencyId, user.CurrencyId, x.Balance)) *100) / 100;
            balance.CurrencyId = user.CurrencyId;
            return new ApiResponseBase
            {
                ResponseObject = balance
            };
        }

        private static ApiResponseBase GetUsers(ApiFilterUser request, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var input = request.MaptToFilterfnUser();
                input.IdentityId = identity.Id;
                var users = userBl.GetUsersPagedModel(input, true);
                var response = new ApiResponseBase
                {
                    ResponseObject = new { users.Count, Entities = users.Entities.MapToUserModels(userBl.GetUserIdentity().TimeZone) }
                };
                return response;
            }
        }

        public static ApiResponseBase SaveUser(UserModel request, SessionIdentity identity, ILog log)
        {

            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) ||
               (request.Email != null && string.IsNullOrWhiteSpace(request.Email)) ||
               (request.NickName != null && string.IsNullOrWhiteSpace(request.NickName)))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
            using var userBl = new UserBll(identity, log) ;
            var timeZone = userBl.GetUserIdentity().TimeZone;
            var input = request.MapToUser(timeZone);
            input.IsTwoFactorEnabled = false;
            if (input.Type == (int)UserTypes.MasterAgent)
                input.Level = (int)AgentLevels.Company;
            else input.Level = 0;
            UserModel user;
            if (input.Id == 0)
                user = userBl.AddUser(input).MapToUserModel(timeZone);
            else
            {
                user =  userBl.EditUser(input, true).MapToUserModel(timeZone);
                CacheManager.RemoveUserFromCache(user.Id);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.User, user.Id));
            }
            return new ApiResponseBase
            {
                ResponseObject = user
            };
        }

        //private static ApiResponseBase ChangeUserCurrentPage(string newPage, SessionIdentity identity, ILog log)
        //{
        //    using (var userBl = new UserBll(identity, log))
        //    {
        //        //var requestMethod = HttpContext.Current.Request.RequestType;
        //        //userBl.ChangeUserCurrentPage(newPage, requestMethod);
        //        return new ApiResponseBase();
        //    }
        //}

        private static ApiResponseBase ExportUsers(ApiFilterUser request, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var users = userBl.ExportUsersModel(request.MaptToFilterUser()).Select(x => x.MapToUserModel(userBl.GetUserIdentity().TimeZone)).ToList();
                string fileName = "ExportUsers.csv";
                string fileAbsPath = userBl.ExportToCSV<UserModel>(fileName, users, null, null, 0, env);

                var response = new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
                return response;
            }
        }

        private static ApiResponseBase GetProjectToken(int projectTypeId, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                string newToken = userBl.GetProjectToken(projectTypeId, identity);
               
                var response = new ApiResponseBase
                {
                    ResponseObject = newToken
                };
                return response;
            }
        }

		private static ApiResponseBase GetUserState(SessionIdentity identity, ILog log)
		{
			using (var userBl = new UserBll(identity, log))
			{
				var state = userBl.GetUserState();
				return new ApiResponseBase
				{
					ResponseObject = state
				};
			}
		}

		private static ApiResponseBase UpdateUserState(string state, SessionIdentity identity, ILog log)
		{
			using (var userBl = new UserBll(identity, log))
			{
				userBl.UpdateUserState(state);
				return new ApiResponseBase();
			}
		}

        private static ApiResponseBase CreateDebitCorrection(UserTransferInput userCorrectionInput, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                using (var documentBl = new DocumentBll(userBl))
                {
                    if (!userCorrectionInput.UserId.HasValue)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                    var user = userBl.GetUserById(userCorrectionInput.UserId.Value);
                    if (user == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);

                    return new ApiResponseBase
                    {
                        ResponseObject = userBl.CreateDebitOnUser(userCorrectionInput, documentBl).MapToDocumentModel(identity.TimeZone)
                    };
                }
            }
        }

        private static ApiResponseBase CreateCreditCorrection(UserTransferInput userCorrectionInput, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                using (var documentBl = new DocumentBll(userBl))
                {
                    if(!userCorrectionInput.UserId.HasValue)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                    var user = userBl.GetUserById(userCorrectionInput.UserId.Value);
                    if (user == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);

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
                return new ApiResponseBase
                {
                    ResponseObject = userBl.GetUserCorrections(filter.MapToFilterUserCorrection()).MapToApiUserCorrections(identity.TimeZone, filter.UserId)
                };
            }
        }

		private static ApiResponseBase GetCommissionPlan(ApiAgentCommission input, SessionIdentity identity, ILog log)
		{
			using (var userBl = new UserBll(identity, log))
			{
                var agent = CacheManager.GetUserById(input.AgentId);
				return new ApiResponseBase
				{
					ResponseObject = userBl.GetAgentCommissionPlan(agent.PartnerId, input.AgentId, null, null).MapToApiAgentCommissions()
				};
			}
		}

		private static ApiResponseBase UpdateCommissionPlan(ApiAgentCommission apiCommissionInput, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                try
                {
                    if (!string.IsNullOrEmpty(apiCommissionInput.TurnoverPercent) &&
                        !decimal.TryParse(apiCommissionInput.TurnoverPercent, out decimal turnoverPersent))
                    {
                        var selections = apiCommissionInput.TurnoverPercent.Split(',');
                        apiCommissionInput.TurnoverPercentsList = new List<ApiTurnoverPercent>();
                        foreach (var s in selections)
                        {
                            var sel = s.Split('-', '|');
                            if (sel.Length != 3)
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
                            var apiTurnoverPercent = new ApiTurnoverPercent
                            {
                                FromCount = Convert.ToInt32(sel[0]),
                                ToCount = Convert.ToInt32(sel[1]),
                                Percent = Convert.ToDecimal(sel[2])
                            };

                            apiCommissionInput.TurnoverPercentsList.Add(apiTurnoverPercent);
                        }
                    }
                    var resp = userBl.UpdateAgentCommission(apiCommissionInput.MapToAgentCommission());
                    return new ApiResponseBase
                    {
                        ResponseObject = resp == null ? new ApiAgentCommission() : resp.MapToApiAgentCommission()
                    };
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    var error = new StringBuilder();
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            error.AppendFormat("Property: {0} Error: {1}",
                                validationError.PropertyName,
                                validationError.ErrorMessage);
                        }
                    }
                    Program.DbLogger.Error(new Exception(error.ToString()));
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
                }
            }
        }

        private static ApiResponseBase SaveApiKey(UserIdentityInput input, SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(input.UserId);
            if(user==null)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
            if (user.SecurityCode != input.ApiKey)
            {
                using (var userBl = new UserBll(identity, log))
                {
                    userBl.ChangeUserSecurityCode(user.Id, user.SecurityCode, input.ApiKey, true, false, true);
                   Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.User, user.Id));
                }
            }
            return new ApiResponseBase();
        }

        private static ApiResponseBase GetUserAccountsBalanceHistoryPaging(
            ApiFilterAccountsBalanceHistory input, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var filter = input.MapToFilterAccountsBalanceHistory(identity.TimeZone);
                var response = new ApiResponseBase
                {
                    ResponseObject = userBl.GetUserAccountsBalanceHistoryPaging(filter)
                            .Select(x => x.MapToApiAccountsBalanceHistoryElement(identity.TimeZone))
                            .ToList()
                };
                return response;
            }
        }
    }
}