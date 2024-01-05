using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.AgentWebApi.Helpers;
using IqSoft.CP.AgentWebApi.Models;
using IqSoft.CP.AgentWebApi.Models.User;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.Common.Models.AgentModels;
using IqSoft.CP.Common.Helpers;

namespace IqSoft.CP.AgentWebApi.ControllerClasses
{
    public static class CommissionController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetCommissionPlan":
                    return GetCommissionPlan(JsonConvert.DeserializeObject<ApiAgentCommission>(request.RequestData), identity, log);
                case "UpdateCommissionPlan":
                    return UpdateCommissionPlan(JsonConvert.DeserializeObject<ApiAgentCommission>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase UpdateCommissionPlan(ApiAgentCommission apiCommissionInput, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                var user = CacheManager.GetUserById(identity.Id);
                if (!string.IsNullOrEmpty(apiCommissionInput.TurnoverPercent))
                {
                    if (!decimal.TryParse(apiCommissionInput.TurnoverPercent, out decimal turnoverPersent))
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
                }
                var resp = userBl.UpdateAgentCommission(apiCommissionInput.MapToAgentCommission());
                return new ApiResponseBase
                {
                    ResponseObject = resp == null ? new ApiAgentCommission() : resp.MapToApiAgentCommission()
                };
            }
        }

        private static ApiResponseBase GetCommissionPlan(ApiAgentCommission apiAgentCommission, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                using (var currencyBl = new CurrencyBll(identity, log))
                {
                    var user = CacheManager.GetUserById(identity.Id);
                    var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                    if (isAgentEmploye)
                    {
                        userBl.CheckPermission(Constants.Permissions.ViewUser);
                        user = CacheManager.GetUserById(user.ParentId.Value);
                    }
                    if (!apiAgentCommission.ClientId.HasValue && (!apiAgentCommission.AgentId.HasValue || apiAgentCommission.AgentId.Value == 0))
                        apiAgentCommission.AgentId = user.Id;
                    var partnerSetting = CacheManager.GetPartnerSettingByKey(user.PartnerId, Constants.PartnerKeys.IsUserNameGeneratable);
                    if (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue != 0)
                    {
                        var currency = user.CurrencyId;
                        if(user.Type == (int)UserTypes.AdminUser)
                        {
                            if (apiAgentCommission.AgentId != user.Id)
                            {
                                var agent = CacheManager.GetUserById(apiAgentCommission.AgentId.Value);
                                if (agent == null)
                                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                                currency = agent.CurrencyId;
                            }
                            else if (!string.IsNullOrEmpty(apiAgentCommission.CurrencyId))
                                currency = apiAgentCommission.CurrencyId;
                        }
                        var partnerCurrencySetting = currencyBl.GetPartnerCurrencies(user.PartnerId).FirstOrDefault(x => x.CurrencyId == currency);
                        
                        var c = userBl.GetAgentCommissionPlan(user.PartnerId, apiAgentCommission.AgentId, apiAgentCommission.ClientId, Constants.PlatformProductId, false).FirstOrDefault();
                        var currAgentCommission = c == null ? new AsianCommissionPlan(partnerCurrencySetting?.ClientMinBet) :
                                JsonConvert.DeserializeObject<AsianCommissionPlan>(c.TurnoverPercent);

                        if (apiAgentCommission.AgentId != user.Id)
                        {
                            var uc = userBl.GetAgentCommissionPlan(user.PartnerId, user.Id, apiAgentCommission.ClientId, Constants.PlatformProductId, false).FirstOrDefault();
                            var currUserCommission = uc == null ? new AsianCommissionPlan(partnerCurrencySetting?.ClientMinBet) :
                                JsonConvert.DeserializeObject<AsianCommissionPlan>(uc.TurnoverPercent);
                            foreach (var pt in currAgentCommission.PositionTaking)
                            {
                                var parPt = currUserCommission.PositionTaking.First(y => y.SportId == pt.SportId);
                                pt.MarketTypes.ForEach(x =>
                                {
                                    x.OwnerPercent = x.AgentPercent;
                                    x.AgentPercent = (user.Type == (int)UserTypes.AdminUser ? 1 :
                                                     parPt.MarketTypes.First(y => y.Id == x.Id).AgentPercent) - x.AgentPercent;
                                });
                            }
                            foreach (var bs in currAgentCommission.BetSettings)
                            {
                                var oldBs = currUserCommission.BetSettings.FirstOrDefault(x => x.Name == bs.Name);
                                if (oldBs == null)
                                {
                                    oldBs = new BetSetting();
                                }

                                bs.MinBetLimit = oldBs.MinBetLimit;
                                bs.MaxBetLimit = oldBs.MaxBetLimit;
                                bs.MaxPerMatchLimit = oldBs.MaxPerMatchLimit;
                                bs.ParentPreventBetting = oldBs.PreventBetting;
                            }
                            if (apiAgentCommission.ClientId == null)
                            {
                                for (int i = 0; i < currAgentCommission.Groups.Count; i++)
                                {
                                    currAgentCommission.Groups[i].ParentValue = currUserCommission.Groups[i].Value;
                                }
                            }
                            else
                            {
                                var cl = CacheManager.GetClientById(apiAgentCommission.ClientId.Value);
                                currAgentCommission.Groups[0].ParentValue = currUserCommission.Groups[cl.CategoryId % 10 - 1].Value;
                                currAgentCommission.Groups[1].ParentValue = currUserCommission.Groups[4].Value;
                                currAgentCommission.Groups[2].ParentValue = currUserCommission.Groups[5].Value;
                            }
                        }
                        else
                        {
                            if (apiAgentCommission.ClientGroup.HasValue)
                            {
                                currAgentCommission.Groups[0].ParentValue = currAgentCommission.Groups[apiAgentCommission.ClientGroup.Value % 10 - 1].Value;
                                currAgentCommission.Groups[1].ParentValue = currAgentCommission.Groups[4].Value;
                                currAgentCommission.Groups[2].ParentValue = currAgentCommission.Groups[5].Value;

                                currAgentCommission.Groups[0].Value = currAgentCommission.Groups[0].ParentValue ?? 0;
                                currAgentCommission.Groups[1].Value = currAgentCommission.Groups[1].ParentValue ?? 0;
                                currAgentCommission.Groups[2].Value = currAgentCommission.Groups[2].ParentValue ?? 0;

                                currAgentCommission.Groups = currAgentCommission.Groups.Take(3).ToList();
                            }
                            else
                            {
                                for (int i = 0; i < currAgentCommission.Groups.Count; i++)
                                {
                                    currAgentCommission.Groups[i].ParentValue = currAgentCommission.Groups[i].Value;
                                }
                            }

                            foreach (var bs in currAgentCommission.BetSettings)
                            {
                                bs.ParentPreventBetting = bs.PreventBetting;
                            }
                        }

                        if (apiAgentCommission.AgentId.HasValue)
                        {
                            var subAgent = CacheManager.GetUserById(apiAgentCommission.AgentId.Value);
                            if (subAgent == null || !subAgent.Path.Contains("/" + user.Id + "/"))
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
                            var subAgentSettings = CacheManager.GetUserSetting(subAgent.Id);
                            return new ApiResponseBase
                            {
                                ResponseObject = new
                                {
                                    subAgent.State,
                                    Commission = currAgentCommission,
                                    subAgentSettings?.CalculationPeriod,
                                    subAgentSettings?.AllowAutoPT
                                }
                            };
                        }
                        var client = CacheManager.GetClientById(apiAgentCommission.ClientId.Value);
                        var ss = CacheManager.GetClientSettingByName(client.Id, "ParentState");
                        var state = client.State;
                        if (ss.NumericValue.HasValue && CustomHelper.Greater((ClientStates)ss.NumericValue, (ClientStates)state))
                            state = Convert.ToInt32(ss.NumericValue.Value);
                        return new ApiResponseBase
                        {
                            ResponseObject = new
                            {
                                State = CustomHelper.MapUserStateToClient.First(x => x.Value == client.State).Key,
                                Commission = currAgentCommission
                            }
                        };
                    }
                    return new ApiResponseBase
                    {
                        ResponseObject = userBl.GetAgentCommissionPlan(user.PartnerId, apiAgentCommission.AgentId, null, null, false).MapToApiAgentCommissions()
                    };
                }
            }
        }
    }
}