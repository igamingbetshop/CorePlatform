using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.AgentWebApi.Filters;
using IqSoft.CP.AgentWebApi.Helpers;
using IqSoft.CP.AgentWebApi.Models;
using IqSoft.CP.AgentWebApi.Models.ClientModels;
using IqSoft.CP.AgentWebApi.Models.User;
using log4net;
using Newtonsoft.Json;
using System;
using System.Web;
using System.Collections.Generic;
using IqSoft.CP.AgentWebApi.ClientModels;
using IqSoft.CP.DAL;
using System.Linq;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.Commission;

namespace IqSoft.CP.AgentWebApi.ControllerClasses
{
    public static class ClientController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "CreateClient":
                    return CreateClient(JsonConvert.DeserializeObject<NewClientModel>(request.RequestData), request.SecurityCode, identity, log);
                case "CloneClient":
                    return CloneClient(JsonConvert.DeserializeObject<NewClientModel>(request.RequestData), identity, log);
                case "GetClients":
                    return GetClients(JsonConvert.DeserializeObject<ApiFilterfnClient>(request.RequestData), identity, log);
                case "UpdateAgentClient":
                    return UpdateAgentClient(JsonConvert.DeserializeObject<ChangeObjectStateInput>(request.RequestData), identity, log);
                case "GetClientInfo":
                    return GetClientInfo(Convert.ToInt32(request.RequestObject), identity, log);
                case "GetClientAccounts":
                    return GetClientAccounts(Convert.ToInt32(request.RequestObject), identity, log);
                case "CreateClientDebitCorrection":
                    return CreateDebitCorrection(JsonConvert.DeserializeObject<ClientCorrectionInput>(request.RequestData), identity, log);
                case "CreateClientCreditCorrection":
                    return CreateCreditCorrection(JsonConvert.DeserializeObject<ClientCorrectionInput>(request.RequestData), identity, log);
                case "GetClientCorrections":
                    return GetClientCorrections(JsonConvert.DeserializeObject<ApiFilterClientCorrection>(request.RequestData), identity, log);
                case "ChangeClientOutrightState":
                    return ChangeClientOutrightState(JsonConvert.DeserializeObject<ApiAgentSettings>(request.RequestData), identity, log);
                case "ChangeClientsDoubleCommissionState":
                    return ChangeClientsDoubleCommissionState(JsonConvert.DeserializeObject<List<ApiAgentSettings>>(request.RequestData), identity, log);
                case "FindClients":
                    return FindClients(JsonConvert.DeserializeObject<string>(request.RequestData), identity, log);
                case "IsUserNameAvailable":
                    return IsUserNameAvailable(JsonConvert.DeserializeObject<ApiUserNameInput>(request.RequestData), identity, log);
                case "UpdateClientSettings":
                    return UpdateClientSettings(JsonConvert.DeserializeObject<NewClientModel>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.MethodNotFound);
        }

        public static ApiResponseBase CreateClient(NewClientModel clientModel, string securityCode, SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            if (user.Type == (int)UserTypes.AdminUser)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
            using (var clientBl = new ClientBll(identity, log))
            using (var userBl = new UserBll(clientBl))
            using (var documentBl = new DocumentBll(clientBl))
            using (var regionBl = new RegionBll(clientBl))
            {
                var ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP");
                if (ip == null)
                    ip = Constants.DefaultIp;
                var client = clientModel.MapToClient();
                if (client.RegionId == 0)
                {
                    var country = HttpContext.Current.Request.Headers.Get("CF-IPCountry");
                    if (country != null)
                    {
                        var region = regionBl.GetRegionByCountryCode(country);
                        if (region != null)
                            client.RegionId = region.Id;
                    }
                }

                var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                if (isAgentEmploye)
                {
                    clientBl.CheckPermission(Constants.Permissions.EditClient);
                    user = CacheManager.GetUserById(user.ParentId.Value);
                }
                client.UserId = user.Id;
                var userNamePrefix = string.Empty;
                var clientSetting = new ClientCustomSettings
                {
                    AllowOutright = clientModel.AllowOutright ?? false,
                    AllowDoubleCommission = clientModel.AllowDoubleCommission ?? false,
                };
                var userSetting = CacheManager.GetUserSetting(user.Id);

                var partnerSetting = CacheManager.GetPartnerSettingByKey(user.PartnerId, Constants.PartnerKeys.IsUserNameGeneratable);
                if (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue != 0)
                {
                    userBl.CheckSecurityCode(securityCode);
                    if ((!userSetting.AllowDoubleCommission && clientSetting.AllowDoubleCommission.Value)
                        || (!userSetting.AllowOutright && clientSetting.AllowOutright.Value))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
                 
                    if (client.UserName.Length != 3 || client.UserName == "000")
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.InvalidUserName);
                    userNamePrefix = UserBll.GenerateUserNamePrefix(user, (int)AgentLevels.Member, user.Type);
                }
                var parentLimits = (userSetting == null || userSetting.Id == 0) ? new List<CountLimit>() : 
                    JsonConvert.DeserializeObject<List<CountLimit>>(userSetting.CountLimits);
                var countLimitByLevel = parentLimits.FirstOrDefault(x => x.Level == (int)AgentLevels.Member)?.Count;
                if (countLimitByLevel != null)
                {
                    var requestedAgentsCount = !clientModel.Count.HasValue || clientModel.Count.Value == 0 ? 1 : clientModel.Count.Value;
                    var existingMembersCount = userBl.GetAgentMemebers(user.Id, false).Count;
                    if (existingMembersCount + requestedAgentsCount > countLimitByLevel)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.MaxLimitExceeded);
                }

                client.PartnerId = identity.PartnerId;
                client.RegistrationIp = ip;
                client.CurrencyId = identity.CurrencyId;
                client.LanguageId = identity.LanguageId;
                var parentBalance = userBl.GetUserBalance(user.Id);
                var givenCredit = clientModel.LevelLimits?.FirstOrDefault(x => x.Level == (int)AgentLevels.Member);
                if (givenCredit == null)
                    givenCredit = new LevelLimit { Limit = 0 };
                clientModel.MaxCredit = givenCredit.Limit;
                clientSetting.MaxCredit = givenCredit.Limit;
                if (clientModel.Count.HasValue && clientModel.MaxCredit * clientModel.Count.Value > parentBalance.Balance)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.LowBalance);
                if (clientModel.CommissionPlan != null)
                {
                    if (clientModel.CommissionPlan.PositionTaking != null)
                    {
                        foreach (var pt in clientModel.CommissionPlan.PositionTaking)
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
                }
                var commissionSettings = clientModel.CommissionPlan == null ? null : JsonConvert.SerializeObject(clientModel.CommissionPlan);

                var clientCorrectionInput = new ClientCorrectionInput
                {
                    Amount = clientModel.MaxCredit.Value,
                    CurrencyId = client.CurrencyId,
                };
                var resultList = new List<fnClientModel>();
                using (var transactionScope = CommonFunctions.CreateTransactionScope())
                {
                    if (clientModel.Count.HasValue && clientModel.Count.Value > 1)
                    {
                        if (clientModel.Count.Value > 50)
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
                        for (int i = 0; i < clientModel.Count; ++i)
                        {
                            client.UserName = userNamePrefix + new string(userBl.FindAvailableUserName(user.Type, (int)AgentLevels.Member, '\0').ToArray());
                            client.MobileNumber = null;
                            var clientItem = RegisterClient(identity, new ClientRegistrationInput { ClientData = client, IsQuickRegistration = false }, log);
                            resultList.Add(clientItem.MapTofnClientModel(identity.TimeZone));
                            var commissionPlan = new AgentCommission
                            {
                                ProductId = Constants.PlatformProductId,
                                TurnoverPercent = commissionSettings,
                                ClientId = clientItem.Id
                            };
                            userBl.UpdateMemberCommission(commissionPlan);
                            clientSetting.ClientId = clientItem.Id;
                            clientBl.SaveClientSetting(clientSetting);
                            clientCorrectionInput.ClientId = clientItem.Id;
                            if (clientCorrectionInput.Amount != 0)
                                clientBl.CreateDebitCorrectionOnClient(clientCorrectionInput, documentBl, false);
                        }
                    }
                    else
                    {
                        client.UserName = userNamePrefix + client.UserName;
                        log.Info(JsonConvert.SerializeObject(client));
                        client = RegisterClient(identity, new ClientRegistrationInput { ClientData = client, IsQuickRegistration = false }, log);
                        resultList.Add(client.MapTofnClientModel(identity.TimeZone));
                        var commissionPlan = new AgentCommission
                        {
                            ProductId = Constants.PlatformProductId,
                            TurnoverPercent = commissionSettings,
                            ClientId = client.Id
                        };
                        userBl.UpdateMemberCommission(commissionPlan);
                        clientSetting.ClientId = client.Id;
                        clientBl.SaveClientSetting(clientSetting);
                        clientCorrectionInput.ClientId = client.Id;
                        if (clientCorrectionInput.Amount != 0)
                            clientBl.CreateDebitCorrectionOnClient(clientCorrectionInput, documentBl, false);
                    }
                    transactionScope.Complete();
                }
                return new ApiResponseBase { ResponseObject = resultList };
            }
        }

        public static ApiResponseBase CloneClient(NewClientModel clientModel, SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            if (user.Type == (int)UserTypes.AdminUser)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
            using (var clientBl = new ClientBll(identity, log))
            using (var userBl = new UserBll(clientBl))
            using (var documentBl = new DocumentBll(clientBl))
            using (var regionBl = new RegionBll(clientBl))
            {
                var ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP");
                if (ip == null)
                    ip = Constants.DefaultIp;

                var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                if (isAgentEmploye)
                {
                    clientBl.CheckPermission(Constants.Permissions.EditClient);
                    user = CacheManager.GetUserById(user.ParentId.Value);
                }
                var client = clientModel.MapToClient();

                if (client.RegionId == 0)
                {
                    var country = HttpContext.Current.Request.Headers.Get("CF-IPCountry");
                    if (country != null)
                    {
                        var region = regionBl.GetRegionByCountryCode(country);
                        if (region != null)
                            client.RegionId = region.Id;
                    }
                }

                var cloningClient = CacheManager.GetClientByUserName(user.PartnerId, clientModel.CloningUserName);
                client.UserId = user.Id;
                var userNamePrefix = string.Empty;
                var ao = CacheManager.GetClientSettingByName(cloningClient.Id, "AllowOutright");
                var adc = CacheManager.GetClientSettingByName(cloningClient.Id, "AllowDoubleCommission");
                var mc = CacheManager.GetClientSettingByName(cloningClient.Id, "MaxCredit");
                var clientSetting = new ClientCustomSettings
                {
                    AllowOutright = Convert.ToBoolean(ao == null || ao.Id == 0 ? 0 : (ao.NumericValue ?? 0)),
                    AllowDoubleCommission = Convert.ToBoolean(adc == null || adc.Id == 0 ? 0 : (adc.NumericValue ?? 0)),
                    MaxCredit = (mc == null || mc.Id == 0 ? 0 : mc.NumericValue)
                };
                var partnerSetting = CacheManager.GetPartnerSettingByKey(user.PartnerId, Constants.PartnerKeys.IsUserNameGeneratable);
                if (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue != 0)
                {
                    var userSetting = CacheManager.GetUserSetting(user.Id);
                    if ((!userSetting.AllowDoubleCommission && clientSetting.AllowDoubleCommission.Value)
                        || (!userSetting.AllowOutright && clientSetting.AllowOutright.Value))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
                    var parentLimits = JsonConvert.DeserializeObject<List<CountLimit>>(userSetting.CountLimits);
                    var countLimitByLevel = parentLimits.First(x => x.Level == (int)AgentLevels.Member).Count;
                    var requestedAgentsCount = !clientModel.Count.HasValue || clientModel.Count.Value == 0 ? 1 : clientModel.Count.Value;
                    var existingMembersCount = userBl.GetAgentMemebers(user.Id, false).Count;
                    if (existingMembersCount + requestedAgentsCount > countLimitByLevel)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.MaxLimitExceeded);

                    if (client.UserName.Length != 3)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.InvalidUserName);
                    userNamePrefix = UserBll.GenerateUserNamePrefix(user, (int)AgentLevels.Member, user.Type);
                }
                client.PartnerId = user.PartnerId;
                client.RegistrationIp = ip;
                client.CurrencyId = user.CurrencyId;
                client.LanguageId = identity.LanguageId;
                var parentBalance = userBl.GetUserBalance(user.Id);
                var givenCredit = clientModel.LevelLimits.FirstOrDefault(x => x.Level == (int)AgentLevels.Member);
                if (givenCredit == null)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongOperationAmount);
                clientModel.MaxCredit = givenCredit.Limit;
                if (clientModel.Count.HasValue && clientModel.MaxCredit * clientModel.Count.Value > parentBalance.Balance)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.LowBalance);

                var commissionSettingInfo = userBl.GetAgentCommissionPlan(cloningClient.PartnerId, null, cloningClient.Id, Constants.PlatformProductId, false).FirstOrDefault();
                var commissionSettings = commissionSettingInfo.TurnoverPercent;
                var clientCorrectionInput = new ClientCorrectionInput
                {
                    Amount = clientModel.MaxCredit.Value,
                    CurrencyId = client.CurrencyId,
                };
                var resultList = new List<fnClientModel>();
                using (var transactionScope = CommonFunctions.CreateTransactionScope())
                {
                    if (clientModel.Count.HasValue && clientModel.Count.Value > 1)
                    {
                        if (clientModel.Count.Value > 50)
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
                        for (int i = 0; i < clientModel.Count; ++i)
                        {
                            client.UserName = userNamePrefix + new string(userBl.FindAvailableUserName(user.Type, (int)AgentLevels.Member, '\0').ToArray());
                            client.MobileNumber = null;
                            var clientItem = RegisterClient(identity, new ClientRegistrationInput { ClientData = client, IsQuickRegistration = false }, log);
                            resultList.Add(clientItem.MapTofnClientModel(identity.TimeZone));
                            var commissionPlan = new AgentCommission
                            {
                                ProductId = Constants.PlatformProductId,
                                TurnoverPercent = commissionSettings,
                                ClientId = clientItem.Id
                            };
                            userBl.UpdateMemberCommission(commissionPlan);
                            clientSetting.ClientId = clientItem.Id;
                            clientBl.SaveClientSetting(clientSetting);
                            clientCorrectionInput.ClientId = clientItem.Id;
                            if (clientCorrectionInput.Amount != 0)
                                clientBl.CreateDebitCorrectionOnClient(clientCorrectionInput, documentBl, false);
                        }
                    }
                    else
                    {
                        client.UserName = userNamePrefix + client.UserName;
                        client = RegisterClient(identity, new ClientRegistrationInput { ClientData = client, IsQuickRegistration = false }, log);
                        resultList.Add(client.MapTofnClientModel(identity.TimeZone));
                        var commissionPlan = new AgentCommission
                        {
                            ProductId = Constants.PlatformProductId,
                            TurnoverPercent = commissionSettings,
                            ClientId = client.Id
                        };
                        userBl.UpdateMemberCommission(commissionPlan);
                        clientSetting.ClientId = client.Id;
                        clientBl.SaveClientSetting(clientSetting);
                        clientCorrectionInput.ClientId = client.Id;
                        if (clientCorrectionInput.Amount != 0)
                            clientBl.CreateDebitCorrectionOnClient(clientCorrectionInput, documentBl, false);
                    }
                    transactionScope.Complete();
                }
                return new ApiResponseBase { ResponseObject = resultList };
            }
        }

        private static Client RegisterClient(SessionIdentity identity, ClientRegistrationInput input, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                input.IsFromAdmin = true;
                return clientBl.RegisterClient(input);
            }
        }

        private static ApiResponseBase GetClients(ApiFilterfnClient filter, SessionIdentity identity, ILog log)
        {
            if (identity.IsAffiliate)
                return GetAffiliateClients(filter, identity, log);
            else
                return GetAgentClients(filter, identity, log);
        }

        private static ApiResponseBase GetAffiliateClients(ApiFilterfnClient filter, SessionIdentity identity, ILog log)
        {
            var input = filter.MapToFilterfnAffiliateClientInfo();
            input.AffiliateId = identity.Id;
            input.PartnerId = identity.PartnerId;
            using (var clientBl = new ClientBll(identity, log))
            using (var affiliateBl = new AffiliateService(clientBl))
            {
                if (filter.AffiliateReferralId.HasValue)
                    input.RefId = affiliateBl.GetReferralLinkById(filter.AffiliateReferralId.Value)?.RefId;
                var resp = clientBl.GetAffiliateClientsPagedModel(input);
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        resp.Count,
                        TotalDepositAmount = resp.Entities.Select(y => y.ConvertedTotalDepositAmount).DefaultIfEmpty(0).Sum(),
                        Entities = resp.Entities.Select(x => x.MapToApiAffiliateClient(identity.TimeZone)).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase GetAgentClients(ApiFilterfnClient filter, SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                using (var userBl = new UserBll(identity, log))
                {
                    var callerId = identity.Id;
                    var agent = CacheManager.GetUserById(identity.Id);
                    if (agent.Type == (int)UserTypes.AgentEmployee)
                    {
                        clientBl.CheckPermission(Constants.Permissions.ViewClient);
                        callerId = agent.ParentId.Value;
                    }
                    if (!userBl.GetSubAgents(callerId, null, null, false, string.Empty).Where(x => x.Type != (int)UserTypes.AgentEmployee).Any(x => x.Id == filter.AgentId))//??
                        filter.AgentId = callerId;
                    var resp = clientBl.GetAgentClients(filter.MapToFilterClientModel(), filter.AgentId.Value, filter.WithDownlines ?? false, filter.ClientId);
                    var agentIds = new List<int>();
                    foreach (var s in resp)
                    {
                        s.ParentsPath = new List<int>();
                        var parents = s.User.Path.Split('/').Where(x => !string.IsNullOrEmpty(x)).ToList();

                        foreach (var p in parents)
                        {
                            var pId = Convert.ToInt32(p);
                            s.ParentsPath.Add(pId);
                            if (!agentIds.Contains(pId))
                                agentIds.Add(pId);
                        }
                        var index = s.ParentsPath.IndexOf(filter.AgentId.Value);
                        s.ParentsPath = s.ParentsPath.Skip(index).ToList();
                    }
                    var commissions = userBl.GetAgentCommissions(agentIds);

                    var caller = CacheManager.GetUserById(callerId);
                    var r = resp.Select(x => x.MapTofnClientModelItem(identity.TimeZone, commissions, caller.Level ?? 0, agent.Id, log))
                                       .Where(x => (!filter.AllowDoubleCommission.HasValue || x.AllowDoubleCommission == filter.AllowDoubleCommission) &&
                                                    (string.IsNullOrEmpty(filter.AgentIdentifier) ||
                                                    (x.UserName.Contains(filter.AgentIdentifier) ||
                                                    (x.NickName != null && x.NickName.Contains(filter.AgentIdentifier)) ||
                                                     x.FirstName.Contains(filter.AgentIdentifier) ||
                                                     x.LastName.Contains(filter.AgentIdentifier)))
                                                   );
                    if (filter.State.HasValue)
                    {
                        var state = filter.State.Value;
                        if (filter.State == (int)UserStates.ForceBlock || filter.State == (int)UserStates.ForceBlockBySecurityCode)
                            state = (int)ClientStates.ForceBlock;
                        r = r.Where(x => x.State == state);
                    }
                    return new ApiResponseBase
                    {
                        ResponseObject = new
                        {
                            Count = r.Count(),
                            Entities = string.IsNullOrEmpty(filter.FieldNameToOrderBy) ? r.OrderBy(x => x.State == (int)UserStates.Disabled).ThenByDescending(x => x.UserName)
                                                                                          .Skip(filter.TakeCount * filter.SkipCount).Take(filter.TakeCount).ToList() :
                                                                                         r.Skip(filter.TakeCount * filter.SkipCount).Take(filter.TakeCount).ToList()
                        }
                    };
                }
            }
        }

        private static ApiResponseBase UpdateClientSettings(NewClientModel input, SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            if (user.Type == (int)UserTypes.AdminUser)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
            using (var userBl = new UserBll(identity, log))
            {
                using (var clientBl = new ClientBll(userBl))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        using (var transactionScope = CommonFunctions.CreateTransactionScope())
                        {
                            if (user.Type == (int)UserTypes.AgentEmployee)
                            {
                                user = CacheManager.GetUserById(user.ParentId.Value);
                                userBl.CheckPermission(AgentEmployeePermissions.FullControl.ToString());
                            }
                            var client = CacheManager.GetClientById(input.Id.Value);
                            if (client == null || !client.UserId.HasValue)
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);

                            var parent = CacheManager.GetUserById(client.UserId.Value);
                            int? parentState = client.State;
                            var ss = CacheManager.GetClientSettingByName(client.Id, "ParentState");
                            if (ss.NumericValue.HasValue)
                                parentState = Convert.ToInt32(ss.NumericValue.Value);
                            if (parentState == (int)ClientStates.Disabled || !parent.Path.Contains("/" + user.Id + "/"))
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
                            input.Group = client.CategoryId;
                            clientBl.ChangeClientDataFromAgent(input.MapToClientFields());
                            if (input.Closed.HasValue)
                            {
                                var newState = client.State;
                                if (input.Closed.Value)
                                    newState = (int)ClientStates.FullBlocked;
                                else if (client.State == (int)ClientStates.FullBlocked)
                                    newState = (int)ClientStates.Active;
                                clientBl.UpdateAgentClient(user.Id, client.Id, newState, string.Empty, documentBl);
                            }
                            if (input.CommissionPlan != null)
                            {
                                if (input.CommissionPlan.PositionTaking != null)
                                {
                                    foreach (var pt in input.CommissionPlan.PositionTaking)
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
                            }
                            var commissionSettings = JsonConvert.SerializeObject(input.CommissionPlan);
                            var commissionPlan = new AgentCommission
                            {
                                ProductId = Constants.PlatformProductId,
                                TurnoverPercent = commissionSettings,
                                ClientId = client.Id
                            };
                            userBl.UpdateMemberCommission(commissionPlan);
                            input.MaxCredit = input.LevelLimits.FirstOrDefault(x => x.Level == (int)AgentLevels.Member).Limit;
                            if (input.MaxCredit.HasValue)
                            {
                                var clientSetting = new ClientCustomSettings
                                {
                                    ClientId = client.Id,
                                    MaxCredit = input.MaxCredit.Value
                                };
                                clientBl.SaveClientSetting(clientSetting);

                                var parentBalance = userBl.GetUserBalance(user.Id);
                                var clientBalance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;

                                var clientCorrectionInput = new ClientCorrectionInput
                                {
                                    Amount = input.MaxCredit.Value - clientBalance,
                                    CurrencyId = client.CurrencyId,
                                    ClientId = client.Id
                                };
                                if (clientCorrectionInput.Amount > parentBalance.Balance)
                                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.LowBalance);

                                if (clientCorrectionInput.Amount > 0)
                                    clientBl.CreateDebitCorrectionOnClient(clientCorrectionInput, documentBl, false);
                                else if (clientCorrectionInput.Amount < 0)
                                {
                                    clientCorrectionInput.Amount = Math.Abs(clientCorrectionInput.Amount);
                                    clientBl.CreateCreditCorrectionOnClient(clientCorrectionInput, documentBl, false);
                                }
                            }
                            var c = userBl.GetAgentCommissionPlan(user.PartnerId, null, input.Id.Value, Constants.PlatformProductId, false).FirstOrDefault();
                            transactionScope.Complete();
                            CacheManager.RemoveClientFromCache(client.Id);
                            Helpers.Helpers.InvokeMessage("RemoveClient", client.Id);
                            
                            return new ApiResponseBase
                            {
                                ResponseObject = new
                                {
                                    State = CustomHelper.MapUserStateToClient.First(x => x.Value == parentState).Key,
                                    Commission = c.TurnoverPercent
                                }
                            };
                        }
                    }
                }
            }
        }

        private static ApiResponseBase UpdateAgentClient(ChangeObjectStateInput changeObjectStateInput, SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            if (user.Type == (int)UserTypes.AdminUser)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
            using (var clientBl = new ClientBll(identity, log))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                    if (isAgentEmploye)
                        clientBl.CheckPermission(Constants.Permissions.EditClient);
                    var res = clientBl.UpdateAgentClient(identity.Id, changeObjectStateInput.ObjectId, CustomHelper.MapUserStateToClient[changeObjectStateInput.State],
                                                         changeObjectStateInput.Password, documentBl).MapTofnClientModel(identity.TimeZone);
                    var adc = CacheManager.GetClientSettingByName(changeObjectStateInput.ObjectId, nameof(res.AllowDoubleCommission));
                    res.AllowDoubleCommission = Convert.ToBoolean(adc == null || adc.Id == 0 ? 0 : (adc.NumericValue ?? 0));
                    var ao = CacheManager.GetClientSettingByName(changeObjectStateInput.ObjectId, nameof(res.AllowOutright));
                    res.AllowOutright = Convert.ToBoolean(ao == null || ao.Id == 0 ? 0 : (ao.NumericValue ?? 0));

                    Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}",
                        Constants.CacheItems.ClientSettings, changeObjectStateInput.ObjectId, "PasswordChangedDate"));
                    Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientSettings,
                                      changeObjectStateInput.ObjectId, "ParentState"));
                    Helpers.Helpers.InvokeMessage("RemoveClient", changeObjectStateInput.ObjectId);
                    return new ApiResponseBase
                    {
                        ResponseObject = res
                    };
                }
            }
        }

        public static ApiResponseBase GetClientInfo(int clientId, SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                using (var userBl = new UserBll(clientBl))
                {
                    var user = userBl.GetUserById(identity.Id);
                    if (user == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                    var response = new ApiResponseBase
                    {
                        ResponseObject = clientBl.GetClientInfo(clientId, false).MapToClientInfoModel()
                    };
                    return response;
                }
            }
        }
        public static ApiResponseBase GetClientAccounts(int clientId, SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                var user = CacheManager.GetUserById(identity.Id);
                if (user == null)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                if (user.Type == (int)UserTypes.AgentEmployee)
                    clientBl.CheckPermission(Constants.Permissions.ViewClient);
                var client = CacheManager.GetClientById(clientId);
                if (client.UserId != identity.Id)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.DontHavePermission);

                var accounts = clientBl.GetClientAccounts(clientId, false).MapToFnAccountModels();
                return new ApiResponseBase
                {
                    ResponseObject = accounts
                };
            }
        }
        public static ApiResponseBase GetClientCorrections(ApiFilterClientCorrection filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                using (var userBl = new UserBll(reportBl))
                {
                    var user = userBl.GetUserById(identity.Id);
                    if (user == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                    var corrections = reportBl.GetClientCorrections(filter.MapToFilterCorrection(), false);

                    return new ApiResponseBase
                    {
                        ResponseObject = corrections.MapToApiClientCorrections(reportBl.GetUserIdentity().TimeZone)
                    };
                }
            }
        }

        private static ApiResponseBase CreateDebitCorrection(ClientCorrectionInput input, SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            if (user.Type == (int)UserTypes.AdminUser)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
            using (var clientBl = new ClientBll(identity, log))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    using (var userBl = new UserBll(clientBl))
                    {
                        var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                        if (isAgentEmploye)
                            userBl.CheckPermission(Constants.Permissions.CreateDebitCorrectionOnClient);
                        var client = CacheManager.GetClientById(input.ClientId);
                        if (client == null || client.UserId != (isAgentEmploye ? user.ParentId : user.Id))
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);

                        var result = clientBl.CreateDebitCorrectionOnClient(input, documentBl, false);
                        Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, client.Id));
                        return new ApiResponseBase
                        {
                            ResponseObject = result.MapToDocumentModel(clientBl.GetUserIdentity().TimeZone)
                        };
                    }
                }
            }
        }

        private static ApiResponseBase CreateCreditCorrection(ClientCorrectionInput input, SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            if (user.Type == (int)UserTypes.AdminUser)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
            using (var clientBl = new ClientBll(identity, log))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    using (var userBl = new UserBll(clientBl))
                    {
                        var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                        if (isAgentEmploye)
                            userBl.CheckPermission(Constants.Permissions.CreateCreditCorrectionOnClient);
                        var client = CacheManager.GetClientById(input.ClientId);
                        if (client == null || client.UserId != (isAgentEmploye ? user.ParentId : user.Id))
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);

                        var result = clientBl.CreateCreditCorrectionOnClient(input, documentBl, false);
                        Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, client.Id));
                        return new ApiResponseBase
                        {
                            ResponseObject = result.MapToDocumentModel(clientBl.GetUserIdentity().TimeZone)
                        };
                    }
                }
            }
        }

        public static ApiResponseBase ChangeClientOutrightState(ApiAgentSettings input, SessionIdentity identity, ILog log)
        {
            var agent = CacheManager.GetUserById(identity.Id);
            if (agent.Type == (int)UserTypes.AdminUser)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);

            using (var userBl = new UserBll(identity, log))
            {
                using (var clientBl = new ClientBll(userBl))
                {
                    var client = CacheManager.GetClientById(input.ObjectId);
                    if (client == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);

                    if (agent.Type == (int)UserTypes.AgentEmployee)
                    {
                        userBl.CheckPermission(AgentEmployeePermissions.FullControl.ToString());
                        agent = CacheManager.GetUserById(agent.ParentId.Value);
                    }
                    var parentAgent = CacheManager.GetUserById(client.UserId.Value);
                    if (parentAgent == null || !parentAgent.Path.Contains(string.Format("/{0}/", agent.Id)))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);
                    var agentSetting = CacheManager.GetUserSetting(parentAgent.Id);
                    if (!agentSetting.AllowOutright || agent.Id != client.UserId)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);

                    var clientSetting = new ClientCustomSettings
                    {
                        ClientId = input.ObjectId,
                        AllowOutright = input.AllowOutright
                    };

                    var res = clientBl.SaveClientSetting(clientSetting);
                    Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}",
                        Constants.CacheItems.ClientSettings, client.Id, nameof(clientSetting.AllowOutright)));

                    return new ApiResponseBase
                    {
                        ResponseObject = new
                        {
                            input.ObjectId,
                            res.AllowOutright
                        }
                    };
                }
            }
        }

        public static ApiResponseBase ChangeClientsDoubleCommissionState(List<ApiAgentSettings> inputList, SessionIdentity identity, ILog log)
        {
            var agent = CacheManager.GetUserById(identity.Id);
            if (agent.Type == (int)UserTypes.AdminUser)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
            using (var userBl = new UserBll(identity, log))
            {
                using (var clientBl = new ClientBll(userBl))
                {
                    if (agent.Type == (int)UserTypes.AgentEmployee)
                    {
                        userBl.CheckPermission(AgentEmployeePermissions.FullControl.ToString());
                        agent = CacheManager.GetUserById(agent.ParentId.Value);
                    }
                    var response = new List<ApiAgentSettings>();
                    foreach (var input in inputList)
                    {
                        var client = CacheManager.GetClientById(input.ObjectId);
                        if (client == null)
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);

                        var parentAgent = CacheManager.GetUserById(client.UserId.Value);
                        if (parentAgent == null || !parentAgent.Path.Contains(string.Format("/{0}/", agent.Id)))
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);
                        var agentSetting = CacheManager.GetUserSetting(parentAgent.Id);
                        if (!agentSetting.AllowDoubleCommission || agent.Id != client.UserId)
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);

                        var clientSetting = new ClientCustomSettings
                        {
                            ClientId = client.Id,
                            AllowDoubleCommission = input.AllowDoubleCommission
                        };
                        var res = clientBl.SaveClientSetting(clientSetting);
                        response.Add(new ApiAgentSettings { ObjectId = client.Id, AllowDoubleCommission = res.AllowDoubleCommission });
                        Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientSettings,
                            client.Id, nameof(clientSetting.AllowDoubleCommission)));
                    }
                    return new ApiResponseBase { ResponseObject = response };
                }
            }
        }

        private static ApiResponseBase FindClients(string clientIdentity, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                using (var clientBl = new ClientBll(identity, log))
                {
                    var user = CacheManager.GetUserById(identity.Id);
                    var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                    if (isAgentEmploye)
                        userBl.CheckPermission(Constants.Permissions.ViewUser);

                    return new ApiResponseBase
                    {
                        ResponseObject = clientBl.FindClients(clientIdentity, false, null).Select(x => x.MapTofnClientModel(identity.TimeZone)).ToList()
                    };
                }
            }
        }

        public static ApiResponseBase IsUserNameAvailable(ApiUserNameInput input, SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                var user = CacheManager.GetUserById(identity.Id);
                var partnerSetting = CacheManager.GetPartnerSettingByKey(user.PartnerId, Constants.PartnerKeys.IsUserNameGeneratable);
                if (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue != 0)
                {
                    if (input.Username.Length != 3 || input.Username == "000")
                        return new ApiResponseBase
                        {
                            ResponseObject = false
                        };
                    input.Username = UserBll.GenerateUserNamePrefix(user, (int)AgentLevels.Member, (int)UserTypes.MasterAgent) + input.Username;
                }
                return new ApiResponseBase
                {
                    ResponseObject = !clientBl.IsClientUserNameExists(input.Username, user.PartnerId)
                };
            }
        }
    }    
}