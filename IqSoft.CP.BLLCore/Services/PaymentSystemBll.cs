﻿using System;
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
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.DAL.Models.Cache;
using Newtonsoft.Json;
using log4net;
using Microsoft.EntityFrameworkCore;

namespace IqSoft.CP.BLL.Services
{
    public class PaymentSystemBll : PermissionBll, IPaymentSystemBll
    {
        #region Constructors

        public PaymentSystemBll(SessionIdentity identity, ILog log)
            : base(identity, log)
        {

        }

        public PaymentSystemBll(BaseBll baseBl)
            : base(baseBl)
        {

        }

        #endregion

        public List<PaymentSystem> GetPaymentSystems()
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPaymentSystems
            });

            return Db.PaymentSystems.ToList();
        }

        public PaymentSystem SavePaymentSystem(PaymentSystem paymentSystem)
        {
            if (!Enum.IsDefined(typeof(OpenModes), paymentSystem.ContentType))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            var currentTime = GetServerDate();
            var dbPaymentSystem = Db.PaymentSystems.FirstOrDefault(x => x.Id == paymentSystem.Id);
            if (dbPaymentSystem == null)
            {
                paymentSystem.SessionId = SessionId;
                paymentSystem.CreationTime = currentTime;
                paymentSystem.LastUpdateTime = currentTime;
                paymentSystem.Translation = CreateTranslation(new fnTranslation
                {
                    ObjectTypeId = (int)ObjectTypes.PaymentSystem,
                    Text = paymentSystem.Name,
                    LanguageId = Constants.DefaultLanguageId
                });
                Db.PaymentSystems.Add(paymentSystem);
                SaveChanges();
                return paymentSystem;
            }
            dbPaymentSystem.SessionId = SessionId;
            dbPaymentSystem.Name = paymentSystem.Name;
            dbPaymentSystem.Type = paymentSystem.Type;
            dbPaymentSystem.ContentType = paymentSystem.ContentType;
            SaveChanges();
            return dbPaymentSystem;
        }

        public PartnerPaymentSetting GetPartnerPaymentSettingById(int partnerPaymentSettingId)
        {
            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreatePartnerPaymentSetting,
                ObjectTypeId = (int)ObjectTypes.PartnerPaymentSetting,
                ObjectId = partnerPaymentSettingId
            });

            if (!checkPermissionResult.HaveAccessForAllObjects &&
                !checkPermissionResult.AccessibleObjects.AsEnumerable().Contains(partnerPaymentSettingId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

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
                !checkPermissionResult.AccessibleObjects.AsEnumerable().Contains(partnerPaymentCurrencyRate.PaymentSettingId))
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
                !checkPermissionResult.AccessibleObjects.AsEnumerable().Contains(partnerPaymentSettingId))
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
                !checkPermissionResult.AccessibleObjects.AsEnumerable().Contains(partnerPaymentSetting.Id))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            if ((partnerPaymentSetting.OpenMode.HasValue && !Enum.IsDefined(typeof(OpenModes), partnerPaymentSetting.OpenMode.Value)) ||
                 partnerPaymentSetting.Commission < 0 || partnerPaymentSetting.Commission >=100 || partnerPaymentSetting.FixedFee < 0)
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            var currentTime = GetServerDate();
            var dbPartnerSetting = Db.PartnerPaymentSettings.Include(x => x.PartnerPaymentCountrySettings).FirstOrDefault(x => x.Id == partnerPaymentSetting.Id);
            partnerPaymentSetting.LastUpdateTime = currentTime;
            partnerPaymentSetting.SessionId = Identity.SessionId;
            if (dbPartnerSetting == null)
            {
                partnerPaymentSetting.CreationTime = currentTime;
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
                var countries = partnerPaymentSetting.PartnerPaymentCountrySettings.Select(x => x.CountryId).ToList();
                Db.PartnerPaymentCountrySettings.Where(x => x.PartnerPaymentSettingId == dbPartnerSetting.Id && !countries.Contains(x.CountryId)).DeleteFromQuery();
                var dbCountries = dbPartnerSetting.PartnerPaymentCountrySettings.Select(x => x.CountryId).ToList();
                countries.RemoveAll(x => dbCountries.Contains(x));
                foreach (var c in countries)
                    Db.PartnerPaymentCountrySettings.Add(new PartnerPaymentCountrySetting { PartnerPaymentSettingId = dbPartnerSetting.Id, CountryId = c });
            }
            SaveChanges();
            CacheManager.UpdateParnerPaymentSettings(partnerPaymentSetting.PartnerId, partnerPaymentSetting.PaymentSystemId, partnerPaymentSetting.CurrencyId, partnerPaymentSetting.Type);
            return partnerPaymentSetting;
        }

        public List<fnPartnerPaymentSetting> GetfnPartnerPaymentSettings(FilterfnPartnerPaymentSetting filter, bool checkPermissions)
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
                        Filter = x => checkP.AccessibleObjects.AsEnumerable().Contains(x.ObjectId)
                    },
                    new CheckPermissionOutput<fnPartnerPaymentSetting>
                    {
                        AccessibleObjects = partnerAccess.AccessibleObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                    }
                };
            }
            else
                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnPartnerPaymentSetting>>();

            return filter.FilterObjects(Db.fn_PartnerPaymentSetting(LanguageId)).OrderBy(x => x.PaymentSystemPriority).ToList();
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
                FirstName = x.Session.User.FirstName,
                LastName = x.Session.User.LastName
            }).ToList();
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
                        AccessibleObjects = partnerAccess.AccessibleObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                    },
                    new CheckPermissionOutput<fnPaymentRequest>
                    {
                        AccessibleObjects = paymentRequestAccess.AccessibleObjects,
                        HaveAccessForAllObjects = paymentRequestAccess.HaveAccessForAllObjects,
                        Filter = x => paymentRequestAccess.AccessibleObjects.AsEnumerable().Contains(x.Id)
                    },
                    new CheckPermissionOutput<fnPaymentRequest>
                    {
                        AccessibleObjects = clientAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                        Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId)
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

                var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliateReferral,
                    ObjectTypeId = (int)ObjectTypes.AffiliateReferral
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
                        AccessibleObjects = partnerAccess.AccessibleObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                    },
                    new CheckPermissionOutput<fnPaymentRequest>
                    {
                        AccessibleObjects = clientCategoryAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientCategoryAccess.HaveAccessForAllObjects,
                        Filter = x => clientCategoryAccess.AccessibleObjects.AsEnumerable().Contains(x.CategoryId)
                    },
                    new CheckPermissionOutput<fnPaymentRequest>
                    {
                        AccessibleObjects = paymentRequestAccess.AccessibleObjects,
                        HaveAccessForAllObjects = paymentRequestAccess.HaveAccessForAllObjects,
                        Filter = x => paymentRequestAccess.AccessibleObjects.AsEnumerable().Contains(x.Id)
                    },
                    new CheckPermissionOutput<fnPaymentRequest>
                    {
                        AccessibleObjects = clientAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                        Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId)
                    },
                    new CheckPermissionOutput<fnPaymentRequest>
                    {
                        AccessibleObjects = affiliateReferralAccess.AccessibleObjects,
                        HaveAccessForAllObjects = affiliateReferralAccess.HaveAccessForAllObjects,
                        Filter = x => x.AffiliateReferralId.HasValue && affiliateReferralAccess.AccessibleObjects.AsEnumerable().Contains(x.AffiliateReferralId.Value)
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

            var totalRequests = (from r in filter.FilterObjects(Db.fn_PaymentRequest())
                                 group r by r.CurrencyId into requests
                                 select new
                                 {
                                     CurrencyId = requests.Key,
                                     TotalAmount = requests.Sum(b => b.Amount),
                                     TotalRequestsCount = requests.Count(),
                                     TotalUniquePlayers = requests.Select(b => b.ClientId).Distinct().Count()
                                 }).ToList();

            var entries = filter.FilterObjects(Db.fn_PaymentRequest(), orderBy);
            if (convertCurrency)
            {
                foreach (var e in entries)
                {
                    e.Amount = ConvertCurrency(e.CurrencyId, CurrencyId, e.Amount);
                }
            }
            return new PaymentRequestsReport
            {
                Entities = entries,
                Count = totalRequests.Sum(x => x.TotalRequestsCount),
                TotalAmount = totalRequests.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalAmount)),
                TotalUniquePlayers = totalRequests.Sum(x => x.TotalUniquePlayers)
            };
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
                    Filter = x => paymentAccess.AccessibleObjects.AsEnumerable().Contains(x.Id)
                },
                new CheckPermissionOutput<fnPaymentRequest>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                }
            };
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
            return filter.FilterObjects(Db.fn_PaymentRequest(), orderBy).ToList();
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
                if (request.Amount != dbRequest.Amount)
                {
                    dbRequest.Amount = request.Amount;
                    var client = CacheManager.GetClientById(dbRequest.ClientId);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, dbRequest.PaymentSystemId, client.CurrencyId, dbRequest.Type);
                    dbRequest.CommissionAmount = (dbRequest.Amount * partnerPaymentSetting.Commission / 100m) + partnerPaymentSetting.FixedFee;
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
                var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });
                if (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != partnerId))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

                return Db.fn_PartnerBankInfo(LanguageId).Where(x => x.PartnerId == partnerId).ToList();
            }
            if (client != null)
            {
                var banks = Db.fn_PartnerBankInfo(LanguageId).Where(x => x.PartnerId == partnerId && x.Active &&
                                                                        (paymentSystemId == null || x.PaymentSystemId == paymentSystemId.Value) &&
                                                                         x.CurrencyId == client.CurrencyId && (type == null || x.Type == type.Value))
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
                        b.ClientPaymentInfos = string.IsNullOrEmpty(b.AccountNumber) ? new List<ClientPaymentInfo>() :
                                               b.AccountNumber.Split(',').Select(x => new ClientPaymentInfo { BankAccountNumber = x, Type = 0 }).ToList();
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
            var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var checkPartnerEditPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditPartnerBank
            });
            if (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != partnerBankInfo.PartnerId) ||
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
                partnerBankInfo.Translation = CreateTranslation(new fnTranslation
                {
                    ObjectTypeId = (int)ObjectTypes.PartnerBank,
                    Text = partnerBankInfo.BankName,
                    LanguageId = Constants.DefaultLanguageId
                });
                partnerBankInfo.CreationTime = currentTime;
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