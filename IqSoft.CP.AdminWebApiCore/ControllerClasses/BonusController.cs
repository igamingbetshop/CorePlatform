﻿using IqSoft.CP.Common;
using IqSoft.CP.AdminWebApi.Models.BonusModels;
using Newtonsoft.Json;
using System.Linq;
using IqSoft.CP.AdminWebApi.Helpers;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using log4net;
using IqSoft.CP.AdminWebApi.Filters.Bonuses;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.DAL.Models.Cache;
using System;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.AdminWebApi.Models.ClientModels;
using System.Collections.Generic;
using IqSoft.CP.Common.Models.Bonus;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class BonusController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetBonuses":
                    return GetBonuses(JsonConvert.DeserializeObject<ApiFilterBonus>(request.RequestData), identity, log);
                case "GetBonusById":
                    return GetBonusById(JsonConvert.DeserializeObject<ApiFilterBonus>(request.RequestData), identity, log);
                case "GetBonusInfo":
                    return GetBonusInfo(Convert.ToInt32(request.RequestData), identity, log);
                case "CreateBonus":
                    return CreateBonus(JsonConvert.DeserializeObject<ApiBonus>(request.RequestData),
                        identity, log);
                case "UpdateBonus":
                    return UpdateBonus(JsonConvert.DeserializeObject<ApiBonus>(request.RequestData),
                        identity, log);
                case "SaveTriggerSetting":
                    return SaveTriggerSetting(JsonConvert.DeserializeObject<ApiTriggerSetting>(request.RequestData), identity, log);
                case "GetTriggerSettings":
                    return GetTriggerSettings(JsonConvert.DeserializeObject<FilterTriggerSetting>(request.RequestData), identity, log);
                case "GetTriggerSettingClients":
                    return GetTriggerSettingClients(Convert.ToInt32(request.RequestData), identity, log);
                case "SaveTriggerGroup":
                    return SaveTriggerGroup(JsonConvert.DeserializeObject<ApiTriggerGroup>(request.RequestData), identity, log);
                case "GetTriggerGroups":
                    return GetTriggerGroups(Convert.ToInt32(request.RequestData), identity, log);
                case "AddTriggerToGroup":
                    return AddTriggerToGroup(JsonConvert.DeserializeObject<TriggerGroupItem>(request.RequestData), identity, log);
                case "RemoveTriggerFromGroup":
                    return RemoveTriggerFromGroup(JsonConvert.DeserializeObject<TriggerGroupItem>(request.RequestData), identity, log);
                case "ClaimBonusForClients":
                    return ClaimBonusForClients(JsonConvert.DeserializeObject<ApiClientBonusInput>(request.RequestData), identity, log);
                case "CloneBonus":
                    return CloneBonus(Convert.ToInt32(request.RequestData), identity, log);
                case "CloneTriggerSetting":
                    return CloneTriggerSetting(Convert.ToInt32(request.RequestData), identity, log);
                case "SaveComplimentaryPointRate":
                    return SaveComplimentaryPointRate(JsonConvert.DeserializeObject<ApiComplimentaryPointRate>(request.RequestData), identity, log);
                case "GetComplimentaryPointRates":
                    return GetComplimentaryPointRates(JsonConvert.DeserializeObject<PartnerComplimentaryPointInput>(request.RequestData), identity, log);
                case "SaveJackpot":
                    return SaveJackpot(JsonConvert.DeserializeObject<ApiJackpot>(request.RequestData), identity, log);
                case "GetJackpots":
                    return GetJackpots(!string.IsNullOrEmpty(request.RequestData) ? Convert.ToInt32(request.RequestData) : (int?)null, identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase GetBonuses(ApiFilterBonus input, SessionIdentity identity, ILog log)
        {
            using var bonusBl = new BonusService(identity, log);
            BllClient client = null;
            if (input.ClientId.HasValue)
                client = CacheManager.GetClientById(input.ClientId.Value);
            int? partnerId = null;
            if (client != null)
                partnerId = client.PartnerId;
            if (partnerId == null && input.PartnerId.HasValue)
                partnerId = input.PartnerId;

            return new ApiResponseBase
            {
                ResponseObject = bonusBl.GetBonuses(partnerId, input.Type, input.IsActive).Select(x => x.MapToApiBonus(identity.TimeZone)).ToList()
            };
        }

        private static ApiResponseBase GetBonusById(ApiFilterBonus input, SessionIdentity identity, ILog log)
        {
            using var bonusBl = new BonusService(identity, log);
            BllClient client = null;
            if (input.ClientId.HasValue)
                client = CacheManager.GetClientById(input.ClientId.Value);
            int? partnerId = null;
            if (client != null)
                partnerId = client.PartnerId;
            return new ApiResponseBase
            {
                ResponseObject = bonusBl.GetBonusById(partnerId, input.BonusId ?? 0)?.MapToApiBonus(identity.TimeZone)
            };
        }

        private static ApiResponseBase GetBonusInfo(int bonusId, SessionIdentity identity, ILog log)
        {
            using var bonusBl = new BonusService(identity, log);
            return new ApiResponseBase
            {
                ResponseObject = bonusBl.GetBonusById(bonusId, true).MapToApiBonus(identity.TimeZone)
            };
        }

        private static ApiResponseBase CreateBonus(ApiBonus createBonusInput, SessionIdentity identity, ILog log)
        {
            if (createBonusInput.Products != null && createBonusInput.Products.Any(x => x.Percent < 0) ||
               (createBonusInput.Currencies != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), createBonusInput.Currencies.Type)) ||
               (createBonusInput.Languages != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), createBonusInput.Languages.Type)) ||
               (createBonusInput.Countries != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), createBonusInput.Countries.Type)) ||
               (createBonusInput.SegmentIds != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), createBonusInput.SegmentIds.Type)) ||
               (createBonusInput.PaymentSystemIds != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), createBonusInput.PaymentSystemIds.Type)))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
            using var bonusBl = new BonusService(identity, log);
            var input = createBonusInput.MapToBonus();
            if (createBonusInput.BonusTypeId == (int)BonusTypes.CampaignWagerSport ||
                createBonusInput.BonusTypeId == (int)BonusTypes.CampaignFreeBet)
            {
                if (createBonusInput.Conditions == null)
                    createBonusInput.Conditions = new BonusCondition();
                input.Condition = JsonConvert.SerializeObject(createBonusInput.Conditions,
                    new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
            }
            return new ApiResponseBase
            {
                ResponseObject = bonusBl.CreateBonus(input, createBonusInput.Percent).MapToApiBonus(identity.TimeZone)
            };
        }

        private static ApiResponseBase UpdateBonus(ApiBonus inp, SessionIdentity identity, ILog log)
        {
            if (inp.Products != null && inp.Products.Any(x => x.Percent < 0) ||
               (inp.Currencies != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), inp.Currencies.Type)) ||
               (inp.Languages != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), inp.Languages.Type)) ||
               (inp.Countries != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), inp.Countries.Type)) ||
               (inp.SegmentIds != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), inp.SegmentIds.Type)) ||
               (inp.PaymentSystemIds != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), inp.PaymentSystemIds.Type)))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
            using var bonusBl = new BonusService(identity, log);
            var input = inp.MapToBonus();
            if (inp.BonusTypeId == (int)BonusTypes.CampaignWagerSport ||
                inp.BonusTypeId == (int)BonusTypes.CampaignFreeBet)
            {
                if (inp.Conditions == null)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
                input.Condition = JsonConvert.SerializeObject(inp.Conditions,
                    new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
            }
            var bonus = bonusBl.UpdateBonus(input).MapToApiBonus(identity.TimeZone);
            CacheManager.RemoveBonusProducts(bonus.Id.Value);
            CacheManager.RemoveBonus(bonus.Id.Value);
            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.BonusProducts, bonus.Id));
            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.BonusInfo, bonus.Id));
            return new ApiResponseBase
            {
                ResponseObject = bonus
            };
        }

        public static ApiResponseBase SaveTriggerSetting(ApiTriggerSetting apiTriggerSetting, SessionIdentity identity, ILog log)
        {
            if (apiTriggerSetting.PaymentSystemIds != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), apiTriggerSetting.PaymentSystemIds.Type))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
            using (var bonusBl = new BonusService(identity, log))
            {
                var input = apiTriggerSetting.MapToTriggerSetting(identity.TimeZone);
                if (apiTriggerSetting.Type == (int)TriggerTypes.BetPlacement ||
                    apiTriggerSetting.Type == (int)TriggerTypes.BetSettlement ||
                    apiTriggerSetting.Type == (int)TriggerTypes.BetPlacementAndSettlement)
                {
                    if (apiTriggerSetting.Conditions == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
                    input.Condition = JsonConvert.SerializeObject(apiTriggerSetting.Conditions,
                        new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                }
                else if (apiTriggerSetting.Type == (int)TriggerTypes.NthDeposit)
                {
                    if (string.IsNullOrEmpty(apiTriggerSetting.Sequence) || !Int32.TryParse(apiTriggerSetting.Sequence, out int result))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
                    input.Condition = apiTriggerSetting.Sequence;
                }
                else if (apiTriggerSetting.Type == (int)TriggerTypes.ManualEvent && apiTriggerSetting.Activate.HasValue)
                {
                    input.Condition = apiTriggerSetting.Activate.Value ? ((int)ManualEvenStatuses.Accepted).ToString() : ((int)ManualEvenStatuses.Rejected).ToString();
                }
                var resp = bonusBl.SaveTriggerSetting(input, apiTriggerSetting.Activate);
                CacheManager.RemoveTriggerSetting(resp.Id);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.TriggerSettings, resp.Id));

                return new ApiResponseBase
                {
                    ResponseObject = resp.MapToApiTriggerSetting(identity.TimeZone)
                };
            }
        }

        public static ApiResponseBase GetTriggerSettingClients(int id, SessionIdentity identity, ILog log)
        {
            using var bonusBl = new BonusService(identity, log);
            var resp = bonusBl.GetTriggerSettingClients(id);
            return new ApiResponseBase
            {
                ResponseObject = resp
            };
        }

        public static ApiResponseBase GetTriggerSettings(FilterTriggerSetting input, SessionIdentity identity, ILog log)
        {
            using var bonusBl = new BonusService(identity, log);
            var resp = bonusBl.GetTriggerSettings(input.SkipCount, input.TakeCount, input.Id, input.PartnerId, input.BonusId, input.Status);
            return new ApiResponseBase
            {
                ResponseObject = new
                {
                    Count = resp.Count,
                    Entities = resp.Entities.Select(x => x.MapToApiTriggerSetting(identity.TimeZone)).OrderByDescending(x => x.Id).ToList()
                }
            };
        }

        public static ApiResponseBase SaveTriggerGroup(ApiTriggerGroup input, SessionIdentity identity, ILog log)
        {
            using var bonusBl = new BonusService(identity, log);
            var resp = new ApiResponseBase
            {
                ResponseObject = bonusBl.SaveTriggerGroup(input.MapToTriggerGroup()).MapToApiTriggerGroup()
            };
            CacheManager.RemoveBonus(input.BonusId);
            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.BonusInfo, input.BonusId));
            return resp;
        }

        public static ApiResponseBase GetTriggerGroups(int bonusId, SessionIdentity identity, ILog log)
        {
            using var bonusBl = new BonusService(identity, log);
            var r = bonusBl.GetTriggerGroups(bonusId);
            return new ApiResponseBase
            {
                ResponseObject = r.Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    Type = x.Type,
                    Priority = x.Priority,
                    TriggerSetting = x.TriggerSettings.Select(y => y.MapToApiTriggerSetting(identity.TimeZone, 0)).ToList()
                }).OrderBy(x => x.Priority).ToList()
            };
        }

        public static ApiResponseBase AddTriggerToGroup(TriggerGroupItem input, SessionIdentity identity, ILog log)
        {
            using var bonusBl = new BonusService(identity, log);
            var resp = bonusBl.AddTriggerSettingToGroup(input.TriggerGroupId, input.TriggerSettingId, input.Order, out int bonusId).
                                         MapToApiTriggerSetting(identity.TimeZone);

            CacheManager.RemoveBonus(bonusId);
            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.BonusInfo, bonusId));
            return new ApiResponseBase
            {
                ResponseObject = resp
            };
        }

        public static ApiResponseBase RemoveTriggerFromGroup(TriggerGroupItem input, SessionIdentity identity, ILog log)
        {
            using var bonusBl = new BonusService(identity, log);
            bonusBl.RemoveTriggerSettingFromGroup(input.TriggerGroupId, input.TriggerSettingId, out int bonusId);
            CacheManager.RemoveBonus(bonusId);
            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.BonusInfo, bonusId));
            return new ApiResponseBase();
        }

        public static ApiResponseBase ClaimBonusForClients(ApiClientBonusInput input, SessionIdentity identity, ILog log)
        {
            var clients = new List<string>();
            try
            {
                var byteArray = Convert.FromBase64String(input.ClientData);
                var data = System.Text.Encoding.UTF8.GetString(byteArray);
                clients = data.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            catch (Exception)
            {
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongConvertion);
            }
            using (var bonusService = new BonusService(identity, log))
            {
                using (var clientService = new ClientBll(identity, log))
                {
                    var bonus = bonusService.GetAvailableBonus(input.BonusSettingId, true);
                    if (!Constants.ClaimingBonusTypes.Contains(bonus.BonusType))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.BonusNotFound);
                    foreach (var c in clients)
                    {
                        var d = c.Split(',').ToList();
                        if (d.Count > 0 && Int32.TryParse(d[0], out int clientId))
                        {
                            var client = CacheManager.GetClientById(clientId);
                            if (client == null)
                                continue;
                            var clientSegmentsIds = new List<int>();
                            if (bonus.BonusSegmentSettings.Any())
                            {
                                var clientClasifications = CacheManager.GetClientClasifications(client.Id);
                                if (clientClasifications.Any())
                                    clientSegmentsIds = clientClasifications.Where(x => x.SegmentId.HasValue && x.ProductId == (int)Constants.PlatformProductId)
                                                                            .Select(x => x.SegmentId.Value).ToList();
                            }
                            if ((bonus.BonusSegmentSettings.Any() &&
                               (bonus.BonusSegmentSettings.Any(x => x.Type == (int)BonusSettingConditionTypes.InSet && !clientSegmentsIds.Contains(x.SegmentId)) ||
                                 bonus.BonusSegmentSettings.Any(x => x.Type == (int)BonusSettingConditionTypes.OutOfSet && clientSegmentsIds.Contains(x.SegmentId)))) ||
                                (bonus.BonusCountrySettings.Any() &&
                               (bonus.BonusCountrySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.CountryId != client.RegionId) ||
                                 bonus.BonusCountrySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet && y.CountryId == client.RegionId))) ||
                                (bonus.BonusCountrySettings.Any() &&
                               (bonus.BonusCurrencySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.CurrencyId != client.CurrencyId) ||
                                 bonus.BonusCurrencySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet && y.CurrencyId == client.CurrencyId))) ||
                                (bonus.BonusLanguageSettings.Any() &&
                               bonus.BonusLanguageSettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.LanguageId != client.LanguageId) &&
                                 bonus.BonusLanguageSettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet && y.LanguageId == client.LanguageId)))
                                continue;
                            var clientBonusItem = new ClientBonusItem
                            {
                                PartnerId = client.PartnerId,
                                BonusId = bonus.Id,
                                BonusType = bonus.BonusType,
                                ClientId = client.Id,
                                ClientUserName = client.UserName,
                                ClientCurrencyId = client.CurrencyId,
                                AccountTypeId = bonus.AccountTypeId.Value,
                                ReusingMaxCount = bonus.ReusingMaxCount,
                                IgnoreEligibility = bonus.IgnoreEligibility,
                                ValidForAwarding = bonus.ValidForAwarding == null ? (DateTime?)null :
                                    DateTime.Now.AddHours(bonus.ValidForAwarding.Value)
                            };
                            var reuseNumber = bonusService.GiveCompainToClient(clientBonusItem, out bool alreadyGiven).ReuseNumber;
                            CacheManager.RemoveClientNotAwardedCampaigns(client.Id);
                            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.NotAwardedCampaigns, client.Id));
                            if (d.Count > 1 && Int32.TryParse(d[1], out int triggerId))
                            {
                                decimal? sourceAmount = null;
                                if (d.Count > 2 && decimal.TryParse(d[2], out decimal newSourceAmount))
                                    sourceAmount = newSourceAmount;
                                var triggers = clientService.ChangeClientBonusTriggerManually(client.Id, triggerId, bonus.Id, reuseNumber, sourceAmount, (int)ClientBonusTriggerStatuses.Realised);
                            }
                        }
                    }
                }
            }
            return new ApiResponseBase();
        }

        public static ApiResponseBase CloneBonus(int bonusId, SessionIdentity identity, ILog log)
        {
            using var bonusBl = new BonusService(identity, log);
            return new ApiResponseBase
            {
                ResponseObject = bonusBl.CloneBonus(bonusId).MapToApiBonus(identity.TimeZone)
            };
        }

        public static ApiResponseBase CloneTriggerSetting(int triggerSettingId, SessionIdentity identity, ILog log)
        {
            using var bonusBl = new BonusService(identity, log);
            return new ApiResponseBase
            {
                ResponseObject = bonusBl.CloneTriggerSetting(triggerSettingId).MapToApiTriggerSetting(identity.TimeZone)
            };
        }

        public static ApiResponseBase SaveComplimentaryPointRate(ApiComplimentaryPointRate complimentaryCoinRate, SessionIdentity identity, ILog log)
        {
            using var bonusBl = new BonusService(identity, log);
            return new ApiResponseBase
            {
                ResponseObject = bonusBl.SaveComplimentaryPointRate(complimentaryCoinRate.MapToComplimentaryRate())
                                        .MapToApiComplimentaryRate(identity.TimeZone)
            };
        }

        public static ApiResponseBase GetComplimentaryPointRates(PartnerComplimentaryPointInput input, SessionIdentity identity, ILog log)
        {
            using var bonusBl = new BonusService(identity, log);
            return new ApiResponseBase
            {
                ResponseObject = bonusBl.GetComplimentaryPointRates(input.PartnerId, input.CurrencyId)
                                        .Select(x => x.MapToApiComplimentaryRate(identity.TimeZone)).ToList()
            };
        }

        public static ApiResponseBase SaveJackpot(ApiJackpot apiJackpot, SessionIdentity identity, ILog log)
        {
            using var bonusBl = new BonusService(identity, log);
            return new ApiResponseBase
            {
                ResponseObject = bonusBl.SaveJackpot(apiJackpot.MapToJackpot()).MapToApiJackpot(identity.TimeZone)
            };
        }

        public static ApiResponseBase GetJackpots(int? jackpotId, SessionIdentity identity, ILog log)
        {
            using var bonusBl = new BonusService(identity, log);
            return new ApiResponseBase
            {
                ResponseObject = bonusBl.GetJackpots(jackpotId)
                                        .Select(x => x.MapToApiJackpot(identity.TimeZone)).ToList()
            };
        }
    }
}