using System;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common.Models;
using System.Text.RegularExpressions;
using log4net;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace IqSoft.CP.BLL.Services
{
    public class PartnerBll : PermissionBll, IPartnerBll
    {
        #region Constructors

        public PartnerBll(SessionIdentity identity, ILog log)
            : base(identity, log)
        {

        }

        public PartnerBll(BaseBll baseBl)
            : base(baseBl)
        {

        }

        #endregion

        public Partner GetPartnerById(int id)
        {
            return Db.Partners.FirstOrDefault(x => x.Id == id);
        }

        public Partner GetpartnerByDomain(string domain)
        {
            return Db.Partners.FirstOrDefault(x => x.SiteUrl.Contains(domain));
        }

        public PagedModel<Partner> GetPartnersPagedModel(FilterPartner filter)
        {
            var checkP = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<Partner>>
            {
                new CheckPermissionOutput<Partner>
                {
                    AccessibleObjects = checkP.AccessibleObjects,
                    HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                    Filter = x => checkP.AccessibleObjects.AsEnumerable().Contains(x.Id)
                }
            };

            Func<IQueryable<Partner>, IOrderedQueryable<Partner>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<Partner>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<Partner>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = partners => partners.OrderByDescending(x => x.Id);
            }

            return new PagedModel<Partner>
            {
                Entities = filter.FilterObjects(Db.Partners, null).ToList(),
                Count = filter.SelectedObjectsCount(Db.Partners)
            };
        }

        public List<Partner> GetPartners(FilterPartner filter, bool checkPermissions = true)
        {
            if (checkPermissions)
            {
                var checkPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });

                filter.CheckPermissionResuts = new List<CheckPermissionOutput<Partner>>
                {
                    new CheckPermissionOutput<Partner>
                    {
                        AccessibleObjects = checkPermission.AccessibleObjects,
                        HaveAccessForAllObjects = checkPermission.HaveAccessForAllObjects,
                        Filter = x => checkPermission.AccessibleObjects.AsEnumerable().Contains(x.ObjectId)
                    }
                };
            }
            return filter.FilterObjects(Db.Partners).ToList();
        }
        public string SavePasswordRegex(int partnerId, string pattern)
        {
            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditPartnerPasswordRegEx,
                ObjectTypeId = ObjectTypes.Partner,
                ObjectId = partnerId
            });
            if (!checkPermissionResult.HaveAccessForAllObjects &&
                !checkPermissionResult.AccessibleObjects.AsEnumerable().Contains(partnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var currentTime = GetServerDate();
            Db.Partners.Where(x => x.Id == partnerId).UpdateFromQuery(x => new Partner { PasswordRegExp = pattern });
            return pattern;
        }

        public Partner SavePartner(Partner partner)
        {
            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreatePartner,
                ObjectTypeId = ObjectTypes.Partner,
                ObjectId = partner.Id
            });
            if (!checkPermissionResult.HaveAccessForAllObjects &&
                !checkPermissionResult.AccessibleObjects.AsEnumerable().Contains(partner.Id))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var currentTime = GetServerDate();
            var dbPartner = Db.Partners.FirstOrDefault(x => x.Id == partner.Id);
            if (!Regex.IsMatch(partner.Name, Constants.NameRegEx))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            if (IsPartnerNameExists(dbPartner?.Id, partner.Name))
                throw CreateException(LanguageId, Constants.Errors.ParterNameAlreadyExists);

            if (dbPartner == null)
            {
                var mainPartner = GetPartnerById(Constants.MainPartnerId);

                partner.AccountingDayStartTime = mainPartner.AccountingDayStartTime;
                partner.ClientMinAge = mainPartner.ClientMinAge;
                partner.PasswordRegExp = mainPartner.PasswordRegExp;
                partner.VerificationType = mainPartner.VerificationType;
                partner.EmailVerificationCodeLength = mainPartner.EmailVerificationCodeLength;
                partner.MobileVerificationCodeLength = mainPartner.MobileVerificationCodeLength;
                partner.UnusedAmountWithdrawPercent = mainPartner.UnusedAmountWithdrawPercent;
                partner.UserSessionExpireTime = mainPartner.UserSessionExpireTime;
                partner.UnpaidWinValidPeriod = mainPartner.UnpaidWinValidPeriod;
                partner.VerificationKeyActiveMinutes = mainPartner.VerificationKeyActiveMinutes;
                partner.AutoApproveBetShopDepositMaxAmount = mainPartner.AutoApproveBetShopDepositMaxAmount;
                partner.ClientSessionExpireTime = mainPartner.ClientSessionExpireTime;
                partner.AutoApproveWithdrawMaxAmount = mainPartner.AutoApproveWithdrawMaxAmount;
                partner.AutoConfirmWithdrawMaxAmount = mainPartner.AutoConfirmWithdrawMaxAmount;

                dbPartner = new Partner { CreationTime = currentTime, Id = partner.Id, PasswordRegExp = partner.PasswordRegExp };

                Db.Partners.Add(dbPartner);
            }
            partner.PasswordRegExp = dbPartner.PasswordRegExp;
            partner.CreationTime = dbPartner.CreationTime;
            partner.LastUpdateTime = currentTime;
            partner.SessionId = SessionId;
            Db.Entry(dbPartner).CurrentValues.SetValues(partner);
            SaveChanges();
            return partner;
        }

        public List<PartnerKey> GetPartnerKeys(int partnerid)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            CheckPermission(Constants.Permissions.ViewPartnerKey);
            if (partnerAccess.HaveAccessForAllObjects || partnerAccess.AccessibleObjects.AsEnumerable().Contains(partnerid))
                return Db.PartnerKeys.Where(x => x.PartnerId == partnerid || (partnerid == Constants.MainPartnerId && x.PartnerId == null)).ToList();
            else
                return new List<PartnerKey>();
        }

        public PartnerKey SavePartnerKey(PartnerKey partnerKey)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            CheckPermission(Constants.Permissions.ViewPartnerKey);
            CheckPermission(Constants.Permissions.EditPartnerKey);
            if ((!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != partnerKey.PartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            PartnerKey dbPartnerKey;
            var keyName = partnerKey.Name;
            if (partnerKey.Id > 0)
            {
                dbPartnerKey = Db.PartnerKeys.FirstOrDefault(x => x.Id == partnerKey.Id);
                if (dbPartnerKey == null)
                    throw CreateException(LanguageId, Constants.Errors.PartnerKeyNotFound);
                keyName = dbPartnerKey.Name;
                var oldValue = new
                {
                    dbPartnerKey.Id,
                    dbPartnerKey.PartnerId,
                    dbPartnerKey.GameProviderId,
                    dbPartnerKey.PaymentSystemId,
                    dbPartnerKey.Name,
                    dbPartnerKey.StringValue,
                    dbPartnerKey.DateValue,
                    dbPartnerKey.NumericValue,
                    dbPartnerKey.NotificationServiceId
                };
                Db.Entry(dbPartnerKey).CurrentValues.SetValues(partnerKey);
                SaveChangesWithHistory((int)ObjectTypes.PartnerKey, dbPartnerKey.Id, JsonConvert.SerializeObject(oldValue), string.Empty);
            }
            else
            {
                dbPartnerKey = Db.PartnerKeys.FirstOrDefault(x => x.Name == partnerKey.Name &&
                (!partnerKey.PartnerId.HasValue || x.PartnerId == partnerKey.PartnerId.Value) &&
                x.GameProviderId == partnerKey.GameProviderId && x.PaymentSystemId == partnerKey.PaymentSystemId &&
                x.NotificationServiceId == partnerKey.NotificationServiceId);
                if (dbPartnerKey != null)
                    throw CreateException(LanguageId, Constants.Errors.NickNameExists);
                Db.PartnerKeys.Add(partnerKey);
            }
            Db.SaveChanges();
            CacheManager.RemovePartnerSettingByKey(partnerKey.PartnerId, partnerKey.GameProviderId, partnerKey.PaymentSystemId,
                                                   partnerKey.NotificationServiceId, keyName);
            return dbPartnerKey ?? partnerKey;
        }

        public PartnerKey GetPartnerKey(int? partnerId, string nickName)
        {
            return Db.PartnerKeys.Where(x => x.Name == nickName && x.PartnerId == partnerId).FirstOrDefault();
        }

        public bool IsPartnerIdExists(int partnerId)
        {
            CheckPermission(Constants.Permissions.CreatePartner);

            return Db.Partners.Any(x => x.Id == partnerId);
        }

        public bool IsPartnerNameExists(int? partnerId, string partnerName)
        {
            return Db.Partners.Any(x => (!partnerId.HasValue || x.Id != partnerId.Value) && x.Name.ToLower() == partnerName.ToLower());
        }

        public bool ChangePartnerAccountBalance(int? partnerId, DateTime endTime)
        {
            using (var transactionScope = CommonFunctions.CreateTransactionScope())
            {
                var currentTime = GetServerDate();
                if (endTime > currentTime) return false;
                var eTime = endTime.Year * 1000000 + endTime.Month * 10000 + endTime.Day * 100 + endTime.Hour;
                var startDate = endTime.AddHours(-Constants.AddMoneyToPartnerAccountPeriodicy);
                var sDate = startDate.Year * 1000000 + startDate.Month * 10000 + startDate.Day * 100 + startDate.Hour;
                var partnerAccounts =
                    Db.Accounts.Where(x => x.ObjectTypeId == (int)ObjectTypes.Partner && (!partnerId.HasValue || x.ObjectId == partnerId.Value)).ToList();
                foreach (var partnerAccount in partnerAccounts)
                {
                    var transactionAmount = Db.Transactions.Where(x => x.Date >= sDate && x.Date < eTime && x.AccountId == partnerAccount.Id);
                    if (transactionAmount.Any())
                        ChangeAccountBalance(transactionAmount.Sum(x => x.Amount), partnerAccount);
                }
                Db.SaveChanges();
                transactionScope.Complete();
            }

            return true;
        }

        public string GetPaymentValueByKey(int? partnerId, int? paymentSystemId, string key)
        {
            return Db.PartnerKeys.Where(x => x.Name == key && x.PartnerId == partnerId && x.PaymentSystemId == paymentSystemId)
                                 .Select(x => x.StringValue).FirstOrDefault();
        }

        public PartnerPaymentLimit GetPaymentLimit(int partnerId, bool checkPermission)
        {
            if (checkPermission)
                CheckPermission(Constants.Permissions.ViewPartnerPaymentLimits);
            return new PartnerPaymentLimit
            {
                PartnerId = partnerId,
                WithdrawMaxCountPerDayPerCustomer = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.WithdrawMaxCountPerDayPerCustomer)?.NumericValue,
                CashWithdrawMaxCountPerDayPerCustomer = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CashWithdrawMaxCountPerDayPerCustomer)?.NumericValue
            };
        }

        public void SetPaymentLimit(PartnerPaymentLimit partnerPaymentLimit, bool checkPermission)
        {
            if (checkPermission)
            {
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });
                if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != partnerPaymentLimit.PartnerId))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

                CheckPermission(Constants.Permissions.EditPartnerPaymentLimits);
            }

            var paymentLimits = Db.PartnerKeys.Where(x => x.PartnerId == partnerPaymentLimit.PartnerId &&
                                                       (x.Name == Constants.PartnerKeys.WithdrawMaxCountPerDayPerCustomer ||
                                                        x.Name == Constants.PartnerKeys.CashWithdrawMaxCountPerDayPerCustomer
                                                       )).ToList();

            var countPerDay = paymentLimits.FirstOrDefault(x => x.Name == Constants.PartnerKeys.WithdrawMaxCountPerDayPerCustomer);
            if (countPerDay != null)
                countPerDay.NumericValue = partnerPaymentLimit.WithdrawMaxCountPerDayPerCustomer;
            else
                Db.PartnerKeys.Add(new PartnerKey
                {
                    PartnerId = partnerPaymentLimit.PartnerId,
                    Name = Constants.PartnerKeys.WithdrawMaxCountPerDayPerCustomer,
                    NumericValue = partnerPaymentLimit.WithdrawMaxCountPerDayPerCustomer
                });

            var cashCountPerDay = paymentLimits.FirstOrDefault(x => x.Name == Constants.PartnerKeys.CashWithdrawMaxCountPerDayPerCustomer);
            if (cashCountPerDay != null)
                cashCountPerDay.NumericValue = partnerPaymentLimit.CashWithdrawMaxCountPerDayPerCustomer;
            else
                Db.PartnerKeys.Add(new PartnerKey
                {
                    PartnerId = partnerPaymentLimit.PartnerId,
                    Name = Constants.PartnerKeys.CashWithdrawMaxCountPerDayPerCustomer,
                    NumericValue = partnerPaymentLimit.CashWithdrawMaxCountPerDayPerCustomer
                });

            Db.SaveChanges();
        }

        #region Export to excel

        public List<Partner> ExportPartners(FilterPartner filter, bool checkPermissions = true)
        {
            if (checkPermissions)
            {
                var checkPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });

                var exportAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ExportPartners
                });

                filter.CheckPermissionResuts = new List<CheckPermissionOutput<Partner>>
                {
                    new CheckPermissionOutput<Partner>
                    {
                        AccessibleObjects = checkPermission.AccessibleObjects,
                        HaveAccessForAllObjects = checkPermission.HaveAccessForAllObjects,
                        Filter = x => checkPermission.AccessibleObjects.AsEnumerable().Contains(x.ObjectId)
                    },
                    new CheckPermissionOutput<Partner>
                    {
                        AccessibleObjects = exportAccess.AccessibleObjects,
                        HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                        Filter = x => exportAccess.AccessibleObjects.AsEnumerable().Contains(x.ObjectId)
                    }
                };
            }

            filter.TakeCount = 0;
            filter.SkipCount = 0;
            return filter.FilterObjects(Db.Partners).ToList();
        }

        public List<Partner> ExportPartnersModel(FilterPartner filter)
        {
            var checkP = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportPartnersModel
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<Partner>>
            {
                new CheckPermissionOutput<Partner>
                {
                    AccessibleObjects = checkP.AccessibleObjects,
                    HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                    Filter = x => checkP.AccessibleObjects.AsEnumerable().Contains(x.Id)
                },
                new CheckPermissionOutput<Partner>
                {
                    AccessibleObjects = exportAccess.AccessibleObjects,
                    HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                    Filter = x => exportAccess.AccessibleObjects.AsEnumerable().Contains(x.Id)
                }
            };

            filter.TakeCount = 0;
            filter.SkipCount = 0;
            var result = filter.FilterObjects(Db.Partners, partners => partners.OrderBy(p => p.Id)).ToList();
            return result;
        }

        #endregion

        #region PartnerDocument
        // BetShop pay amount to partner or vice versa
        public BetShopReconing PayBetShopDebt(int betShopId, decimal amount, string currencyId, long? externalOperationId)
        {
            CheckPermission(Constants.Permissions.PayBetShopDebt);
            using (var betShopBl = new BetShopBll(this))
            {
                using (var documentBl = new DocumentBll(this))
                {
                    using (var scope = CommonFunctions.CreateTransactionScope())
                    {
                        var betShop = betShopBl.GetBetShopById(betShopId, false);
                        if (betShop == null)
                            throw CreateException(LanguageId, Constants.Errors.BetShopNotFound);
                        var operationAmount = Math.Abs(amount);
                        var operation = new Operation
                        {
                            Amount = operationAmount,
                            CurrencyId = currencyId,
                            Type = (int)OperationTypes.PayBetShopDebt,
                            ExternalOperationId = externalOperationId,
                            OperationItems = new List<OperationItem>()
                        };
                        var betShopAcc = documentBl.GetOrCreateAccount(betShopId, (int)ObjectTypes.BetShop, betShop.CurrencyId,
                            (int)AccountTypes.BetShopDebtToPartner);
                        var partnerAcc = documentBl.GetOrCreateAccount(betShop.PartnerId, (int)ObjectTypes.Partner,
                            betShop.CurrencyId, (int)Common.Enums.AccountTypes.PartnerBalance);

                        var betShopItem = new OperationItem
                        {
                            AccountId = betShopAcc.Id,
                            Amount = operationAmount,
                            CurrencyId = currencyId,
                            OperationTypeId = (int)OperationTypes.PayBetShopDebt,
                            AccountTypeId = (int)Common.Enums.AccountTypes.BetShopDebtToPartner,
                            ObjectTypeId = (int)ObjectTypes.BetShop
                        };
                        var partnerItem = new OperationItem
                        {
                            AccountId = partnerAcc.Id,
                            Amount = operationAmount,
                            CurrencyId = currencyId,
                            OperationTypeId = (int)OperationTypes.PayBetShopDebt,
                            AccountTypeId = (int)Common.Enums.AccountTypes.PartnerBalance,
                            ObjectTypeId = (int)ObjectTypes.Partner
                        };
                        if (amount > 0)
                        {
                            betShopItem.Type = (int)TransactionTypes.Credit;
                            partnerItem.Type = (int)TransactionTypes.Debit;
                        }
                        else
                        {
                            betShopItem.Type = (int)TransactionTypes.Debit;
                            partnerItem.Type = (int)TransactionTypes.Credit;
                        }
                        operation.OperationItems.Add(betShopItem);
                        operation.OperationItems.Add(partnerItem);
                        var document = documentBl.CreateDocument(operation);
                        var balance = GetObjectBalanceWithConvertion((int)ObjectTypes.BetShop, betShopId, currencyId);
                        var betShopReconing = new BetShopReconing
                        {
                            Amount = amount,
                            CurrencyId = currencyId,
                            BetShopId = betShopId,
                            CreationTime = GetServerDate(),
                            UserId = Identity.Id,
                            BetShopAvailiableBalance = balance.AvailableBalance,
                            DocumentId = document.Id
                        };
                        Db.BetShopReconings.Add(betShopReconing);
                        betShop.CurrentLimit = betShop.DefaultLimit;
                        Db.SaveChanges();
                        scope.Complete();
                        return betShopReconing;
                    }
                }
            }
        }

        #endregion

        public List<string> GetPartnerLenguages(int partnerId)
        {
            return Db.PartnerLanguageSettings.Where(x => x.PartnerId == partnerId && x.State == (int)PartnerLanguageStates.Active).Select(x => x.LanguageId).ToList();
        }

        public List<string> GetPartnerCurrencies(int partnerId)
        {
            return Db.PartnerCurrencySettings.Where(x => x.PartnerId == partnerId && x.State == (int)PartnerLanguageStates.Active).Select(x => x.CurrencyId).ToList();
        }

        public Dictionary<int, FtpModel> GetPartnerEnvironments(int partnerId)
        {
            var partners = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partners.HaveAccessForAllObjects && partners.AccessibleObjects.All(x => x != partnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            return Db.PartnerKeys.Where(x => x.PartnerId == partnerId && x.PaymentSystemId == null &&
                                            (x.Name == Constants.PartnerKeys.FtpServer || 
                                             x.Name == Constants.PartnerKeys.FtpUserName || 
                                             x.Name == Constants.PartnerKeys.FtpPassword))
                                .AsEnumerable().GroupBy(x => x.NotificationServiceId.Value)
                                .ToDictionary(x => x.Key, x => new FtpModel
                                {
                                    Url = x.Where(y => y.Name == Constants.PartnerKeys.FtpServer).Select(y => y.StringValue).FirstOrDefault(),
                                    UserName = x.Where(y => y.Name == Constants.PartnerKeys.FtpUserName).Select(y => y.StringValue).FirstOrDefault(),
                                    Password = x.Where(y => y.Name == Constants.PartnerKeys.FtpPassword).Select(y => y.StringValue).FirstOrDefault()
                                });
        }
        public List<SecurityQuestion> GetPartnerSecurityQuestions(int? partnerId)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (partnerId.HasValue && !partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != partnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var query = Db.SecurityQuestions.AsQueryable();
            if (partnerId.HasValue)
                query = query.Where(x => x.PartnerId == partnerId);
            else if (!partnerAccess.HaveAccessForAllObjects)
                query = query.Where(x => partnerAccess.AccessibleObjects.Contains(x.PartnerId));
            return query.ToList();
        }

        public SecurityQuestion SavePartnerSecurityQuestion(SecurityQuestion securityQuestion)
        {
            var partners = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            if (!partners.HaveAccessForAllObjects && partners.AccessibleObjects.All(x => x != securityQuestion.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var currentDate = DateTime.UtcNow;
            SecurityQuestion dbSecurityQuestion;
            if (securityQuestion.Id == 0)
            {
                dbSecurityQuestion = new SecurityQuestion
                {
                    PartnerId = securityQuestion.PartnerId,
                    NickName = securityQuestion.NickName,
                    Status = securityQuestion.Status,
                    CreationTime = currentDate,
                    LastUpdateTime = currentDate,
                    Translation = CreateTranslation(new fnTranslation
                    {
                        ObjectTypeId = (int)ObjectTypes.SecurityQuestion,
                        Text = securityQuestion.QuestionText,
                        LanguageId = Constants.DefaultLanguageId
                    })
                };
                Db.SecurityQuestions.Add(dbSecurityQuestion);
                Db.SaveChanges();
                return dbSecurityQuestion;
            }
            dbSecurityQuestion = Db.SecurityQuestions.FirstOrDefault(x => x.Id == securityQuestion.Id);
            if (dbSecurityQuestion == null)
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            dbSecurityQuestion.NickName = securityQuestion.NickName;
            dbSecurityQuestion.Status = securityQuestion.Status;
            dbSecurityQuestion.LastUpdateTime = currentDate;
            Db.SaveChanges();
            CacheManager.RemovePartnerSecurityQuestionsByKey(securityQuestion.PartnerId, string.Empty);
            return dbSecurityQuestion;
        }
    }
}
