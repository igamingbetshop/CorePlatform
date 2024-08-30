using System;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Filters.PaymentRequests;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Report;
using log4net;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.DAL.Models.Cache;
using System.Data.Entity;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.AdminModels;


namespace IqSoft.CP.BLL.Services
{
    public class PaymentSystemBll : PermissionBll, IPaymentSystemBll
    {
        #region Constructors

        public PaymentSystemBll(SessionIdentity identity, ILog log, int? timeout = null)
            : base(identity, log, timeout)
        {

        }

        public PaymentSystemBll(BaseBll baseBl)
            : base(baseBl)
        {

        }

        #endregion

        public List<PaymentSystem> GetPaymentSystems(bool? isActive)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPaymentSystems
            });
            if (isActive.HasValue)
                return Db.PaymentSystems.Where(x => x.IsActive == isActive).ToList();
            return Db.PaymentSystems.ToList();
        }

        public void SavePaymentSystem(ApiPaymentSystemModel apiPaymentSystemModel)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPaymentSystems,
                ObjectTypeId = (int)ObjectTypes.PaymentSystem
            });
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditPaymentSystem,
                ObjectTypeId = (int)ObjectTypes.PaymentSystem
            });

            if (apiPaymentSystemModel.ContentType.HasValue && !Enum.IsDefined(typeof(OpenModes), apiPaymentSystemModel.ContentType) ||
                ((apiPaymentSystemModel.Ids == null || !apiPaymentSystemModel.Ids.Any()) && apiPaymentSystemModel.Id.HasValue && apiPaymentSystemModel.Id <= 0))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            var p = apiPaymentSystemModel.Ids ?? new List<int>();
            if (!apiPaymentSystemModel.IsActive.Value &&
               Db.PartnerPaymentSettings.Any(x => x.State == (int)PartnerPaymentSettingStates.Active &&
                                                 (x.PaymentSystemId == apiPaymentSystemModel.Id || p.Contains(x.PaymentSystemId))))
                throw CreateException(LanguageId, Constants.Errors.ImpermissiblePaymentSetting);

            var currentTime = DateTime.Now;
            if (apiPaymentSystemModel.Ids == null || !apiPaymentSystemModel.Ids.Any())
            {
                if (Db.PaymentSystems.Any(x => x.Name.ToLower() == apiPaymentSystemModel.Name.ToLower() && x.Id != apiPaymentSystemModel.Id))
                    throw CreateException(LanguageId, Constants.Errors.NickNameExists);
                var dbPaymentSystem = Db.PaymentSystems.FirstOrDefault(x => x.Id == apiPaymentSystemModel.Id);
                if (dbPaymentSystem == null)
                {
                    if (Db.PaymentSystems.Any(x => x.Name == apiPaymentSystemModel.Name))
                        throw CreateException(LanguageId, Constants.Errors.NickNameExists);
                    Db.PaymentSystems.Add(new PaymentSystem
                    {
                        Id = apiPaymentSystemModel.Id.Value,
                        Name = apiPaymentSystemModel.Name,
                        PeriodicityOfRequest = apiPaymentSystemModel.PeriodicityOfRequest ?? 0,
                        PaymentRequestSendCount = apiPaymentSystemModel.PaymentRequestSendCount ?? 0,
                        Type = apiPaymentSystemModel.Type ?? 0,
                        IsActive = apiPaymentSystemModel.IsActive ?? true,
                        ContentType = apiPaymentSystemModel.ContentType ?? 0,
                        CreationTime = currentTime,
                        LastUpdateTime = currentTime,
                        Translation = CreateTranslation(new fnTranslation
                        {
                            ObjectTypeId = (int)ObjectTypes.PaymentSystem,
                            Text = apiPaymentSystemModel.Name,
                            LanguageId = Constants.DefaultLanguageId
                        })
                    });
                }
                else
                {
                    dbPaymentSystem.SessionId = SessionId;
                    dbPaymentSystem.Name = apiPaymentSystemModel.Name;
                    if (apiPaymentSystemModel.Type.HasValue)
                        dbPaymentSystem.Type = apiPaymentSystemModel.Type.Value;
                    if (apiPaymentSystemModel.ContentType.HasValue)
                        dbPaymentSystem.ContentType = apiPaymentSystemModel.ContentType.Value;
                    if (apiPaymentSystemModel.IsActive.HasValue)
                        dbPaymentSystem.IsActive = apiPaymentSystemModel.IsActive.Value;
                    dbPaymentSystem.LastUpdateTime = currentTime;
                }
                SaveChanges();
            }
            else
            {
                Db.PaymentSystems.Where(x => apiPaymentSystemModel.Ids.Contains(x.Id)).ToList()
                    .ForEach(x =>
                    {
                        if (apiPaymentSystemModel.Type.HasValue)
                            x.Type = apiPaymentSystemModel.Type.Value;
                        if (apiPaymentSystemModel.ContentType.HasValue)
                            x.ContentType = apiPaymentSystemModel.ContentType.Value;
                        if (apiPaymentSystemModel.IsActive.HasValue)
                            x.IsActive = apiPaymentSystemModel.IsActive.Value;
                        x.LastUpdateTime = currentTime;
                    });
                SaveChanges();
            }
            if (apiPaymentSystemModel.Id.HasValue && apiPaymentSystemModel.Id > 0)
                CacheManager.RemoveKeysFromCache(string.Format("{0}_{1}", Constants.CacheItems.PaymentSystems, apiPaymentSystemModel.Id.Value));
            else
                apiPaymentSystemModel.Ids.ForEach(x =>
                {
                    CacheManager.RemoveKeysFromCache(string.Format("{0}_{1}", Constants.CacheItems.PaymentSystems, x));
                });
        }

        public PartnerPaymentSetting GetPartnerPaymentSettingById(int partnerPaymentSettingId, bool checkPermissions = true)
        {
            if (checkPermissions)
            {
                var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.CreatePartnerPaymentSetting,
                    ObjectTypeId = (int)ObjectTypes.PartnerPaymentSetting,
                    ObjectId = partnerPaymentSettingId
                });

                if (!checkPermissionResult.HaveAccessForAllObjects &&
                    !checkPermissionResult.AccessibleObjects.Contains(partnerPaymentSettingId))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            return Db.PartnerPaymentSettings.Include(x => x.PartnerPaymentCountrySettings).Include(x => x.PaymentSystem)
                                            .FirstOrDefault(x => x.Id == partnerPaymentSettingId);
        }

        public PartnerPaymentCurrencyRate SavePartnerPaymentCurrencyRate(PartnerPaymentCurrencyRate partnerPaymentCurrencyRate, out PartnerPaymentSetting partnerPaymentSetting)
        {
            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreatePartnerPaymentSetting,
                ObjectTypeId = (int)ObjectTypes.PartnerPaymentSetting,
                ObjectId = partnerPaymentCurrencyRate.PaymentSettingId
            });

            if (!checkPermissionResult.HaveAccessForAllObjects &&
                !checkPermissionResult.AccessibleObjects.Contains(partnerPaymentCurrencyRate.PaymentSettingId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            partnerPaymentSetting = Db.PartnerPaymentSettings.FirstOrDefault(x => x.Id == partnerPaymentCurrencyRate.PaymentSettingId);
            if (partnerPaymentSetting == null)
                throw CreateException(LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
            var dbPartnerPaymentCurrencyRate = Db.PartnerPaymentCurrencyRates.FirstOrDefault(x => x.PaymentSettingId == partnerPaymentCurrencyRate.PaymentSettingId &&
            x.CurrencyId == partnerPaymentCurrencyRate.CurrencyId);
            if (dbPartnerPaymentCurrencyRate != null)
            {
                dbPartnerPaymentCurrencyRate.Rate = partnerPaymentCurrencyRate.Rate;
                partnerPaymentCurrencyRate.Id = dbPartnerPaymentCurrencyRate.Id;
            }
            else
                Db.PartnerPaymentCurrencyRates.Add(partnerPaymentCurrencyRate);
            SaveChanges();
            CacheManager.UpdateParnerPaymentSettings(partnerPaymentSetting.PartnerId, partnerPaymentSetting.PaymentSystemId, partnerPaymentSetting.CurrencyId, partnerPaymentSetting.Type);
            return partnerPaymentCurrencyRate;
        }

        public List<PartnerPaymentCurrencyRate> GetPartnerPaymentCurrencyRates(int partnerPaymentSettingId)
        {
            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartnerPaymentSetting,
                ObjectTypeId = (int)ObjectTypes.PartnerPaymentSetting,
                ObjectId = partnerPaymentSettingId
            });

            if (!checkPermissionResult.HaveAccessForAllObjects &&
                !checkPermissionResult.AccessibleObjects.Contains(partnerPaymentSettingId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            return Db.PartnerPaymentCurrencyRates.Where(x => x.PaymentSettingId == partnerPaymentSettingId).ToList();
        }

        public PartnerPaymentSetting SavePartnerPaymentSetting(PartnerPaymentSetting partnerPaymentSetting)
        {
            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreatePartnerPaymentSetting,
                ObjectTypeId = (int)ObjectTypes.PartnerPaymentSetting,
                ObjectId = partnerPaymentSetting.Id
            });

            if (!checkPermissionResult.HaveAccessForAllObjects &&
                !checkPermissionResult.AccessibleObjects.Contains(partnerPaymentSetting.Id))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            if ((partnerPaymentSetting.OpenMode.HasValue && (
                !Enum.IsDefined(typeof(OpenModes), partnerPaymentSetting.OpenMode.Value / 10) ||
                !Enum.IsDefined(typeof(OpenModes), partnerPaymentSetting.OpenMode.Value % 10))) ||
                 partnerPaymentSetting.Commission < 0 || partnerPaymentSetting.Commission >= 100 || partnerPaymentSetting.FixedFee < 0 ||
                 partnerPaymentSetting.ApplyPercentAmount < 0)
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            var currentTime = GetServerDate();
            var paymentSystem = CacheManager.GetPaymentSystemById(partnerPaymentSetting.PaymentSystemId);
            if (paymentSystem == null || (!paymentSystem.IsActive && partnerPaymentSetting.State == (int)PartnerPaymentSettingStates.Active))
                throw CreateException(LanguageId, Constants.Errors.PaymentSystemNotFound);

            var dbPartnerSetting = Db.PartnerPaymentSettings.Include(x => x.PartnerPaymentCountrySettings).FirstOrDefault(x => x.Id == partnerPaymentSetting.Id);
            partnerPaymentSetting.LastUpdateTime = currentTime;
            partnerPaymentSetting.SessionId = Identity.SessionId;
            if (dbPartnerSetting == null)
            {
                partnerPaymentSetting.CreationTime = currentTime;
                partnerPaymentSetting.LastUpdateTime = currentTime;
                Db.PartnerPaymentSettings.Add(partnerPaymentSetting);
            }
            else
            {
                if (string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                    partnerPaymentSetting.UserName = dbPartnerSetting.UserName;
                if (string.IsNullOrEmpty(partnerPaymentSetting.Password))
                    partnerPaymentSetting.Password = dbPartnerSetting.Password;
                partnerPaymentSetting.CreationTime = dbPartnerSetting.CreationTime;
                Db.Entry(dbPartnerSetting).CurrentValues.SetValues(partnerPaymentSetting);
                if (partnerPaymentSetting.PartnerPaymentCountrySettings != null)
                {
                    if (!partnerPaymentSetting.PartnerPaymentCountrySettings.Any())
                        Db.PartnerPaymentCountrySettings.Where(x => x.PartnerPaymentSettingId == dbPartnerSetting.Id).DeleteFromQuery();
                    else
                    {
                        var type = partnerPaymentSetting.PartnerPaymentCountrySettings.First().Type;
                        var countries = partnerPaymentSetting.PartnerPaymentCountrySettings.Select(x => x.CountryId).ToList();
                        Db.PartnerPaymentCountrySettings.Where(x => x.PartnerPaymentSettingId == dbPartnerSetting.Id &&
                                                                  (x.Type != type || !countries.Contains(x.CountryId))).DeleteFromQuery();
                        var dbCountries = Db.PartnerPaymentCountrySettings.Where(x => x.PartnerPaymentSettingId == dbPartnerSetting.Id)
                                                                          .Select(x => x.CountryId).ToList();
                        countries.RemoveAll(x => dbCountries.Contains(x));
                        foreach (var c in countries)
                            Db.PartnerPaymentCountrySettings.Add(new PartnerPaymentCountrySetting { PartnerPaymentSettingId = dbPartnerSetting.Id, CountryId = c, Type = type });

                    }
                }
                if (partnerPaymentSetting.PartnerPaymentSegmentSettings != null)
                {
                    if (!partnerPaymentSetting.PartnerPaymentSegmentSettings.Any())
                        Db.PartnerPaymentSegmentSettings.Where(x => x.PartnerPaymentSettingId == dbPartnerSetting.Id).DeleteFromQuery();
                    else
                    {
                        var type = partnerPaymentSetting.PartnerPaymentSegmentSettings.First().Type;
                        var countries = partnerPaymentSetting.PartnerPaymentSegmentSettings.Select(x => x.SegmentId).ToList();
                        Db.PartnerPaymentSegmentSettings.Where(x => x.PartnerPaymentSettingId == dbPartnerSetting.Id &&
                                                                  (x.Type != type || !countries.Contains(x.SegmentId))).DeleteFromQuery();
                        var dbCountries = Db.PartnerPaymentSegmentSettings.Where(x => x.PartnerPaymentSettingId == dbPartnerSetting.Id)
                                                                          .Select(x => x.SegmentId).ToList();
                        countries.RemoveAll(x => dbCountries.Contains(x));
                        foreach (var c in countries)
                            Db.PartnerPaymentSegmentSettings.Add(new PartnerPaymentSegmentSetting { PartnerPaymentSettingId = dbPartnerSetting.Id, SegmentId = c, Type = type });
                    }
                }
            }
            SaveChanges();
            CacheManager.UpdateParnerPaymentSettings(partnerPaymentSetting.PartnerId, partnerPaymentSetting.PaymentSystemId, partnerPaymentSetting.CurrencyId, partnerPaymentSetting.Type);
            return partnerPaymentSetting;
        }

        public List<fnPartnerPaymentSetting> GetfnPartnerPaymentSettings(FilterfnPartnerPaymentSetting filter, bool checkPermissions, int partnerId)
        {
            if (checkPermissions)
            {
                var checkP = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartnerPaymentSetting,
                    ObjectTypeId = (int)ObjectTypes.PartnerPaymentSetting
                });

                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });

                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnPartnerPaymentSetting>>
                {
                    new CheckPermissionOutput<fnPartnerPaymentSetting>
                    {
                        AccessibleObjects = checkP.AccessibleObjects,
                        HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                        Filter = x => checkP.AccessibleObjects.Contains(x.ObjectId)
                    },
                    new CheckPermissionOutput<fnPartnerPaymentSetting>
                    {
                        AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                    }
                };
            }
            else
                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnPartnerPaymentSetting>>();
            var paymentSettings = filter.FilterObjects(Db.fn_PartnerPaymentSetting(LanguageId)).OrderBy(x => x.PaymentSystemPriority).ToList();
            return paymentSettings;
        }

        public List<PaymentRequestHistoryElement> GetPaymentRequestHistories(List<long> requestIds, int? status = null)
        {
            var query = Db.PaymentRequestHistories.Where(x => requestIds.Contains(x.RequestId));
            if (status != null)
                query = query.Where(x => x.Status == status.Value);
            return query.Select(x => new PaymentRequestHistoryElement
            {
                Id = x.Id,
                RequestId = x.RequestId,
                Status = x.Status,
                Comment = x.Comment,
                CreationTime = x.CreationTime,
                FirstName = x.UserSession.User.FirstName,
                LastName = x.UserSession.User.LastName
            }).ToList();
        }

        public PaymentRequestHistoryClients SuccessPaymantRequest(int clientId)
        {

            FilterfnPaymentRequest filter = new FilterfnPaymentRequest
            {
                ClientIds = new FiltersOperation
                {
                    IsAnd = false,
                    OperationTypeList = new List<FiltersOperationType> { new FiltersOperationType { OperationTypeId = (int)FilterOperations.IsEqualTo, IntValue = clientId } }
                },
                States = new FiltersOperation
                {
                    IsAnd = false,
                    OperationTypeList = new List<FiltersOperationType> {
                    new FiltersOperationType { OperationTypeId = (int)FilterOperations.IsEqualTo, IntValue =  (int) PaymentRequestStates.ApprovedManually },
                    new FiltersOperationType { OperationTypeId = (int)FilterOperations.IsEqualTo, IntValue =  (int) PaymentRequestStates.Approved }
                   }
                }
            };

            var paymentRequests = GetPaymentRequests(filter, false).ToList();
            return new PaymentRequestHistoryClients
            {
                TotalWithdrawAmount = paymentRequests?.Where(x => x.Type == (int)PaymentRequestTypes.Withdraw).Sum(x => x.Amount),
                TotalDepositAmount = paymentRequests?.Where(x => x.Type == (int)PaymentRequestTypes.Deposit).Sum(x => x.Amount),
                LastDepositDate = paymentRequests?.Where(x => x.Type == (int)PaymentRequestTypes.Deposit).OrderByDescending(x => x.CreationTime).FirstOrDefault()?.CreationTime,
                CountOfDeposits = paymentRequests?.Where(x => x.Type == (int)PaymentRequestTypes.Deposit).Count(),
                CountOfDepositLastWeek = paymentRequests?.Where(x => x.Type == (int)PaymentRequestTypes.Deposit &&x.CreationTime>= DateTime.Now.AddDays(-7)&&x.CreationTime <=  DateTime.Now)
                .OrderByDescending(x => x.CreationTime).Take(7).Count(),
                FirstDepositDate = paymentRequests?.Where(x => x.Type == (int)PaymentRequestTypes.Deposit).OrderBy(x => x.CreationTime).FirstOrDefault()?.CreationTime,
                LastWithdrawDate = paymentRequests?.Where(x => x.Type == (int)PaymentRequestTypes.Withdraw).OrderByDescending(x => x.CreationTime).FirstOrDefault()?.CreationTime,
            };
        }

        public List<fnPaymentRequest> GetPaymentRequests(FilterfnPaymentRequest filter, bool checkPermissions)
        {
            if (checkPermissions)
            {
                var clientAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });

                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });

                var paymentRequestAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPaymentRequests,
                    ObjectTypeId = (int)ObjectTypes.PaymentRequest
                });

                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnPaymentRequest>>
                {
                    new CheckPermissionOutput<fnPaymentRequest>
                    {
                        AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId ?? 0)
                    },
                    new CheckPermissionOutput<fnPaymentRequest>
                    {
                        AccessibleObjects = paymentRequestAccess.AccessibleObjects,
                        HaveAccessForAllObjects = paymentRequestAccess.HaveAccessForAllObjects,
                        Filter = x => paymentRequestAccess.AccessibleObjects.Contains(x.Id)
                    },
                    new CheckPermissionOutput<fnPaymentRequest>
                    {
                        AccessibleObjects = clientAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                        Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId ?? 0)
                    }
                };
            }
            else
                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnPaymentRequest>>();

            Func<IQueryable<fnPaymentRequest>, IOrderedQueryable<fnPaymentRequest>> orderBy;
            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnPaymentRequest>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnPaymentRequest>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = payment => payment.OrderByDescending(x => x.Id);
            }

            return filter.FilterObjects(Db.fn_PaymentRequest()).ToList();
        }

        public PaymentRequest GetPaymentRequestByPaymentSetting(int partnerPaymentSettingId, int state)
        {
            return Db.PaymentRequests.FirstOrDefault(x => x.PartnerPaymentSettingId == partnerPaymentSettingId && x.Status == state);
        }

        public PaymentRequestsReport GetPaymentRequestsPaging(FilterfnPaymentRequest filter, bool convertCurrency, bool checkPermissions)
        {
            if (checkPermissions)
            {
                var clientAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });

                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });

                var clientCategoryAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClientByCategory,
                    ObjectTypeId = (int)ObjectTypes.ClientCategory
                });

                var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliate,
                    ObjectTypeId = (int)ObjectTypes.Affiliate
                });

                var paymentRequestAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPaymentRequests,
                    ObjectTypeId = (int)ObjectTypes.PaymentRequest
                });

                var paymentStatusAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPaymentStatuses,
                    ObjectTypeId = (int)ObjectTypes.Enumeration
                });

                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnPaymentRequest>>
                {
                    new CheckPermissionOutput<fnPaymentRequest>
                    {
                        AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId ?? 0)
                    },
                    new CheckPermissionOutput<fnPaymentRequest>
                    {
                        AccessibleObjects = clientCategoryAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientCategoryAccess.HaveAccessForAllObjects,
                        Filter = x => clientCategoryAccess.AccessibleObjects.Contains(x.CategoryId ?? 0)
                    },
                    new CheckPermissionOutput<fnPaymentRequest>
                    {
                        AccessibleObjects = paymentRequestAccess.AccessibleObjects,
                        HaveAccessForAllObjects = paymentRequestAccess.HaveAccessForAllObjects,
                        Filter = x => paymentRequestAccess.AccessibleObjects.Contains(x.Id)
                    },
                    new CheckPermissionOutput<fnPaymentRequest>
                    {
                        AccessibleObjects = clientAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                        Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId ?? 0)
                    },
                    new CheckPermissionOutput<fnPaymentRequest>
                    {
                        AccessibleStringObjects = affiliateAccess.AccessibleStringObjects,
                        HaveAccessForAllObjects = affiliateAccess.HaveAccessForAllObjects,
                        Filter = x => !string.IsNullOrEmpty(x.AffiliateId) && affiliateAccess.AccessibleStringObjects.Contains(x.AffiliateId)
                    },
                    new CheckPermissionOutput<fnPaymentRequest>
                    {
                        AccessibleObjects = paymentStatusAccess.AccessibleObjects,
                        HaveAccessForAllObjects = paymentStatusAccess.HaveAccessForAllObjects,
                        Filter = x => paymentStatusAccess.AccessibleObjects.Contains(x.Status)
                    }
                };
            }
            else
                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnPaymentRequest>>();

            Func<IQueryable<fnPaymentRequest>, IOrderedQueryable<fnPaymentRequest>> orderBy;
            if (filter.OrderBy.HasValue)
                orderBy = QueryableUtilsHelper.OrderByFunc<fnPaymentRequest>(filter.FieldNameToOrderBy, filter.OrderBy.Value);
            else
                orderBy = payment => payment.OrderByDescending(x => x.Id);

            var totalRequests = (from r in filter.FilterObjects(Db.fn_PaymentRequest())
                                 group r by r.CurrencyId into requests
                                 select new
                                 {
                                     CurrencyId = requests.Key,
                                     TotalAmount = requests.Sum(b => b.Amount),
                                     TotalFinalAmount = requests.Sum(b => b.FinalAmount),
                                     TotalRequestsCount = requests.Count(),
                                     TotalUniquePlayers = requests.Select(b => b.ClientId).Distinct().Count()
                                 }).ToList();

            // var newFilter = filter.Copy(); ??
            var entries = filter.FilterObjects(Db.fn_PaymentRequest(), orderBy).ToList();
            if (convertCurrency)
            {
                foreach (var e in entries)
                {
                    e.ConvertedAmount = ConvertCurrency(e.CurrencyId, CurrencyId, e.Amount);
                }
            }
            return new PaymentRequestsReport
            {
                Entities = entries,
                Count = totalRequests.Sum(x => x.TotalRequestsCount),
                TotalAmount = totalRequests.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalAmount)),
                TotalFinalAmount = totalRequests.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalFinalAmount ?? 0)),
                TotalUniquePlayers = totalRequests.Sum(x => x.TotalUniquePlayers)
            };
        }

        public PaymentRequest GetClientLastWithdrawRequest(int clientId)
        {
            return Db.PaymentRequests.Where(x => x.ClientId == clientId && x.Type == (int)PaymentRequestTypes.Withdraw &&
                                                 x.Status == (int)PaymentRequestStates.Pending || x.Status == (int)PaymentRequestStates.Confirmed)
                                     .OrderByDescending(x => x.Id).FirstOrDefault();
        }

        public PaymentRequest GetPaymentRequestById(long id)
        {
            return Db.PaymentRequests.FirstOrDefault(x => x.Id == id);
        }

        public PaymentRequest GetPaymentRequestByExternalId(string externalId, int paymentSystemId)
        {
            return Db.PaymentRequests.FirstOrDefault(x => x.PaymentSystemId == paymentSystemId && x.ExternalTransactionId == externalId);
        }

        public ClientSession GetClientSessionById(long id)
        {
            return Db.ClientSessions.FirstOrDefault(x => x.Id == id);
        }

        public fnPaymentRequest GetfnPaymentRequestById(long id)
        {
            return Db.fn_PaymentRequest().FirstOrDefault(x => x.Id == id);
        }

        public PaymentRequest GetPaymentRequest(string externalTransactionId, int paymentSystemId, int type)
        {
            return Db.PaymentRequests.FirstOrDefault(x => x.ExternalTransactionId == externalTransactionId && x.PaymentSystemId == paymentSystemId && x.Type == type);
        }

        public List<PaymentRequest> GetPaymentRequests(int paymentSystemId, int status)
        {
            return Db.PaymentRequests.Where(x => x.Status == status && x.PaymentSystemId == paymentSystemId).ToList();
        }

        public List<Document> DeleteDepositFromBetShop(int paymentRequestId)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.DeleteDepositFromBetShop
            });
            var paymentRequest = GetPaymentRequestById(paymentRequestId);
            if (paymentRequest.PaymentSystem.Name != Constants.PaymentSystems.BetShop)
                throw CreateException(LanguageId, Constants.Errors.CanNotDeleteRollbackDocument);

            using (var documentBl = new DocumentBll(this))
            {
                var document = Db.Documents.FirstOrDefault(x => x.ExternalTransactionId == paymentRequestId.ToString() &&
                    x.OperationTypeId == (int)OperationTypes.TransferFromBetShopToClient && x.GameProviderId == null && x.ProductId == null && x.ParentId == null);
                if (document == null)
                    throw CreateException(string.Empty, Constants.Errors.DocumentNotFound);

                var shift = Db.CashDeskShifts.Where(x =>
                            x.CashDeskId == document.CashDeskId && x.CashierId == document.UserId)
                            .OrderByDescending(x => x.Id)
                            .FirstOrDefault();
                if (shift == null || shift.State == (int)CashDeskShiftStates.Closed || shift.StartTime > document.CreationTime)
                    throw CreateException(string.Empty, Constants.Errors.ShiftNotFound);

                var documentList = new List<Document> { document };
                var resp = documentBl.RollBackPaymentRequest(documentList);
                paymentRequest.Status = (int)PaymentRequestStates.CanceledByUser;
                Db.SaveChanges();
                return resp;
            }
        }

        #region Export to excel

        private List<fnPaymentRequest> ExportPaymentRequests(FilterfnPaymentRequest filter)
        {
            filter.WithPendings = true;
            var paymentAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPaymentRequests,
                ObjectTypeId = (int)ObjectTypes.PaymentRequest
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnPaymentRequest>>
            {
                new CheckPermissionOutput<fnPaymentRequest>
                {
                    AccessibleObjects = paymentAccess.AccessibleObjects,
                    HaveAccessForAllObjects = paymentAccess.HaveAccessForAllObjects,
                    Filter = x => paymentAccess.AccessibleObjects.Contains(x.Id)
                },
                new CheckPermissionOutput<fnPaymentRequest>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId ?? 0)
                }
            };
            Func<IQueryable<fnPaymentRequest>, IOrderedQueryable<fnPaymentRequest>> orderBy;
            if (filter.OrderBy.HasValue)
                orderBy = QueryableUtilsHelper.OrderByFunc<fnPaymentRequest>(filter.FieldNameToOrderBy, filter.OrderBy.Value);
            else
                orderBy = payment => payment.OrderByDescending(x => x.Id);
            filter.TakeCount = 0;
            filter.SkipCount = 0;
            var result = filter.FilterObjects(Db.fn_PaymentRequest(), orderBy).ToList();
            foreach (var e in result)
            {
                e.ConvertedAmount = ConvertCurrency(e.CurrencyId, CurrencyId, e.Amount);
            }
            return result;
        }

        public List<fnPaymentRequest> ExportDepositPaymentRequests(FilterfnPaymentRequest filter)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportDeposit
            });

            return ExportPaymentRequests(filter);
        }

        public List<fnPaymentRequest> ExportWithdrawalPaymentRequests(FilterfnPaymentRequest filter)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportWithdrawal
            });
            return ExportPaymentRequests(filter);
        }

        #endregion

        public void ChangePaymentRequestDetails(PaymentRequest request)
        {
            var dbRequest = GetPaymentRequestById(request.Id);
            if (dbRequest != null)
            {
                dbRequest.PaymentSystemId = request.PaymentSystemId;
                dbRequest.ExternalTransactionId = request.ExternalTransactionId;
                dbRequest.Info = request.Info;
                dbRequest.Parameters = request.Parameters;
                dbRequest.CardNumber = request.CardNumber;
                dbRequest.CountryCode = request.CountryCode;
                if (request.Amount != dbRequest.Amount)
                {
                    dbRequest.Amount = request.Amount;
                    var client = CacheManager.GetClientById(dbRequest.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, dbRequest.PaymentSystemId, client.CurrencyId, dbRequest.Type);
                    var commissionAmount = partnerPaymentSetting.FixedFee;
                    if (!partnerPaymentSetting.ApplyPercentAmount.HasValue || request.Amount >= partnerPaymentSetting.ApplyPercentAmount)
                        commissionAmount += request.Amount * partnerPaymentSetting.Commission / 100;
                    dbRequest.CommissionAmount = commissionAmount;
                }
                dbRequest.LastUpdateTime = DateTime.UtcNow;
                Db.SaveChanges();
            }
        }

        public List<PaymentRequestHistory> GetPaymentRequestComments(long requestId)
        {
            return Db.PaymentRequestHistories.Where(x => x.RequestId == requestId).ToList();
        }

        public List<fnPartnerBankInfo> GetPartnerBanks(int partnerId, int? paymentSystemId, bool checkPermission, int? type, BllClient client = null)
        {
            if (checkPermission)
            {
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });
                if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != partnerId))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

                return Db.fn_PartnerBankInfo(LanguageId).Where(x => x.PartnerId == partnerId).ToList();
            }
            if (client != null)
            {
                var banks = Db.fn_PartnerBankInfo(LanguageId).Where(x => x.PartnerId == partnerId && x.Active &&
                                                                        (paymentSystemId == null || x.PaymentSystemId == paymentSystemId.Value) &&
                                                                        x.CurrencyId == client.CurrencyId &&
                                                                        (type == null || x.Type == type.Value))
                                                             .OrderBy(x => x.Order).ThenBy(x => x.BankName).ToList();

                if (type == (int)BankInfoTypes.BankForCustomer)
                {
                    var clientAccounts = Db.ClientPaymentInfoes.Where(x => x.ClientId == client.Id).ToList();
                    foreach (var b in banks)
                        b.ClientPaymentInfos = clientAccounts.Where(x => x.BankName == b.BankName).ToList();
                }
                else if (type == (int)BankInfoTypes.BankForCompany)
                {
                    foreach (var b in banks)
                    {
                        if (!string.IsNullOrEmpty(b.AccountNumber))
                            b.ClientPaymentInfos = b.AccountNumber.Split(',').Select(x => new ClientPaymentInfo { BankAccountNumber = x, BankIBAN = b.IBAN, Type = 0 }).ToList();
                        else if (!string.IsNullOrEmpty(b.IBAN))
                            b.ClientPaymentInfos = b.IBAN.Split(',').Select(x => new ClientPaymentInfo { BankIBAN = x, Type = 0 }).ToList();
                        else
                            b.ClientPaymentInfos = new List<ClientPaymentInfo>();
                    }
                }

                return banks;
            }

            return Db.fn_PartnerBankInfo(LanguageId).Where(x => x.PartnerId == partnerId && x.Active &&
                                                                (paymentSystemId == null || x.PaymentSystemId == paymentSystemId.Value))
                                                    .OrderBy(x => x.Order).ToList();
        }

        public fnPartnerBankInfo GetBankInfoById(int bankInfoId)
        {
            return Db.fn_PartnerBankInfo(LanguageId).FirstOrDefault(x => x.Id == bankInfoId);
        }

        public fnPartnerBankInfo UpdatePartnerBankInfo(PartnerBankInfo partnerBankInfo)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var checkPartnerEditPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditPartnerBank
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != partnerBankInfo.PartnerId) ||
                !checkPartnerEditPermission.HaveAccessForAllObjects)
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var currentTime = GetServerDate();
            partnerBankInfo.LastUpdateTime = currentTime;
            if (partnerBankInfo.Id > 0)
            {
                var dbPartnerBankInfo = Db.PartnerBankInfoes.Where(x => x.Id == partnerBankInfo.Id).FirstOrDefault();
                if (dbPartnerBankInfo != null)
                {
                    dbPartnerBankInfo.PaymentSystemId = partnerBankInfo.PaymentSystemId;
                    dbPartnerBankInfo.BankName = partnerBankInfo.BankName;
                    dbPartnerBankInfo.BankCode = partnerBankInfo.BankCode;
                    dbPartnerBankInfo.OwnerName = partnerBankInfo.OwnerName;
                    dbPartnerBankInfo.BranchName = partnerBankInfo.BranchName;
                    dbPartnerBankInfo.IBAN = partnerBankInfo.IBAN;
                    dbPartnerBankInfo.AccountNumber = partnerBankInfo.AccountNumber;
                    dbPartnerBankInfo.CurrencyId = partnerBankInfo.CurrencyId;
                    dbPartnerBankInfo.Active = partnerBankInfo.Active;
                    dbPartnerBankInfo.Type = partnerBankInfo.Type;
                    dbPartnerBankInfo.Order = partnerBankInfo.Order;
                    partnerBankInfo.LastUpdateTime = currentTime;
                    partnerBankInfo.CreationTime = dbPartnerBankInfo.CreationTime;
                    partnerBankInfo.PartnerId = dbPartnerBankInfo.PartnerId;
                }
            }
            else
            {
                var dbPartnerBankInfo = Db.PartnerBankInfoes.Where(x => x.Type == partnerBankInfo.Type && x.PartnerId == partnerBankInfo.PartnerId &&
                    x.PaymentSystemId == partnerBankInfo.PaymentSystemId && x.BankName == partnerBankInfo.BankName &&
                    x.CurrencyId == partnerBankInfo.CurrencyId && x.Type == partnerBankInfo.Type).FirstOrDefault();
                if (dbPartnerBankInfo != null)
                    throw CreateException(LanguageId, Constants.Errors.ClientPaymentInfoAlreadyExists);

                partnerBankInfo.CreationTime = currentTime;
                partnerBankInfo.Translation = CreateTranslation(new fnTranslation
                {
                    ObjectTypeId = (int)ObjectTypes.PartnerBank,
                    Text = partnerBankInfo.BankName,
                    LanguageId = Constants.DefaultLanguageId
                });
                Db.PartnerBankInfoes.Add(partnerBankInfo);
            }
            Db.SaveChanges();
            return Db.fn_PartnerBankInfo(LanguageId).Where(x => x.Id == partnerBankInfo.Id).FirstOrDefault();
        }

        public int GetPaymentRequestsCount(List<int> statuses, List<int> paymentSystems, int? agentId)
        {
            var request = Db.PaymentRequests.Where(x => statuses.Contains(x.Status) && paymentSystems.Contains(x.PaymentSystemId));
            if (agentId != null)
                request = request.Where(x => x.Client.User.Path.Contains("/" + agentId.Value + "/"));
            return request.Count();
        }
    }
}