using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Helpers;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.AffiliateModels;
using IqSoft.CP.Common.Models.UserModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters.Affiliate;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Affiliates;
using IqSoft.CP.DAL.Models.Notification;
using IqSoft.CP.DataWarehouse;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Document = IqSoft.CP.DAL.Document;
using AffiliateReferral = IqSoft.CP.DAL.AffiliateReferral;
using System.Text.RegularExpressions;
using System.Transactions;
using IqSoft.CP.DAL.Filters;

namespace IqSoft.CP.BLL.Services
{
    public class AffiliateService : PermissionBll, IAffiliateService
    {
        public AffiliateService(SessionIdentity identity, ILog log)
            : base(identity, log)
        {

        }

        public AffiliateService(BaseBll baseBl)
            : base(baseBl)
        {

        }

        public Affiliate RegisterAffiliate(Affiliate newAffiliate, string reCaptcha)
        {
            if (newAffiliate.RegionId == 0)
                newAffiliate.RegionId = Constants.DefaultRegionId;
            if (string.IsNullOrEmpty(newAffiliate.LanguageId))
                newAffiliate.LanguageId = Constants.DefaultLanguageId;
            newAffiliate.MobileNumber = newAffiliate.MobileNumber ?? string.Empty;
            VerifyAffiliateFields(newAffiliate, reCaptcha);
            var currentDate = DateTime.UtcNow;
            var rand = new Random();
            var salt = rand.Next();
            newAffiliate.CreationTime  = currentDate;
            newAffiliate.LastUpdateTime  = currentDate;
            newAffiliate.Salt = salt;
            newAffiliate.PasswordHash = CommonFunctions.ComputeClientPasswordHash(newAffiliate.Password, salt);
            newAffiliate.State = (int)AffiliateStates.PendingForApproval;
            Db.Affiliates.Add(newAffiliate);
            Db.SaveChanges();
            return newAffiliate;
        }

        private void VerifyAffiliateFields(Affiliate affiliate, string reCaptcha)
        {
            if (!Enum.IsDefined(typeof(Gender), affiliate.Gender))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            if (string.IsNullOrWhiteSpace(affiliate.Email) || !IsValidEmail(affiliate.Email))
                throw CreateException(LanguageId, Constants.Errors.InvalidEmail);
            if (!string.IsNullOrEmpty(affiliate.MobileNumber) && !IsMobileNumber(affiliate.MobileNumber))
                throw CreateException(LanguageId, Constants.Errors.InvalidMobile);
            if (affiliate.Id == 0)
            {
                var dbAffiliate = Db.Affiliates.Where(x => x.PartnerId == affiliate.PartnerId &&
                                                      (x.Email.ToLower() == affiliate.Email.ToLower() ||
                                                      (!string.IsNullOrEmpty(affiliate.MobileNumber) && x.MobileNumber == affiliate.MobileNumber)))
                                           .FirstOrDefault();
                if (dbAffiliate != null)
                {
                    if (dbAffiliate.Email.ToLower() == affiliate.Email.ToLower())
                        throw CreateException(LanguageId, Constants.Errors.EmailExists);
                    if (!string.IsNullOrWhiteSpace(affiliate.MobileNumber) && dbAffiliate.MobileNumber == affiliate.MobileNumber)
                        throw CreateException(LanguageId, Constants.Errors.MobileExists);
                }
            }

            VerifyAffiliatePassword(affiliate);
            CheckSiteCaptcha(affiliate.PartnerId, reCaptcha);
        }

        private void VerifyAffiliatePassword(Affiliate affiliate)
        {
            var partner = CacheManager.GetPartnerById(affiliate.PartnerId);
            var partnerConfig = CacheManager.GetConfigKey(partner.Id, Constants.PartnerKeys.ProhibitPasswordContainingPersonalData);
            var unallowedKeys = new List<string>();
            if (!string.IsNullOrEmpty(partnerConfig) && partnerConfig == "1")
            {
                if (!string.IsNullOrEmpty(affiliate.FirstName))
                    unallowedKeys.Add(affiliate.FirstName.ToLower());
                if (!string.IsNullOrEmpty(affiliate.LastName))
                    unallowedKeys.Add(affiliate.LastName.ToLower());
                if (!string.IsNullOrEmpty(affiliate.UserName))
                    unallowedKeys.Add(affiliate.UserName.ToLower());
            }
            if (!Regex.IsMatch(affiliate.Password, partner.PasswordRegExp) || unallowedKeys.Any(affiliate.Password.ToLower().Contains))
                throw CreateException(LanguageId, Constants.Errors.PasswordContainsPersonalData);
        }

        public PagedModel<fnAffiliate> GetfnAffiliates(FilterfnAffiliate filter)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliates
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnAffiliate>>
                {
                    new CheckPermissionOutput<fnAffiliate>
                    {
                        AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                    }
                };

            Func<IQueryable<fnAffiliate>, IOrderedQueryable<fnAffiliate>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnAffiliate>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnAffiliate>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = aff => aff.OrderByDescending(x => x.Id);
            }

            return new PagedModel<fnAffiliate>
            {
                Entities = filter.FilterObjects(Db.fn_Affiliate(), orderBy),
                Count = filter.SelectedObjectsCount(Db.fn_Affiliate())
            };
        }
        public BllAffiliate GetAffiliateById(int id, bool checkPermission)
        {
            var response = new BllAffiliate();

            var affiliate = Db.Affiliates.FirstOrDefault(x => x.Id == id);
            if (affiliate == null)
                throw CreateException(LanguageId, Constants.Errors.AffiliateNotFound);
            if (checkPermission)
            {
                GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliates
                });
                GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.UpdateAffiliate
                });

                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });
                if (!partnerAccess.HaveAccessForAllObjects && !partnerAccess.AccessibleIntegerObjects.Contains(affiliate.PartnerId))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            var commissionPlan = Db.AffiliateCommissions.Where(x => x.AffiliateId == affiliate.Id).ToList();
            return affiliate.ToBllAffiliate(commissionPlan);
        }
        public Affiliate UpdateAffiliate(fnAffiliate input)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliates
            });
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.UpdateAffiliate
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            var affiliate = Db.Affiliates.FirstOrDefault(x => x.Id == input.Id);
            if (affiliate == null)
                throw CreateException(LanguageId, Constants.Errors.AffiliateNotFound);
            if (!partnerAccess.HaveAccessForAllObjects && !partnerAccess.AccessibleIntegerObjects.Contains(affiliate.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            if (affiliate.State != (int)AffiliateStates.PendingForApproval && input.State == (int)AffiliateStates.PendingForApproval)
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

            affiliate.State = input.State;
            affiliate.ClientId = input.ClientId;
            Db.SaveChanges();
            
            using (var notificationBll = new NotificationBll(Identity, Log))
            {
                notificationBll.SendNotificationMessage(new NotificationModel
                {
                    PartnerId = affiliate.PartnerId,
                    ObjectId = affiliate.Id,
                    ObjectTypeId = (int)ObjectTypes.Affiliate,
                    MobileOrEmail = affiliate.Email,
                    MessageType = (int)ClientMessageTypes.Email,
                    ClientInfoType = (int)ClientInfoTypes.AffiliateConfirmationEmail
                }, out int responseCode);
            }

            return affiliate;
        }

        public List<fnAccount> GetAffiliateAccounts(int affiliateId)
        {
            var affiliate = Db.Affiliates.FirstOrDefault(x => x.Id == affiliateId) ??
                throw CreateException(LanguageId, Constants.Errors.AffiliateNotFound);

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliates
            });
            var affiliaeAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliates,
                ObjectTypeId = ObjectTypes.Affiliate
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if ((!affiliaeAccess.HaveAccessForAllObjects && affiliaeAccess.AccessibleObjects.All(x => x != affiliateId)) ||
                (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != affiliate.PartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            return GetfnAccounts(new FilterfnAccount
            {
                ObjectId = affiliateId,
                ObjectTypeId = (int)ObjectTypes.Affiliate
            });
        }

        public void UpdateCommissionPlan(ApiAffiliateCommission input)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliates
            });
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.UpdateAffiliate
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            var affiliate = Db.Affiliates.FirstOrDefault(x => x.Id == input.AffiliateId) ??
                throw CreateException(LanguageId, Constants.Errors.AffiliateNotFound);
            if (!partnerAccess.HaveAccessForAllObjects && !partnerAccess.AccessibleIntegerObjects.Contains(affiliate.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var dbCommissions = Db.AffiliateCommissions.Where(x => x.AffiliateId == affiliate.Id).ToList();
            input.GetType().GetProperties().Where(x => !x.PropertyType.IsValueType && x.GetValue(input, null) != null).ToList()
                 .ForEach(commission =>
                 {
                     var commType = (AffiliateCommissionTypes)Enum.Parse(typeof(AffiliateCommissionTypes), commission.Name.Replace("Commission", string.Empty));
                     switch (commType)
                     {
                         case AffiliateCommissionTypes.FixedFee:
                             if (input.FixedFeeCommission.GetType().GetProperties()
                                                        .Any(y => y != null && !string.IsNullOrWhiteSpace(y.ToString()) &&
                                                                  y.GetValue(input.FixedFeeCommission, null) != null))
                             {
                                 var fixedFee = dbCommissions.FirstOrDefault(x => x.CommissionType == (int)commType);
                                 if (input.FixedFeeCommission.Amount < 0 || input.FixedFeeCommission.TotalDepositAmount <= 0)
                                     throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
                                 if (fixedFee == null)
                                 {
                                     Db.AffiliateCommissions.Add(new AffiliateCommission
                                     {
                                         AffiliateId = affiliate.Id,
                                         CommissionType = (int)AffiliateCommissionTypes.FixedFee,
                                         CurrencyId = input.FixedFeeCommission.CurrencyId,
                                         Amount = input.FixedFeeCommission.Amount,
                                         TotalDepositAmount = input.FixedFeeCommission.TotalDepositAmount,
                                         RequireVerification = input.FixedFeeCommission.RequireVerification
                                     });
                                 }
                                 else
                                 {
                                     fixedFee.CurrencyId = input.FixedFeeCommission.CurrencyId;
                                     fixedFee.Amount = input.FixedFeeCommission.Amount;
                                     fixedFee.TotalDepositAmount = input.FixedFeeCommission.TotalDepositAmount;
                                     fixedFee.RequireVerification = input.FixedFeeCommission.RequireVerification;
                                 }
                             }
                             break;
                         case AffiliateCommissionTypes.Deposit:
                             if (input.DepositCommission.GetType().GetProperties()
                                                        .Any(y => y != null && !string.IsNullOrWhiteSpace(y.ToString()) &&
                                                                  y.GetValue(input.DepositCommission, null) != null))
                             {
                                 var deposit = dbCommissions.FirstOrDefault(x => x.CommissionType == (int)commType);
                                 if (!input.DepositCommission.Percent.HasValue || input.DepositCommission.Percent <= 0 || input.DepositCommission.UpToAmount <= 0)
                                     throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
                                 if (deposit == null)
                                 {
                                     Db.AffiliateCommissions.Add(new AffiliateCommission
                                     {
                                         AffiliateId = affiliate.Id,
                                         CommissionType = (int)AffiliateCommissionTypes.Deposit,
                                         CurrencyId = input.DepositCommission.CurrencyId,
                                         Percent = input.DepositCommission.Percent,
                                         UpToAmount = input.DepositCommission.UpToAmount,
                                         DepositCount = input.DepositCommission.DepositCount
                                     });
                                 }
                                 else
                                 {
                                     deposit.Percent = input.DepositCommission.Percent;
                                     deposit.CurrencyId = input.DepositCommission.CurrencyId;
                                     deposit.UpToAmount = input.DepositCommission.UpToAmount;
                                     deposit.DepositCount = input.DepositCommission.DepositCount;
                                 }
                             }
                             break;
                         case AffiliateCommissionTypes.Turnover:
                         case AffiliateCommissionTypes.GGR:
                         case AffiliateCommissionTypes.NGR:
                             var value = commission.GetValue(input, null);
                             var commissionItems = ((IEnumerable)value).Cast<BetCommission>();
                             if (commissionItems != null && commissionItems.Any())
                             {
                                 if (commissionItems.Any(x => x.Percent <= 0))
                                     throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
                                 foreach (var c in commissionItems)
                                 {
                                     var dbItem = dbCommissions.FirstOrDefault(x => x.AffiliateId == affiliate.Id &&
                                                                                    x.CommissionType == (int)commType && x.ProductId == c.ProductId);
                                     if (dbItem == null)
                                     {
                                         if (c.Percent != null)
                                         {
                                             Db.AffiliateCommissions.Add(new AffiliateCommission
                                             {
                                                 AffiliateId = affiliate.Id,
                                                 CommissionType = (int)commType,
                                                 ProductId = c.ProductId,
                                                 Percent = c.Percent
                                             });
                                         }
                                     }
                                     else
                                     {
                                         if (c.Percent == null)
                                             Db.AffiliateCommissions.Remove(dbItem);
                                         else
                                             dbItem.Percent = c.Percent;
                                     }
                                 }
                             }
                             break;
                         default:
                             break;
                     }
                 });
            Db.SaveChanges();
        }

        public void GiveCommission(DateTime toDate, ILog log)
        {
            var currentTime = DateTime.UtcNow;
            var fromDate = DateTime.UtcNow.AddMonths(-1); // ??
            var tDate = toDate.Year * (int)1000000 + toDate.Month * 10000 + toDate.Day * 100 + toDate.Hour;
            var fDate = fromDate.Year * (int)1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
            try
            {
                using (var documentBl = new DocumentBll(this))
                {
                    using (var dwh = new IqSoftDataWarehouseEntities())
                    {
                        using (var transactionScope = new TransactionScope(TransactionScopeOption.Suppress))
                        {
                            var affiliates = Db.AffiliateCommissions.Where(x => (x.CommissionType == (int)AffiliateCommissionTypes.Turnover ||
                                                                         x.CommissionType == (int)AffiliateCommissionTypes.GGR ||
                                                                         x.CommissionType == (int)AffiliateCommissionTypes.NGR) && x.ProductId.HasValue)
                                                            .Select(x => x.AffiliateId.ToString()).Distinct().ToList();
                            Db.AffiliateReferrals.Include(x => x.Clients).Where(x => x.Type == (int)AffiliateReferralTypes.InternalAffiliatePlatform &&
                                                               x.AffiliatePlatformId % 100 == 0 && affiliates.Contains(x.AffiliateId))
                                                 .GroupBy(x => x.AffiliateId)
                                                 .Select(x => new { AffiliateId = x.Key, Clients = x.SelectMany(y => y.Clients.Select(z => z.Id)).ToList() }).ToList()
                            .ForEach(affiliateClients =>
                            {
                                var activies = dwh.Bets.Where(x => affiliateClients.Clients.Contains(x.ClientId.Value) &&
                                                              x.State != (int)BetDocumentStates.Deleted &&
                                                              x.State != (int)BetDocumentStates.Uncalculated &&
                                                              x.BetDate >= fDate && x.BetDate < tDate)
                                                       .GroupBy(x => new { x.ProductId, x.CurrencyId })
                                                       .Select(x => new
                                                       {
                                                           x.Key.ProductId,
                                                           x.Key.CurrencyId,
                                                           TotalBetAmount = x.Select(y => y.BetAmount).DefaultIfEmpty(0).Sum(),
                                                           TotalWinAmount = x.Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                                                           BonusBetAmount = x.Where(y => y.BonusId.HasValue && y.BonusId.Value > 0).Select(y => y.BonusAmount ?? 0).DefaultIfEmpty(0).Sum(),
                                                           BonusWinAmount = x.Where(y => y.BonusId.HasValue && y.BonusId.Value > 0).Select(y => y.BonusWinAmount ?? 0).DefaultIfEmpty(0).Sum(),
                                                       }).ToList();
                                var affId = Convert.ToInt32(affiliateClients.AffiliateId);
                                Db.AffiliateCommissions.Where(x => x.AffiliateId == affId && x.Percent > 0 && x.ProductId.HasValue &&
                                                                                   (x.CommissionType == (int)AffiliateCommissionTypes.Turnover ||
                                                                                    x.CommissionType == (int)AffiliateCommissionTypes.GGR ||
                                                                                    x.CommissionType == (int)AffiliateCommissionTypes.NGR))
                                                                        .GroupBy(x => x.CommissionType)
                                                                        .Select(x => new { CommissionType = x.Key, Commission = x.Select(y => new { y.ProductId, y.Percent }) }).ToList()
                                .ForEach(commission =>
                                {
                                    var currencyAmountPairs = new Dictionary<string, decimal>();
                                    var productCommission = commission.Commission;
                                    switch (commission.CommissionType)
                                    {
                                        case (int)AffiliateCommissionTypes.Turnover:
                                            foreach (var bets in activies)
                                            {
                                                var productIds = CacheManager.GetProductById(bets.ProductId).Path.Split('/')
                                                .Where(x => int.TryParse(x, out int i)).Select(x => Convert.ToInt32(x)).ToList();
                                                for (int i = productIds.Count()-1; i>=0; --i)
                                                {
                                                    var c = productCommission.FirstOrDefault(x => x.ProductId == productIds[i]);
                                                    if (c!= null)
                                                    {
                                                        var turnover = bets.TotalBetAmount * c.Percent / 100 ?? 0;
                                                        if (currencyAmountPairs.ContainsKey(bets.CurrencyId))
                                                            currencyAmountPairs[bets.CurrencyId] += turnover;
                                                        else
                                                            currencyAmountPairs.Add(bets.CurrencyId, turnover);
                                                        break;
                                                    }
                                                }
                                            }
                                            break;
                                        case (int)AffiliateCommissionTypes.GGR:
                                            foreach (var bets in activies)
                                            {
                                                var productIds = CacheManager.GetProductById(bets.ProductId).Path.Split('/')
                                              .Where(x => int.TryParse(x, out int i)).Select(x => Convert.ToInt32(x)).ToList();
                                                for (int i = productIds.Count()-1; i>=0; --i)
                                                {
                                                    var c = productCommission.FirstOrDefault(x => x.ProductId == productIds[i]);
                                                    if (c!= null)
                                                    {
                                                        var ggr = (bets.TotalBetAmount - bets.TotalWinAmount) * c.Percent / 100  ?? 0;
                                                        if (currencyAmountPairs.ContainsKey(bets.CurrencyId))
                                                            currencyAmountPairs[bets.CurrencyId] += ggr;
                                                        else
                                                            currencyAmountPairs.Add(bets.CurrencyId, ggr);
                                                        break;
                                                    }
                                                }
                                            }
                                            break;
                                        case (int)AffiliateCommissionTypes.NGR:
                                            foreach (var bets in activies)
                                            {
                                                var productIds = CacheManager.GetProductById(bets.ProductId).Path.Split('/')
                                                 .Where(x => int.TryParse(x, out int i)).Select(x => Convert.ToInt32(x)).ToList();
                                                for (int i = productIds.Count()-1; i>=0; --i)
                                                {
                                                    var c = productCommission.FirstOrDefault(x => x.ProductId == productIds[i]);
                                                    if (c!= null)
                                                    {
                                                        var ngr = bets.TotalBetAmount - bets.BonusBetAmount - (bets.TotalWinAmount - bets.BonusWinAmount) * c.Percent / 100  ?? 0;
                                                        if (currencyAmountPairs.ContainsKey(bets.CurrencyId))
                                                            currencyAmountPairs[bets.CurrencyId] += ngr;
                                                        else
                                                            currencyAmountPairs.Add(bets.CurrencyId, ngr);
                                                        break;
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                    if (currencyAmountPairs.Any(x => x.Value > 0))
                                    {
                                        var affiliate = Db.Affiliates.FirstOrDefault(x => x.Id == affId);
                                        foreach (var ca in currencyAmountPairs)
                                        {
                                            if (ca.Value > 0)
                                            {
                                                var input = new ClientOperation
                                                {
                                                    Amount = ca.Value,
                                                    ExternalTransactionId = commission.CommissionType + "_" + (currentTime.Year * (int)1000000 + currentTime.Month * 10000 + currentTime.Day * 100 + currentTime.Hour),
                                                    OperationTypeId = (int)OperationTypes.AffiliateBonus,
                                                    PartnerId = affiliate.PartnerId,
                                                    CurrencyId = ca.Key,
                                                    AccountTypeId = (int)AccountTypes.AffiliateManagerBalance,
                                                    Creator = affiliate.Id
                                                };
                                                CreateDebitToAffiliate(affiliate.Id, input, documentBl);
                                            }
                                        }
                                    }
                                    Db.SaveChanges();
                                });
                            });
                            transactionScope.Complete();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e);
                log.Error(fDate + "_" + tDate);
            }
        }

        public SessionIdentity LoginAffiliate(LoginInput loginInput)
        {
            var currentTime = DateTime.UtcNow;
            var affiliate = Db.Affiliates.FirstOrDefault(x => x.UserName == loginInput.Identifier && x.PartnerId == loginInput.PartnerId);
            if (affiliate == null)
                throw CreateException(loginInput.LanguageId, Constants.Errors.WrongLoginParameters);
            if (affiliate.State != (int)AffiliateStates.Active)
                throw CreateException(loginInput.LanguageId, Constants.Errors.UserBlocked);

            var passwordHash = CommonFunctions.ComputeClientPasswordHash(loginInput.Password, affiliate.Salt);
            if (affiliate.PasswordHash != passwordHash)
            {
                Log.Info("LoginAffiliate_" + affiliate.PasswordHash + "_" + passwordHash);
                throw CreateException(loginInput.LanguageId, Constants.Errors.WrongLoginParameters);
            }

            Db.UserSessions.Where(x => x.AffiliateId == affiliate.Id && x.State == (int)SessionStates.Active).
                UpdateFromQuery(x => new UserSession { State = (int)SessionStates.Inactive, EndTime = currentTime, LogoutType = (int)LogoutTypes.MultipleDevice });
            var userSession = new UserSession
            {
                AffiliateId = affiliate.Id,
                LanguageId = loginInput.LanguageId ?? Constants.Languages.English,
                Ip = loginInput.Ip,
                CashDeskId = loginInput.CashDeskId,
                State = (int)SessionStates.Active,
                StartTime = currentTime,
                LastUpdateTime = currentTime,
                Token = Guid.NewGuid().ToString()
            };
            Db.UserSessions.Add(userSession);
            Db.SaveChanges();

            var newIdentity = new SessionIdentity
            {
                PartnerId = affiliate.PartnerId,
                LoginIp = loginInput.Ip,
                LanguageId = loginInput.LanguageId ?? Constants.Languages.English,
                SessionId = userSession.Id,
                Id = affiliate.Id,
                Token = userSession.Token
            };

            return newIdentity;
        }
        public List<AffiliateReferral> GetReferralLinks()
        {
            var platformId = Identity.PartnerId * 100;
            return Db.AffiliateReferrals.Where(x => x.AffiliatePlatformId == platformId &&
                x.AffiliateId == Identity.Id.ToString() && x.Type == (int)AffiliateReferralTypes.InternalAffiliatePlatform).OrderByDescending(x => x.Id).ToList();
        }
        public AffiliateReferral GetReferralLinkById(int id)
        {
            return Db.AffiliateReferrals.FirstOrDefault(x => x.Id == id);
        }
        public AffiliateReferral GenerateNewLink()
        {
            var platformId = Identity.PartnerId * 100;
            var count = Db.AffiliateReferrals.Where(x => x.AffiliatePlatformId == platformId &&
                x.AffiliateId == Identity.Id.ToString() && x.Type == (int)AffiliateReferralTypes.InternalAffiliatePlatform).Count();
            if (count >= Constants.AffiliateMaximumLinksCount)
                throw CreateException(Identity.LanguageId, Constants.Errors.MaxLimitExceeded);

            var item = new AffiliateReferral
            {
                AffiliatePlatformId = Identity.PartnerId * 100,
                AffiliateId = Identity.Id.ToString(),
                RefId = CommonFunctions.GetRandomString(20),
                Type = (int)AffiliateReferralTypes.InternalAffiliatePlatform,
                CreationTime = DateTime.UtcNow,
                Status = (int)AffiliateReferralStatuses.Active,
                LastProcessedBonusTime = null
            };
            Db.AffiliateReferrals.Add(item);
            Db.SaveChanges();
            return item;
        }

        public Affiliate GetAffiliateByIdentifier(int partnerId, string identifier)
        {
            var query = Db.Affiliates.Where(x => x.PartnerId == partnerId);
            if (IsValidEmail(identifier))
                query = query.Where(x => x.Email == identifier);
            else
                query = query.Where(x => x.UserName == identifier);
            return query.FirstOrDefault();
        }

        public Affiliate RecoverPassword(int partnerId, string recoveryToken, string newPassword)
        {
            var clientInfo = Db.ClientInfoes.FirstOrDefault(x => x.Data == recoveryToken && x.PartnerId == partnerId && x.ObjectTypeId == (int)ObjectTypes.Affiliate) ??
                throw CreateException(LanguageId, Constants.Errors.WrongToken);
            if (clientInfo.State == (int)ClientInfoStates.Expired)
                throw CreateException(LanguageId, Constants.Errors.TokenExpired);
            var affiliate = Db.Affiliates.First(x => x.Id == clientInfo.ObjectId);
            VerifyAffiliatePassword(affiliate);
            affiliate.PasswordHash = CommonFunctions.ComputeClientPasswordHash(newPassword, affiliate.Salt);
            affiliate.LastUpdateTime = DateTime.UtcNow;
            clientInfo.State = (int)ClientInfoStates.Expired;
            Db.SaveChanges();
            return affiliate;
        }

        public void ChangeAffiliatePassword(int affiliateId, string oldPassword, string newPassword)
        {
            var dbAffiliate = Db.Affiliates.FirstOrDefault(x => x.Id == affiliateId);
            if (dbAffiliate == null)
                throw CreateException(LanguageId, Constants.Errors.AffiliateNotFound);
            //Check regex
            var newPasswordHash = CommonFunctions.ComputeUserPasswordHash(newPassword, dbAffiliate.Salt);
            var oldPasswordHash = CommonFunctions.ComputeUserPasswordHash(oldPassword, dbAffiliate.Salt);
            if (oldPasswordHash != dbAffiliate.PasswordHash)
                throw CreateException(LanguageId, Constants.Errors.WrongPassword);
            if (newPasswordHash == oldPasswordHash)
                throw CreateException(LanguageId, Constants.Errors.PasswordMatches);
            var currentTime = GetServerDate();
            dbAffiliate.PasswordHash = newPasswordHash;
            dbAffiliate.LastUpdateTime = currentTime;
            dbAffiliate.LastUpdateTime = currentTime;
            Db.SaveChanges();
        }

        public Document CreateDebitToAffiliate(int affiliateId, ClientOperation transaction, DocumentBll documentBl)
        {
            var operation = new Operation
            {
                Amount = transaction.Amount,
                CurrencyId = transaction.CurrencyId,
                Type = transaction.OperationTypeId,
                ExternalTransactionId = transaction.ExternalTransactionId,
                GameProviderId = transaction.GameProviderId,
                Info = transaction.Info,
                ClientId = transaction.ClientId,
                PartnerProductId = transaction.PartnerProductId,
                ParentId = transaction.ParentDocumentId,
                RoundId = transaction.RoundId,
                ProductId = transaction.ProductId,
                State = transaction.State,
                TicketInfo = transaction.Info,
                Creator = transaction.Creator,
                OperationItems = new List<OperationItem>()
            };

            operation.OperationItems.Add(new OperationItem
            {
                AccountTypeId = transaction.AccountTypeId,
                ObjectId = affiliateId,
                ObjectTypeId = (int)ObjectTypes.Affiliate,
                Amount = transaction.Amount,
                CurrencyId = transaction.CurrencyId,
                Type = (int)TransactionTypes.Debit,
                OperationTypeId = transaction.OperationTypeId,

            });

            operation.OperationItems.Add(new OperationItem
            {
                ObjectId = transaction.PartnerId,
                ObjectTypeId = (int)ObjectTypes.Partner,
                AccountTypeId = (int)AccountTypes.PartnerBalance,
                Amount = transaction.Amount,
                CurrencyId = transaction.CurrencyId,
                Type = (int)TransactionTypes.Credit,
                OperationTypeId = transaction.OperationTypeId
            });

            var document = documentBl.CreateDocument(operation);
            Db.SaveChanges();
            return document;
        }

        public Document CreateDebitToAffiliateClient(ClientCorrectionInput correction, DocumentBll documentBl)
        {
            var client = CacheManager.GetClientById(correction.ClientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            var acc = GetAccount(correction.AccountId.Value);
            if(acc == null || acc.ObjectId != Identity.Id || acc.TypeId != (int)AccountTypes.AffiliateManagerBalance)
                throw CreateException(LanguageId, Constants.Errors.AccountNotFound);
            Document document = null;
            using (var scope = CommonFunctions.CreateTransactionScope())
            {
                var operation = new Operation
                {
                    Type = (int)OperationTypes.DebitCorrectionOnClient,
                    Creator = Identity.Id,
                    Info = correction.Info,
                    ClientId = correction.ClientId,
                    Amount = correction.Amount,
                    CurrencyId = acc.CurrencyId,
                    ExternalTransactionId = correction.ExternalTransactionId,
                    ProductId = correction.ProductId,
                    OperationItems = new List<OperationItem>()
                };

                var item = new OperationItem
                {
                    AccountTypeId = (int)AccountTypes.ClientUnusedBalance,
                    ObjectId = client.Id,
                    ObjectTypeId = (int)ObjectTypes.Client,
                    Amount = ConvertCurrency(acc.CurrencyId, client.CurrencyId, correction.Amount),
                    CurrencyId = client.CurrencyId,
                    Type = (int)TransactionTypes.Debit,
                    OperationTypeId = (int)OperationTypes.DebitCorrectionOnClient
                };
                operation.OperationItems.Add(item);

                var affiliate = Db.Affiliates.FirstOrDefault(x => x.Id == Identity.Id);
                if (affiliate == null)
                    throw CreateException(LanguageId, Constants.Errors.AffiliateNotFound);

                item = new OperationItem
                {
                    AccountTypeId = (int)AccountTypes.AffiliateManagerBalance,
                    ObjectId = Identity.Id,
                    ObjectTypeId = (int)ObjectTypes.Affiliate,
                    Amount = correction.Amount,
                    CurrencyId = acc.CurrencyId,
                    Type = (int)TransactionTypes.Credit,
                    OperationTypeId = (int)OperationTypes.DebitCorrectionOnClient
                };
                operation.OperationItems.Add(item);
                document = documentBl.CreateDocument(operation);
                Db.SaveChanges();
                scope.Complete();
            }
            return document;
        }

        public Document CreateDebitOnAffiliate(TransferInput transferInput, DocumentBll documentBl)
        {
            var affiliate = Db.Affiliates.FirstOrDefault(x => x.Id == transferInput.AffiliateId.Value) ??
               throw CreateException(LanguageId, Constants.Errors.AffiliateNotFound);
            CheckPermission(Constants.Permissions.CreateDebitCorrectionOnAffiliate);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var checkAffPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliate,
                ObjectTypeId = ObjectTypes.Affiliate
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != affiliate.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            if (!checkAffPermission.HaveAccessForAllObjects && checkAffPermission.AccessibleObjects.All(x => x != transferInput.AffiliateId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var userPermissions = CacheManager.GetUserPermissions(Identity.Id);
            var permission = userPermissions.FirstOrDefault(x => x.PermissionId == Constants.Permissions.EditPartnerAccounts || x.IsAdmin);

            var operation = new Operation
            {
                Type = (int)OperationTypes.DebitCorrectionOnAffiliate,
                Creator = Identity.Id,
                UserId = transferInput.AffiliateId,
                Amount = transferInput.Amount,
                CurrencyId = transferInput.CurrencyId,
                OperationItems = new List<OperationItem>()
            };
            var item = new OperationItem
            {
                AccountTypeId = (int)AccountTypes.AffiliateManagerBalance,
                ObjectId = affiliate.Id,
                ObjectTypeId = (int)ObjectTypes.Affiliate,
                Amount = transferInput.Amount,
                CurrencyId = transferInput.CurrencyId,
                Type = (int)TransactionTypes.Debit,
                OperationTypeId = (int)OperationTypes.DebitCorrectionOnAffiliate
            };
            operation.OperationItems.Add(item);

            item = new OperationItem
            {
                AccountTypeId = permission != null ? (int)AccountTypes.PartnerBalance : (int)AccountTypes.UserBalance,
                ObjectId = permission != null ? Identity.PartnerId : Identity.Id,
                ObjectTypeId = permission != null ? (int)ObjectTypes.Partner : (int)ObjectTypes.User,
                Amount = transferInput.Amount,
                CurrencyId = transferInput.CurrencyId,
                Type = (int)TransactionTypes.Credit,
                OperationTypeId = (int)OperationTypes.DebitCorrectionOnUser
            };
            operation.OperationItems.Add(item);
            var document = documentBl.CreateDocument(operation);
            Db.SaveChanges();
            return document;
        }

        public Document CreateCreditOnAffiliate(TransferInput transferInput, DocumentBll documentBl)
        {
            var affiliate = Db.Affiliates.FirstOrDefault(x => x.Id == transferInput.AffiliateId.Value) ??
               throw CreateException(LanguageId, Constants.Errors.AffiliateNotFound);
            var creator = CacheManager.GetUserById(Identity.Id);
            CheckPermission(Constants.Permissions.CreateDebitCorrectionOnAffiliate);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var checkAffPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliate,
                ObjectTypeId = ObjectTypes.Affiliate
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != affiliate.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            if (!checkAffPermission.HaveAccessForAllObjects && checkAffPermission.AccessibleObjects.All(x => x != transferInput.AffiliateId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var userPermissions = CacheManager.GetUserPermissions(Identity.Id);
            var permission = userPermissions.FirstOrDefault(x => x.PermissionId == Constants.Permissions.EditPartnerAccounts || x.IsAdmin);

            var operation = new Operation
            {
                Type = (int)OperationTypes.CreditCorrectionOnAffiliate,
                Creator = Identity.Id,
                Info = transferInput.Info,
                UserId = transferInput.AffiliateId,
                Amount = transferInput.Amount,
                CurrencyId = transferInput.CurrencyId,
                OperationItems = new List<OperationItem>()
            };
            var item = new OperationItem
            {
                AccountTypeId = permission != null ? (int)AccountTypes.PartnerBalance : (int)AccountTypes.UserBalance,
                ObjectId = permission != null ? Identity.PartnerId : Identity.Id,
                ObjectTypeId = permission != null ? (int)ObjectTypes.Partner : (int)ObjectTypes.User,
                Amount = transferInput.Amount,
                CurrencyId = transferInput.CurrencyId,
                Type = (int)TransactionTypes.Debit,
                OperationTypeId = (int)OperationTypes.CreditCorrectionOnAffiliate
            };
            operation.OperationItems.Add(item);
            item = new OperationItem
            {
                AccountTypeId = (int)AccountTypes.AffiliateManagerBalance,
                ObjectId = affiliate.Id,
                ObjectTypeId = (int)ObjectTypes.User,
                Amount = transferInput.Amount,
                CurrencyId = transferInput.CurrencyId,
                Type = (int)TransactionTypes.Credit,
                OperationTypeId = (int)OperationTypes.CreditCorrectionOnAffiliate
            };
            operation.OperationItems.Add(item);
            var document = documentBl.CreateDocument(operation);
            Db.SaveChanges();
            return document;
        }

        #region ExternalAffiliates

        public IQueryable<DAL.AffiliatePlatform> GetAffiliatePlatforms()
        {
            return Db.AffiliatePlatforms.Where(x => Constants.ReportingAffiliates.Contains(x.Name));
        }

        public IQueryable<DAL.Client> GetClients(List<int> affiliatePlatforms, DateTime fromDate, DateTime toDate)
        {
            var currentTime = DateTime.UtcNow;
            return Db.Clients.Where(x => affiliatePlatforms.Contains(x.AffiliateReferral.AffiliatePlatform.Id) &&
                                                                       x.CreationTime > fromDate && x.CreationTime < toDate);
        }

        public List<ClientActivityModel> GetClientActivity(List<AffiliatePlatformModel> affClients, int brandId, DateTime fromDate, DateTime upToDate)
        {
            var affiliateClientActivies = new List<ClientActivityModel>();
            var fDate = fromDate.Year * (long)1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
            var fPaymentDate = fromDate.Year * (long)100000000 + fromDate.Month * 1000000 + fromDate.Day * 10000 + fromDate.Hour * 100 + fromDate.Minute;
            var tDate = upToDate.Year * (long)1000000 + upToDate.Month * 10000 + upToDate.Day * 100 + upToDate.Hour;
            var tPaymentDate = upToDate.Year * (long)100000000 + upToDate.Month * 1000000 + upToDate.Day * 10000 + upToDate.Hour * 100 + upToDate.Minute;
            var dateString = fromDate.ToString("yyyy-MM-dd");
            using (var dwh = new IqSoftDataWarehouseEntities())
            {
                foreach (var client in affClients)
                {
                    var clientActivityModel = new ClientActivityModel
                    {
                        CustomerId = client.ClientId,
                        BTag = client.ClickId,
                        ActivityDate = dateString,
                        BrandId = brandId,
                        CurrencyId = client.CurrencyId
                    };

                    var paymentData = Db.PaymentRequests.Where(x => x.ClientId == client.ClientId &&
                                                                   (x.Status == (int)PaymentRequestStates.Approved ||
                                                                    x.Status == (int)PaymentRequestStates.ApprovedManually) &&
                                                                    x.Date >= fPaymentDate && x.Date < tPaymentDate)
                                                        .GroupBy(x => x.Type)
                                                        .Select(x => new
                                                        {
                                                            Type = x.Key,
                                                            Amount = x.Sum(y => y.Amount),
                                                            Count = x.Count()
                                                        }).ToList();
                    if (paymentData.Count != 0)
                    {
                        clientActivityModel.TotalDepositsAmount = paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Deposit || x.Type == (int)PaymentRequestTypes.ManualDeposit)
                                                                             .Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.TotalWithdrawalsAmount = paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Withdraw).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.TotalDepositsCount = paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Deposit || x.Type == (int)PaymentRequestTypes.ManualDeposit)
                                                                           .Select(x => x.Count).DefaultIfEmpty(0).Sum();
                    }
                    var activies = dwh.Bets.Where(x => x.ClientId == client.ClientId &&
                                                      x.State != (int)BetDocumentStates.Deleted &&
                                                      x.State != (int)BetDocumentStates.Uncalculated &&
                                                      x.BetDate >= fDate && x.BetDate < tDate)
                                          .GroupBy(x => x.ProductId == Constants.SportsbookProductId ? 1 : 2)
                                          .Select(x => new
                                          {
                                              ProductId = x.Key,
                                              BetAmount = x.Where(y => !y.BonusId.HasValue || y.BonusId == 0).Select(y => y.BetAmount).DefaultIfEmpty(0).Sum(),
                                              WinAmount = x.Where(y => !y.BonusId.HasValue || y.BonusId == 0).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                                              BonusBetAmount = x.Where(y => y.BonusId.HasValue && y.BonusId.Value > 0).Select(y => y.BetAmount).DefaultIfEmpty(0).Sum(),
                                              BonusWinAmount = x.Where(y => y.BonusId.HasValue && y.BonusId.Value > 0).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                                              Count = x.Count()
                                          });
                    if (activies.Count() != 0)
                    {
                        clientActivityModel.SportGrossRevenue = activies.Where(y => y.ProductId == 1).Select(y => y.BetAmount - y.WinAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.CasinoGrossRevenue = activies.Where(y => y.ProductId == 2).Select(y => y.BetAmount - y.WinAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.SportBonusBetsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.CasinoBonusBetsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.SportBonusWinsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.CasinoBonusWinsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.SportTotalWinsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.CasinoTotalWinsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.TotalBetsCount = activies.Select(y => y.Count).DefaultIfEmpty(0).Sum();
                    }
                    affiliateClientActivies.Add(clientActivityModel);
                }
            }
            return affiliateClientActivies;
        }

        public List<NetreferActivityModel> GetNetreferClientActivity(List<AffiliatePlatformModel> affClients, int brandId, DateTime fromDate, DateTime upToDate)
        {
            var affiliateClientActivies = new List<NetreferActivityModel>();
            var fDate = fromDate.Year * (long)1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
            var fPaymentDate = fromDate.Year * (long)100000000 + fromDate.Month * 1000000 + fromDate.Day * 10000 + fromDate.Hour * 100 + fromDate.Minute;
            var tDate = upToDate.Year * (long)1000000 + upToDate.Month * 10000 + upToDate.Day * 100 + upToDate.Hour;
            var tPaymentDate = upToDate.Year * (long)100000000 + upToDate.Month * 1000000 + upToDate.Day * 10000 + upToDate.Hour * 100 + upToDate.Minute;
            var dateString = fromDate.ToString("dd/MM/yyyy");
            using (var dwh = new IqSoftDataWarehouseEntities())
            {
                foreach (var client in affClients)
                {
                    var transactions = Db.Documents.Where(x => x.ClientId == client.ClientId &&
                                               (x.OperationTypeId == (int)OperationTypes.BonusWin ||
                                                x.OperationTypeId == (int)OperationTypes.CreditCorrectionOnClient ||
                                                x.OperationTypeId == (int)OperationTypes.DebitCorrectionOnClient) &&
                                                x.Date > fDate && x.Date <= tDate).GroupBy(x => x.OperationTypeId)
                                                .ToDictionary(x => x.Key, x => x.Select(y => y.Amount).DefaultIfEmpty(0).Sum());

                    var clientActivityModel = new NetreferActivityModel
                    {
                        CustomerId = client.ClientId,
                        BTag = client.ClickId,
                        ActivityDate = dateString,
                        BrandID = brandId,
                        CurrencyId = client.CurrencyId
                    };
                    var addItem = false;
                    var transactionsCount = 0;
                    var paymentData = Db.PaymentRequests.Where(x => x.ClientId == client.ClientId &&
                                                                   (x.Status == (int)PaymentRequestStates.Approved ||
                                                                    x.Status == (int)PaymentRequestStates.ApprovedManually) &&
                                                                    x.Date >= fPaymentDate && x.Date < tPaymentDate)
                                                        .GroupBy(x => x.Type)
                                                        .Select(x => new
                                                        {
                                                            Type = x.Key,
                                                            Amount = x.Sum(y => y.Amount),
                                                            Count = x.Count()
                                                        }).ToList();
                    if (paymentData.Count != 0)
                    {
                        addItem = true;
                        clientActivityModel.Deposit = ConvertCurrency(client.CurrencyId, Constants.Currencies.Euro,
                                                                      paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Deposit || x.Type == (int)PaymentRequestTypes.ManualDeposit)
                                                                                 .Select(x => x.Amount).DefaultIfEmpty(0).Sum());
                        clientActivityModel.Withdrawals = ConvertCurrency(client.CurrencyId, Constants.Currencies.Euro,
                                                                          paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Withdraw).Select(x => x.Amount).DefaultIfEmpty(0).Sum());
                        transactionsCount = paymentData.Select(x => x.Count).DefaultIfEmpty(0).Sum();
                    }
                    var activies = dwh.Bets.Where(x => x.ClientId == client.ClientId &&
                                                      x.State != (int)BetDocumentStates.Deleted &&
                                                      x.State != (int)BetDocumentStates.Uncalculated &&
                                                      x.BetDate >= fDate && x.BetDate < tDate)
                                          .Select(x => new
                                          {
                                              BetAmount = (x.BonusId == null || x.BonusId == 0) ? x.BetAmount : 0,
                                              WinAmount = (x.BonusId == null || x.BonusId == 0) ? x.WinAmount : 0,
                                              BonusBetAmount = (x.BonusId.HasValue && x.BonusId.Value > 0) ? x.BetAmount : 0
                                          });
                    if (activies.Count() != 0)
                    {
                        addItem = true;
                        clientActivityModel.GrossRevenue = ConvertCurrency(client.CurrencyId, Constants.Currencies.Euro,
                                                            activies.Select(y => y.BetAmount - y.WinAmount).DefaultIfEmpty(0).Sum());
                        clientActivityModel.Turnover = ConvertCurrency(client.CurrencyId, Constants.Currencies.Euro,
                                                       activies.Select(y => y.BetAmount).DefaultIfEmpty(0).Sum());
                        clientActivityModel.Payout = ConvertCurrency(client.CurrencyId, Constants.Currencies.Euro,
                                                     activies.Select(y => y.WinAmount).DefaultIfEmpty(0).Sum());
                        transactionsCount += activies.Count();
                    }
                    if (addItem)
                    {
                        clientActivityModel.Adjustments = ConvertCurrency(client.CurrencyId, Constants.Currencies.Euro,
                                                        (transactions.ContainsKey((int)OperationTypes.CreditCorrectionOnClient) ? transactions[(int)OperationTypes.CreditCorrectionOnClient] : 0));
                        clientActivityModel.Bonuses = ConvertCurrency(client.CurrencyId, Constants.Currencies.Euro,
                                                      (transactions.ContainsKey((int)OperationTypes.BonusWin) ? transactions[(int)OperationTypes.BonusWin] : 0));
                        clientActivityModel.Transactions = transactionsCount;
                        affiliateClientActivies.Add(clientActivityModel);
                    }
                }
            }
            return affiliateClientActivies;
        }

        public List<MyAffiliateClientActivityModel> GetMyAffiliateClientActivity(List<AffiliatePlatformModel> affClients, int brandId, DateTime fromDate, DateTime upToDate)
        {
            var affiliateClientActivies = new List<MyAffiliateClientActivityModel>();
            var fDate = fromDate.Year * (long)1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
            var fPaymentDate = fromDate.Year * (long)100000000 + fromDate.Month * 1000000 + fromDate.Day * 10000 + fromDate.Hour * 100 + fromDate.Minute;
            var tDate = upToDate.Year * (long)1000000 + upToDate.Month * 10000 + upToDate.Day * 100 + upToDate.Hour;
            var tPaymentDate = upToDate.Year * (long)100000000 + upToDate.Month * 1000000 + upToDate.Day * 10000 + upToDate.Hour * 100 + upToDate.Minute;
            var clientIds = affClients.Select(x => x.ClientId).ToList();
            var clientTransactions = Db.Documents.Where(x => clientIds.Contains(x.ClientId.Value) &&
                                             (x.OperationTypeId == (int)OperationTypes.BonusWin ||
                                              x.OperationTypeId == (int)OperationTypes.CreditCorrectionOnClient ||
                                              x.OperationTypeId == (int)OperationTypes.DebitCorrectionOnClient) &&
                                              x.Date > fDate && x.Date <= tDate)
                                  .GroupBy(x => x.ClientId)
                                  .ToDictionary(x => x.Key, x =>
                                  new
                                  {
                                      BonusWin = x.Where(y => y.OperationTypeId == (int)OperationTypes.BonusWin).Select(y => y.Amount).DefaultIfEmpty(0).Sum(),
                                      CreditCorrectionOnClient = x.Where(y => y.OperationTypeId == (int)OperationTypes.CreditCorrectionOnClient && y.TypeId != (int)OperationTypes.ChargeBack)
                                                                  .Select(y => y.Amount).DefaultIfEmpty(0).Sum(),
                                      DebitCorrectionOnClient = x.Where(y => y.OperationTypeId == (int)OperationTypes.DebitCorrectionOnClient)
                                                                 .Select(y => y.Amount).DefaultIfEmpty(0).Sum(),
                                      ChargeBack = x.Where(y => y.OperationTypeId == (int)OperationTypes.CreditCorrectionOnClient && y.TypeId == (int)OperationTypes.ChargeBack)
                                                    .Select(y => y.Amount).DefaultIfEmpty(0).Sum()
                                  });
            var dateString = fromDate.ToString("yyyy-MM-dd");
            using (var dwh = new IqSoftDataWarehouseEntities())
            {
                foreach (var client in affClients)
                {
                    var clientActivityModel = new MyAffiliateClientActivityModel
                    {
                        CustomerId = client.ClientId,
                        BTag = client.ClickId,
                        AffiliateId = client.AffiliateId,
                        ActivityDate = dateString,
                        BrandId = brandId,
                        CurrencyId = client.CurrencyId
                    };

                    var paymentData = Db.PaymentRequests.Where(x => x.ClientId == client.ClientId &&
                                                                   (x.Status == (int)PaymentRequestStates.Approved ||
                                                                    x.Status == (int)PaymentRequestStates.ApprovedManually) &&
                                                                    x.Date >= fPaymentDate && x.Date < tPaymentDate)
                                                        .GroupBy(x => x.Type)
                                                        .Select(x => new
                                                        {
                                                            Type = x.Key,
                                                            Amount = x.Sum(y => y.Amount),
                                                            Count = x.Count()
                                                        }).ToList();
                    if (paymentData.Count != 0)
                    {
                        clientActivityModel.TotalDepositsAmount = paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Deposit).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.ManualDepositAmount = paymentData.Where(x => x.Type == (int)PaymentRequestTypes.ManualDeposit).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.TotalWithdrawalsAmount = paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Withdraw).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.TotalDepositsCount = paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Deposit || x.Type == (int)PaymentRequestTypes.ManualDeposit)
                                                                           .Select(x => x.Count).DefaultIfEmpty(0).Sum();
                    }
                    var activies = from b in dwh.Bets
                                   join p in dwh.Products on b.ProductId equals p.Id
                                   where b.ClientId == client.ClientId && b.State != (int)BetDocumentStates.Deleted &&
                                         b.State != (int)BetDocumentStates.Uncalculated && b.CalculationDate.HasValue &&
                                         b.CalculationDate >= fDate && b.CalculationDate < tDate
                                   group b by ((p.ExternalId == "sports-betting" || p.ExternalId == "live-sports" || p.ExternalId == "esports") ? 1 : 2) into g
                                   select new
                                   {
                                       ProductId = g.Key,
                                       TotalBetAmount = g.Select(y => y.BetAmount).DefaultIfEmpty(0).Sum(),
                                       TotalWinAmount = g.Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                                       BonusBetAmount = g.Where(y => y.BonusId.HasValue && y.BonusId.Value > 0).Select(y => y.BonusAmount ?? 0).DefaultIfEmpty(0).Sum(),
                                       BonusWinAmount = g.Where(y => y.BonusId.HasValue && y.BonusId.Value > 0).Select(y => y.BonusWinAmount ?? 0).DefaultIfEmpty(0).Sum(),
                                       Count = g.Count()
                                   };

                    if (activies.Count() != 0)
                    {
                        clientActivityModel.SportTotalBetsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.TotalBetAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.SportBonusBetsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.SportTotalWinsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.TotalWinAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.SportBonusWinsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.SportGrossRevenue = clientActivityModel.SportTotalBetsAmount - clientActivityModel.SportTotalWinsAmount;

                        clientActivityModel.CasinoTotalBetsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.TotalBetAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.CasinoBonusBetsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.CasinoTotalWinsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.TotalWinAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.CasinoBonusWinsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.CasinoGrossRevenue = clientActivityModel.CasinoTotalBetsAmount - clientActivityModel.CasinoTotalWinsAmount;

                        clientActivityModel.TotalBetsCount = activies.Select(y => y.Count).DefaultIfEmpty(0).Sum();
                    }
                    var transactions = clientTransactions.ContainsKey(client.ClientId) ? clientTransactions[client.ClientId] : null;
                    clientActivityModel.ConvertedBonusAmount = transactions?.BonusWin ?? 0;
                    clientActivityModel.CreditCorrectionOnClient = transactions?.CreditCorrectionOnClient ?? 0;
                    clientActivityModel.DebitCorrectionOnClient = transactions?.DebitCorrectionOnClient ?? 0;
                    clientActivityModel.ChargeBack = transactions?.ChargeBack ?? 0;
                    var balances = CacheManager.GetClientCurrentBalance(client.ClientId).Balances;

                    var balance = balances.Where(x => x.TypeId != (int)AccountTypes.ClientBonusBalance && x.TypeId != (int)AccountTypes.ClientCompBalance)
                                          .Select(x => x.Balance).DefaultIfEmpty(0).Sum();
                    var bonusBalance = balances.Where(x => x.TypeId == (int)AccountTypes.ClientBonusBalance).Select(x => x.Balance).DefaultIfEmpty(0).Sum();
                    clientActivityModel.AvailableBalance = Math.Round(balance, 2);
                    clientActivityModel.BonusBalance = Math.Round(bonusBalance, 2);

                    clientActivityModel.NGR = (clientActivityModel.SportTotalBetsAmount - clientActivityModel.SportBonusBetsAmount) +
                        (clientActivityModel.CasinoTotalBetsAmount - clientActivityModel.CasinoBonusBetsAmount) -
                        (clientActivityModel.SportTotalWinsAmount - clientActivityModel.SportBonusWinsAmount) -
                        (clientActivityModel.CasinoTotalWinsAmount - clientActivityModel.CasinoBonusWinsAmount) -
                        clientActivityModel.ConvertedBonusAmount;

                    if (client.FirstDepositDate.HasValue)
                    {
                        var currentDay = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                        if (client.FirstDepositDate >= currentDay && client.FirstDepositDate < currentDay.AddHours(24))
                            clientActivityModel.FirstDepositAmount = Db.PaymentRequests.FirstOrDefault(x => x.ClientId == client.ClientId && x.Amount > 0 &&
                                                                  (x.Type == (int)PaymentRequestTypes.Deposit || x.Type == (int)PaymentRequestTypes.ManualDeposit) &&
                                                                  (x.Status == (int)PaymentRequestStates.Approved ||
                                                                   x.Status == (int)PaymentRequestStates.ApprovedManually))?.Amount ?? 0;
                    }

                    affiliateClientActivies.Add(clientActivityModel);
                }
            }
            return affiliateClientActivies;
        }

        public AffilkaActivityModel GetAffilkaClientActivity(List<AffiliatePlatformModel> affClients, DateTime fromDate, DateTime upToDate)
        {
            var currentDate = DateTime.UtcNow;
            var affiliateClientActivies = new List<ActivityItem>();
            var fDate = fromDate.Year * (long)1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
            var fPaymentDate = fromDate.Year * (long)100000000 + fromDate.Month * 1000000 + fromDate.Day * 10000 + fromDate.Hour * 100 + fromDate.Minute;
            var tDate = upToDate.Year * (long)1000000 + upToDate.Month * 10000 + upToDate.Day * 100 + upToDate.Hour;
            var tPaymentDate = upToDate.Year * (long)100000000 + upToDate.Month * 1000000 + upToDate.Day * 10000 + upToDate.Hour * 100 + upToDate.Minute;
            using (var dwh = new IqSoftDataWarehouseEntities())
            {
                foreach (var client in affClients)
                {
                    var addItem = false;
                    var excluded = CacheManager.GetClientSettingByName(client.ClientId, Constants.ClientSettings.SelfExcluded)?.DateValue > currentDate;
                    if (!excluded)
                        excluded = CacheManager.GetClientSettingByName(client.ClientId, Constants.ClientSettings.SystemExcluded)?.DateValue > currentDate;
                    var affilkaActivityModel = new ActivityItem
                    {
                        Tag = client.ClickId,
                        ClientId = client.ClientId.ToString(),
                        Currency = client.CurrencyId,
                        Disabled = client.ClientStatus != (int)ClientStates.Active,
                        SelfExcluded = excluded
                    };
                    if ((affilkaActivityModel.Disabled || excluded) && client.ClientLastUpdateTime >= fromDate && client.ClientLastUpdateTime >= upToDate)
                        addItem = true;
                    var transactions = Db.Documents.Where(x => x.ClientId == client.ClientId &&
                                              (x.OperationTypeId == (int)OperationTypes.BonusWin ||
                                               x.OperationTypeId == (int)OperationTypes.CreditCorrectionOnClient ||
                                               x.OperationTypeId == (int)OperationTypes.DebitCorrectionOnClient) &&
                                               x.Date > fDate && x.Date <= tDate).GroupBy(x => x.OperationTypeId)
                                               .ToDictionary(x => x.Key, x => x.Select(y => y.Amount).DefaultIfEmpty(0).Sum());
                    var deposits = Db.PaymentRequests.Where(x => x.ClientId == client.ClientId &&
                                                                   (x.Type == (int)PaymentRequestTypes.Deposit ||
                                                                    x.Type == (int)PaymentRequestTypes.ManualDeposit) &&
                                                                   (x.Status == (int)PaymentRequestStates.Approved ||
                                                                    x.Status == (int)PaymentRequestStates.ApprovedManually) &&
                                                                    x.Date >= fPaymentDate && x.Date < tPaymentDate)
                                                        .Select(x => new DepositModel
                                                        {
                                                            Id = x.Id.ToString(),
                                                            Amount = (int)(x.Amount * 100),
                                                            ProcessedAt = x.LastUpdateTime.ToString()
                                                        }).ToList();
                    if (deposits.Any())
                    {
                        addItem = true;
                        affilkaActivityModel.Deposits = deposits;
                        affilkaActivityModel.TotalDepositAmount = (int)deposits.Sum(x => x.Amount);
                        affilkaActivityModel.TotalDepositCount = deposits.Count;
                    }
                    else affilkaActivityModel.Deposits = new List<DepositModel>();
                    var withdrawals = Db.PaymentRequests.Where(x => x.ClientId == client.ClientId &&
                                                                 x.Type == (int)PaymentRequestTypes.Withdraw &&
                                                                 (x.Status == (int)PaymentRequestStates.Approved ||
                                                                  x.Status == (int)PaymentRequestStates.ApprovedManually) &&
                                                                  x.Date >= fPaymentDate && x.Date < tPaymentDate);
                    if (withdrawals.Any())
                    {
                        addItem = true;
                        affilkaActivityModel.TotalWithdrawAmount = (int)withdrawals.Sum(x => x.Amount);
                        affilkaActivityModel.TotalWithdrawCount = withdrawals.Count();
                    }
                    var activies = dwh.Bets.Where(x => x.ClientId == client.ClientId &&
                                                      x.State != (int)BetDocumentStates.Deleted &&
                                                      x.State != (int)BetDocumentStates.Uncalculated &&
                                                      x.BetDate >= fDate && x.BetDate < tDate)
                                          .GroupBy(x => x.ProductId == Constants.SportsbookProductId ? 1 : 2)
                                          .Select(x => new
                                          {
                                              ProductId = x.Key,
                                              BetAmount = x.Select(y => y.BetAmount).DefaultIfEmpty(0).Sum(),
                                              UncalculatedBetAmount = x.Where(y => y.State == (int)BetDocumentStates.Uncalculated).Select(y => y.BetAmount).DefaultIfEmpty(0).Sum(),
                                              RollbackedBetAmount = x.Where(y => y.State == (int)BetDocumentStates.Deleted || y.State == (int)BetDocumentStates.Returned).Select(y => y.BetAmount).DefaultIfEmpty(0).Sum(),
                                              CalculatedBetAmount = x.Where(y => y.State == (int)BetDocumentStates.Won || y.State == (int)BetDocumentStates.Lost).Select(y => y.BetAmount).DefaultIfEmpty(0).Sum(),
                                              WinAmount = x.Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                                              BonusBetAmount = x.Where(y => y.BonusId.HasValue && y.BonusId.Value > 0).Select(y => y.BonusAmount ?? 0).DefaultIfEmpty(0).Sum(),
                                              BonusWinAmount = x.Where(y => y.BonusId.HasValue && y.BonusId.Value > 0).Select(y => y.BonusWinAmount ?? 0).DefaultIfEmpty(0).Sum(),
                                              Count = x.Count()
                                          });

                    if (activies.Count() != 0)
                    {
                        addItem = true;
                        affilkaActivityModel.TotalBetsInCents = (int)(activies.Where(y => y.ProductId == 2).Select(y => y.CalculatedBetAmount).DefaultIfEmpty(0).Sum() * 100);
                        affilkaActivityModel.TotalWinsInCents = (int)(activies.Where(y => y.ProductId == 2).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum() * 100);
                        affilkaActivityModel.CasinoBetsCount = activies.Where(y => y.ProductId == 2 && y.CalculatedBetAmount > 0).Select(y => y.Count).DefaultIfEmpty(0).Sum();

                        affilkaActivityModel.SportTotalBetsInCents = (int)(activies.Where(y => y.ProductId == 1).Select(y => y.BetAmount).DefaultIfEmpty(0).Sum() * 100);
                        affilkaActivityModel.SportTotalCaclculatedBetsInCents = (int)(activies.Where(y => y.ProductId == 1).Select(y => y.CalculatedBetAmount).DefaultIfEmpty(0).Sum() * 100);
                        affilkaActivityModel.SportCancelledBetsInCents = (int)(activies.Where(y => y.ProductId == 1).Select(y => y.RollbackedBetAmount).DefaultIfEmpty(0).Sum() * 100);
                        affilkaActivityModel.SportRejectedBetsInCents = affilkaActivityModel.SportCancelledBetsInCents;
                        affilkaActivityModel.SportTotalWinsInCent = (int)(activies.Where(y => y.ProductId == 1).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum() * 100);

                    }
                    if (addItem)
                    {
                        var credit = transactions.ContainsKey((int)OperationTypes.CreditCorrectionOnClient) ? transactions[(int)OperationTypes.CreditCorrectionOnClient] : 0;
                        var debit = transactions.ContainsKey((int)OperationTypes.DebitCorrectionOnClient) ? transactions[(int)OperationTypes.DebitCorrectionOnClient] : 0;
                        var bonusWin = Db.ClientBonus.Where(x => x.ClientId == client.ClientId &&
                                                                 x.Status == (int)ClientBonusStatuses.Active &&
                                                                 x.AwardingTime >= fromDate && x.AwardingTime < upToDate)
                                                     .Select(x => x.BonusPrize).DefaultIfEmpty(0).Sum();
                        affilkaActivityModel.BonusAmount = (int)(bonusWin * 100);
                        affilkaActivityModel.CorrectionOnClient = (int)((debit - credit) * 100);
                        affiliateClientActivies.Add(affilkaActivityModel);
                    }
                }
            }
            return new AffilkaActivityModel
            {
                FromDate = fromDate.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'"),
                ToDate = upToDate.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'"),
                Items = affiliateClientActivies
            };
        }
      
        public TrackierActivityModel GetTrackierClientActivity(string brandId, AffiliatePlatformModel client, DateTime fromDate, DateTime upToDate)
        {
            var currentDate = DateTime.UtcNow;
            var affiliateClientActivies = new List<ActivityItem>();
            var fDate = fromDate.Year * (long)1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
            var fPaymentDate = fromDate.Year * (long)100000000 + fromDate.Month * 1000000 + fromDate.Day * 10000 + fromDate.Hour * 100 + fromDate.Minute;
            var tDate = upToDate.Year * (long)1000000 + upToDate.Month * 10000 + upToDate.Day * 100 + upToDate.Hour;
            var tPaymentDate = upToDate.Year * (long)100000000 + upToDate.Month * 1000000 + upToDate.Day * 10000 + upToDate.Hour * 100 + upToDate.Minute;
            var trackierActivityModel = new TrackierActivityModel
            {
                CustomerId = client.ClientId.ToString(),
                Currency = client.CurrencyId,
                Timestamp = ((DateTimeOffset)currentDate).ToUnixTimeSeconds(),
                Date = currentDate.ToString("yyyy-MM-dd"),
                ProductId = "1",
                BrandId = brandId
            };
            using (var dwh = new IqSoftDataWarehouseEntities())
            {              
                var paymentData = Db.PaymentRequests.Where(x => x.ClientId == client.ClientId &&
                                                                   (x.Status == (int)PaymentRequestStates.Approved ||
                                                                    x.Status == (int)PaymentRequestStates.ApprovedManually) &&
                                                                    x.Date >= fPaymentDate && x.Date < tPaymentDate)
                                                        .GroupBy(x => x.Type)
                                                        .Select(x => new
                                                        {
                                                            Type = x.Key,
                                                            Amount = x.Sum(y => y.Amount),
                                                            Count = x.Count()
                                                        }).ToList();
                if (paymentData.Count != 0)
                {
                    trackierActivityModel.Deposits = paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Deposit).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                    trackierActivityModel.Withdrawls = paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Withdraw).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                }

                var activies = dwh.Bets.Where(x => x.ClientId == client.ClientId &&
                                  x.State != (int)BetDocumentStates.Deleted &&
                                  x.State != (int)BetDocumentStates.Uncalculated &&
                                  x.BetDate >= fDate && x.BetDate < tDate)
                      .Select(x => new
                      {
                          BetAmount = (x.BonusId == null || x.BonusId == 0) ? x.BetAmount : 0,
                          WinAmount = (x.BonusId == null || x.BonusId == 0) ? x.WinAmount : 0,
                          BonusBetAmount = x.BonusAmount,
                          BonusWinAmount = x.BonusWinAmount
                      });
                if (activies.Count() != 0)
                {
                    trackierActivityModel.Bets =  activies.Select(y => y.BetAmount).DefaultIfEmpty(0).Sum();
                    trackierActivityModel.Wins =  activies.Select(y => y.WinAmount).DefaultIfEmpty(0).Sum();
                    trackierActivityModel.Bonuses = activies.Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum() ?? 0;
                }
            }
            return trackierActivityModel;
        }

        /* public AffilkaActivityModel GetAffiseClientActivity(List<AffiliatePlatformModel> affClients, DateTime fromDate, DateTime upToDate)
         {
             var affiliateClientActivies = new List<ActivityItem>();
             var fDate = fromDate.Year * (long)1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
             var fPaymentDate = fromDate.Year * (long)100000000 + fromDate.Month * 1000000 + fromDate.Day * 10000 + fromDate.Hour*100 + fromDate.Minute;
             var tDate = upToDate.Year * (long)1000000 + upToDate.Month * 10000 + upToDate.Day * 100 + upToDate.Hour;
             var tPaymentDate = upToDate.Year * (long)100000000 + upToDate.Month * 1000000 + upToDate.Day * 10000 + upToDate.Hour*100 + upToDate.Minute;

             foreach (var client in affClients)
             {
                 var affisectivityModel = new AffiseActivityModel
                 {
                     Tag= client.ClickId,
                     ClientId = client.ClientId.ToString(),
                     Currency = client.CurrencyId
                 };
                 var activies = Db.Bets.Where(x => x.ClientId == client.ClientId &&
                                                   x.State != (int)BetDocumentStates.Deleted &&
                                                   x.State != (int)BetDocumentStates.Uncalculated &&
                                                   (!x.BonusId.HasValue || x.BonusId.Value== 0 ) &&
                                                   x.BetDate >= fDate && x.BetDate < tDate)
                                       .Select(x => x.BetAmount - x.WinAmount).DefaultIfEmpty(0).Sum();


             }
             return new AffilkaActivityModel
             {
                 FromDate = fromDate.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'"),
                 ToDate = upToDate.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'"),
                 Items = affiliateClientActivies
             };
         }*/

        public List<DIMClientActivityModel> GetDIMClientActivity(List<AffiliatePlatformModel> affClients, int brandId, DateTime fromDate, DateTime upToDate)
        {
            var affiliateClientActivies = new List<DIMClientActivityModel>();
            var fDate = fromDate.Year * (long)1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
            var fPaymentDate = fromDate.Year * (long)100000000 + fromDate.Month * 1000000 + fromDate.Day * 10000 + fromDate.Hour * 100 + fromDate.Minute;
            var tDate = upToDate.Year * (long)1000000 + upToDate.Month * 10000 + upToDate.Day * 100 + upToDate.Hour;
            var tPaymentDate = upToDate.Year * (long)100000000 + upToDate.Month * 1000000 + upToDate.Day * 10000 + upToDate.Hour * 100 + upToDate.Minute;

            var bonusWinAmounts = Db.Documents.Where(x => x.OperationTypeId == (int)OperationTypes.BonusWin &&
                                                          x.Date > fDate && x.Date <= tDate)
                                              .GroupBy(x => x.ClientId)
                                              .Select(x => new { ClientId = x.Key, Amount = x.Select(y => y.Amount).DefaultIfEmpty(0).Sum() }).ToList();
            var dateString = fromDate.ToString("yyyy-MM-dd");
            using (var dwh = new IqSoftDataWarehouseEntities())
            {
                foreach (var client in affClients)
                {
                    var paymentData = Db.PaymentRequests.Where(x => x.ClientId == client.ClientId &&
                                                                   (x.Status == (int)PaymentRequestStates.Approved ||
                                                                    x.Status == (int)PaymentRequestStates.ApprovedManually) &&
                                                                    x.Date >= fPaymentDate && x.Date < tPaymentDate).GroupBy(x => x.Type).Select(x => new
                                                                    {
                                                                        Type = x.Key,
                                                                        Amount = x.Sum(y => y.Amount),
                                                                        Count = x.Count()
                                                                    }).ToList();

                    if (paymentData.Count != 0)
                    {
                        affiliateClientActivies.Add(new DIMClientActivityModel
                        {
                            CustomerId = client.ClientId,
                            BTag = client.ClickId,
                            ActivityDate = dateString,
                            BrandId = brandId,
                            TotalDepositsAmount = paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Deposit).Select(x => x.Amount).DefaultIfEmpty(0).Sum(),
                            TotalWithdrawalsAmount = paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Withdraw).Select(x => x.Amount).DefaultIfEmpty(0).Sum(),
                            PaymentTransactions = paymentData.Sum(x => x.Count),
                            CurrencyId = client.CurrencyId
                        });
                    }
                    var bonusWinAmount = bonusWinAmounts.FirstOrDefault(x => x.ClientId == client.ClientId);
                    var activies = dwh.Bets.Where(x => x.ClientId == client.ClientId &&
                                                      x.State != (int)BetDocumentStates.Deleted &&
                                                      x.State != (int)BetDocumentStates.Uncalculated &&
                                                      x.BetDate >= fDate && x.BetDate < tDate)
                                          .GroupBy(x => x.ProductId == Constants.SportsbookProductId ? 1 :
                                                        x.ProductId == Constants.PokerProductId ? 3 :
                                                        x.ProductId == Constants.MahjongProductId ? 4 : 2)
                                          .Select(x => new
                                          {
                                              ProductId = x.Key,
                                              BetAmount = x.Where(y => !y.BonusId.HasValue || y.BonusId == 0).Select(y => y.BetAmount).DefaultIfEmpty(0).Sum(),
                                              WinAmount = x.Where(y => !y.BonusId.HasValue || y.BonusId == 0).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                                              BonusBetAmount = x.Where(y => y.BonusId.HasValue && y.BonusId.Value > 0).Select(y => y.BetAmount).DefaultIfEmpty(0).Sum(),
                                              BonusWinAmount = x.Where(y => y.BonusId.HasValue && y.BonusId.Value > 0).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                                              GGR = x.Select(y => y.Rake >= 0 ? y.Rake : (y.BetAmount - (y.BonusAmount == null ? y.WinAmount : y.BonusAmount))).DefaultIfEmpty(0).Sum(),
                                              Count = x.Count()
                                          });

                    if (activies.Count() != 0)
                    {
                        var existItem = affiliateClientActivies.FirstOrDefault(x => x.CustomerId == client.ClientId);
                        if (existItem != null)
                        {
                            existItem.SportGrossRevenue = activies.Where(y => y.ProductId == 1).Select(x => x.GGR).DefaultIfEmpty(0).Sum() ?? 0;
                            existItem.CasinoGrossRevenue = activies.Where(y => y.ProductId == 2).Select(x => x.GGR).DefaultIfEmpty(0).Sum() ?? 0;
                            existItem.PokerGrossRevenue = activies.Where(y => y.ProductId == 3).Select(x => x.GGR).DefaultIfEmpty(0).Sum();
                            existItem.MahjongGrossRevenue = activies.Where(y => y.ProductId == 4).Select(x => x.GGR).DefaultIfEmpty(0).Sum();

                            existItem.SportBonusBetsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum();
                            existItem.CasinoBonusBetsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum();
                            existItem.PokerBonusBetsAmount = activies.Where(y => y.ProductId == 3).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum();
                            existItem.MahjongBonusBetsAmount = activies.Where(y => y.ProductId == 4).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum();

                            existItem.SportBonusWinsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum();
                            existItem.CasinoBonusWinsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum();
                            existItem.PokerBonusWinsAmount = activies.Where(y => y.ProductId == 3).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum();
                            existItem.MahjongBonusWinsAmount = activies.Where(y => y.ProductId == 4).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum();

                            existItem.SportTotalWinsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum();
                            existItem.CasinoTotalWinsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum();
                            existItem.PokerTotalWinAmount = activies.Where(y => y.ProductId == 3).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum();
                            existItem.MahjongTotalWinAmount = activies.Where(y => y.ProductId == 4).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum();

                            existItem.TotalTransactions += activies.Select(y => y.Count).DefaultIfEmpty(0).Sum();
                            existItem.TotalConvertedBonusAmount = bonusWinAmount?.Amount ?? 0;
                        }
                        else
                            affiliateClientActivies.Add(new DIMClientActivityModel
                            {
                                CustomerId = client.ClientId,
                                CurrencyId = client.CurrencyId,
                                BTag = client.ClickId,
                                ActivityDate = dateString,
                                BrandId = brandId,
                                SportGrossRevenue = activies.Where(y => y.ProductId == 1).Select(x => x.GGR).DefaultIfEmpty(0).Sum() ?? 0,
                                CasinoGrossRevenue = activies.Where(y => y.ProductId == 2).Select(x => x.GGR).DefaultIfEmpty(0).Sum() ?? 0,
                                PokerGrossRevenue = activies.Where(y => y.ProductId == 3).Select(x => x.GGR).DefaultIfEmpty(0).Sum(),
                                MahjongGrossRevenue = activies.Where(y => y.ProductId == 4).Select(x => x.GGR).DefaultIfEmpty(0).Sum(),

                                SportBonusBetsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum(),
                                CasinoBonusBetsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum(),
                                PokerBonusBetsAmount = activies.Where(y => y.ProductId == 3).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum(),
                                MahjongBonusBetsAmount = activies.Where(y => y.ProductId == 4).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum(),

                                SportBonusWinsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum(),
                                CasinoBonusWinsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum(),
                                PokerBonusWinsAmount = activies.Where(y => y.ProductId == 3).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum(),
                                MahjongBonusWinsAmount = activies.Where(y => y.ProductId == 4).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum(),

                                SportTotalWinsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                                CasinoTotalWinsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                                PokerTotalWinAmount = activies.Where(y => y.ProductId == 3).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                                MahjongTotalWinAmount = activies.Where(y => y.ProductId == 4).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),

                                TotalTransactions = activies.Select(y => y.Count).DefaultIfEmpty(0).Sum(),
                                TotalConvertedBonusAmount = bonusWinAmount?.Amount ?? 0
                            });
                    }
                }
            }
            return affiliateClientActivies;
        }

        public List<IncomeAccessActivityModel> GetIncomeAccessClientActivity(List<AffiliatePlatformModel> affClients, int brandId, DateTime fromDate, DateTime upToDate)
        {
            var affiliateClientActivies = new List<IncomeAccessActivityModel>();
            var fDate = fromDate.Year * (long)1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
            var fPaymentDate = fromDate.Year * (long)100000000 + fromDate.Month * 1000000 + fromDate.Day * 10000 + fromDate.Hour * 100 + fromDate.Minute;
            var tDate = upToDate.Year * (long)1000000 + upToDate.Month * 10000 + upToDate.Day * 100 + upToDate.Hour;
            var tPaymentDate = upToDate.Year * (long)100000000 + upToDate.Month * 1000000 + upToDate.Day * 10000 + upToDate.Hour * 100 + upToDate.Minute;
            var clientIds = affClients.Select(x => x.ClientId).ToList();
            var clientTransactions = Db.Documents.Where(x => clientIds.Contains(x.ClientId.Value) &&
                                                             x.OperationTypeId == (int)OperationTypes.CreditCorrectionOnClient &&
                                                             x.Date > fDate && x.Date <= tDate)
                                                 .GroupBy(x => x.ClientId)
                                                 .ToDictionary(x => x.Key, x =>
                                                      new
                                                      {
                                                          ChargeBack = x.Where(y => y.OperationTypeId == (int)OperationTypes.CreditCorrectionOnClient &&
                                                                                    y.TypeId == (int)OperationTypes.ChargeBack)
                                                                             .Select(y => y.Amount).DefaultIfEmpty(0).Sum()
                                                      });
            var dateString = fromDate.ToString("yyyy-MM-dd");
            using (var dwh = new IqSoftDataWarehouseEntities())
            {
                foreach (var client in affClients)
                {
                    var clientActivityModel = new IncomeAccessActivityModel
                    {
                        CustomerId = client.ClientId,
                        BTag = client.ClickId,
                        ActivityDate = dateString,
                        UserName = client.ClientName
                    };

                    var paymentData = Db.PaymentRequests.Where(x => x.ClientId == client.ClientId &&
                                                                   (x.Status == (int)PaymentRequestStates.Approved ||
                                                                    x.Status == (int)PaymentRequestStates.ApprovedManually) &&
                                                                    x.Date >= fPaymentDate && x.Date < tPaymentDate).GroupBy(x => x.Type).Select(x => new
                                                                    {
                                                                        Type = x.Key,
                                                                        Amount = x.Sum(y => y.Amount),
                                                                        Count = x.Count()
                                                                    }).ToList();
                    clientActivityModel.TotalDepositsAmount = paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Deposit).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                    var activies = dwh.Bets.Where(x => x.ClientId == client.ClientId &&
                                                      x.State != (int)BetDocumentStates.Deleted &&
                                                      x.State != (int)BetDocumentStates.Uncalculated && x.CalculationDate.HasValue &&
                                                      x.CalculationDate >= fDate && x.CalculationDate < tDate &&
                                                      x.ProductId == Constants.SportsbookProductId)
                                          .GroupBy(x => x.ProductId)
                                          .Select(x => new
                                          {
                                              ProductId = x.Key,
                                              TotalBetAmount = x.Select(y => y.BetAmount).DefaultIfEmpty(0).Sum(),
                                              TotalWinAmount = x.Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                                              BonusBetAmount = x.Where(y => y.BonusId.HasValue && y.BonusId.Value > 0).Select(y => y.BonusAmount ?? 0).DefaultIfEmpty(0).Sum(),
                                              BonusWinAmount = x.Where(y => y.BonusId.HasValue && y.BonusId.Value > 0).Select(y => y.BonusWinAmount ?? 0).DefaultIfEmpty(0).Sum(),
                                              Count = x.Count()
                                          });

                    if (activies.Count() != 0)
                    {
                        clientActivityModel.SportTotalBetsAmount = activies.Select(y => y.TotalBetAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.SportBonusBetsAmount = activies.Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.SportTotalWinsAmount = activies.Select(y => y.TotalWinAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.SportBonusWinsAmount = activies.Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.SportGrossRevenue = clientActivityModel.SportTotalBetsAmount - clientActivityModel.SportTotalWinsAmount;
                        clientActivityModel.TotalBetsCount = activies.Select(y => y.Count).DefaultIfEmpty(0).Sum();
                    }
                    var transactions = clientTransactions.ContainsKey(client.ClientId) ? clientTransactions[client.ClientId] : null;
                    clientActivityModel.ChargeBack = transactions?.ChargeBack ?? 0;

                    affiliateClientActivies.Add(clientActivityModel);
                }
                return affiliateClientActivies;
            }
        }

        public List<ClientActivityModel> GetScaleoClientActivity(List<AffiliatePlatformModel> affClients, DateTime fromDate, DateTime upToDate)
        {
            var affiliateClientActivies = new List<ClientActivityModel>();
            var fDate = fromDate.Year * (long)1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
            var fPaymentDate = fromDate.Year * (long)100000000 + fromDate.Month * 1000000 + fromDate.Day * 10000 + fromDate.Hour * 100 + fromDate.Minute;
            var tDate = upToDate.Year * (long)1000000 + upToDate.Month * 10000 + upToDate.Day * 100 + upToDate.Hour;
            var tPaymentDate = upToDate.Year * (long)100000000 + upToDate.Month * 1000000 + upToDate.Day * 10000 + upToDate.Hour * 100 + upToDate.Minute;
            var dateString = fromDate.ToString("yyyy-MM-dd");
            using (var dwh = new IqSoftDataWarehouseEntities())
            {
                foreach (var client in affClients)
                {
                    var clientActivityModel = new ClientActivityModel
                    {
                        CustomerId = client.ClientId,
                        BTag = client.ClickId,
                        ActivityDate = dateString,
                        CurrencyId = client.CurrencyId
                    };

                    var paymentData = Db.PaymentRequests.Where(x => x.ClientId == client.ClientId &&
                                                                   (x.Status == (int)PaymentRequestStates.Approved ||
                                                                    x.Status == (int)PaymentRequestStates.ApprovedManually) &&
                                                                    x.Date >= fPaymentDate && x.Date < tPaymentDate)
                                                        .GroupBy(x => x.Type)
                                                        .Select(x => new
                                                        {
                                                            Type = x.Key,
                                                            Amount = x.Sum(y => y.Amount),
                                                            Count = x.Count()
                                                        }).ToList();
                    if (paymentData.Count != 0)
                    {
                        clientActivityModel.TotalDepositsAmount = paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Deposit || x.Type == (int)PaymentRequestTypes.ManualDeposit)
                                                                             .Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.TotalWithdrawalsAmount = paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Withdraw).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.TotalDepositsCount = paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Deposit || x.Type == (int)PaymentRequestTypes.ManualDeposit)
                                                                           .Select(x => x.Count).DefaultIfEmpty(0).Sum();
                    }
                    var activies = dwh.Bets.Where(x => x.ClientId == client.ClientId &&
                                                      x.State != (int)BetDocumentStates.Deleted &&
                                                      x.State != (int)BetDocumentStates.Uncalculated &&
                                                      x.BetDate >= fDate && x.BetDate < tDate)
                                          .GroupBy(x => x.ProductId == Constants.SportsbookProductId ? 1 : 2)
                                          .Select(x => new
                                          {
                                              ProductId = x.Key,
                                              BetAmount = x.Where(y => !y.BonusId.HasValue || y.BonusId == 0).Select(y => y.BetAmount).DefaultIfEmpty(0).Sum(),
                                              WinAmount = x.Where(y => !y.BonusId.HasValue || y.BonusId == 0).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                                              BonusBetAmount = x.Where(y => y.BonusId.HasValue && y.BonusId.Value > 0).Select(y => y.BetAmount).DefaultIfEmpty(0).Sum(),
                                              BonusWinAmount = x.Where(y => y.BonusId.HasValue && y.BonusId.Value > 0).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                                              Count = x.Count()
                                          });
                    if (activies.Count() != 0)
                    {
                        clientActivityModel.SportGrossRevenue = activies.Where(y => y.ProductId == 1).Select(y => y.BetAmount - y.WinAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.CasinoGrossRevenue = activies.Where(y => y.ProductId == 2).Select(y => y.BetAmount - y.WinAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.SportBonusBetsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.CasinoBonusBetsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.SportBonusWinsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.CasinoBonusWinsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.SportTotalWinsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum();
                        clientActivityModel.CasinoTotalWinsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum();
                    }

                    if (client.FirstDepositDate.HasValue)
                    {
                        var currentDay = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                        if (client.FirstDepositDate >= currentDay && client.FirstDepositDate < currentDay.AddHours(24))
                            clientActivityModel.FirstDepositAmount = Db.PaymentRequests.FirstOrDefault(x => x.ClientId == client.ClientId && x.Amount > 0 &&
                                                                  (x.Type == (int)PaymentRequestTypes.Deposit || x.Type == (int)PaymentRequestTypes.ManualDeposit) &&
                                                                  (x.Status == (int)PaymentRequestStates.Approved ||
                                                                   x.Status == (int)PaymentRequestStates.ApprovedManually))?.Amount ?? 0;
                    }

                    affiliateClientActivies.Add(clientActivityModel);
                }
            }
            return affiliateClientActivies;
        }

		public ScaleoEventData CreateScaleoReport(List<RegistrationActivityModel> newRegisteredClients, List<ClientActivityModel> scaleoClientActivies, string apiKey, int partnerId)
        {
            var data = new ScaleoEventData()
            {
                status = "success",
                code = 200,
                data = new Data()
            };
            var events = new List<Event>();
			foreach (var reg in newRegisteredClients)
			{
				events.Add(new Event()
				{
					type = "reg",
					timestamp = reg.RegistrationDate,
					click_id = reg.BTag,
					player_id = reg.CustomerId.ToString(),
					amount = 0,
					currency = reg.CustomerCurrencyId
				});
			}
			foreach (var activity in scaleoClientActivies)
			{
				if (activity.TotalDepositsAmount > 0)
				{
					events.Add(new Event()
					{
						type = "dep",
						timestamp = activity.ActivityDate,
						click_id = activity.BTag,
						player_id = activity.CustomerId.ToString(),
						amount = activity.TotalDepositsAmount,
						currency = activity.CurrencyId
					});
				}
				if (activity.TotalWithdrawalsAmount > 0)
				{
					events.Add(new Event()
					{
						type = "wdr",
						timestamp = activity.ActivityDate,
						click_id = activity.BTag,
						player_id = activity.CustomerId.ToString(),
						amount = activity.TotalWithdrawalsAmount,
						currency = activity.CurrencyId
					});
				}
				if (activity.FirstDepositAmount > 0)
				{
					events.Add(new Event()
					{
						type = "ftd",
						timestamp = activity.ActivityDate,
						click_id = activity.BTag,
						player_id = activity.CustomerId.ToString(),
						amount = activity.FirstDepositAmount,
						currency = activity.CurrencyId
					});
				}
				if (activity.CasinoTotalBetsAmount > 0 || activity.SportTotalBetsAmount > 0)
				{
					new Event()
					{
						type = "bet",
						timestamp = activity.ActivityDate,
						click_id = activity.BTag,
						player_id = activity.CustomerId.ToString(),
						amount = activity.CasinoTotalBetsAmount + activity.SportTotalBetsAmount,
						currency = activity.CurrencyId
					};
				};
				if (activity.CasinoTotalWinsAmount > 0 || activity.SportTotalWinsAmount > 0)
				{
					new Event()
					{
						type = "bet",
						timestamp = activity.ActivityDate,
						click_id = activity.BTag,
						player_id = activity.CustomerId.ToString(),
						amount = activity.CasinoTotalWinsAmount + activity.SportTotalWinsAmount,
						currency = activity.CurrencyId
					};
				};
				if (activity.CasinoBonusBetsAmount > 0 || activity.CasinoBonusWinsAmount > 0 ||
				   activity.SportBonusBetsAmount > 0 || activity.SportBonusWinsAmount > 0)
				{
					new Event()
					{
						type = "bon",
						timestamp = activity.ActivityDate,
						click_id = activity.BTag,
						player_id = activity.CustomerId.ToString(),
						amount = activity.CasinoBonusBetsAmount + activity.CasinoBonusWinsAmount
							   + activity.SportBonusBetsAmount + activity.SportBonusWinsAmount,
						currency = activity.CurrencyId
					};
				};
            }
            data.data.events = events;
            return data;
        }

        #endregion
    }
}