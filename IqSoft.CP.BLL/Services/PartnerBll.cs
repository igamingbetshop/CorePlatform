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
using log4net;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common.Models;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Web.UI.WebControls;
using IqSoft.CP.DAL.Models.Clients;
using System.Threading.Tasks;
using static IqSoft.CP.Common.Constants;
using System.Web;

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

        public List<int> GetActivePartnerIds()
        {
            return Db.Partners.Where(x => x.State == (int)PartnerStates.Active).Select(x => x.Id).ToList();
        }

        public PagedModel<Partner> GetPartnersPagedModel(FilterPartner filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<Partner>>
            {
                new CheckPermissionOutput<Partner>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.Id)
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
                Entities = filter.FilterObjects(Db.Partners, orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.Partners)
            };
        }

        public List<Partner> GetPartners(FilterPartner filter, bool checkPermissions = true)
        {
            if (checkPermissions)
            {
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });

                filter.CheckPermissionResuts = new List<CheckPermissionOutput<Partner>>
                {
                    new CheckPermissionOutput<Partner>
                    {
                        AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleIntegerObjects.Contains((int)x.ObjectId)
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
                !checkPermissionResult.AccessibleObjects.Contains(partnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var currentTime = GetServerDate();
            var dbPartner = Db.Partners.Where(x => x.Id == partnerId).UpdateFromQuery(x => new Partner { PasswordRegExp = pattern });
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
                !checkPermissionResult.AccessibleObjects.Contains(partner.Id))
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
            if (partnerAccess.HaveAccessForAllObjects || partnerAccess.AccessibleIntegerObjects.Contains(partnerid))
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
            if ((!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != partnerKey.PartnerId)))
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
                if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != partnerPaymentLimit.PartnerId))
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
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
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
                        AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleIntegerObjects.Contains((int)x.ObjectId)
                    },
                    new CheckPermissionOutput<Partner>
                    {
                        AccessibleObjects = exportAccess.AccessibleObjects,
                        HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                        Filter = x => exportAccess.AccessibleObjects.Contains(x.ObjectId)
                    }
                };
            }

            filter.TakeCount = 0;
            filter.SkipCount = 0;
            return filter.FilterObjects(Db.Partners).ToList();
        }

        public List<Partner> ExportPartnersModel(FilterPartner filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
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
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.Id)
                },
                new CheckPermissionOutput<Partner>
                {
                    AccessibleObjects = exportAccess.AccessibleObjects,
                    HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                    Filter = x => exportAccess.AccessibleObjects.Contains(x.Id)
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
                            (int)AccountTypes.BetShopBalance);
                        var partnerAcc = documentBl.GetOrCreateAccount(betShop.PartnerId, (int)ObjectTypes.Partner,
                            betShop.CurrencyId, (int)Common.Enums.AccountTypes.PartnerBalance);

                        var betShopItem = new OperationItem
                        {
                            AccountId = betShopAcc.Id,
                            Amount = operationAmount,
                            CurrencyId = currencyId,
                            OperationTypeId = (int)OperationTypes.PayBetShopDebt,
                            AccountTypeId = (int)Common.Enums.AccountTypes.BetShopBalance,
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
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != partnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);


            return Db.PartnerKeys.Where(x => x.PartnerId == partnerId && x.PaymentSystemId == null &&
                   (x.Name == Constants.PartnerKeys.FtpServer || x.Name == Constants.PartnerKeys.FtpUserName || x.Name == Constants.PartnerKeys.FtpPassword)).
                   GroupBy(x => x.NotificationServiceId.Value).
                   ToDictionary(x => x.Key, x => new FtpModel
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
            if (partnerId.HasValue && !partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != partnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var query = Db.SecurityQuestions.AsQueryable();
            if (partnerId.HasValue)
                query = query.Where(x => x.PartnerId == partnerId);
            else if (!partnerAccess.HaveAccessForAllObjects)
                query = query.Where(x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId));
            return query.ToList();
        }

        public SecurityQuestion SavePartnerSecurityQuestion(SecurityQuestion securityQuestion)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != securityQuestion.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var currentDate = DateTime.UtcNow;
            SecurityQuestion dbSecurityQuestion;
            if (securityQuestion.Id == 0)
            {
                dbSecurityQuestion = Db.SecurityQuestions.Add(new SecurityQuestion
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
                });
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

        public List<PartnerKey> CopyPartnerKeys(int fromPartnerId, int toPartnerId)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            CheckPermission(Constants.Permissions.ViewPartnerKey);
            CheckPermission(Constants.Permissions.EditPartnerKey);
            if (!partnerAccess.HaveAccessForAllObjects && (!partnerAccess.AccessibleIntegerObjects.Contains(fromPartnerId) ||
                !partnerAccess.AccessibleObjects.Contains(toPartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var dbToPartnerProducts = Db.PartnerKeys.Where(x => x.PartnerId == toPartnerId).Select(x => x.Name).ToList();
            Db.PartnerKeys.Where(x => x.PartnerId == fromPartnerId && !dbToPartnerProducts.Contains(x.Name)).GroupBy(x => x.Name).Select(y => y.FirstOrDefault())
            .InsertFromQuery(x => new
            {
                PartnerId = toPartnerId,
                x.GameProviderId,
                x.PaymentSystemId,
                x.Name
            });
            return GetPartnerKeys(toPartnerId);
        }

        public PagedModel<Email> GetEmails(FilterEmail filter, bool checkPermissions)
        {
            if (checkPermissions)
            {
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });

                filter.CheckPermissionResuts = new List<CheckPermissionOutput<Email>>
                {
                    new CheckPermissionOutput<Email>
                    {
                        AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                    }
                };
            }
            return new PagedModel<Email>
            {
                Entities = filter.FilterObjects(Db.Emails, email => email.OrderByDescending(y => y.Id)),
                Count = filter.SelectedObjectsCount(Db.Emails)
            };
        }

        public List<DAL.Models.Cache.BllPartnerCountrySetting> GetPartnerCountrySettings(int partnerId, int type)
        {
            var partner = CacheManager.GetPartnerById(partnerId) ??
                throw CreateException(LanguageId, Constants.Errors.PartnerNotFound);
            CheckPermission(Constants.Permissions.ViewPartnerCountrySetting);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && !partnerAccess.AccessibleIntegerObjects.Contains(partner.Id))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            return CacheManager.GetPartnerCountrySettings(partner.Id, type, LanguageId);
        }

        public PartnerCountrySetting SavePartnerCountrySetting(PartnerCountrySetting input, out List<int> clientIds)
        {
            var partner = CacheManager.GetPartnerById(input.PartnerId) ??
                throw CreateException(LanguageId, Constants.Errors.PartnerNotFound);
            CheckPermission(Constants.Permissions.ViewPartnerCountrySetting);
            CheckPermission(Constants.Permissions.EditPartnerCountrySetting);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && !partnerAccess.AccessibleIntegerObjects.Contains(partner.Id))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            if (!Enum.IsDefined(typeof(PartnerCountrySettingTypes), input.Type))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            var region = CacheManager.GetRegionById(input.RegionId, LanguageId);
            if (region == null || region.TypeId != (int)RegionTypes.Country)
                throw CreateException(LanguageId, Constants.Errors.RegionNotFound);
            clientIds = new List<int>();
            var dbSetting = Db.PartnerCountrySettings.FirstOrDefault(x => x.PartnerId == input.PartnerId && x.Type == input.Type && x.RegionId == input.RegionId);
            if (dbSetting != null)
                return dbSetting;
            Db.PartnerCountrySettings.Add(input);
            Db.SaveChanges();
            CacheManager.RemoveKeysFromCache(string.Format("{0}_{1}_{2}_", Constants.CacheItems.PartnerCountrySetting, input.PartnerId, input.Type));
            if (input.Type == (int)PartnerCountrySettingTypes.HighRisk)
            {
                clientIds = Db.Clients.Where(x => x.PartnerId == input.PartnerId &&
                                                   (x.CountryId == input.RegionId || x.RegionId == input.RegionId))
                                        .Select(x => x.Id).ToList();
                using (var clientBll = new ClientBll(this))
                    clientIds.ForEach(c =>
                    {
                        var clientSettings = new ClientCustomSettings
                        {
                            ClientId = c,
                            UnderMonitoring = (int)UnderMonitoringTypes.HighRiskCountry
                        };
                        clientBll.SaveClientSetting(clientSettings);
                    });
            }
            return input;
        }

        public void RemovePartnerCountrySetting(PartnerCountrySetting input, out List<int> clientIds)
        {
            var partner = CacheManager.GetPartnerById(input.PartnerId) ??
                throw CreateException(LanguageId, Constants.Errors.PartnerNotFound);
            CheckPermission(Constants.Permissions.ViewPartnerCountrySetting);
            CheckPermission(Constants.Permissions.EditPartnerCountrySetting);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && !partnerAccess.AccessibleIntegerObjects.Contains(partner.Id))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            Db.PartnerCountrySettings.Where(x => x.Id == input.Id).DeleteFromQuery();
            CacheManager.RemoveKeysFromCache(string.Format("{0}_{1}_{2}_", Constants.CacheItems.PartnerCountrySetting, input.PartnerId, input.Type));
            clientIds = new List<int>();

            if (input.Type == (int)PartnerCountrySettingTypes.HighRisk)
            {
                var highRiskCountries = CacheManager.GetPartnerCountrySettings(input.PartnerId, input.Type, LanguageId)
                                                    .Where(x => x.PartnerId == input.PartnerId && x.Type == input.Type)
                                                    .Select(x => x.RegionId).ToList();
                if (highRiskCountries != null && highRiskCountries.Any())
                {
                    var clients = Db.ClientSettings.Where(x => x.Name == Constants.ClientSettings.UnderMonitoring &&
                                                 x.StringValue.Contains(((int)UnderMonitoringTypes.HighRiskCountry).ToString()) &&
                                                 x.Client.PartnerId == input.PartnerId &&
                                                (!x.Client.CountryId.HasValue || !highRiskCountries.Contains(x.Client.CountryId.Value)) &&
                                                 !highRiskCountries.Contains(x.Client.RegionId)).ToList();
                    var currentDate = DateTime.UtcNow;
                    Parallel.ForEach(clients, c =>
                    {
                        var underMonitoringTypes = JsonConvert.DeserializeObject<List<int>>(c.StringValue);
                        underMonitoringTypes.Remove((int)UnderMonitoringTypes.HighRiskCountry);
                        c.StringValue = JsonConvert.SerializeObject(underMonitoringTypes);
                        c.LastUpdateTime = currentDate;
                    });
                    Db.SaveChanges();
                    clientIds.AddRange(clients.Select(x => x.ClientId));
                }
            }
        }

        public List<fnCharacter> GetCharacters(int? partnerId, int? id)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewCharacter,
                ObjectTypeId = ObjectTypes.Character
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            var result = Db.fn_Character(Identity.LanguageId).AsQueryable();
            if (partnerId != null)
                result = result.Where(x => x.PartnerId == partnerId);
            if (id != null)
                result = result.Where(x => x.Id == id);
            List<fnCharacter> fnCharacter;
            if (partnerAccess.HaveAccessForAllObjects)
                fnCharacter = result.OrderByDescending(x => x.Id).ToList();
            else
                fnCharacter = result.Where(x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)).OrderByDescending(x => x.Id).ToList();

            return fnCharacter;
        }

        public fnCharacter GetCharacterById(int id)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewCharacter,
                ObjectTypeId = ObjectTypes.Character
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            return Db.fn_Character(Identity.LanguageId).FirstOrDefault(x => x.Id == id);
        }

        public Character SaveCharacter(Character input, int environmentTypeId, string extension)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditCharacter,
                ObjectTypeId = ObjectTypes.Character
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != input.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var partner = CacheManager.GetPartnerById(input.PartnerId);
            var currentTime = DateTime.UtcNow;
            var dbCharacter = Db.Characters.FirstOrDefault(x => x.Id == input.Id);
            if (dbCharacter != null)
            {
                var imageVersion = dbCharacter.ImageUrl?.Split('?');
                dbCharacter.ImageUrl = imageVersion?.Length > 0 ? imageVersion[0] + "?ver=" + ((imageVersion?.Length > 1 ? Convert.ToInt32(imageVersion[1]?.Replace("ver=", string.Empty)) : 0) + 1) : string.Empty;
                var backgroundImage = dbCharacter.BackgroundImageUrl?.Split('?');
                dbCharacter.BackgroundImageUrl = backgroundImage?.Length > 0 ? backgroundImage[0] + "?ver=" + ((backgroundImage?.Length > 1 ? Convert.ToInt32(backgroundImage[1]?.Replace("ver=", string.Empty)) : 0) + 1) : string.Empty;
                dbCharacter.MobileBackgroundImageData = dbCharacter.BackgroundImageUrl?.Replace("/assets/images/characters/background/", "/assets/images/characters/background/mobile/");
                dbCharacter.NickName = input.NickName;
                dbCharacter.Status = input.Status;
                dbCharacter.Order = input.Order;
                dbCharacter.LastUpdateTime = currentTime;
                dbCharacter.CompPoints = input.CompPoints;
                dbCharacter.ParentId = input.ParentId;
            }
            else
            {
                dbCharacter = input.Copy();
                dbCharacter.Translation = CreateTranslation(new fnTranslation
                {
                    ObjectTypeId = (int)ObjectTypes.Character,
                    Text = input.Description,
                    LanguageId = Constants.DefaultLanguageId
                });
                dbCharacter.Translation1 = CreateTranslation(new fnTranslation
                {
                    ObjectTypeId = (int)ObjectTypes.Character,
                    Text = input.Title,
                    LanguageId = Constants.DefaultLanguageId
                });
                var order = input.Order;
                int? compPoints = 0;
                if (input.ParentId != null)
                {
                    var childrenCount = Db.Characters.Where(x => x.ParentId == input.ParentId && x.PartnerId == input.PartnerId).ToList().Count();
                    order = childrenCount + 1;
                    compPoints = childrenCount == 0 ? compPoints : input.CompPoints;
                }
                dbCharacter.Order = order;
                dbCharacter.CompPoints = compPoints;
                dbCharacter.CreationTime = currentTime;
                dbCharacter.LastUpdateTime = currentTime;
                Db.Characters.Add(dbCharacter);
                Db.SaveChanges();
                if (!string.IsNullOrEmpty(input.ImageData))
                    dbCharacter.ImageUrl = "/assets/images/characters/" + dbCharacter.Id.ToString() + extension + "?ver=1";
                if (!string.IsNullOrEmpty(input.BackgroundImageData))
                    dbCharacter.BackgroundImageUrl = "/assets/images/characters/background/" + dbCharacter.Id.ToString() + extension + "?ver=1";
                if (!string.IsNullOrEmpty(input.MobileBackgroundImageData))
                    dbCharacter.MobileBackgroundImageData = "/assets/images/characters/background/mobile/" + dbCharacter.Id.ToString() + extension + "?ver=1";
                Db.SaveChanges();
            }
            if (!string.IsNullOrEmpty(input.ImageData) || !string.IsNullOrEmpty(input.BackgroundImageData) || !string.IsNullOrEmpty(input.MobileBackgroundImageData))
            {
                var ftpModel = GetPartnerEnvironments(input.PartnerId)[environmentTypeId];
                if (ftpModel == null)
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.PartnerKeyNotFound);
                try
                {
                    var imgName = dbCharacter.Id.ToString() + extension;
                    if (!string.IsNullOrEmpty(input.ImageData))
                    {
                        var path = "/assets/images/characters/" + imgName;
                        byte[] bytes = Convert.FromBase64String(input.ImageData);
                        UploadFtpImage(bytes, ftpModel, "ftp://" + ftpModel.Url + "/coreplatform/website/" + partner.Name + path);
                    }
                    if (!string.IsNullOrEmpty(input.BackgroundImageData))
                    {
                        var path = "/assets/images/characters/background/" + imgName;
                        byte[] bytes = Convert.FromBase64String(input.BackgroundImageData);
                        UploadFtpImage(bytes, ftpModel, "ftp://" + ftpModel.Url + "/coreplatform/website/" + partner.Name + path);
                    }
                    if (!string.IsNullOrEmpty(input.MobileBackgroundImageData))
                    {
                        var path = "/assets/images/characters/background/mobile/" + imgName;
                        byte[] bytes = Convert.FromBase64String(input.MobileBackgroundImageData);
                        UploadFtpImage(bytes, ftpModel, "ftp://" + ftpModel.Url + "/coreplatform/website/" + partner.Name + path);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
            Db.SaveChanges();
            CacheManager.RemoveCharacters(input.PartnerId);
            return dbCharacter;
        }

        public void DeleteCharacterById(Character input)
        {
            var dbCharacter = Db.Characters.FirstOrDefault(x => x.Id == input.Id);
            Db.Characters.Remove(dbCharacter);
            Db.SaveChanges();
            CacheManager.RemoveCharacters(input.PartnerId);
        }

        public static Common.Models.WebSiteModels.ApiRestrictionModel GetApiPartnerRestrictions(int partnerId, Constants.SystemModuleTypes systemModuleTypes)
        {
            var moduleName = Enum.GetName(typeof(SystemModuleTypes), systemModuleTypes);
            var registrationLimitPerDay = CacheManager.GetConfigKey(partnerId, moduleName + Constants.PartnerKeys.RegistrationLimitPerDay);
            return new Common.Models.WebSiteModels.ApiRestrictionModel
            {
                WhitelistedCountries = CacheManager.GetConfigParameters(partnerId, moduleName + Constants.PartnerKeys.WhitelistedCountries)
                                       .Select(x => x.Value).ToList() ?? new List<string>(),
                BlockedCountries = CacheManager.GetConfigParameters(partnerId, moduleName + Constants.PartnerKeys.BlockedCountries)
                                  .Select(x => x.Value).ToList() ?? new List<string>(),
                WhitelistedIps = CacheManager.GetConfigParameters(partnerId, moduleName + Constants.PartnerKeys.WhitelistedIps)
                                .Select(x => x.Value).ToList() ?? new List<string>(),
                BlockedIps = CacheManager.GetConfigParameters(partnerId, moduleName + Constants.PartnerKeys.BlockedIps)
                            .Select(x => x.Value).ToList() ?? new List<string>(),
                RegistrationLimitPerDay = int.TryParse(registrationLimitPerDay, out int limit) ? limit : (int?)null,
                ConnectingIPHeader = CacheManager.GetConfigKey(partnerId, moduleName + Constants.PartnerKeys.ConnectingIPHeader),
                IPCountryHeader = CacheManager.GetConfigKey(partnerId, moduleName + Constants.PartnerKeys.IPCountryHeader)
            };
        }

        public static void CheckApiRestrictions(int partnerId, Constants.SystemModuleTypes systemModuleTypes)
        {
            var ipCountry = HttpContext.Current.Request.Headers.Get("CF-IPCountry") ?? string.Empty;
            var ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP");
            if (string.IsNullOrEmpty(ip))
                ip = "127.0.0.1";
            var apiRestrictions = GetApiPartnerRestrictions(partnerId, systemModuleTypes);
            if (apiRestrictions.BlockedIps.Contains(ip))
                throw CreateException(string.Empty, Constants.Errors.DontHavePermission);
            if (!apiRestrictions.WhitelistedIps.Any(x => x.IsIpEqual(ip)) &&
                apiRestrictions.WhitelistedCountries.Any() && !apiRestrictions.WhitelistedCountries.Contains(ipCountry))
                throw CreateException(string.Empty, Constants.Errors.DontHavePermission);
        }
    }
}
