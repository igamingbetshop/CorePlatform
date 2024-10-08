using IqSoft.CP.Common;
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
                case "GetClientAvailableBonus":
                    return GetClientAvailableBonus(JsonConvert.DeserializeObject<ApiFilterBonus>(request.RequestData), identity, log);
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
                case "DeleteTriggerSetting":
                    return DeleteTriggerSetting(Convert.ToInt32(request.RequestData), identity, log);
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
                case "RemoveTriggerGroup":
                    return RemoveTriggerGroup(JsonConvert.DeserializeObject<TriggerGroupItem>(request.RequestData), identity, log);
                case "ClaimBonusForClients":
                    return ClaimBonusForClients(JsonConvert.DeserializeObject<ApiClientBonusInput>(request.RequestData), identity, log);
                case "CloneBonus":
                    return CloneBonus(Convert.ToInt32(request.RequestData), identity, log);
                case "DeleteBonus":
                    return DeleteBonus(Convert.ToInt32(request.RequestData), identity, log);
                case "CloneTriggerSetting":
                    return CloneTriggerSetting(Convert.ToInt32(request.RequestData), identity, log);
                case "SaveComplimentaryPointRate":
                    return SaveComplimentaryPointRate(JsonConvert.DeserializeObject<ApiComplimentaryPointRate>(request.RequestData), identity, log);
                case "GetComplimentaryPointRates":
                    return GetComplimentaryPointRates(JsonConvert.DeserializeObject<PartnerComplimentaryPointInput>(request.RequestData), identity, log);
                case "SaveJackpot":
                    return SaveJackpot(JsonConvert.DeserializeObject<ApiJackpot>(request.RequestData), identity, log);
                case "GetJackpots":
                    return GetJackpots(JsonConvert.DeserializeObject<ApiJackpot>(request.RequestData), identity, log);
                case "GetTournamentLeaderboard":
                    return GetTournamentLeaderboard(JsonConvert.DeserializeObject<ApiFilterBonus>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        #region Bonuses

        private static ApiResponseBase GetBonuses(ApiFilterBonus input, SessionIdentity identity, ILog log)
        {
            using (var bonusBl = new BonusService(identity, log))
            {
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
                    ResponseObject = bonusBl.GetBonuses(partnerId, input.Type, input.Status).Select(x => x.MapToApiBonus(identity.TimeZone)).ToList()
                };
            }
        }

        private static ApiResponseBase GetClientAvailableBonus(ApiFilterBonus input, SessionIdentity identity, ILog log)
        {
            using (var bonusService = new BonusService(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = bonusService.GetClientAvailableBonuses(input.ClientId ?? 0,input.Type, true).Select(x => x.MapToApiBonus(identity.TimeZone)).ToList()
                };
            }
        }

        private static ApiResponseBase GetBonusById(ApiFilterBonus input, SessionIdentity identity, ILog log)
        {
            using (var bonusBl = new BonusService(identity, log))
            {
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
        }

        private static ApiResponseBase GetBonusInfo(int bonusId, SessionIdentity identity, ILog log)
        {
            using (var bonusBl = new BonusService(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = bonusBl.GetBonusById(bonusId, true).MapToApiBonus(identity.TimeZone)
                };
            }
        }

        private static ApiResponseBase CreateBonus(ApiBonus createBonusInput, SessionIdentity identity, ILog log)
        {
            if (createBonusInput.Products != null && createBonusInput.Products.Any(x => x.Percent < 0) ||
               (createBonusInput.Currencies != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), createBonusInput.Currencies.Type)) ||
               (createBonusInput.Languages != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), createBonusInput.Languages.Type)) ||
               (createBonusInput.Countries != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), createBonusInput.Countries.Type)) ||
               (createBonusInput.SegmentIds != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), createBonusInput.SegmentIds.Type)) ||
               (createBonusInput.PaymentSystemIds != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), createBonusInput.PaymentSystemIds.Type)) ||
               !Enum.IsDefined(typeof(BonusStatuses), createBonusInput.Status))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
            using (var bonusBl = new BonusService(identity, log))
            {
                var input = createBonusInput.MapToBonus(identity.TimeZone);
                if (createBonusInput.BonusTypeId == (int)BonusTypes.CampaignWagerSport ||
                    createBonusInput.BonusTypeId == (int)BonusTypes.CampaignWagerCasino ||
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
        }

        private static ApiResponseBase UpdateBonus(ApiBonus inp, SessionIdentity identity, ILog log)
        {
            if (inp.Products != null && inp.Products.Any(x => x.Percent < 0) ||
               (inp.Currencies != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), inp.Currencies.Type)) ||
               (inp.Languages != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), inp.Languages.Type)) ||
               (inp.Countries != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), inp.Countries.Type)) ||
               (inp.SegmentIds != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), inp.SegmentIds.Type)) ||
               (inp.PaymentSystemIds != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), inp.PaymentSystemIds.Type)) ||
               !Enum.IsDefined(typeof(BonusStatuses), inp.Status))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);

            using (var bonusBl = new BonusService(identity, log))
            {
                var input = inp.MapToBonus(identity.TimeZone);
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
        }

        private static ApiResponseBase DeleteBonus(int bonusId, SessionIdentity identity, ILog log)
        {

            using (var bonusBl = new BonusService(identity, log))
            {
                var resp = bonusBl.DeleteBonus(bonusId);
                CacheManager.RemoveBonus(resp.Id);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.BonusInfo, bonusId));

                return new ApiResponseBase
                {
                    ResponseObject = resp.MapToApiBonus(identity.TimeZone)
                };
            }
        }

        private static ApiResponseBase ClaimBonusForClients(ApiClientBonusInput input, SessionIdentity identity, ILog log)
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
                    if (!Constants.ClaimingBonusTypes.Contains(bonus.Type))
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
                                var clientClassifications = CacheManager.GetClientClassifications(client.Id);
                                if (clientClassifications.Any())
                                    clientSegmentsIds = clientClassifications.Where(x => x.SegmentId.HasValue && x.ProductId == Constants.PlatformProductId)
                                                                            .Select(x => x.SegmentId.Value).ToList();
                            }
                            if ((bonus.BonusSegmentSettings.Any() &&
                                (bonus.BonusSegmentSettings.Any(x => x.Type == (int)BonusSettingConditionTypes.InSet && !clientSegmentsIds.Contains(x.SegmentId)) ||
                                 bonus.BonusSegmentSettings.Any(x => x.Type == (int)BonusSettingConditionTypes.OutOfSet && clientSegmentsIds.Contains(x.SegmentId)))) ||
                                (bonus.BonusCountrySettings.Any() &&
                                (bonus.BonusCountrySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.CountryId != (client.CountryId ?? client.RegionId)) ||
                                 bonus.BonusCountrySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet && y.CountryId == (client.CountryId ?? client.RegionId)))) ||
                                (bonus.BonusCurrencySettings.Any() &&
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
                                Type = bonus.Type,
                                ClientId = client.Id,
                                ClientUserName = client.UserName,
                                ClientCurrencyId = client.CurrencyId,
                                FinalAccountTypeId = bonus.FinalAccountTypeId.Value,
                                ReusingMaxCount = bonus.ReusingMaxCount,
                                WinAccountTypeId = bonus.WinAccountTypeId,
                                ValidForAwarding = bonus.ValidForAwarding == null ? (DateTime?)null :
                                    DateTime.Now.AddHours(bonus.ValidForAwarding.Value)
                            };
                            var reuseNumber = bonusService.GiveCompainToClient(clientBonusItem, out int awardedStatus).ReuseNumber;
                            if (awardedStatus == 0)
                            {
                                CacheManager.RemoveClientNotAwardedCampaigns(client.Id);
                                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.NotAwardedCampaigns, client.Id));
                                if (d.Count > 1 && Int32.TryParse(d[1], out int triggerId))
                                {
                                    decimal? sourceAmount = null;
                                    if (d.Count > 2 && decimal.TryParse(d[2], out decimal newSourceAmount))
                                        sourceAmount = newSourceAmount;
                                    clientService.ChangeClientBonusTriggerManually(client.Id, triggerId, bonus.Id, reuseNumber,
                                        sourceAmount, (int)ClientBonusTriggerStatuses.Realised);
                                }
                            }
                        }
                    }
                }
            }
            return new ApiResponseBase();
        }

        private static ApiResponseBase CloneBonus(int bonusId, SessionIdentity identity, ILog log)
        {
            using (var bonusBl = new BonusService(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = bonusBl.CloneBonus(bonusId).MapToApiBonus(identity.TimeZone)
                };
            }
        }

        private static ApiResponseBase GetTournamentLeaderboard(ApiFilterBonus input, SessionIdentity identity, ILog log)
        {
            using (var bonusBl = new BonusService(identity, log))
            {
                var bonus = bonusBl.GetBonusById(null, input.BonusId.Value);
                if (bonus == null)
                    throw BaseBll.CreateException(input.LanguageId, Constants.Errors.PartnerNotFound);

                var response = CacheManager.GetTournamentLeaderboard(input.BonusId.Value);
                var resp = response.Select(x => x.ToApiLeaderboardItem(identity.CurrencyId)).OrderByDescending(x => x.Points).ThenBy(x => x.Name).ToList();
                foreach (var item in resp)
                {
                    item.Order = resp.IndexOf(item) + 1;
                }
                return new ApiResponseBase
                {
                    ResponseObject = resp
                };
            }
        }

        #endregion

        #region Triggers

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

        public static ApiResponseBase GetTriggerSettings(FilterTriggerSetting input, SessionIdentity identity, ILog log)
        {
            using (var bonusBl = new BonusService(identity, log))
            {
                var resp = bonusBl.GetTriggerSettings(input.SkipCount, input.TakeCount, input.Id, input.PartnerId, input.BonusId, input.Status);
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        resp.Count,
                        Entities = resp.Entities.Select(x => x.MapToApiTriggerSetting(identity.TimeZone)).OrderByDescending(x => x.Id).ToList()
                    }
                };
            }
        }

        public static ApiResponseBase DeleteTriggerSetting(int triggerSettingId, SessionIdentity identity, ILog log)
        {

            using (var bonusBl = new BonusService(identity, log))
            {
                var resp = bonusBl.DeleteTriggerSetting(triggerSettingId);
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
            using (var bonusBl = new BonusService(identity, log))
            {
                var resp = bonusBl.GetTriggerSettingClients(id);
                return new ApiResponseBase
                {
                    ResponseObject = resp
                };
            }
        }

        public static ApiResponseBase SaveTriggerGroup(ApiTriggerGroup input, SessionIdentity identity, ILog log)
        {
            using (var bonusBl = new BonusService(identity, log))
            {
                var resp = new ApiResponseBase
                {
                    ResponseObject = bonusBl.SaveTriggerGroup(input.MapToTriggerGroup()).MapToApiTriggerGroup()
                };
                CacheManager.RemoveBonus(input.BonusId);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.BonusInfo, input.BonusId));
                return resp;
            }
        }

        public static ApiResponseBase GetTriggerGroups(int bonusId, SessionIdentity identity, ILog log)
        {
            using (var bonusBl = new BonusService(identity, log))
            {
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
        }

        public static ApiResponseBase AddTriggerToGroup(TriggerGroupItem input, SessionIdentity identity, ILog log)
        {
            using (var bonusBl = new BonusService(identity, log))
            {
                var resp = bonusBl.AddTriggerSettingToGroup(input.TriggerGroupId, input.TriggerSettingId, input.Order, out int bonusId).
                                             MapToApiTriggerSetting(identity.TimeZone);

                CacheManager.RemoveBonus(bonusId);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.BonusInfo, bonusId));
                return new ApiResponseBase
                {
                    ResponseObject = resp
                };
            }
        }

        public static ApiResponseBase RemoveTriggerGroup(TriggerGroupItem input, SessionIdentity identity, ILog log)
        {
            using (var bonusBl = new BonusService(identity, log))
            {
                var bonusId = bonusBl.RemoveTriggerGroup(input.TriggerGroupId);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.BonusInfo, bonusId));
                return new ApiResponseBase();
            }
        }

        public static ApiResponseBase RemoveTriggerFromGroup(TriggerGroupItem input, SessionIdentity identity, ILog log)
        {
            using (var bonusBl = new BonusService(identity, log))
            {
                bonusBl.RemoveTriggerSettingFromGroup(input.TriggerGroupId, input.TriggerSettingId, out int bonusId);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.BonusInfo, bonusId));
                return new ApiResponseBase();
            }
        }

        public static ApiResponseBase CloneTriggerSetting(int triggerSettingId, SessionIdentity identity, ILog log)
        {
            using (var bonusBl = new BonusService(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = bonusBl.CloneTriggerSetting(triggerSettingId).MapToApiTriggerSetting(identity.TimeZone)
                };
            }
        }

        #endregion

        #region Jackpots

        public static ApiResponseBase SaveJackpot(ApiJackpot apiJackpot, SessionIdentity identity, ILog log)
        {
            using (var bonusBl = new BonusService(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = bonusBl.SaveJackpot(apiJackpot.MapToJackpot(identity.TimeZone)).MapToApiJackpot(identity.TimeZone)
                };
            }
        }

        public static ApiResponseBase GetJackpots(ApiJackpot apiJackpot, SessionIdentity identity, ILog log)
        {
            using (var bonusBl = new BonusService(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = bonusBl.GetJackpots(apiJackpot.Id)
                                            .Select(x => x.MapToApiJackpot(identity.TimeZone)).ToList()
                };
            }
        }

        #endregion

        #region CompPoints

        public static ApiResponseBase SaveComplimentaryPointRate(ApiComplimentaryPointRate complimentaryCoinRate, SessionIdentity identity, ILog log)
        {
            using (var bonusBl = new BonusService(identity, log))
            {
                if (complimentaryCoinRate.Rate < 0)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongOperationAmount);
                var dbComplimentaryRate = bonusBl.SaveComplimentaryPointRate(complimentaryCoinRate.MapToComplimentaryRate())
                                            .MapToApiComplimentaryRate(identity.TimeZone);
                Helpers.Helpers.InvokeMessage("RemoveComplimentaryPointRate", dbComplimentaryRate.PartnerId, dbComplimentaryRate.ProductId, dbComplimentaryRate.CurrencyId);
                return new ApiResponseBase
                {
                    ResponseObject = dbComplimentaryRate
                };
            }
        }

        public static ApiResponseBase GetComplimentaryPointRates(PartnerComplimentaryPointInput input, SessionIdentity identity, ILog log)
        {
            using (var bonusBl = new BonusService(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = bonusBl.GetComplimentaryPointRates(input.PartnerId, input.CurrencyId)
                                            .Select(x => x.MapToApiComplimentaryRate(identity.TimeZone)).ToList()
                };
            }
        }

        #endregion
    }
}