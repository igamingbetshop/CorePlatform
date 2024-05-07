using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Helpers;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.Bonus;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Bonuses;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DataWarehouse;
using log4net;
using Newtonsoft.Json;
using static IqSoft.CP.Common.Constants;
using Client = IqSoft.CP.DAL.Client;
using Document = IqSoft.CP.DAL.Document;
using ClientBonu = IqSoft.CP.DAL.ClientBonu;
using AffiliateReferral = IqSoft.CP.DAL.AffiliateReferral;
using Bonu = IqSoft.CP.DAL.Bonu;

namespace IqSoft.CP.BLL.Services
{
    public class BonusService : PermissionBll, IBonusService
    {
        #region Constructors

        public BonusService(SessionIdentity identity, ILog log, int? timeout = null)
            : base(identity, log, timeout)
        {

        }

        public BonusService(BaseBll baseBl)
            : base(baseBl)
        {

        }

        #endregion

        public Bonu CreateBonus(Bonu bonus, decimal? percent)
        {
            using (var ts = CommonFunctions.CreateTransactionScope())
            {
                var currentTime = DateTime.UtcNow;
                var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.EditBonuses
                });
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });

                if (!bonusAccess.HaveAccessForAllObjects ||
                    (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != bonus.PartnerId)))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

                if ((bonus.Type == (int)BonusTypes.SignupRealBonus && !CheckBonusInfoValidity(bonus.Info)) ||
                    !Enum.IsDefined(typeof(BonusStatuses), bonus.Status))
                    throw CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                else if (bonus.Type == (int)BonusTypes.AffiliateBonus && (bonus.MinAmount == null ||
                     Db.Bonus.Any(x => x.Type == (int)BonusTypes.AffiliateBonus && x.Status == (int)BonusStatuses.Active &&
                     x.FinishTime >= currentTime && x.PartnerId == bonus.PartnerId)))
                    throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
                else if ((bonus.Type == (int)BonusTypes.CampaignWagerCasino || bonus.Type == (int)BonusTypes.CampaignWagerSport) &&
                    (!bonus.TurnoverCount.HasValue || bonus.TurnoverCount <= 0 || !bonus.ValidForAwarding.HasValue ||
                    bonus.ValidForAwarding <= 0 || !bonus.ValidForSpending.HasValue || bonus.ValidForSpending <= 0))
                    throw CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                else if (bonus.Type == (int)BonusTypes.CampaignFreeSpin &&
                        (!bonus.ValidForSpending.HasValue || bonus.ValidForSpending <= 0))
                    throw CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                else if (bonus.Type == (int)BonusTypes.CampaignFreeBet)
                {
                    bonus.AllowSplit = bonus.AllowSplit ?? false;
                    bonus.RefundRollbacked = bonus.RefundRollbacked ?? false;
                    if (!bonus.MinAmount.HasValue || bonus.MinAmount.Value <= 0)
                        throw CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                    bonus.TurnoverCount = 1;
                }
                else if (bonus.Type == (int)BonusTypes.CashBackBonus && !bonus.AutoApproveMaxAmount.HasValue)
                    throw CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                else if (bonus.Type == (int)BonusTypes.AggregatedFreeSpin && 
                    Db.Bonus.FirstOrDefault(x => x.Type == (int)BonusTypes.AggregatedFreeSpin && x.Status == (int)BonusStatuses.Active && x.PartnerId == bonus.PartnerId) != null)
                    throw CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientAlreadyHasActiveBonus);
                if (bonus.MinAmount.HasValue && bonus.MaxAmount.HasValue && bonus.MinAmount > bonus.MaxAmount)
                    throw CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                bonus.Translation = CreateTranslation(new fnTranslation
                {
                    ObjectTypeId = (int)ObjectTypes.Bonus,
                    Text = bonus.Name,
                    LanguageId = Constants.DefaultLanguageId
                });
                bonus.TotalReceiversCount = 0;
                bonus.TotalGranted = 0;
                bonus.CreationTime = currentTime;
                bonus.LastUpdateTime = currentTime;
                Db.Bonus.Add(bonus);
                Db.SaveChanges();

                if (bonus.Type == (int)BonusTypes.AffiliateBonus ||
                    bonus.Type == (int)BonusTypes.CashBackBonus)
                    Db.BonusProducts.Add(new BonusProduct { BonusId = bonus.Id, ProductId = Constants.PlatformProductId, Percent = percent ?? 0 });
                else if (bonus.Type == (int)BonusTypes.SignupRealBonus ||
                         bonus.Type == (int)BonusTypes.CampaignCash ||
                         bonus.Type == (int)BonusTypes.CampaignWagerCasino)
                    Db.BonusProducts.Add(new BonusProduct { BonusId = bonus.Id, ProductId = Constants.PlatformProductId, Percent = 100 });
                else if (bonus.Type == (int)BonusTypes.CampaignWagerSport ||
                         bonus.Type == (int)BonusTypes.CampaignFreeBet)
                    Db.BonusProducts.Add(new BonusProduct { BonusId = bonus.Id, ProductId = Constants.SportsbookProductId, Percent = 100 });
                else if (bonus.Type == (int)BonusTypes.CampaignFreeSpin)
                    Db.BonusProducts.Add(new BonusProduct { BonusId = bonus.Id, ProductId = Constants.PlatformProductId });
                Db.SaveChanges();
                ts.Complete();
            }
            return bonus;
        }

        public Bonu CloneBonus(int bonusId)
        {
            using (var contentBll = new ContentBll(this))
            {
                var dbBonus = Db.Bonus.Include(x => x.Translation.TranslationEntries)
                                      .Include(x => x.BonusSegmentSettings)
                                      .Include(x => x.BonusCountrySettings)
                                      .Include(x => x.BonusCurrencySettings)
                                      .Include(x => x.BonusLanguageSettings)
                                      .Include(x => x.BonusProducts)
                                      .Include(x => x.BonusPaymentSystemSettings)
                                      .Include(x => x.AmountCurrencySettings)
                                      .Include(x => x.TriggerGroups.Select(y => y.TriggerGroupSettings))
                                      .FirstOrDefault(x => x.Id == bonusId) ??
                    throw CreateException(LanguageId, Constants.Errors.BonusNotFound);
                var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.EditBonuses
                });
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });

                if (!bonusAccess.HaveAccessForAllObjects ||
                    (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != dbBonus.PartnerId)))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
                var currentTime = DateTime.UtcNow;
                if (dbBonus.Type == (int)BonusTypes.AggregatedFreeSpin && dbBonus.Status == (int)BonusStatuses.Active)
                    throw CreateException(LanguageId, Constants.Errors.ClientAlreadyHasActiveBonus);
                var name = dbBonus.Name + "_" + ((long)currentTime.Year * 10000000000 + (long)currentTime.Month * 100000000 +
                    (long)currentTime.Day * 1000000 + (long)currentTime.Hour * 10000 + (long)currentTime.Minute * 100 + currentTime.Second);
                var newBonus = new Bonu
                {
                    Name = name,
                    Description = dbBonus.Description,
                    PartnerId = dbBonus.PartnerId,
                    FinalAccountTypeId = dbBonus.FinalAccountTypeId,
                    Status = dbBonus.Status,
                    StartTime = dbBonus.StartTime,
                    FinishTime = dbBonus.FinishTime,
                    LastExecutionTime = currentTime,
                    Period = dbBonus.Period,
                    Type = dbBonus.Type,
                    Info = dbBonus.Info,
                    TurnoverCount = dbBonus.TurnoverCount,
                    MinAmount = dbBonus.MinAmount,
                    MaxAmount = dbBonus.MaxAmount,
                    Sequence = dbBonus.Sequence,
                    Priority = dbBonus.Priority,
                    WinAccountTypeId = dbBonus.WinAccountTypeId,
                    ValidForAwarding = dbBonus.ValidForAwarding,
                    ValidForSpending = dbBonus.ValidForSpending,
                    ReusingMaxCount = dbBonus.ReusingMaxCount,
                    ResetOnWithdraw = dbBonus.ResetOnWithdraw,
                    CreationTime = currentTime,
                    LastUpdateTime = currentTime,
                    AllowSplit = dbBonus.AllowSplit,
                    RefundRollbacked = dbBonus.RefundRollbacked,
                    Condition = dbBonus.Condition,
                    MaxGranted = dbBonus.MaxGranted,
                    MaxReceiversCount = dbBonus.MaxReceiversCount,
                    LinkedBonusId = dbBonus.LinkedBonusId,
                    AutoApproveMaxAmount = dbBonus.AutoApproveMaxAmount,
                    Translation = CreateTranslation(new fnTranslation
                    {
                        ObjectTypeId = (int)ObjectTypes.Bonus,
                        Text = dbBonus.Name,
                        LanguageId = Constants.DefaultLanguageId
                    }),
                    FreezeBonusBalance = dbBonus.FreezeBonusBalance,
                    Regularity = dbBonus.Regularity,
                    DayOfWeek = dbBonus.DayOfWeek,
                    ReusingMaxCountInPeriod = dbBonus.ReusingMaxCountInPeriod,
                    TotalGranted = 0,
                    TotalReceiversCount = 0
                };
                newBonus.BonusSegmentSettings = new List<BonusSegmentSetting>();
                dbBonus.BonusSegmentSettings.ToList().ForEach(x => newBonus.BonusSegmentSettings.Add(
                    new BonusSegmentSetting { BonusId = newBonus.Id, SegmentId = x.SegmentId, Type = x.Type }));
                newBonus.BonusCountrySettings = new List<BonusCountrySetting>();
                dbBonus.BonusCountrySettings.ToList().ForEach(x => newBonus.BonusCountrySettings.Add(
                    new BonusCountrySetting { BonusId = newBonus.Id, CountryId = x.CountryId, Type = x.Type }));
                newBonus.BonusCurrencySettings = new List<BonusCurrencySetting>();
                dbBonus.BonusCurrencySettings.ToList().ForEach(x => newBonus.BonusCurrencySettings.Add(
                    new BonusCurrencySetting { BonusId = newBonus.Id, CurrencyId = x.CurrencyId, Type = x.Type }));
                newBonus.BonusLanguageSettings = new List<BonusLanguageSetting>();
                dbBonus.BonusLanguageSettings.ToList().ForEach(x => newBonus.BonusLanguageSettings.Add(
                    new BonusLanguageSetting { BonusId = newBonus.Id, LanguageId = x.LanguageId, Type = x.Type }));
                newBonus.BonusPaymentSystemSettings = new List<BonusPaymentSystemSetting>();
                dbBonus.BonusPaymentSystemSettings.ToList().ForEach(x => newBonus.BonusPaymentSystemSettings.Add(
                    new BonusPaymentSystemSetting { BonusId = newBonus.Id, PaymentSystemId = x.PaymentSystemId, Type = x.Type }));
                newBonus.AmountCurrencySettings = new List<AmountCurrencySetting>();
                dbBonus.AmountCurrencySettings.ToList().ForEach(x => newBonus.AmountCurrencySettings.Add(
                    new AmountCurrencySetting
                    {
                        BonusId = newBonus.Id,
                        CurrencyId = x.CurrencyId,
                        TriggerId = x.TriggerId,
                        MinAmount = x.MinAmount,
                        MaxAmount = x.MaxAmount,
                        UpToAmount=x.UpToAmount
                    }));

                newBonus.BonusProducts = new List<BonusProduct>();
                dbBonus.BonusProducts.ToList().ForEach(x => newBonus.BonusProducts.Add(
                    new BonusProduct {
                        BonusId = newBonus.Id, 
                        ProductId = x.ProductId,
                        Percent = x.Percent,
                        Count = x.Count,
                        Lines = x.Lines,
                        Coins = x.Coins,
                        CoinValue = x.CoinValue,
                        BetValueLevel = x.BetValueLevel
                    }));

                Db.Bonus.Add(newBonus);

                Db.SaveChanges();
                dbBonus.Translation.TranslationEntries.Select(x =>
                    new fnTranslation
                    {
                        LanguageId = x.LanguageId,
                        ObjectTypeId = (int)ObjectTypes.Banner,
                        Text = x.Text,
                        TranslationId = newBonus.TranslationId.Value
                    }
                ).ToList().ForEach(x => contentBll.SaveTranslation(x));

                foreach (var tg in dbBonus.TriggerGroups)
                {
                    var newTg = new TriggerGroup
                    {
                        BonusId = newBonus.Id,
                        Name = tg.Name,
                        Type = tg.Type,
                        Priority = tg.Priority
                    };
                    foreach (var tgs in tg.TriggerGroupSettings)
                    {
                        newTg.TriggerGroupSettings.Add(new TriggerGroupSetting
                        {
                            SettingId = tgs.SettingId,
                            Order = tgs.Order,
                            TriggerGroup = newTg
                        });
                    }
                    Db.TriggerGroups.Add(newTg);
                }
                Db.SaveChanges();

                return newBonus;
            }
        }

        private bool CheckBonusInfoValidity(string info)
        {
            bool isValid = false;
            try
            {
                decimal[] prizeAmounts = info.Split(',').Select(x => Convert.ToDecimal(x)).ToArray();
                foreach (var amount in prizeAmounts)
                    if (amount < 0)
                        return isValid;
                isValid = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return isValid;
        }

        public List<fnBonus> GetBonuses(int? partnerId, int? type, int? status)
        {
            var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBonuses
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            if (!bonusAccess.HaveAccessForAllObjects ||
                (partnerId.HasValue && !partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != partnerId.Value)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var query = Db.fn_Bonus(LanguageId).Where(x => x.Status != (int)BonusStatuses.Deleted);

            if (type != null)
                query = query.Where(x => x.Type == type.Value);

            if (partnerId.HasValue)
                query = query.Where(x => x.PartnerId == partnerId.Value);
            if (status.HasValue)
                query = query.Where(x => x.Status == status.Value);
            if (!partnerAccess.HaveAccessForAllObjects)
                query = query.Where(x => partnerAccess.AccessibleObjects.Contains(x.PartnerId));

            return query.OrderByDescending(x => x.Id).ToList();
        }

        public Bonu GetBonusById(int? partnerId, int bonusId)
        {
            var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBonuses
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            if (!bonusAccess.HaveAccessForAllObjects ||
                (partnerId.HasValue && !partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != partnerId.Value)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            return Db.Bonus.Include(x => x.BonusProducts).FirstOrDefault(x => x.Id == bonusId);
        }

        public List<BonusProduct> GetBonusProducts(FilterBonusProduct filter)
        {
            var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBonuses
            });
            var checkProducts = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewProduct,
                ObjectTypeId = ObjectTypes.Product
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<BonusProduct>>
            {
                new CheckPermissionOutput<BonusProduct>
                {
                    AccessibleObjects = checkProducts.AccessibleObjects,
                    HaveAccessForAllObjects = checkProducts.HaveAccessForAllObjects,
                    Filter = x=> checkProducts.AccessibleObjects.Contains(x.ProductId)
                },
                new CheckPermissionOutput<BonusProduct>
                {
                    AccessibleObjects = bonusAccess.AccessibleObjects,
                    HaveAccessForAllObjects = bonusAccess.HaveAccessForAllObjects,
                    Filter = x=> bonusAccess.AccessibleObjects.Contains(x.BonusId)
                }
            };
            return filter.FilterObjects(Db.BonusProducts).ToList();
        }
        public Bonu UpdateBonus(Bonu bon)
        {
            var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditBonuses
            });
            if (!bonusAccess.HaveAccessForAllObjects)
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            if (bon.MinAmount.HasValue && bon.MaxAmount.HasValue && bon.MinAmount > bon.MaxAmount)
                throw CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);

            var dbBonus = Db.Bonus.Include(x => x.BonusLanguageSettings)
                                  .Include(x => x.BonusCurrencySettings)
                                  .Include(x => x.BonusCountrySettings)
                                  .Include(x => x.BonusSegmentSettings)
                                  .Include(x => x.BonusPaymentSystemSettings)
                                  .FirstOrDefault(x => x.Id == bon.Id) ??
                throw CreateException(LanguageId, Constants.Errors.BonusNotFound);
            var bonusProducts = Db.BonusProducts.Where(x => x.BonusId == dbBonus.Id).ToList();
            var oldValue = JsonConvert.SerializeObject(dbBonus.ToBonusInfo());
            if (dbBonus.Type == (int)BonusTypes.AggregatedFreeSpin)
            {
                if (bon.Status == (int)BonusStatuses.Active &&
                    Db.Bonus.FirstOrDefault(x => x.Id != dbBonus.Id && x.Type == (int)BonusTypes.AggregatedFreeSpin && x.Status == (int)BonusStatuses.Active) != null)
                    throw CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientAlreadyHasActiveBonus);
                if (bon.BonusProducts != null)
                    throw CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
            }

            if (bon.BonusProducts == null)
            {
                if (((dbBonus.Type == (int)BonusTypes.CampaignWagerCasino || dbBonus.Type == (int)BonusTypes.CampaignWagerSport) &&
                (!bon.ValidForAwarding.HasValue || bon.ValidForAwarding <= 0 || !bon.ValidForSpending.HasValue || bon.ValidForSpending <= 0)) ||
                 (dbBonus.Type == (int)BonusTypes.CashBackBonus && !bon.AutoApproveMaxAmount.HasValue) ||
                 (dbBonus.Type == (int)BonusTypes.CampaignFreeSpin && (!bon.ValidForSpending.HasValue || bon.ValidForSpending <= 0)))
                    throw CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                if (string.IsNullOrEmpty(bon.Name))
                    dbBonus.Name = bon.Name;
                dbBonus.Status = bon.Status;
                dbBonus.Description = bon.Description;
                dbBonus.Priority = bon.Priority;
                dbBonus.WinAccountTypeId = bon.WinAccountTypeId;
                dbBonus.ValidForAwarding = bon.ValidForAwarding;
                dbBonus.ValidForSpending = bon.ValidForSpending;
                dbBonus.ReusingMaxCount = bon.ReusingMaxCount;
                dbBonus.ResetOnWithdraw = bon.ResetOnWithdraw;
                dbBonus.MaxGranted = bon.MaxGranted;
                dbBonus.MaxReceiversCount = bon.MaxReceiversCount;
                dbBonus.LinkedBonusId = bon.LinkedBonusId;
                dbBonus.AutoApproveMaxAmount = bon.AutoApproveMaxAmount;
                dbBonus.TurnoverCount = bon.TurnoverCount;
                if (bon.StartTime != DateTime.MinValue)
                    dbBonus.StartTime = bon.StartTime;
                if (bon.FinishTime != DateTime.MinValue)
                    dbBonus.FinishTime = bon.FinishTime;
                dbBonus.MinAmount = bon.MinAmount;
                dbBonus.MaxAmount = bon.MaxAmount;
                if (!string.IsNullOrEmpty(bon.Condition))
                    dbBonus.Condition = bon.Condition;
                if (bon.Sequence.HasValue)
                    dbBonus.Sequence = bon.Sequence;
                dbBonus.LastUpdateTime = GetServerDate();
                dbBonus.FreezeBonusBalance = bon.FreezeBonusBalance;
                dbBonus.Regularity = bon.Regularity;
                dbBonus.DayOfWeek = bon.DayOfWeek;
                dbBonus.ReusingMaxCountInPeriod = bon.ReusingMaxCountInPeriod;

                if (bon.BonusLanguageSettings == null || !bon.BonusLanguageSettings.Any())
                    Db.BonusLanguageSettings.Where(x => x.BonusId == dbBonus.Id).DeleteFromQuery();
                else
                {
                    var type = bon.BonusLanguageSettings.First().Type;
                    var languages = bon.BonusLanguageSettings.Select(x => x.LanguageId).ToList();
                    Db.BonusLanguageSettings.Where(x => x.BonusId == dbBonus.Id && (x.Type != type || !languages.Contains(x.LanguageId))).DeleteFromQuery();
                    var dbLang = Db.BonusLanguageSettings.Where(x => x.BonusId == dbBonus.Id).Select(x => x.LanguageId).ToList();
                    languages.RemoveAll(x => dbLang.Contains(x));
                    foreach (var l in languages)
                    {
                        Db.BonusLanguageSettings.Add(new BonusLanguageSetting { BonusId = dbBonus.Id, LanguageId = l, Type = type });
                    }
                }

                if (bon.BonusCurrencySettings == null || !bon.BonusCurrencySettings.Any())
                    Db.BonusCurrencySettings.Where(x => x.BonusId == dbBonus.Id).DeleteFromQuery();
                else
                {
                    var type = bon.BonusCurrencySettings.First().Type;
                    var currencies = bon.BonusCurrencySettings.Select(x => x.CurrencyId).ToList();
                    Db.BonusCurrencySettings.Where(x => x.BonusId == dbBonus.Id && (x.Type != type || !currencies.Contains(x.CurrencyId))).DeleteFromQuery();
                    var dbCurr = Db.BonusCurrencySettings.Where(x => x.BonusId == dbBonus.Id).Select(x => x.CurrencyId).ToList();
                    currencies.RemoveAll(x => dbCurr.Contains(x));
                    foreach (var c in currencies)
                        Db.BonusCurrencySettings.Add(new BonusCurrencySetting { BonusId = dbBonus.Id, CurrencyId = c, Type = type });
                }

                if (bon.BonusCountrySettings == null || !bon.BonusCountrySettings.Any())
                    Db.BonusCountrySettings.Where(x => x.BonusId == dbBonus.Id).DeleteFromQuery();
                else
                {
                    var type = bon.BonusCountrySettings.First().Type;
                    var countries = bon.BonusCountrySettings.Select(x => x.CountryId).ToList();
                    Db.BonusCountrySettings.Where(x => x.BonusId == dbBonus.Id && (x.Type != type || !countries.Contains(x.CountryId))).DeleteFromQuery();
                    var dbCountries = Db.BonusCountrySettings.Where(x => x.BonusId == dbBonus.Id).Select(x => x.CountryId).ToList();
                    countries.RemoveAll(x => dbCountries.Contains(x));
                    foreach (var c in countries)
                        Db.BonusCountrySettings.Add(new BonusCountrySetting { BonusId = dbBonus.Id, CountryId = c, Type = type });
                }

                if (bon.BonusSegmentSettings == null || !bon.BonusSegmentSettings.Any())
                    Db.BonusSegmentSettings.Where(x => x.BonusId == dbBonus.Id).DeleteFromQuery();
                else
                {
                    var type = bon.BonusSegmentSettings.First().Type;
                    var segments = bon.BonusSegmentSettings.Select(x => x.SegmentId).ToList();
                    Db.BonusSegmentSettings.Where(x => x.BonusId == dbBonus.Id && (x.Type != type || !segments.Contains(x.SegmentId))).DeleteFromQuery();

                    var dbSegments = Db.BonusSegmentSettings.Where(x => x.BonusId == dbBonus.Id).Select(x => x.SegmentId).ToList();
                    segments.RemoveAll(x => dbSegments.Contains(x));
                    foreach (var c in segments)
                        Db.BonusSegmentSettings.Add(new BonusSegmentSetting { BonusId = dbBonus.Id, SegmentId = c, Type = type });
                }

                if (bon.BonusPaymentSystemSettings == null || !bon.BonusPaymentSystemSettings.Any())
                    Db.BonusPaymentSystemSettings.Where(x => x.BonusId == dbBonus.Id).DeleteFromQuery();
                else
                {
                    var type = bon.BonusPaymentSystemSettings.First().Type;
                    var paymentSystems = bon.BonusPaymentSystemSettings.Select(x => x.PaymentSystemId).ToList();
                    Db.BonusPaymentSystemSettings.Where(x => x.BonusId == dbBonus.Id && (x.Type != type || !paymentSystems.Contains(x.PaymentSystemId)))
                                                 .DeleteFromQuery();
                    var dbPaymentSystems = Db.BonusPaymentSystemSettings.Where(x => x.BonusId == dbBonus.Id).Select(x => x.PaymentSystemId).ToList();
                    paymentSystems.RemoveAll(x => dbPaymentSystems.Contains(x));
                    foreach (var ps in paymentSystems)
                        Db.BonusPaymentSystemSettings.Add(new BonusPaymentSystemSetting { BonusId = dbBonus.Id, PaymentSystemId = ps, Type = type });
                }

                if (bon.AmountCurrencySettings == null || !bon.AmountCurrencySettings.Any())
                    Db.AmountCurrencySettings.Where(x => x.BonusId == dbBonus.Id).DeleteFromQuery();
                else
                {
                    var currencyIds = bon.AmountCurrencySettings.Select(x => x.CurrencyId).ToList();
                    Db.AmountCurrencySettings.Where(x => x.BonusId == dbBonus.Id && !currencyIds.Contains(x.CurrencyId)).DeleteFromQuery();

                    var dbAmountSettings = Db.AmountCurrencySettings.Where(x => x.BonusId == dbBonus.Id).Select(x => x.CurrencyId).ToList();

                    foreach (var cs in bon.AmountCurrencySettings)
                    {
                        if (!dbAmountSettings.Contains(cs.CurrencyId))
                        {
                            Db.AmountCurrencySettings.Add(new AmountCurrencySetting
                            {
                                CurrencyId = cs.CurrencyId,
                                BonusId = dbBonus.Id,
                                MinAmount = cs.MinAmount,
                                MaxAmount = cs.MaxAmount,
                                UpToAmount = cs.UpToAmount
                            });
                        }
                        else
                        {
                            Db.AmountCurrencySettings.Where(x => x.BonusId == dbBonus.Id && x.CurrencyId == cs.CurrencyId).UpdateFromQuery(x => new AmountCurrencySetting
                            {
                                MinAmount = x.MinAmount,
                                MaxAmount = x.MaxAmount,
                                UpToAmount = x.UpToAmount
                            });
                        }
                    }
                }

                if (dbBonus.Type == (int)BonusTypes.AffiliateBonus ||
                    dbBonus.Type == (int)BonusTypes.SignupRealBonus)
                {
                    var per = bon.BonusProducts?.FirstOrDefault(x => x.ProductId == Constants.PlatformProductId)?.Percent ?? 0;
                    var p = bonusProducts.FirstOrDefault(x => x.ProductId == Constants.PlatformProductId);
                    if (p != null)
                        p.Percent = per;
                    else
                        Db.BonusProducts.Add(new BonusProduct
                        {
                            BonusId = dbBonus.Id,
                            ProductId = Constants.PlatformProductId,
                            Percent = per
                        });
                }
                else if (dbBonus.Type == (int)BonusTypes.CampaignFreeBet)
                {
                    dbBonus.AllowSplit = bon.AllowSplit;
                    dbBonus.RefundRollbacked = bon.RefundRollbacked;
                }
                else if (dbBonus.Type == (int)BonusTypes.CampaignWagerCasino || dbBonus.Type == (int)BonusTypes.CampaignWagerSport)
                {
                    dbBonus.RefundRollbacked = bon.RefundRollbacked;
                    dbBonus.Info = bon.Info;
                }
            }
            else
            {
                if (dbBonus.Type == (int)BonusTypes.CampaignFreeSpin || dbBonus.Type == (int)BonusTypes.CampaignWagerCasino)
                {
                    var productIds = bon.BonusProducts.Where(x => x.Count > 0 ||
                    x.Lines.HasValue || x.Coins.HasValue || x.CoinValue.HasValue || x.BetValueLevel.HasValue).Select(x => x.ProductId);
                    if (Db.Products.Any(x => productIds.Contains(x.Id) && x.Id != (int)Constants.PlatformProductId &&
                        (!x.GameProviderId.HasValue || !x.FreeSpinSupport.HasValue || !x.FreeSpinSupport.Value)))
                        throw CreateException(LanguageId, Constants.Errors.UnavailableFreespin);
                }
                foreach (var bp in bon.BonusProducts)
                {
                    var p = bonusProducts.FirstOrDefault(x => x.Id == bp.Id);                        
                    if (p != null)
                    {
                        if ((!bp.Percent.HasValue || bp.Percent == -1) && dbBonus.Type != (int)BonusTypes.CampaignFreeSpin)
                        {
                            if (bp.ProductId == Constants.PlatformProductId)
                                p.Percent = 0;
                            else
                                Db.BonusProducts.Where(x => x.Id == bp.Id).DeleteFromQuery();
                        }
                        else
                        {
                            p.Percent = bp.Percent;
                            if (bp.ProductId != Constants.PlatformProductId)
                            {
                                p.Count = bp.Count;
                                p.Lines = bp.Lines;
                                p.Coins = bp.Coins;
                                p.CoinValue = bp.CoinValue;
                                p.BetValueLevel = bp.BetValueLevel;
                            }
                        }
                    }
                    else if (bp.Percent >= 0 ||
                        ((dbBonus.Type == (int)BonusTypes.CampaignFreeSpin || dbBonus.Type == (int)BonusTypes.CampaignWagerCasino) &&
                        (bp.Count.HasValue || bp.Lines.HasValue || bp.Coins.HasValue || bp.CoinValue.HasValue)))
                    {
                        Db.BonusProducts.Add(new BonusProduct
                        {
                            BonusId = dbBonus.Id,
                            ProductId = bp.ProductId,
                            Percent = bp.Percent,
                            Count = bp.Count,
                            Lines = bp.Lines,
                            Coins = bp.Coins,
                            CoinValue = bp.CoinValue,
                            BetValueLevel = bp.BetValueLevel
                        });
                    }
                }
            }
            SaveChangesWithHistory((int)ObjectTypes.Bonus, dbBonus.Id, oldValue);
            return Db.Bonus.Include(x => x.BonusProducts).Include(x => x.AmountCurrencySettings).FirstOrDefault(x => x.Id == bon.Id);
        }

        public Bonu UpdateBonusExternalId(Bonu bon)
        {
            var dbBonus = Db.Bonus.FirstOrDefault(x => x.Id == bon.Id);
            if (dbBonus == null)
                throw CreateException(LanguageId, Constants.Errors.BonusNotFound);
            dbBonus.Sequence = bon.Sequence;
            dbBonus.Info = bon.Info;
            dbBonus.LastUpdateTime = GetServerDate();
            Db.SaveChanges();
            return Db.Bonus.Include(x => x.BonusProducts).FirstOrDefault(x => x.Id == bon.Id);
        }

        public TriggerSetting SaveTriggerSetting(TriggerSetting triggerSetting, bool? activate)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditBonuses
            });
            if (!bonusAccess.HaveAccessForAllObjects ||
               (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != triggerSetting.PartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            if (!triggerSetting.Status.HasValue)
                triggerSetting.Status = (int)TriggerStatuses.Active;
            else if (!Enum.IsDefined(typeof(TriggerStatuses), triggerSetting.Status))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            var currentTime = GetServerDate();

            switch (triggerSetting.Type)
            {
                case (int)TriggerTypes.CampainLinkCode:
                    if (string.IsNullOrEmpty(triggerSetting.BonusSettingCodes) || !Int32.TryParse(triggerSetting.BonusSettingCodes, out int bonusId))
                        throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

                    var bonusTypes = new List<int> { (int)BonusTypes.CampaignWagerCasino, (int)BonusTypes.CampaignWagerSport };
                    var activBonuses = Db.Bonus.Where(x => x.Id == bonusId && x.PartnerId == triggerSetting.PartnerId &&
                                                           x.Status == (int)BonusStatuses.Active && bonusTypes.Contains(x.Type) && x.Info == "1" &&
                                                           x.StartTime < currentTime && x.FinishTime > currentTime).FirstOrDefault();
                    if (activBonuses == null)
                        throw CreateException(LanguageId, Constants.Errors.BonusNotFound);
                    break;
                case (int)TriggerTypes.PromotionalCode:
                case (int)TriggerTypes.SignupCode:
                    if (string.IsNullOrEmpty(triggerSetting.BonusSettingCodes))
                        throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
                    if (!Db.PromoCodes.Any(x => x.PartnerId == triggerSetting.PartnerId && x.Code == triggerSetting.BonusSettingCodes))
                    {
                        Db.PromoCodes.Add(new PromoCode
                        {
                            PartnerId = triggerSetting.PartnerId,
                            Code = triggerSetting.BonusSettingCodes,
                            Type = triggerSetting.Type == (int)TriggerTypes.PromotionalCode ? (int)PromoCodeType.CampainActivationCode : (int)PromoCodeType.RegistrationCode,
                            State = (int)PromoCodesState.Active
                        });
                    }
                    break;
                case (int)TriggerTypes.NthDeposit:
                case (int)TriggerTypes.AnyDeposit:
                case (int)TriggerTypes.CompPointSpend:
                    if (triggerSetting.Percent <= 0)
                        throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
                    break;
                case (int)TriggerTypes.DailyDeposit:
                    if (triggerSetting.Percent < 0 || triggerSetting.MinBetCount < 1)
                        throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
                    break;
                case (int)TriggerTypes.SignUp:
                    if (triggerSetting.MinAmount <= 0)
                        throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
                    break;
                case (int)TriggerTypes.SegmentChange:
                    if (!triggerSetting.SegmentId.HasValue)
                        throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
                    break;
                default:
                    break;
            }
            if (triggerSetting.TriggerProductSettings == null)
            {
                if (!Enum.IsDefined(typeof(TriggerTypes), triggerSetting.Type))
                    throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

                var dbTriggerSetting = Db.TriggerSettings.Include(x => x.BonusPaymentSystemSettings).FirstOrDefault(x => x.Id == triggerSetting.Id);
                if (dbTriggerSetting == null)
                {
                    dbTriggerSetting = new TriggerSetting
                    {
                        Translation = CreateTranslation(new fnTranslation
                        {
                            ObjectTypeId = (int)ObjectTypes.Trigger,
                            Text = triggerSetting.Name,
                            LanguageId = Constants.DefaultLanguageId
                        }),
                        Name = triggerSetting.Name,
                        Description = triggerSetting.Description,
                        Type = triggerSetting.Type,
                        StartTime = triggerSetting.StartTime,
                        FinishTime = triggerSetting.FinishTime,
                        Percent = triggerSetting.Percent,
                        BonusSettingCodes = triggerSetting.BonusSettingCodes,
                        PartnerId = triggerSetting.PartnerId,
                        MinAmount = triggerSetting.MinAmount,
                        MaxAmount = triggerSetting.MaxAmount,
                        MinBetCount = triggerSetting.MinBetCount,
                        SegmentId = triggerSetting.SegmentId,
                        DayOfWeek = triggerSetting.DayOfWeek,
                        Condition = triggerSetting.Condition,
                        Status = triggerSetting.Status,
                        CreationTime = currentTime,
                        LastUpdateTime = currentTime,
                        UpToAmount = triggerSetting.UpToAmount,
                        ConsiderBonusBets = triggerSetting.ConsiderBonusBets
                    };
                    Db.TriggerSettings.Add(dbTriggerSetting);
                    Db.SaveChanges();
                    if (triggerSetting.BonusPaymentSystemSettings != null)
                    {
                        triggerSetting.BonusPaymentSystemSettings.ToList().ForEach(x => Db.BonusPaymentSystemSettings.Add(new BonusPaymentSystemSetting
                        {
                            TriggerId = dbTriggerSetting.Id,
                            PaymentSystemId = x.PaymentSystemId,
                            Type = x.Type
                        }));
                        Db.SaveChanges();
                    }
                    if(triggerSetting.AmountCurrencySettings != null)
                    {
                        triggerSetting.AmountCurrencySettings.ToList().ForEach(x => Db.AmountCurrencySettings.Add(new AmountCurrencySetting
                        {
                            CurrencyId = x.CurrencyId,
                            TriggerId = x.TriggerId,
                            MinAmount = x.MinAmount,
                            MaxAmount = x.MaxAmount,
                            UpToAmount = x.UpToAmount
                        }));
                        Db.SaveChanges();
                    }

                    triggerSetting.Id = dbTriggerSetting.Id;
                }
                else
                {
                    if(triggerSetting.Status != (int)TriggerStatuses.Active &&
                        Db.Bonus.Any(x => x.Status == (int)BonusStatuses.Active && x.StartTime <= currentTime && x.FinishTime > currentTime &&
                                          x.TriggerGroups.Any(t => t.TriggerGroupSettings.Any(g => g.SettingId == dbTriggerSetting.Id))))
                        throw CreateException(LanguageId, Constants.Errors.NotAllowed);

                    if (triggerSetting.Type != dbTriggerSetting.Type)
                        throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

                    var oldvalue = JsonConvert.SerializeObject(dbTriggerSetting.ToTriggerInfo());
                    triggerSetting.PartnerId = dbTriggerSetting.PartnerId;
                    triggerSetting.TranslationId = dbTriggerSetting.TranslationId;
                    triggerSetting.CreationTime = dbTriggerSetting.CreationTime;
                    triggerSetting.LastUpdateTime = currentTime;
                    Db.Entry(dbTriggerSetting).CurrentValues.SetValues(triggerSetting);
                    SaveChangesWithHistory((int)ObjectTypes.Trigger, dbTriggerSetting.Id, oldvalue);

                    if (triggerSetting.BonusPaymentSystemSettings != null)
                    {
                        var paymentSystems = triggerSetting.BonusPaymentSystemSettings.Select(x => x.PaymentSystemId).ToList();
                        Db.BonusPaymentSystemSettings.Where(x => x.TriggerId == dbTriggerSetting.Id && !paymentSystems.Contains(x.PaymentSystemId)).DeleteFromQuery();
                        var dbPaymentSystems = Db.BonusPaymentSystemSettings.Where(x => x.TriggerId == dbTriggerSetting.Id).Select(x => x.PaymentSystemId).ToList();
                        paymentSystems.RemoveAll(x => dbPaymentSystems.Contains(x));
                        foreach (var ps in paymentSystems)
                            Db.BonusPaymentSystemSettings.Add(new BonusPaymentSystemSetting
                            {
                                TriggerId = dbTriggerSetting.Id,
                                PaymentSystemId = ps,
                                Type = triggerSetting.BonusPaymentSystemSettings.FirstOrDefault()?.Type ?? 1
                            });
                        Db.SaveChanges();
                    }
                    if (triggerSetting.AmountCurrencySettings != null)
                    {
                        var currencyIds = triggerSetting.AmountCurrencySettings.Select(x => x.CurrencyId).ToList();
                        Db.AmountCurrencySettings.Where(x => x.TriggerId == dbTriggerSetting.Id && !currencyIds.Contains(x.CurrencyId)).DeleteFromQuery();
                        var dbAmountCurrencies = Db.AmountCurrencySettings.Where(x => x.TriggerId == dbTriggerSetting.Id).Select(x => x.CurrencyId).ToList();

                        foreach (var cs in triggerSetting.AmountCurrencySettings)
                        {
                            if (!dbAmountCurrencies.Contains(cs.CurrencyId))
                                Db.AmountCurrencySettings.Add(new AmountCurrencySetting
                                {
                                    CurrencyId = cs.CurrencyId,
                                    TriggerId = cs.TriggerId,
                                    MinAmount = cs.MinAmount,
                                    MaxAmount = cs.MaxAmount,
                                    UpToAmount = cs.UpToAmount
                                });
                            else
                                Db.AmountCurrencySettings.Where(x => x.CurrencyId == cs.CurrencyId && x.TriggerId == dbTriggerSetting.Id).UpdateFromQuery(x => new AmountCurrencySetting
                                {
                                    MinAmount = cs.MinAmount,
                                    MaxAmount = cs.MaxAmount,
                                    UpToAmount = cs.UpToAmount
                                });
                        }
                        Db.SaveChanges();
                    }
                }
                if (triggerSetting.Type == (int)TriggerTypes.ManualEvent && activate.HasValue && activate.Value)
                {
                    var bonuses = Db.Bonus.Where(x => x.Status == (int)BonusStatuses.Active && 
                        x.TriggerGroups.Any(y => y.TriggerGroupSettings.Any(z => z.SettingId == triggerSetting.Id))).Select(x => x.Id).ToList();
                    foreach (var b in bonuses)
                    {
                        Db.ClientBonusTriggers.Add(
                            new ClientBonusTrigger
                            {
                                TriggerSetting = dbTriggerSetting,
                                BonusId = b,
                                CreationTime = DateTime.UtcNow
                            });
                    }

                    Db.SaveChanges();
                }
            }
            else
            {
                var triggerProducts = Db.TriggerProductSettings.Where(x => x.TriggerSettingId == triggerSetting.Id).ToList();
                foreach (var bp in triggerSetting.TriggerProductSettings)
                {
                    var p = triggerProducts.FirstOrDefault(x => x.ProductId == bp.ProductId);
                    if (p != null)
                    {
                        if (bp.Percent == -1)
                            Db.TriggerProductSettings.Where(x => x.Id == bp.Id).DeleteFromQuery();
                        else
                            p.Percent = bp.Percent;
                    }
                    else if (bp.Percent != -1)
                        Db.TriggerProductSettings.Add(new TriggerProductSetting
                        {
                            TriggerSettingId = triggerSetting.Id,
                            ProductId = bp.ProductId,
                            Percent = bp.Percent
                        });
                    Db.SaveChanges();
                }
            }

            var res = Db.TriggerSettings.Include(x => x.TriggerProductSettings).Include(x => x.BonusPaymentSystemSettings).
                Include(x => x.AmountCurrencySettings).FirstOrDefault(x => x.Id == triggerSetting.Id);
            return res;
        }

        public TriggerSetting CloneTriggerSetting(int triggerSettingId)
        {
            var dbTriggerSetting = Db.TriggerSettings.Include(x => x.BonusPaymentSystemSettings).FirstOrDefault(x => x.Id == triggerSettingId);
            if (dbTriggerSetting == null)
                throw CreateException(LanguageId, Constants.Errors.TriggerSettingNotFound);

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditBonuses
            });
            if (!bonusAccess.HaveAccessForAllObjects ||
               (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != dbTriggerSetting.PartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var currentTime = DateTime.UtcNow;
            var name = dbTriggerSetting.Name + "_" + ((long)currentTime.Year * 10000000000 + (long)currentTime.Month * 100000000 +
                (long)currentTime.Day * 1000000 + (long)currentTime.Hour * 10000 + (long)currentTime.Minute * 100 + currentTime.Second);
            var newTriggerSetting = new TriggerSetting
            {
                Translation = CreateTranslation(new fnTranslation
                {
                    ObjectTypeId = (int)ObjectTypes.Trigger,
                    Text = dbTriggerSetting.Name,
                    LanguageId = Constants.DefaultLanguageId
                }),
                Name = name,
                Description = dbTriggerSetting.Description,
                Type = dbTriggerSetting.Type,
                StartTime = dbTriggerSetting.StartTime,
                FinishTime = dbTriggerSetting.FinishTime,
                Percent = dbTriggerSetting.Percent,
                BonusSettingCodes = dbTriggerSetting.BonusSettingCodes,
                PartnerId = dbTriggerSetting.PartnerId,
                MinAmount = dbTriggerSetting.MinAmount,
                MaxAmount = dbTriggerSetting.MaxAmount,
                SegmentId = dbTriggerSetting.SegmentId,
                DayOfWeek = dbTriggerSetting.DayOfWeek,
                Condition = dbTriggerSetting.Condition,
                CreationTime = currentTime,
                LastUpdateTime = currentTime,
                UpToAmount = dbTriggerSetting.UpToAmount,
                ConsiderBonusBets = dbTriggerSetting.ConsiderBonusBets
            };
            newTriggerSetting.TriggerProductSettings = new List<TriggerProductSetting>();
            dbTriggerSetting.TriggerProductSettings.ToList().ForEach(x => newTriggerSetting.TriggerProductSettings.Add(
                new TriggerProductSetting { ProductId = x.ProductId, Percent = x.Percent, TriggerSettingId = newTriggerSetting.Id }));
            Db.TriggerSettings.Add(newTriggerSetting);
            Db.SaveChanges();
            dbTriggerSetting.BonusPaymentSystemSettings.ToList().ForEach(x => Db.BonusPaymentSystemSettings.Add(
            new BonusPaymentSystemSetting
            {
                TriggerId = newTriggerSetting.Id,
                PaymentSystemId = x.PaymentSystemId,
                Type = x.Type
            }));
            dbTriggerSetting.AmountCurrencySettings.ToList().ForEach(x => Db.AmountCurrencySettings.Add(
            new AmountCurrencySetting
            {
                TriggerId = newTriggerSetting.Id,
                CurrencyId = x.CurrencyId,
                MinAmount = x.MinAmount,
                MaxAmount = x.MaxAmount,
                UpToAmount = x.UpToAmount
            }));
            Db.SaveChanges();
            return newTriggerSetting;
        }
        public TriggerSetting DeleteTriggerSetting(int triggerSettingId)
        {
            var triggerSetting = Db.TriggerSettings.Where(x => x.Id == triggerSettingId).FirstOrDefault();
            if (triggerSetting == null)
                throw CreateException(LanguageId, Constants.Errors.TriggerSettingNotFound);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditBonuses
            });
            if (!bonusAccess.HaveAccessForAllObjects ||
               (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != triggerSetting.PartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var currentTime = DateTime.UtcNow;
            var relatedActiveBonus = Db.Bonus.Where(x => x.Status == (int)BonusStatuses.Active
                 && x.StartTime <= currentTime
                 && x.FinishTime > currentTime
                 && x.TriggerGroups.Any(t => t.TriggerGroupSettings.Any(g => g.SettingId == triggerSettingId))).FirstOrDefault();
            
            if(relatedActiveBonus != null)
                throw CreateException(LanguageId, Constants.Errors.NotAllowed);

            triggerSetting.Status = (int)TriggerStatuses.Deleted;
            Db.SaveChanges();
            return triggerSetting;
        }

        public Bonu DeleteBonus(int bonusId)
        {
            var bonus = Db.Bonus.Where(x => x.Id == bonusId).FirstOrDefault();
            if (bonus == null)
                throw CreateException(LanguageId, Constants.Errors.BonusNotFound);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditBonuses
            });
            if (!bonusAccess.HaveAccessForAllObjects ||
               (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != bonus.PartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var currentTime = DateTime.UtcNow;
            if (bonus.StartTime <= currentTime && bonus.FinishTime > currentTime && bonus.Status == (int)BonusStatuses.Active)
                throw CreateException(LanguageId, Constants.Errors.NotAllowed);

            var clientBonus = Db.ClientBonus.Where(x => x.BonusId == bonus.Id && x.Status == (int)ClientBonusStatuses.Active).FirstOrDefault();
            if (clientBonus != null)
                throw CreateException(LanguageId, Constants.Errors.NotAllowed);

            bonus.Status = (int)BonusStatuses.Deleted;
            Db.SaveChanges();
            return bonus;
        }

        public object GetTriggerSettingClients(int id)
        {
            return Db.ClientBonusTriggers.Where(x => x.TriggerId == id).Select(x => new
            {
                Id = x.Id,
                BonusId = x.BonusId,
                ClientId = x.ClientId,
                FirstName = x.Client.FirstName,
                LastName = x.Client.LastName,
                Username = x.Client.UserName,
                SourceAmount = x.SourceAmount,
                DateTime = x.CreationTime,
                ReuseNumber = x.ReuseNumber
            }).ToList();
        }

        public PagedModel<TriggerSetting> GetTriggerSettings(int skipCount, int takeCount, int? id, int? partnerId, int? bonusId, int? status)
        {
            if (bonusId.HasValue)
            {
                var bonus = Db.Bonus.FirstOrDefault(x => x.Id == bonusId.Value);
                if (bonus == null)
                    throw CreateException(LanguageId, Constants.Errors.BonusNotFound);
                partnerId = bonus.PartnerId;
            }
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBonuses
            });
            var currentTime = DateTime.UtcNow;
            var queryTriggers = Db.TriggerSettings.Include(x => x.TriggerProductSettings).Include(x => x.AmountCurrencySettings).Where(x => x.Status != (int)TriggerStatuses.Deleted);
            if (status == (int)TriggerStatuses.Active)
                queryTriggers = queryTriggers.Where(x => x.FinishTime > currentTime && (x.Status == (int)TriggerStatuses.Active || x.Status == null));
            else if (status == (int)TriggerStatuses.Inactive)
                queryTriggers = queryTriggers.Where(x => x.FinishTime <= currentTime || x.Status == (int)TriggerStatuses.Inactive);

            if (partnerId.HasValue)
                queryTriggers = queryTriggers.Where(x => x.PartnerId == partnerId.Value);
            else if (!partnerAccess.HaveAccessForAllObjects)
                queryTriggers = queryTriggers.Where(x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId));

            if (id.HasValue)
                queryTriggers = queryTriggers.Where(x => x.Id == id.Value);
            
            var count = queryTriggers.Count();
            return new PagedModel<TriggerSetting>
            {
                Count = count,
                Entities = queryTriggers.OrderByDescending(x => x.Id).Skip(skipCount * takeCount).Take(takeCount).ToList()
            };
        }

        public List<TriggerGroupItem> GetTriggerGroups(int bonusId)
        {
            var bonus = GetBonusById(bonusId);
            if (bonus == null)
                throw CreateException(LanguageId, Constants.Errors.BonusNotFound);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditBonuses
            });
            if (!bonusAccess.HaveAccessForAllObjects ||
               (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != bonus.PartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            return Db.TriggerGroups.Where(x => x.BonusId == bonusId).Select(x => new TriggerGroupItem
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.Type,
                Priority = x.Priority,
                TriggerSettings = x.TriggerGroupSettings.Select(y => new TriggerSettingItem
                {
                    Id = y.TriggerSetting.Id,
                    Name = y.TriggerSetting.Name,
                    Description = y.TriggerSetting.Description,
                    Type = y.TriggerSetting.Type,
                    StartTime = y.TriggerSetting.StartTime,
                    FinishTime = y.TriggerSetting.FinishTime,
                    Percent = y.TriggerSetting.Percent,
                    BonusSettingCodes = y.TriggerSetting.BonusSettingCodes,
                    PartnerId = y.TriggerSetting.PartnerId,
                    CreationTime = y.TriggerSetting.CreationTime,
                    LastUpdateTime = y.TriggerSetting.LastUpdateTime,
                    MinAmount = y.TriggerSetting.MinAmount,
                    MaxAmount = y.TriggerSetting.MaxAmount,
                    DayOfWeek = y.TriggerSetting.DayOfWeek,
                    SegmentId = y.TriggerSetting.SegmentId,
                    Order = y.Order
                }).ToList()
            }).ToList();
        }

        public TriggerGroup SaveTriggerGroup(TriggerGroup triggerGroup)
        {
            var bonus = GetBonusById(triggerGroup.BonusId);
            if (bonus == null)
                throw CreateException(LanguageId, Constants.Errors.BonusNotFound);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditBonuses
            });
            if (!bonusAccess.HaveAccessForAllObjects ||
               (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != bonus.PartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            if (!Enum.IsDefined(typeof(TriggerGroupType), triggerGroup.Type))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

            var dbTriggerGroup = Db.TriggerGroups.FirstOrDefault(x => x.Name == triggerGroup.Name && x.BonusId == triggerGroup.BonusId);
            if (dbTriggerGroup == null)
            {
                dbTriggerGroup = new TriggerGroup
                {
                    Name = triggerGroup.Name,
                    BonusId = bonus.Id,
                    Type = triggerGroup.Type,
                    Priority = triggerGroup.Priority
                };
                Db.TriggerGroups.Add(dbTriggerGroup);
            }
            else
            {
                dbTriggerGroup.Name = triggerGroup.Name;
                dbTriggerGroup.Type = triggerGroup.Type;
                dbTriggerGroup.Priority = triggerGroup.Priority;
            }
            Db.SaveChanges();
            return dbTriggerGroup;
        }

        public TriggerSetting AddTriggerSettingToGroup(int triggerGroupId, int triggerSettingId, int order, out int bonusId)
        {
            var dbTriggerGroup = Db.TriggerGroups.Include(x => x.Bonu).FirstOrDefault(x => x.Id == triggerGroupId);
            if (dbTriggerGroup == null)
                throw CreateException(LanguageId, Constants.Errors.TriggerGroupNotFound);
            if (dbTriggerGroup.Bonu.TotalReceiversCount > 0)
                throw CreateException(LanguageId, Constants.Errors.ClientAlreadyHasActiveBonus);

            bonusId = dbTriggerGroup.BonusId;
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditBonuses
            });
            if (!bonusAccess.HaveAccessForAllObjects ||
               (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != dbTriggerGroup.Bonu.PartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var dbTriggerSetting = Db.TriggerSettings.FirstOrDefault(x => x.Id == triggerSettingId);
            if (dbTriggerSetting == null)
                throw CreateException(LanguageId, Constants.Errors.TriggerSettingNotFound);

            if (dbTriggerSetting.PartnerId != dbTriggerGroup.Bonu.PartnerId)
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            var triggerGroup = Db.TriggerGroups.Include(x => x.TriggerGroupSettings).FirstOrDefault(x => x.Id == triggerGroupId);
            if(triggerGroup == null)
                throw CreateException(LanguageId, Constants.Errors.TriggerGroupNotFound);
            if (triggerGroup.Priority == 0 && (triggerGroup.TriggerGroupSettings.Any() || !Constants.AutoclaimingTriggers.Contains(dbTriggerSetting.Type)))
                throw CreateException(LanguageId, Constants.Errors.NotAllowed);            
            if ((dbTriggerSetting.Type == (int)TriggerTypes.SignIn || dbTriggerSetting.Type == (int)TriggerTypes.SignUp) &&
                (Db.TriggerGroups.Any(x => x.BonusId == dbTriggerGroup.BonusId && 
                x.TriggerGroupSettings.Any(y => y.TriggerSetting.Type == dbTriggerSetting.Type))))
                throw CreateException(LanguageId, Constants.Errors.NotAllowed);

            var dbTriggerGroupSettingItem = Db.TriggerGroupSettings.FirstOrDefault(x => x.GroupId == triggerGroupId &&
                                                                          x.SettingId == triggerSettingId);
            if (dbTriggerGroupSettingItem == null)
            {
                dbTriggerGroupSettingItem = new TriggerGroupSetting
                {
                    GroupId = triggerGroupId,
                    SettingId = triggerSettingId,
                    Order = order
                };

                Db.TriggerGroupSettings.Add(dbTriggerGroupSettingItem);
                Db.SaveChanges();
            }
            dbTriggerSetting.Order = order;
            return dbTriggerSetting;
        }

        public int RemoveTriggerGroup(int triggerGroupId)
        {
            var dbTriggerGroup = Db.TriggerGroups.Include(x => x.Bonu).Include(x => x.TriggerGroupSettings).FirstOrDefault(x => x.Id == triggerGroupId) ??
                throw CreateException(LanguageId, Constants.Errors.TriggerGroupNotFound);
            var bonusId = dbTriggerGroup.BonusId;
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditBonuses
            });
            if (!bonusAccess.HaveAccessForAllObjects ||
               (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != dbTriggerGroup.Bonu.PartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var oldValue = new
            {
                BonusId = bonusId,
                GroupId = dbTriggerGroup.Id,
                GroupType = dbTriggerGroup.Type,
                TriggerIds = dbTriggerGroup.TriggerGroupSettings.Select(x => x.SettingId).ToList()
            };
            Db.TriggerGroupSettings.Where(x => x.GroupId == dbTriggerGroup.Id).DeleteFromQuery();
            Db.TriggerGroups.Where(x => x.Id == dbTriggerGroup.Id).DeleteFromQuery();
            SaveChangesWithHistory((int)ObjectTypes.Bonus, bonusId, JsonConvert.SerializeObject(oldValue));
            CacheManager.RemoveBonus(bonusId);
            return bonusId;
        }

        public void RemoveTriggerSettingFromGroup(int triggerGroupId, int triggerSettingId, out int bonusId)
        {
            var dbTriggerSetting = Db.TriggerSettings.FirstOrDefault(x => x.Id == triggerSettingId) ??
                throw CreateException(LanguageId, Constants.Errors.TriggerSettingNotFound);
            var dbTriggerGroup = Db.TriggerGroups.Include(x => x.Bonu).FirstOrDefault(x => x.Id == triggerGroupId) ??
                throw CreateException(LanguageId, Constants.Errors.TriggerGroupNotFound);
            if (dbTriggerGroup.Bonu.TotalReceiversCount > 0)
                throw CreateException(LanguageId, Constants.Errors.ClientAlreadyHasActiveBonus);

            bonusId = dbTriggerGroup.BonusId;

            var setting = Db.TriggerGroupSettings.Include(x => x.TriggerSetting).FirstOrDefault(x => x.GroupId == triggerGroupId && x.SettingId == triggerSettingId);
            if (setting == null)
                throw CreateException(LanguageId, Constants.Errors.TriggerSettingNotFound);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditBonuses
            });
            if (!bonusAccess.HaveAccessForAllObjects ||
               (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != setting.TriggerSetting.PartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var oldValue = new
            {
                BonusId = bonusId,
                GroupId = dbTriggerGroup.Id,
                GroupType = dbTriggerGroup.Type,
                TriggerIds = dbTriggerGroup.TriggerGroupSettings.Select(x => x.SettingId).ToList()
            };
            Db.TriggerGroupSettings.Where(x => x.Id == setting.Id).DeleteFromQuery();
            SaveChangesWithHistory((int)ObjectTypes.Bonus, bonusId, JsonConvert.SerializeObject(oldValue));
            CacheManager.RemoveBonus(bonusId);
        }

        public void CalculateCashBackBonus()
        {
            var currentTime = GetServerDate();
            var date = (long)currentTime.Year * 100000000 + (long)currentTime.Month * 1000000 +
                       (long)currentTime.Day * 10000 + (long)currentTime.Hour * 100 + (long)currentTime.Minute;
            var reuseNumber = currentTime.Year * 10000 + currentTime.Month * 100 + currentTime.Day;
            // var reuseNumber = currentTime.Year * 100000000 + currentTime.Month * 1000000 + currentTime.Day * 10000 + currentTime.Minute; //for testing
            var bonuses = Db.Bonus.Include(x => x.AmountCurrencySettings).Where(x => x.Status == (int)BonusStatuses.Active && x.StartTime < currentTime && x.FinishTime > currentTime &&
                                              DbFunctions.AddHours(x.LastExecutionTime, x.Period) < currentTime &&
                                              x.Type == (int)BonusTypes.CashBackBonus &&
                                              (!x.MaxGranted.HasValue || x.TotalGranted < x.MaxGranted) &&
                                              (!x.MaxReceiversCount.HasValue || x.TotalReceiversCount < x.MaxReceiversCount))
                                  .GroupBy(x => x.PartnerId).ToList();
            var partnerIds = bonuses.Select(x => x.Key).ToList();
            var partners = Db.Partners.Where(x => partnerIds.Contains(x.Id)).ToDictionary(x => x.Id, x => x.CurrencyId);
            
            using (var clientBl = new ClientBll(this))
            {
                foreach (var partnerBonus in bonuses)
                {
                    var partnerCurrency = partners[partnerBonus.Key];
                    foreach (var bs in partnerBonus)
                    {
                        try
                        {
                            var bonus = Db.Bonus.Include(x => x.BonusProducts).First(x => x.Id == bs.Id);
                            var finishTime = bonus.LastExecutionTime.AddHours(bonus.Period);
                            var bets = clientBl.GetCashBackBonusBets(bonus.PartnerId, bonus.LastExecutionTime, finishTime);
                            using (var transactionScope = CommonFunctions.CreateTransactionScope(20))
                            {
                                Db.sp_GetBonusLock(bs.Id);
                                Db.ClientBonus.Where(x => x.Status == (int)ClientBonusStatuses.NotAwarded && x.BonusId == bs.Id)
                                              .UpdateFromQuery(x => new ClientBonu
                                              {
                                                  Status = (int)ClientBonusStatuses.Expired,
                                                  CalculationTime = currentTime
                                              });   
                                if (!bets.Any())
                                {
                                    bonus.LastExecutionTime = finishTime;
                                    Db.SaveChanges();
                                    transactionScope.Complete();
                                    continue;
                                }
                                var fDate = bonus.LastExecutionTime.Year * 1000000 + bonus.LastExecutionTime.Month * 10000 + bonus.LastExecutionTime.Day * 100 +
                                            bonus.LastExecutionTime.Hour;
                                var tDate = finishTime.Year * 1000000 + finishTime.Month * 10000 + finishTime.Day * 100 + finishTime.Hour;

                                var productBets = bets.GroupBy(x => x.ProductId).ToList();
                                foreach (var bet in productBets)
                                {
                                    var productId = bet.Key;
                                    while (true)
                                    {
                                        var p = bonus.BonusProducts.FirstOrDefault(x => x.ProductId == productId);
                                        if (p != null)
                                        {
                                            bet.All(x => { x.Percent = p.Percent ?? 0; x.ProductId = p.ProductId; return true; }); //check
                                            break;
                                        }
                                        var product = Db.Products.First(x => x.Id == productId);
                                        if (product.ParentId == null) break;
                                        productId = product.ParentId.Value;
                                    }
                                }

                                var bonusWins = Db.Documents.Where(x => x.OperationTypeId == (int)OperationTypes.BonusWin &&
                                                                     x.Date >= fDate && x.Date < tDate)
                                                              .GroupBy(x => x.ClientId)
                                                              .Select(x => new { ClientId = x.Key, WinAmount = x.Sum(y => y.Amount) })
                                                              .ToList();
                                var bonusSegments = bonus.BonusSegmentSettings.Select(x => x.SegmentId).ToList();  
                                var bonusCurrencies = bonus.BonusCurrencySettings.Select(x => x.CurrencyId).ToList();

                                var betQuery = (from b in bets
                                                group b by new { b.ClientId, b.CurrencyId }
                                                   into y
                                                select
                                                    new
                                                    {
                                                        ClientId = y.Key.ClientId,
                                                        CurrencyId = y.Key.CurrencyId,
                                                        Amount = y.Sum(x => x.Amount * x.Percent) / 100
                                                    }).Where(x => x.Amount > 0);

                                if (bonusCurrencies.Any())
                                    betQuery = betQuery.Where(x => bonusCurrencies.Contains(x.CurrencyId));
                                if (bonusSegments.Any())
                                {
                                    var clients = Db.ClientClassifications.Where(x => x.SegmentId.HasValue && bonusSegments.Contains(x.SegmentId.Value) &&
                                                                                      x.ProductId == (int)Constants.PlatformProductId)
                                                                          .Select(x => x.ClientId).Distinct().ToList();
                                    betQuery = betQuery.Where(x => clients.Contains(x.ClientId));
                                }
                                var bonusAmounts = betQuery.GroupBy(x => x.CurrencyId).ToList();

                                foreach (var bonusAmountByCurrency in bonusAmounts)
                                {
                                    var cItem = bs.AmountCurrencySettings?.FirstOrDefault(x => x.CurrencyId == bonusAmountByCurrency.Key);
                                    var minAmount = cItem != null && cItem.MinAmount.HasValue ? cItem.MinAmount.Value :
                                        ConvertCurrency(partnerCurrency, bonusAmountByCurrency.Key, bs.MinAmount.Value);
                                    var maxAmount = cItem != null && cItem.MaxAmount.HasValue ? cItem.MaxAmount.Value :
                                        ConvertCurrency(partnerCurrency, bonusAmountByCurrency.Key, bs.MaxAmount.Value);
                                    
                                    var autoApprovedAmount = ConvertCurrency(partnerCurrency, bonusAmountByCurrency.Key, bs.AutoApproveMaxAmount.Value);

                                    foreach (var bonusAmount in bonusAmountByCurrency)
                                    {
                                        var finalAmount = bonusAmount.Amount;
                                        var bw = bonusWins.FirstOrDefault(x => x.ClientId == bonusAmount.ClientId);
                                        if (bw != null)
                                            finalAmount -= bw.WinAmount;

                                        if (finalAmount < minAmount)
                                            continue;
                                        var amount = Math.Min(maxAmount, finalAmount);
                                        Db.ClientBonus.Add(new ClientBonu
                                        {
                                            BonusId = bs.Id,
                                            ClientId = bonusAmount.ClientId,
                                            Status = amount <= autoApprovedAmount ? (int)ClientBonusStatuses.Active : (int)ClientBonusStatuses.NotAwarded,
                                            BonusPrize = amount,
                                            CreationTime = currentTime,
                                            CreationDate = date,
                                            ReuseNumber = reuseNumber
                                        });
                                    }
                                }
                                bonus.LastExecutionTime = finishTime;
                                Db.SaveChanges();
                                transactionScope.Complete();
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                        }
                    }
                }
            }
        }

        public List<int> AwardCashbackBonus(DateTime lastExecutionTime)
        {
            var clientIds = new List<int>();
            using (var clientBl = new ClientBll(this))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    using (var transactionScope = CommonFunctions.CreateTransactionScope(20))
                    {
                        var currentTime = DateTime.UtcNow;
                        var dbClientOperations = Db.ClientBonus.Include(x => x.Bonu).Include(x => x.Client).Include(x => x.Client.Partner)
                                                               .Where(x => x.Bonu.Type == (int)BonusTypes.CashBackBonus &&
                                                                           x.Status == (int)ClientBonusStatuses.Active).Take(100).ToList();

                        var wageringBonus = new List<ClientBonusItem>();
                        dbClientOperations.ForEach(x =>
                        {
                            var convertedAmount = ConvertCurrency(x.Client.CurrencyId, x.Client.Partner.CurrencyId, x.BonusPrize);
                            if ((x.Bonu.MaxGranted != null && convertedAmount + x.Bonu.TotalGranted > x.Bonu.MaxGranted.Value) ||
                            (x.Bonu.MaxReceiversCount != null && 1 + x.Bonu.TotalReceiversCount > x.Bonu.MaxReceiversCount.Value))
                                x.Status = (int)ClientBonusStatuses.NotAwarded;
                            else
                            {
                                if (!x.Bonu.LinkedBonusId.HasValue)
                                {
                                    var input = new ClientOperation
                                    {
                                        ClientId = x.ClientId,
                                        Amount = x.BonusPrize,
                                        OperationTypeId = (int)OperationTypes.CashBackBonus,
                                        PartnerId = x.Bonu.PartnerId,
                                        CurrencyId = x.Client.CurrencyId,
                                        AccountTypeId = x.Bonu.FinalAccountTypeId ?? (int)AccountTypes.ClientUnusedBalance
                                    };
                                    clientBl.CreateDebitToClient(input, x.ClientId, string.Empty, documentBl, null);
                                    clientIds.Add(x.ClientId);
                                }
                                else
                                    wageringBonus.Add(new ClientBonusItem
                                    {
                                        PartnerId = x.Bonu.PartnerId,
                                        BonusId = x.Bonu.LinkedBonusId.Value,
                                        ClientId = x.ClientId,
                                        BonusAmount = x.BonusPrize
                                    });

                                x.Status = (int)ClientBonusStatuses.Closed;
                            }

                            x.CalculationTime = currentTime;
                            x.Bonu.TotalReceiversCount++;
                            x.Bonu.TotalGranted += convertedAmount;
                        });
                        var grouppedWageringBonus = wageringBonus.GroupBy(x => x.BonusId);
                        foreach (var bon in grouppedWageringBonus)
                        {
                            var bonus = Db.Bonus.Include(x => x.BonusSegmentSettings)
                                .Include(x => x.BonusCountrySettings)
                                .Include(x => x.BonusCurrencySettings)
                                .Include(x => x.BonusLanguageSettings)
                                .FirstOrDefault(x => x.Id == bon.Key && x.Status == (int)BonusStatuses.Active &&
                                                     x.StartTime <= currentTime && x.FinishTime > currentTime &&
                                                   (!x.MaxGranted.HasValue || x.TotalGranted < x.MaxGranted) &&
                                                   (!x.MaxReceiversCount.HasValue || x.TotalReceiversCount < x.MaxReceiversCount));

                            if (bonus != null && (bonus.Type == (int)BonusTypes.CampaignWagerCasino ||
                               bonus.Type == (int)BonusTypes.CampaignCash ||
                               bonus.Type == (int)BonusTypes.CampaignFreeBet ||
                               bonus.Type == (int)BonusTypes.CampaignWagerSport))
                            {
                                foreach (var c in bon)
                                {
                                    var client = CacheManager.GetClientById(c.ClientId);
                                    var clientSegmentsIds = new List<int>();
                                    var clientClassifications = CacheManager.GetClientClassifications(client.Id);
                                    if (clientClassifications.Any())
                                        clientSegmentsIds = clientClassifications.Where(x => x.SegmentId.HasValue && x.ProductId == (int)Constants.PlatformProductId)
                                                                                .Select(x => x.SegmentId.Value).ToList();
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
                                    c.Type = bonus.Type;
                                    c.ClientCurrencyId = client.CurrencyId;
                                    c.FinalAccountTypeId = bonus.FinalAccountTypeId.Value;
                                    c.ReusingMaxCount = bonus.ReusingMaxCount;
                                    c.WinAccountTypeId = bonus.WinAccountTypeId;
                                    c.ValidForAwarding = bonus.ValidForAwarding == null ? (DateTime?)null : DateTime.Now.AddHours(bonus.ValidForAwarding.Value);
                                    c.ValidForSpending = bonus.ValidForSpending == null ? (DateTime?)null : DateTime.Now.AddHours(bonus.ValidForSpending.Value);
                                    GiveCompainToClient(c, out int awardedStatus);
                                }
                            }
                        }

                        Db.SaveChanges();
                        transactionScope.Complete();
                    }
                }
            }
            return clientIds.Distinct().ToList();
        }

        public List<int> GiveBonusToAffiliateManagers()
        {
            var currentTime = GetServerDate();
            var bonuses = Db.Bonus.Include(x => x.Partner).Where(x => x.Status == (int)BonusStatuses.Active && x.StartTime <= currentTime && x.FinishTime > currentTime &&
                                                                       DbFunctions.AddHours(x.LastExecutionTime, x.Period) < currentTime &&
                                                                       x.Type == (int)BonusTypes.AffiliateBonus &&
                                                                      (!x.MaxGranted.HasValue || x.TotalGranted < x.MaxGranted) &&
                                                                      (!x.MaxReceiversCount.HasValue || x.TotalReceiversCount < x.MaxReceiversCount)).ToList();
            var clientIds = new List<int>();
            using (var clientBl = new ClientBll(this))
            {
                using (var documentBl = new DocumentBll(this))
                {
                    using (var dwh = new IqSoftDataWarehouseEntities())
                    {
                        foreach (var bonus in bonuses)
                        {
                            var toDate = bonus.LastExecutionTime.AddHours(bonus.Period);
                            toDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, toDate.Hour, 0, 0);
                            var fromDate = bonus.LastExecutionTime;
                            var tDate = (long)toDate.Year * 1000000 + (long)toDate.Month * 10000 + (long)toDate.Day * 100 + (long)toDate.Hour;
                            var fDate = (long)fromDate.Year * 1000000 + (long)fromDate.Month * 10000 + (long)fromDate.Day * 100 + (long)fromDate.Hour;
                            var affiliatesProfit = dwh.fn_AffiliateClient(fDate, tDate, bonus.PartnerId).ToList();

                            using (var transactionScope = CommonFunctions.CreateTransactionScope(5))
                            {
                                var bonusProduct = Db.BonusProducts.FirstOrDefault(x => x.ProductId == Constants.PlatformProductId && x.BonusId == bonus.Id);
                                if (bonusProduct == null || bonusProduct.Percent == null || bonusProduct.Percent == 0)
                                {
                                    bonus.LastExecutionTime = new DateTime(toDate.Year, toDate.Month, toDate.Day, toDate.Hour, 0, 0);
                                    Db.SaveChanges();
                                    transactionScope.Complete();
                                    continue;
                                }
                                var percent = bonusProduct.Percent.Value;

                                foreach (var aff in affiliatesProfit)
                                {
                                    if (int.TryParse(aff.AffiliateManagerId, out int affiliateManagerId))
                                    {
                                        var affManager = Db.Clients.FirstOrDefault(x => x.Id == affiliateManagerId);
                                        if (affManager != null)
                                        {
                                            var ggr = (aff.TotalBetAmount - aff.TotalWinAmount) ?? 0;
                                            if (ggr > 0)
                                            {
                                                var amount = ConvertCurrency(Constants.DefaultCurrencyId, affManager.CurrencyId, ggr) * percent / 100;
                                                if (amount < ConvertCurrency(bonus.Partner.CurrencyId, affManager.CurrencyId, bonus.MinAmount.Value))
                                                    continue;
                                                amount = Math.Min(ConvertCurrency(bonus.Partner.CurrencyId, affManager.CurrencyId, bonus.MaxAmount.Value), amount);

                                                var input = new ClientOperation
                                                {
                                                    ClientId = affiliateManagerId,
                                                    Amount = amount,
                                                    OperationTypeId = (int)OperationTypes.AffiliateBonus,
                                                    PartnerId = bonus.PartnerId,
                                                    Info = bonus.Id.ToString(),
                                                    CurrencyId = affManager.CurrencyId,
                                                    AccountTypeId = bonus.FinalAccountTypeId ?? (int)AccountTypes.ClientUsedBalance,
                                                    Creator = aff.ClientId
                                                };
                                                clientBl.CreateDebitToClient(input, affiliateManagerId, string.Empty, documentBl, null);
                                                clientIds.Add(affiliateManagerId);
                                            }
                                            Db.AffiliateReferrals.Where(x => x.AffiliatePlatformId == bonus.PartnerId * 100 &&
                                                                             x.Type == (int)AffiliateReferralTypes.WebsiteInvitation &&
                                                                             x.AffiliateId == affiliateManagerId.ToString()).
                                            UpdateFromQuery(x => new AffiliateReferral { LastProcessedBonusTime = bonus.LastExecutionTime });
                                            dwh.AffiliateReferrals.Where(x => x.AffiliatePlatformId == bonus.PartnerId * 100 &&
                                                                             x.Type == (int)AffiliateReferralTypes.WebsiteInvitation &&
                                                                             x.AffiliateId == affiliateManagerId.ToString()).
                                            UpdateFromQuery(x => new AffiliateReferral { LastProcessedBonusTime = bonus.LastExecutionTime });
                                        }
                                    }
                                }
                                bonus.LastExecutionTime = new DateTime(toDate.Year, toDate.Month, toDate.Day, toDate.Hour, 0, 0);
                                Db.SaveChanges();
                                transactionScope.Complete();
                            }
                        }
                    }
                }
            }
            return clientIds;
        }

        public decimal[] ShuffleWelcomeBonusItems()
        {
            var currentTime = DateTime.UtcNow;

            var bonus = Db.Bonus.FirstOrDefault(b => b.PartnerId == Identity.PartnerId && b.Type == (int)BonusTypes.SignupRealBonus && 
                b.Status == (int)BonusStatuses.Active && b.StartTime <= currentTime && b.FinishTime > currentTime);
            if (bonus == null || String.IsNullOrEmpty(bonus.Info))
                throw CreateException(Identity.LanguageId, Constants.Errors.ProductNotFound);

            decimal[] shuffledItems = JsonConvert.DeserializeObject<decimal[]>(bonus.Info);
            var rand = new Random();
            int length = shuffledItems.Length;
            for (int i = 0; i < length - 1; i++)
            {
                int randNum = i + rand.Next(length - i);
                decimal temp = shuffledItems[randNum];
                shuffledItems[randNum] = shuffledItems[i];
                shuffledItems[i] = temp;
            }
            return shuffledItems;
        }

        public void CreateSignupRealBonus(int clientId, decimal bonusPrize)
        {
            var currentDate = DateTime.UtcNow;
            var bonus = Db.Bonus.FirstOrDefault(b => b.PartnerId == Identity.PartnerId && b.Type == (int)BonusTypes.SignupRealBonus &&
                                                     b.Status == (int)BonusStatuses.Active && b.StartTime <= currentDate && b.FinishTime > currentDate &&
                                                    (!b.MaxGranted.HasValue || b.TotalGranted < b.MaxGranted) &&
                                                    (!b.MaxReceiversCount.HasValue || b.TotalReceiversCount < b.MaxReceiversCount));
            if (bonus == null || String.IsNullOrEmpty(bonus.Info))
                throw CreateException(Identity.LanguageId, Constants.Errors.BonusNotFound);

            Db.ClientBonus.Add(new ClientBonu
            {
                BonusId = bonus.Id,
                ClientId = clientId,
                Status = (int)ClientBonusStatuses.Active,
                BonusPrize = bonusPrize,
                CreationTime = currentDate,
                AwardingTime = currentDate,
                CreationDate = (long)currentDate.Year * 100000000 + currentDate.Month * 1000000 + currentDate.Day * 10000 + currentDate.Hour * 100 + currentDate.Minute
            });
            Db.SaveChanges();
        }

        public void GiveWelcomeRealBonus(Client client, DocumentBll documentBl)
        {
            var bonus = Db.Bonus.FirstOrDefault(b => b.PartnerId == client.PartnerId && b.Type == (int)BonusTypes.SignupRealBonus);
            if (bonus != null)
            {
                var clientBonus = Db.ClientBonus.FirstOrDefault(cb => cb.BonusId == bonus.Id && cb.ClientId == client.Id && cb.Status == (int)ClientBonusStatuses.Active);
                if (clientBonus == null)
                    return;
                clientBonus.Status = (int)ClientBonusStatuses.Inactive;
                documentBl.CreateBonusDocumnet(client, clientBonus.BonusPrize, (int)OperationTypes.WelcomeBonus, (int)AccountTypes.ClientUnusedBalance);
            }
        }

        public void GiveWageringBonus(Bonu bi, Client client, decimal bonusAmount, long reuseNumber)
        {
            Document result = null;
            using (var transactionScope = CommonFunctions.CreateTransactionScope())
            {
                using (var clientBl = new ClientBll(this))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var wagerBonusTypes = new List<int> { (int)BonusTypes.CampaignWagerCasino, (int)BonusTypes.CampaignWagerSport };

                        if (wagerBonusTypes.Contains(bi.Type))
                        {
                            if (Db.ClientBonus.Any(x => x.ClientId == client.Id && x.Status != (int)ClientBonusStatuses.NotAwarded &&
                                x.BonusId == bi.Id && x.ReuseNumber == reuseNumber))
                                throw CreateException(Identity.LanguageId, Constants.Errors.ClientAlreadyHasActiveBonus, info: "GiveWageringBonus_" + bi.Id + "_" + client.Id);
                        }
                        var account = Db.Accounts.FirstOrDefault(x => x.ObjectId == client.Id &&
                            x.ObjectTypeId == (int)ObjectTypes.Client && x.AccountType.Id == (int)AccountTypes.ClientBonusBalance);
                        if (account != null && account.Balance != 0)
                        {
                            var correctionInput = new ClientCorrectionInput
                            {
                                Amount = account.Balance,
                                AccountId = account.Id,
                                AccountTypeId = (int)AccountTypes.ClientBonusBalance,
                                CurrencyId = client.CurrencyId,
                                ClientId = client.Id,
                                Info = "RemoveOldBonus"
                            };
                            clientBl.CreateCreditCorrectionOnClient(correctionInput, documentBl, false);
                        }
                        var currentDate = DateTime.UtcNow;
                        if (bi.Type != (int)BonusTypes.CampaignWagerCasino && bi.Type != (int)BonusTypes.CampaignWagerSport &&
                            bi.Type != (int)BonusTypes.CampaignFreeBet && bi.Type != (int)BonusTypes.CampaignCash)
                        {
                            Db.ClientBonus.Add(new ClientBonu
                            {
                                BonusId = bi.Id,
                                ClientId = client.Id,
                                Status = (int)ClientBonusStatuses.Active,
                                BonusPrize = bonusAmount,
                                TurnoverAmountLeft = bonusAmount * bi.TurnoverCount,
                                CreationTime = currentDate,
                                AwardingTime = currentDate,
                                CreationDate = (long)currentDate.Year * 100000000 + currentDate.Month * 1000000 +
                                currentDate.Day * 10000 + currentDate.Hour * 100 + currentDate.Minute,
                                ReuseNumber = 1
                            });
                            Db.SaveChanges();
                        }
                        var input = new ClientOperation
                        {
                            ClientId = client.Id,
                            Amount = bonusAmount,
                            OperationTypeId = (int)OperationTypes.WageringBonus,
                            PartnerId = client.PartnerId,
                            CurrencyId = client.CurrencyId,
                            AccountTypeId = (int)AccountTypes.ClientBonusBalance
                        };
                        result = clientBl.CreateDebitToClient(input, client.Id, client.UserName, documentBl, null);
                        transactionScope.Complete();
                    }
                }
            }
        }
        public void GiveJackpotWin()
        {
            using (var transactionScope = CommonFunctions.CreateTransactionScope())
            {
                using (var clientBl = new ClientBll(this))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var dbJackpotTriggers = Db.JobTriggers.Include(x => x.Jackpot)
                                                              .Include(x => x.Client.Partner)
                                                              .Where(x => x.Type == (int)JobTriggerTypes.JackpotWin &&
                                                                          x.JackpotId.HasValue &&
                                                                          x.Jackpot.WinnerId == x.ClientId).ToList();
                        var mainPartner = CacheManager.GetPartnerById(Constants.MainPartnerId);
                        foreach (var jp in dbJackpotTriggers)
                        {
                            var jackpotCurrencyId = jp.Jackpot.PartnerId.HasValue ? jp.Client.Partner.CurrencyId : mainPartner.CurrencyId;
                            var input = new ClientOperation
                            {
                                ClientId = jp.Jackpot.WinnerId,
                                Amount = BaseBll.ConvertCurrency(jackpotCurrencyId, jp.Client.CurrencyId, jp.Jackpot.Amount),
                                OperationTypeId = (int)OperationTypes.Jackpot,
                                PartnerId = jp.Client.PartnerId,
                                CurrencyId = jp.Client.CurrencyId,
                                AccountTypeId = (int)AccountTypes.ClientUsedBalance
                            };
                            clientBl.CreateDebitToClient(input, jp.ClientId, string.Empty, documentBl, null);
                        }
                        Db.JobTriggers.RemoveRange(dbJackpotTriggers);
                        transactionScope.Complete();
                    }
                }
            }
        }

        public ClientBonu GivetFreeSpinBonus(int clientId, DateTime validUntil, int spinCount, int productId)
        {
            var client = CacheManager.GetClientById(clientId) ??
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            var bonus = CacheManager.GetAggregatedFreeSpin(client.PartnerId) ??
                     throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.BonusNotFound);

            var currentTime = DateTime.UtcNow;
            if (bonus.StartTime > currentTime || bonus.FinishTime < currentTime)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.BonusNotFound);
            
            if (validUntil < bonus.StartTime || validUntil > bonus.FinishTime)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.RequestExpired);
            var clientBonus = new ClientBonu
            {
                BonusId = bonus.Id,
                ClientId = clientId,
                Status = (int)ClientBonusStatuses.Finished,
                BonusPrize = spinCount,
                FinalAmount = productId,
                ValidUntil = validUntil,
                CreationTime = currentTime,
                AwardingTime = currentTime,
                CreationDate = (long)currentTime.Year * 100000000 + currentTime.Month * 1000000 + currentTime.Day * 10000 + currentTime.Hour * 100 + currentTime.Minute,
                ReuseNumber = (Int64)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds
            };
            Db.ClientBonus.Add(clientBonus);
            Db.SaveChanges();
            return clientBonus;
        }

        public ClientBonusInfo GiveCompainToClient(ClientBonusItem clientBonusItem, out int awardedStatus)
        {
            awardedStatus = 0;
            var currentTime = DateTime.UtcNow;
            var activeBonuses = Db.ClientBonus.Where(x => x.ClientId == clientBonusItem.ClientId && x.BonusId == clientBonusItem.BonusId).ToList();
            long reuseNumber = 1;
            var bonus = CacheManager.GetBonusById(clientBonusItem.BonusId);
            
            if (activeBonuses.Any())
            {
                var max = activeBonuses.Select(x => x.ReuseNumber ?? 1).Max();
                if (max >= bonus.ReusingMaxCount)
                {
                    awardedStatus = 1;
                    return new ClientBonusInfo { BonusId = clientBonusItem.BonusId, ReuseNumber = max };
                }
                reuseNumber = max + 1;
            }
            var ab = activeBonuses.FirstOrDefault(x => (x.ReuseNumber ?? 1) == reuseNumber);
            if (ab == null)
            {
                var currentDate = GetServerDate();
                if(bonus.Regularity != null)
                {
                    if (bonus.DayOfWeek != null && (bonus.DayOfWeek.Value % 7) != (int)currentTime.DayOfWeek)
                    {
                        awardedStatus = 2;
                        return new ClientBonusInfo { BonusId = clientBonusItem.BonusId, ReuseNumber = reuseNumber };
                    }
                    else if (bonus.ReusingMaxCountInPeriod != null)
                    {
                        DateTime? fromDate = null;
                        if (bonus.Regularity.Value == (int)BonusRegularities.Daily)
                            fromDate = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day);
                        else if (bonus.Regularity.Value == (int)BonusRegularities.Weekly)
                        {
                            int diff = (7 + (currentTime.DayOfWeek - DayOfWeek.Monday)) % 7;
                            fromDate = DateTime.Today.AddDays(-1 * diff);
                        }
                        else if (bonus.Regularity.Value == (int)BonusRegularities.Monthly)
                            fromDate = new DateTime(currentTime.Year, currentTime.Month, 1, 0, 0, 0);

                        if (fromDate != null)
                        {
                            var grantedCount = activeBonuses.Where(x => x.CreationTime >= fromDate).Count();
                            if (grantedCount >= bonus.ReusingMaxCountInPeriod.Value)
                            {
                                awardedStatus = 2;
                                return new ClientBonusInfo { BonusId = clientBonusItem.BonusId, ReuseNumber = reuseNumber };
                            }
                        }
                    }
                }
                Db.ClientBonus.Add(new ClientBonu
                {
                    BonusId = clientBonusItem.BonusId,
                    ClientId = clientBonusItem.ClientId,
                    Status = (int)ClientBonusStatuses.NotAwarded,
                    CreationTime = currentDate,
                    CreationDate = (long)currentDate.Year * 100000000 + currentDate.Month * 1000000 +
                                    currentDate.Day * 10000 + currentDate.Hour * 100 + currentDate.Minute,
                    ValidUntil = clientBonusItem.ValidForAwarding,
                    CalculationTime = clientBonusItem.Type == (int)BonusTypes.CampaignCash ? currentDate : (DateTime?)null,
                    ReuseNumber = reuseNumber,
                    BonusPrize = clientBonusItem.BonusAmount
                });
                Db.SaveChanges();
            }
            else
                awardedStatus = 1;

            return new ClientBonusInfo { BonusId = clientBonusItem.BonusId, ReuseNumber = reuseNumber };
        }


        public List<Bonu> GetClientAvailableBonus(int clientId, int? type, bool checkPermission)
        {
            var client = CacheManager.GetClientById(clientId) ??
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            if (checkPermission)
            {
                var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.CreateBonus
                });
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });
                if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != client.PartnerId))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            var clientSegmentsIds = new List<int>();
            var clientClassifications = CacheManager.GetClientClassifications(client.Id);
            if (clientClassifications.Any())
                clientSegmentsIds = clientClassifications.Where(x => x.SegmentId.HasValue && x.ProductId == (int)Constants.PlatformProductId)
                                                         .Select(x => x.SegmentId.Value).ToList();
            var currentTime = DateTime.UtcNow;
            return Db.Bonus.Where(x => x.Status == (int)BonusStatuses.Active &&
                                       x.StartTime <= currentTime && x.FinishTime > currentTime &&
                                       x.PartnerId == client.PartnerId && (!type.HasValue || x.Type == type) &&
                                     (!x.MaxGranted.HasValue || x.TotalGranted < x.MaxGranted) &&
                                     (!x.MaxReceiversCount.HasValue || x.TotalReceiversCount < x.MaxReceiversCount) &&
                                     (!x.BonusSegmentSettings.Any() ||
                                      (x.BonusSegmentSettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && clientSegmentsIds.Contains(y.SegmentId)) &&
                                      !x.BonusSegmentSettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet && clientSegmentsIds.Contains(y.SegmentId)))) &&
                                     (!x.BonusCountrySettings.Any() ||
                                      (x.BonusCountrySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.CountryId == (client.CountryId ?? client.RegionId)) &&
                                      !x.BonusCountrySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet && y.CountryId == (client.CountryId ?? client.RegionId)))) &&
                                     (!x.BonusCurrencySettings.Any() ||
                                      (x.BonusCurrencySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.CurrencyId == client.CurrencyId) &&
                                      !x.BonusCurrencySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet && y.CurrencyId == client.CurrencyId))) &&
                                     (!x.BonusLanguageSettings.Any() ||
                                      (x.BonusLanguageSettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.LanguageId == client.LanguageId) &&
                                      !x.BonusLanguageSettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet && y.LanguageId == client.LanguageId)))).ToList();
        }

        public Bonu GetAvailableBonus(int bonusSettingId, bool checkPermission)
        {
            var currentTime = DateTime.UtcNow;
            var bonus = Db.Bonus.Include(x => x.BonusSegmentSettings)
                                .Include(x => x.BonusCountrySettings)
                                .Include(x => x.BonusCurrencySettings)
                                .Include(x => x.BonusLanguageSettings)
                                .FirstOrDefault(x => x.Id == bonusSettingId && x.Status == (int)BonusStatuses.Active &&
                                                     x.StartTime <= currentTime && x.FinishTime > currentTime &&
                                                   (!x.MaxGranted.HasValue || x.TotalGranted < x.MaxGranted) &&
                                                   (!x.MaxReceiversCount.HasValue || x.TotalReceiversCount < x.MaxReceiversCount));
            if (bonus == null)
                throw CreateException(Identity.LanguageId, Constants.Errors.BonusNotFound);
            if (checkPermission)
            {
                var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.CreateBonus
                });
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });
                if ((!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != bonus.PartnerId)))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            return bonus;
        }

        public Bonu GetBonusById(int bonusId, bool checkPermission = false)
        {
            var dbBonus = Db.Bonus.Include(x => x.BonusProducts).Include(x => x.AmountCurrencySettings).FirstOrDefault(x => x.Id == bonusId);
            if (dbBonus == null)
                throw CreateException(LanguageId, Constants.Errors.BonusNotFound);
            if (checkPermission)
            {
                var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewBonuses
                });
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });

                if (!bonusAccess.HaveAccessForAllObjects ||
                    (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != dbBonus.PartnerId)))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            return dbBonus;
        }

        public BonusProduct GetBonusProduct(int bonusId, int productId)
        {
            var bonusProduct = Db.BonusProducts.FirstOrDefault(x => x.BonusId == bonusId && x.ProductId == productId);
            return bonusProduct;
        }

        public ComplimentaryPointRate SaveComplimentaryPointRate(ComplimentaryPointRate complimentaryRate)
        {
            CheckPermission(Constants.Permissions.ViewComplimentaryRates);
            CheckPermission(Constants.Permissions.EditComplimentaryRates);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != complimentaryRate.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            
            var dbComplimentaryRate = Db.ComplimentaryPointRates.FirstOrDefault(x => x.PartnerId == complimentaryRate.PartnerId &&
                                                                                     x.ProductId == complimentaryRate.ProductId &&
                                                                                     x.CurrencyId == complimentaryRate.CurrencyId);
            if (dbComplimentaryRate == null)
            {
                if(complimentaryRate.Rate == -1)
                    return complimentaryRate;
                complimentaryRate.CreationDate = DateTime.UtcNow;
                complimentaryRate.LastUpdateDate = DateTime.UtcNow;
                Db.ComplimentaryPointRates.Add(complimentaryRate);
                Db.SaveChanges();
                CacheManager.RemoveComplimentaryPointRate(complimentaryRate.PartnerId, complimentaryRate.ProductId, complimentaryRate.CurrencyId);
                return complimentaryRate;
            }
            if (complimentaryRate.Rate == -1)
            {
                Db.ComplimentaryPointRates.Where(x => x.Id == dbComplimentaryRate.Id).DeleteFromQuery();
                CacheManager.RemoveComplimentaryPointRate(dbComplimentaryRate.PartnerId, dbComplimentaryRate.ProductId, dbComplimentaryRate.CurrencyId);
                return complimentaryRate;
            }
            dbComplimentaryRate.Rate = complimentaryRate.Rate;
            dbComplimentaryRate.LastUpdateDate = DateTime.UtcNow;
            Db.SaveChanges();
            CacheManager.RemoveComplimentaryPointRate(dbComplimentaryRate.PartnerId, dbComplimentaryRate.ProductId, dbComplimentaryRate.CurrencyId);
            return dbComplimentaryRate;
        }

        public List<ComplimentaryPointRate> GetComplimentaryPointRates(int partnerId, string currencyId)
        {
            CheckPermission(Constants.Permissions.ViewComplimentaryRates);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != partnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            return Db.ComplimentaryPointRates.Where(x => x.PartnerId == partnerId && x.CurrencyId == currencyId).ToList();
        }

        public Jackpot SaveJackpot(Jackpot jackpot)
        {
            CheckPermission(Constants.Permissions.ViewJackpot);
            CheckPermission(Constants.Permissions.EditJackpot);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (jackpot.PartnerId.HasValue && !partnerAccess.HaveAccessForAllObjects &&
                 partnerAccess.AccessibleIntegerObjects.All(x => x != jackpot.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            if (jackpot.JackpotSettings != null && jackpot.JackpotSettings.Any(x => x.Percent < -1))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

            var currentDate = DateTime.UtcNow;
            if (jackpot.Id > 0)
            {
                var dbJackpot = Db.Jackpots.Include(x => x.JackpotSettings).FirstOrDefault(x => x.Id == jackpot.Id);
                if (dbJackpot == null)
                    throw CreateException(LanguageId, Constants.Errors.BonusNotFound);
                var oldvalue = new
                {
                    dbJackpot.Id,
                    dbJackpot.Name,
                    dbJackpot.FinishTime,
                    dbJackpot.Amount,
                    dbJackpot.PartnerId,
                    dbJackpot.Type,
                    dbJackpot.WinAmount,
                    dbJackpot.CreationDate,
                    dbJackpot.LastUpdateDate,
                    JackpotSettings = dbJackpot.JackpotSettings.Select(x => new { x.Id, x.ProductId, x.Percent })
                };
                dbJackpot.Name = jackpot.Name;
                dbJackpot.FinishTime = jackpot.FinishTime;
                dbJackpot.LastUpdateDate = currentDate;
                Db.JackpotSettings.Where(x => x.JackpotId == jackpot.Id &&
                                          (!jackpot.JackpotSettings.Any(y => y.ProductId == x.ProductId) ||
                                            jackpot.JackpotSettings.Any(y => y.ProductId == x.ProductId && y.Percent == -1)))
                                  .DeleteFromQuery();
                foreach (var js in jackpot.JackpotSettings.Where(x => x.Percent != -1))
                {
                    var dbJackpotSetting = Db.JackpotSettings.AddIfNotExists(js, x => x.JackpotId == jackpot.Id && x.ProductId == js.ProductId);
                    dbJackpotSetting.Percent = js.Percent;
                }
                SaveChangesWithHistory((int)ObjectTypes.Jackpot, dbJackpot.Id, JsonConvert.SerializeObject(oldvalue));
                Db.SaveChanges();
                return dbJackpot;
            }
            if (jackpot.RightBorder - jackpot.LeftBorder < 10000) //?? depends on currency ratio
                throw CreateException(LanguageId, Constants.Errors.InvalidDataRange);
            if (jackpot.Type == (int)JackpotTypes.Progressive)
                jackpot.Amount = 0;
            else if (jackpot.Type == (int)JackpotTypes.Fixed)
            {
                if (jackpot.Amount <= 0)
                    throw CreateException(LanguageId, Constants.Errors.WrongOperationAmount);
                jackpot.WinAmount = "0";
            }
            else if (Enum.IsDefined(typeof(JackpotTypes), jackpot.Type))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            jackpot.CreationDate = currentDate;
            jackpot.LastUpdateDate = currentDate;
            jackpot.WinnerId = null;
            jackpot.WinAmount = string.Empty;
            Db.Jackpots.Add(jackpot);
            Db.SaveChanges();
            if (jackpot.Type == (int)JackpotTypes.Progressive)
            {
                var random = new Random();
                var secondaryAmount = random.Next(jackpot.LeftBorder, jackpot.RightBorder).ToString();
                jackpot.WinAmount = AESEncryptHelper.EncryptString(jackpot.Id.ToString(), secondaryAmount);
                Db.SaveChanges();
            }
            return jackpot;
        }

        public List<Jackpot> GetJackpots(int? jackpotId)
        {
            CheckPermission(Constants.Permissions.ViewJackpot);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (jackpotId.HasValue)
            {
                var dbJackpot = Db.Jackpots.Include(x => x.JackpotSettings).FirstOrDefault(x => x.Id == jackpotId.Value);
                if (dbJackpot == null)
                    throw CreateException(LanguageId, Constants.Errors.BonusNotFound);

                if (dbJackpot.PartnerId.HasValue && !partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != dbJackpot.PartnerId))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
                return new List<Jackpot> { dbJackpot };
            }
            if (partnerAccess.HaveAccessForAllObjects)
                return Db.Jackpots.Include(x => x.JackpotSettings).ToList();
            return Db.Jackpots.Include(x => x.JackpotSettings)
                              .Where(x => !x.PartnerId.HasValue || partnerAccess.AccessibleObjects.Any(y => y == x.PartnerId))
                              .ToList();
        }

        public List<Jackpot> GetJackpots(int partnerId)
        {
            return Db.Jackpots.Where(x => (!x.PartnerId.HasValue || x.PartnerId == partnerId) && !x.WinnerId.HasValue).ToList();
        }

        #region Affiliate System

        public void GiveFixedFeeCommission()
        {
            var fromDate = DateTime.UtcNow.AddMonths(-1);
            var clients = Db.ClientSettings.OrderByDescending(x => x.Id).Include(x => x.Client.AffiliateReferral).
                Where(x => x.Name == ClientSettings.AffiliateCommissionGranted && x.NumericValue == 0).Take(10000).ToList();
            var affiliateClients = clients.GroupBy(x => new { AffiliateId = Convert.ToInt32(x.Client.AffiliateReferral.AffiliateId), PartnerId = x.Client.PartnerId }).ToList();
            using (var affiliateService = new AffiliateService(this))
            {
                using (var documentBl = new DocumentBll(this))
                {
                    foreach (var ac in affiliateClients)
                    {
                        using (var transactionScope = CommonFunctions.CreateTransactionScope(5))
                        {
                            decimal totalGrantedAmount = 0;
                            var commission = Db.AffiliateCommissions.FirstOrDefault(x => x.AffiliateId == ac.Key.AffiliateId && x.CommissionType == (int)AffiliateCommissionTypes.FixedFee);
                           
                            if (commission == null)
                            {
                                foreach (var c in ac)
                                {
                                    c.NumericValue = 1;
                                }
                            }
                            else
                            {
                                foreach (var c in ac)
                                {
                                    if (commission.RequireVerification == null || commission.RequireVerification == false || 
                                        (commission.RequireVerification == true && c.Client.IsDocumentVerified &&
                                        c.Client.IsEmailVerified && c.Client.IsMobileNumberVerified))
                                    {
                                        var totalDeposit = ConvertCurrency(c.Client.CurrencyId, commission.CurrencyId, CacheManager.GetTotalDepositAmounts(c.ClientId, (int)PeriodsOfTime.All));
                                        if (commission.TotalDepositAmount == null || totalDeposit >= commission.TotalDepositAmount)
                                        {
                                            c.NumericValue = 2;
                                            if(commission.Amount != null)
                                                totalGrantedAmount += commission.Amount.Value;
                                        }
                                    }

                                    if (c.NumericValue == 0 && c.Client.CreationTime < fromDate)
                                        c.NumericValue = 1;
                                }
                            }

                            if (totalGrantedAmount > 0)
                            {
                                var currentDate = DateTime.UtcNow;
                                var input = new ClientOperation
                                {
                                    Amount = totalGrantedAmount,
                                    ExternalTransactionId = (int)AffiliateCommissionTypes.FixedFee + "_" + (currentDate.Year * (int)1000000 + currentDate.Month * 10000 + currentDate.Day * 100 + currentDate.Hour),
                                    OperationTypeId = (int)OperationTypes.AffiliateBonus,
                                    PartnerId = ac.Key.PartnerId,
                                    CurrencyId = commission.CurrencyId,
                                    AccountTypeId = (int)AccountTypes.AffiliateManagerBalance,
                                    Creator = ac.Key.AffiliateId
                                };
                                affiliateService.CreateDebitToAffiliate(ac.Key.AffiliateId, input, documentBl);
                            }
                            Db.SaveChanges();
                            transactionScope.Complete();
                        }
                    }
                }
            }
        }

        #endregion
    }
}