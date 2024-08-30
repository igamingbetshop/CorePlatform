using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Helpers;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Filters.Bets;
using IqSoft.CP.DAL.Filters.Clients;
using IqSoft.CP.DAL.Filters.Reporting;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Dashboard;
using IqSoft.CP.DAL.Models.PlayersDashboard;
using IqSoft.CP.DAL.Models.RealTime;
using IqSoft.CP.DAL.Models.Report;
using Newtonsoft.Json;
using static IqSoft.CP.Common.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NDBConetxt=System.Data.Entity.DbContext;

namespace IqSoft.CP.BLL.Services
{
    public class ReportBll : PermissionBll, IReportBll
    {
        private static readonly string ConfigurationString;


        #region Constructors
        static ReportBll()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configurationRoot = builder.Build();
            ConfigurationString = configurationRoot.GetSection("ConnectionStrings").GetSection("IqSoftCorePlatformLogger").Value;
        }

        public ReportBll(SessionIdentity identity, ILog log, IWebHostEnvironment env = null, int? timeout = null) : base(identity, log, env, timeout)
        {

        }

        public ReportBll(BaseBll baseBl) : base(baseBl)
        {

        }

        #endregion


        public List<ShiftInfo> GetShifts(DateTime startTime, DateTime endTime, int cashDeskId, int? cashierId)
        {
            var response = new List<ShiftInfo>();
            var shifts = Db.CashDeskShifts.Include(x => x.Cashier).Include(x => x.CashDesk.BetShop).Where(x =>
                            (x.EndTime == null || (x.StartTime >= startTime && x.EndTime <= endTime)) &&
                            (cashierId == null || x.CashierId == cashierId) && x.CashDeskId == cashDeskId).ToList();
            foreach (var s in shifts)
            {
                var info = new ShiftInfo
                {
                    Id = s.Id,
                    Number = s.Number,
                    CashierFirstName = s.Cashier.FirstName,
                    CashierLastName = s.Cashier.LastName,
                    BetShopId = s.CashDesk.BetShopId,
                    CashDeskId = s.CashDeskId,
                    BetShopAddress = s.CashDesk.BetShop.Address,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    StartAmount = s.StartAmount,
                    BetAmount = s.BetAmount,
                    PayedWin = s.PayedWinAmount,
                    DepositToInternetClient = s.DepositAmount,
                    WithdrawFromInternetClient = s.WithdrawAmount,
                    DebitCorrectionOnCashDesk = s.DebitCorrectionAmount,
                    CreditCorrectionOnCashDesk = s.CreditCorrectionAmount,
                    BonusAmount = s.BonusAmount,
                    EndAmount = s.EndAmount
                };
                if (s.State == (int)CashDeskShiftStates.Active)
                {
                    /*var sInfo = Db.fn_ShiftReport(s.StartTime, DateTime.UtcNow, cashDeskId, Identity.Id).FirstOrDefault();
                    if (sInfo != null)
                    {
                        info.BetAmount = sInfo.BetAmount;
                        info.PayedWin = sInfo.PayedWin;
                        info.DepositToInternetClient = sInfo.DepositToInternetClient;
                        info.WithdrawFromInternetClient = sInfo.WithdrawFromInternetClient;
                        info.DebitCorrectionOnCashDesk = sInfo.DebitCorrectionOnCashDesk;
                        info.CreditCorrectionOnCashDesk = sInfo.CreditCorrectionOnCashDesk;
                    }*/
                }
                response.Add(info);
            }
            return response;
        }

        public BetShopReconingOutput GetBetShopReconingsPage(FilterfnBetShopReconing filter)
        {
            var checkP = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShopReconing,
                ObjectTypeId = (int)ObjectTypes.BetShopReconing
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var betShopAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShop,
                ObjectTypeId = (int)ObjectTypes.BetShop
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnBetShopReconing>>
            {
                new CheckPermissionOutput<fnBetShopReconing>
                {
                    AccessibleObjects = checkP.AccessibleObjects,
                    HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                    Filter = x=> checkP.AccessibleObjects.AsEnumerable().Contains(x.ObjectId)
                },
                new CheckPermissionOutput<fnBetShopReconing>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnBetShopReconing>
                {
                    AccessibleObjects = betShopAccess.AccessibleObjects,
                    HaveAccessForAllObjects = betShopAccess.HaveAccessForAllObjects,
                    Filter = x => betShopAccess.AccessibleObjects.AsEnumerable().Contains(x.BetShopId)
                }
            };

            PagedModel<fnBetShopReconing> pgmBetShopReconing = new PagedModel<fnBetShopReconing>
            {
                Entities = filter.FilterObjects(Db.fn_BetShopReconing(), reconings => reconings.OrderBy(x => x.Id)).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_BetShopReconing())
            };

            return new BetShopReconingOutput
            {
                Entities = pgmBetShopReconing.Entities,
                Count = pgmBetShopReconing.Count,
                TotalAmount = pgmBetShopReconing.Entities.Sum(x => x.Amount),
                TotalBalance = pgmBetShopReconing.Entities.Sum(x => x.BetShopAvailiableBalance)
            };
        }

        public List<fnCashDeskTransaction> GetCashDeskTransactions(FilterCashDeskTransaction filter, string languageId)
        {
            var viewCashDeskTransactionsAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewCashDeskTransactions
            });
            var betShopAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShop,
                ObjectTypeId = (int)ObjectTypes.BetShop
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnCashDeskTransaction>>
            {
                new CheckPermissionOutput<fnCashDeskTransaction>
                {
                    AccessibleObjects = viewCashDeskTransactionsAccess.AccessibleObjects,
                    HaveAccessForAllObjects = viewCashDeskTransactionsAccess.HaveAccessForAllObjects,
                    Filter = x => viewCashDeskTransactionsAccess.AccessibleObjects.AsEnumerable().Contains(x.ObjectId)
                },
                new CheckPermissionOutput<fnCashDeskTransaction>
                {
                    AccessibleObjects = betShopAccess.AccessibleObjects,
                    HaveAccessForAllObjects = betShopAccess.HaveAccessForAllObjects,
                    Filter = x => betShopAccess.AccessibleObjects.AsEnumerable().Contains(x.BetShopId)
                },
                new CheckPermissionOutput<fnCashDeskTransaction>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                }
            };
            if (string.IsNullOrEmpty(languageId))
                languageId = LanguageId;
            return filter.FilterObjects(Db.fn_CashDeskTransaction(languageId), tr => tr.OrderByDescending(x => x.Id)).ToList();
        }

        public CashdeskTransactionsReport GetCashDeskTransactionsPage(FilterCashDeskTransaction filter)
        {
            return new CashdeskTransactionsReport();
        }

        #region Dashboard And RealTime

        public BetsInfo GetBetsInfoForDashboard(FilterDashboard filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewDashboard
            });

            int partnerId = filter.PartnerId ?? 0;
            var partnerIds = new List<long>();
            if (partnerId == 0 && !partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.Any())
            {
                partnerIds = partnerAccess.AccessibleObjects.ToList();
            }
            else if (partnerId != 0 || partnerAccess.HaveAccessForAllObjects)
            {
                partnerIds.Add(partnerId);
            }

            var betShopObjects = new List<fnBetShopBetForDashboard>();
            var objects = new List<fnInternetBetForDashboard>();
            if (partnerIds.Any())
            {
                var fDate = (long)filter.FromDate.Year * 1000000 + (long)filter.FromDate.Month * 10000 + (long)filter.FromDate.Day * 100 + (long)filter.FromDate.Hour;
                var tDate = (long)filter.ToDate.Year * 1000000 + (long)filter.ToDate.Month * 10000 + (long)filter.ToDate.Day * 100 + (long)filter.ToDate.Hour;
                var bSource = Db.fn_BetShopBetForDashboard(fDate, tDate, 0).ToList();
                var source = Db.fn_InternetBetForDashboard(fDate, tDate, 0).ToList();
                foreach (var id in partnerIds)
                {
                    betShopObjects.AddRange(bSource.Where(x => x.PartnerId == id || id == 0).ToList());
                    objects.AddRange(source.Where(x => x.PartnerId == id || id == 0).ToList());
                }
            }

            var betShopBets = betShopObjects.Select(x => new BetsInfo
            {
                CurrencyId = x.CurrencyId,
                TotalBetsAmount = x.TotalBetAmount ?? 0,
                TotalWinsAmount = x.TotalWinAmount ?? 0,
                TotalBetsCount = x.TotalCount ?? 0
            }).ToList();

            var internetBets = objects.Select(x => new BetsInfo
            {
                CurrencyId = x.CurrencyId,
                DeviceTypeId = x.DeviceTypeId,
                TotalBetsAmount = x.TotalBetAmount ?? 0,
                TotalBetsCount = x.TotalCount ?? 0,
                TotalWinsAmount = x.TotalWinAmount ?? 0,
                TotalPlayersCount = x.TotalPlayersCount ?? 0
            }).ToList();
            var allBets = betShopBets;
            allBets.AddRange(internetBets);
            return new BetsInfo
            {
                TotalBetsCount = allBets.Sum(x => x.TotalBetsCount),
                TotalBetsCountFromWebSite = allBets.Where(x => x.DeviceTypeId == (int)DeviceTypes.Desktop).Sum(x => x.TotalBetsCount),
                TotalBetsCountFromMobile = allBets.Where(x => x.DeviceTypeId == (int)DeviceTypes.Mobile).Sum(x => x.TotalBetsCount),
                TotalBetsCountFromWap = allBets.Where(x => x.DeviceTypeId == (int)DeviceTypes.Terminal).Sum(x => x.TotalBetsCount),

                TotalBetsAmount = allBets.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalBetsAmount)),
                TotalBetsFromWebSite = allBets.Where(x => x.DeviceTypeId == (int)DeviceTypes.Desktop).Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalBetsAmount)),
                TotalBetsFromMobile = allBets.Where(x => x.DeviceTypeId == (int)DeviceTypes.Mobile).Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalBetsAmount)),
                TotalBetsFromWap = allBets.Where(x => x.DeviceTypeId == (int)DeviceTypes.Terminal).Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalBetsAmount)),
                TotalGGR = allBets.Where(x => x.DocumentState != 4 && x.DocumentState != 1).Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalBetsAmount - x.TotalWinsAmount)),
                TotalGGRFromWebSite = allBets.Where(x => x.DocumentState != 4 && x.DocumentState != 1 && x.DeviceTypeId == (int)DeviceTypes.Desktop).Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalBetsAmount - x.TotalWinsAmount)),
                TotalGGRFromMobile = allBets.Where(x => x.DocumentState != 4 && x.DocumentState != 1 && x.DeviceTypeId == (int)DeviceTypes.Mobile).Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalBetsAmount - x.TotalWinsAmount)),
                TotalGGRFromWap = allBets.Where(x => x.DocumentState != 4 && x.DocumentState != 1 && x.DeviceTypeId == (int)DeviceTypes.Terminal).Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalBetsAmount - x.TotalWinsAmount)),

                TotalPlayersCount = allBets.Sum(x => x.TotalPlayersCount),
                TotalPlayersCountFromWebSite = allBets.Where(x => x.DeviceTypeId == (int)DeviceTypes.Desktop).Sum(x => x.TotalPlayersCount),
                TotalPlayersCountFromMobile = allBets.Where(x => x.DeviceTypeId == (int)DeviceTypes.Mobile).Sum(x => x.TotalPlayersCount),
                TotalPlayersCountFromWap = allBets.Where(x => x.DeviceTypeId == (int)DeviceTypes.Terminal).Sum(x => x.TotalPlayersCount)
            };
        }

        public List<DepositsInfo> GetDepositsForDashboard(FilterDashboard filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewDepositsTotals
            });

            var fDate = (long)filter.FromDate.Year * 1000000 + (long)filter.FromDate.Month * 10000 + (long)filter.FromDate.Day * 100 + (long)filter.FromDate.Hour;
            var tDate = (long)filter.ToDate.Year * 1000000 + (long)filter.ToDate.Month * 10000 + (long)filter.ToDate.Day * 100 + (long)filter.ToDate.Hour;

            var paymentRequestFilter = new FilterPaymentRequest
            {
                FromDate = fDate,
                ToDate = tDate,
                PartnerId = filter.PartnerId,
                Type = (int)PaymentRequestTypes.Deposit
            };
            if (!partnerAccess.HaveAccessForAllObjects)
            {
                if (partnerAccess.AccessibleObjects.Any())
                    paymentRequestFilter.PartnerId = Convert.ToInt32(partnerAccess.AccessibleObjects.First());
                else
                    paymentRequestFilter.PartnerId = -1;
            }

            var deposits = (from d in paymentRequestFilter.FilterObjects(Db.PaymentRequests, d => d.OrderByDescending(x => x.Id))
                            group d by new { d.Status }
                into deps
                            select new DepositsInfo
                            {
                                Status = deps.Key.Status,
                                TotalPlayersCount = deps.Select(x => x.ClientId).Distinct().Count(),
                                Deposits = (from dp in deps
                                            group dp by new { dp.PaymentSystemId, dp.CurrencyId }
                                    into depositInfoes
                                            select new DepositInfo
                                            {
                                                CurrencyId = depositInfoes.Key.CurrencyId,
                                                PaymentSystemId = depositInfoes.Key.PaymentSystemId,
                                                TotalAmount = depositInfoes.Sum(x => x.Amount),
                                                TotalDepositsCount = depositInfoes.Count(),
                                                TotalPlayersCount = depositInfoes.Select(x => x.ClientId).Distinct().Count()
                                            }).ToList()
                            }).ToList();

            foreach (var deposit in deposits)
            {
                deposit.Deposits = (from dp in deposit.Deposits
                                    group dp by new { dp.PaymentSystemId }
                    into depositInfoes
                                    select new DepositInfo
                                    {
                                        PaymentSystemId = depositInfoes.Key.PaymentSystemId,
                                        PaymentSystemName = CacheManager.GetPaymentSystemById(depositInfoes.Key.PaymentSystemId).Name,
                                        TotalAmount = depositInfoes.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalAmount)),
                                        TotalDepositsCount = depositInfoes.Count(),
                                        TotalPlayersCount = depositInfoes.Sum(x => x.TotalPlayersCount)
                                    }).ToList();
            }
            return deposits;
        }

        public object GetMemberPaymentsForDashboard(FilterDashboard filter)
        {
            var fDate = (long)filter.FromDate.Year * 1000000 + (long)filter.FromDate.Month * 10000 + (long)filter.FromDate.Day * 100 + (long)filter.FromDate.Hour;
            var tDate = (long)filter.ToDate.Year * 1000000 + (long)filter.ToDate.Month * 10000 + (long)filter.ToDate.Day * 100 + (long)filter.ToDate.Hour;

            var paymentRequestFilter = new FilterPaymentRequest
            {
                FromDate = fDate,
                ToDate = tDate,
                AgentId = Identity.Id
            };

            var user = CacheManager.GetUserById(Identity.Id);
            if (user.Type == (int)UserTypes.AgentEmployee)
            {
                CheckPermission(Constants.Permissions.ViewDashboard);
                paymentRequestFilter.AgentId = user.ParentId;
            }
            var query = paymentRequestFilter.FilterObjects(Db.PaymentRequests).Where(x => x.Status == (int)PaymentRequestStates.Approved ||
                                                                                            x.Status == (int)PaymentRequestStates.ApprovedManually);
            var firstTimeDepositors = (from c in Db.Clients
                                       join pr in Db.PaymentRequests on c.Id equals pr.ClientId
                                       where c.User.Path.Contains("/" + filter.AgentId + "/") &&
                                             c.FirstDepositDate >= filter.FromDate && c.FirstDepositDate < filter.ToDate &&
                                             pr.Type == (int)PaymentRequestTypes.Deposit &&
                                            (pr.Status == (int)PaymentRequestStates.Approved ||pr.Status == (int)PaymentRequestStates.ApprovedManually)
                                       group pr by pr.ClientId into grp
                                       select new
                                       {
                                           ClientId = grp.Key,
                                           FirstDeposit = grp.OrderBy(x => x.Id).FirstOrDefault()
                                       }).ToList();
            return new
            {
                FirstDepositAmount = firstTimeDepositors.Select(x => x.FirstDeposit.Amount).DefaultIfEmpty(0).Sum(),
                FirstDepositCount = firstTimeDepositors.Count,
                TotalDepositAmount = query.Where(x => x.Type == (int)PaymentRequestTypes.Deposit).Select(x => x.Amount).DefaultIfEmpty(0).Sum(),
                TotalDepositsCount = query.Where(x => x.Type == (int)PaymentRequestTypes.Deposit).Count(),
                TotalWithdrawAmount = query.Where(x => x.Type == (int)PaymentRequestTypes.Withdraw).Select(x => x.Amount).DefaultIfEmpty(0).Sum(),
                TotalWithdrawsCount = query.Where(x => x.Type == (int)PaymentRequestTypes.Withdraw).Count()
            };
        }

        public List<WithdrawalsInfo> GetWithdrawalsForDashboard(FilterDashboard filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewWithdrawalsTotals
            });
            var fDate = (long)filter.FromDate.Year * 1000000 + (long)filter.FromDate.Month * 10000 + (long)filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = (long)filter.ToDate.Year * 1000000 + (long)filter.ToDate.Month * 10000 + (long)filter.ToDate.Day * 100 + filter.ToDate.Hour;

            var paymentRequestFilter = new FilterPaymentRequest
            {
                FromDate = fDate,
                ToDate = tDate,
                PartnerId = filter.PartnerId,
                Type = (int)PaymentRequestTypes.Withdraw
            };
            if (!partnerAccess.HaveAccessForAllObjects)
            {
                if (partnerAccess.AccessibleObjects.Any())
                    paymentRequestFilter.PartnerId = Convert.ToInt32(partnerAccess.AccessibleObjects.First());
                else
                    paymentRequestFilter.PartnerId = -1;
            }
            var withdrawals = (from w in paymentRequestFilter.FilterObjects(Db.PaymentRequests, d => d.OrderByDescending(x => x.Id))
                               group w by new { w.Status } into withs
                               select new WithdrawalsInfo
                               {
                                   Status = withs.Key.Status,
                                   TotalPlayersCount = withs.Select(x => x.ClientId).Distinct().Count(),
                                   Withdrawals = (from wt in withs
                                                  group wt by new { wt.PaymentSystemId, wt.CurrencyId } into withdrawalInfoes
                                                  select new WithdrawalInfo
                                                  {
                                                      CurrencyId = withdrawalInfoes.Key.CurrencyId,
                                                      PaymentSystemId = withdrawalInfoes.Key.PaymentSystemId,
                                                      TotalAmount = withdrawalInfoes.Sum(x => x.Amount),
                                                      TotalWithdrawalsCount = withdrawalInfoes.Count(),
                                                      TotalPlayersCount = withdrawalInfoes.Select(x => x.ClientId).Distinct().Count()
                                                  }).ToList()
                               }).ToList();
            foreach (var withdrawal in withdrawals)
            {
                withdrawal.Withdrawals = (from wt in withdrawal.Withdrawals
                                          group wt by new { wt.PaymentSystemId }
                    into withdrawalInfoes
                                          select new WithdrawalInfo
                                          {
                                              PaymentSystemId = withdrawalInfoes.Key.PaymentSystemId,
                                              PaymentSystemName = CacheManager.GetPaymentSystemById(withdrawalInfoes.Key.PaymentSystemId).Name,
                                              TotalAmount =
                                                  withdrawalInfoes.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalAmount)),
                                              TotalWithdrawalsCount = withdrawalInfoes.Count(),
                                              TotalPlayersCount = withdrawalInfoes.Sum(x => x.TotalPlayersCount)
                                          }).ToList();
            }
            return withdrawals;
        }

        public ProvidersBetsInfo GetProviderBetsForDashboard(FilterDashboard filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            int partnerId = filter.PartnerId != null ? filter.PartnerId.Value : 0;
            if (!partnerAccess.HaveAccessForAllObjects)
            {
                if (partnerAccess.AccessibleObjects.Any())
                    partnerId = Convert.ToInt32(partnerAccess.AccessibleObjects.First());
                else
                    partnerId = -1;
            }

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewDashboard
            });
            var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * 1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            var bSource = Db.fn_BetShopBetForDashboard(fDate, tDate, partnerId).ToList();
            var betShopBets = bSource.Select(x => new ProviderBetsInfo
            {
                ProviderId = x.ProviderId ?? 0,
                CurrencyId = x.CurrencyId,
                TotalBetsAmount = x.TotalBetAmount ?? 0,
                TotalWinsAmount = x.TotalWinAmount ?? 0,
                TotalBetsCount = x.TotalCount ?? 0,
                TotalGGR = (x.TotalBetAmount - x.TotalWinAmount) ?? 0,
                TotalBetsAmountFromBetShop = x.TotalBetAmount ?? 0
            }).ToList();
            var source = Db.fn_InternetBetForDashboard(fDate, tDate, partnerId).ToList();
            var internetBets = (from ib in source
                                group ib by new { ib.ProviderId, ib.CurrencyId } into bets
                                let totalBetsAmount = bets.Sum(b => b.TotalBetAmount)
                                select new ProviderBetsInfo
                                {
                                    ProviderId = bets.Key.ProviderId ?? 0,
                                    CurrencyId = bets.Key.CurrencyId,
                                    TotalPlayersCount = bets.Sum(b => b.TotalPlayersCount) ?? 0,
                                    TotalBetsAmount = totalBetsAmount ?? 0,
                                    TotalWinsAmount = bets.Sum(b => b.TotalWinAmount) ?? 0,
                                    TotalBetsCount = bets.Sum(b => b.TotalCount) ?? 0,
                                    TotalGGR = bets.Sum(b => b.TotalBetAmount - b.TotalWinAmount) ?? 0,
                                    TotalBetsAmountFromInternet = totalBetsAmount ?? 0
                                });

            var allBets = betShopBets;
            allBets.AddRange(internetBets);
            var result = allBets.GroupBy(b => b.ProviderId).Select(b => new ProviderBetsInfo
            {
                ProviderId = b.Key,
                TotalPlayersCount = b.Sum(x => x.TotalPlayersCount),
                TotalBetsAmount = b.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalBetsAmount)),
                TotalWinsAmount = b.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalWinsAmount)),
                TotalBetsCount = b.Sum(x => x.TotalBetsCount),
                TotalGGR = b.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalGGR)),
                TotalBetsAmountFromBetShop = b.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalBetsAmountFromBetShop)),
                TotalBetsAmountFromInternet = b.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalBetsAmountFromInternet))
            }).ToList();

            var q = Db.Bets.Where(x => x.BetDate >= fDate && x.BetDate < tDate && x.ClientId != null);
            if (partnerId != 0)
                q = q.Where(x => x.Client.PartnerId == partnerId);
            return new ProvidersBetsInfo
            {
                TotalPlayersCount = q.Select(b => b.ClientId).Distinct().Count(),
                TotalBetsAmount = result.Sum(x => x.TotalBetsAmount),
                TotalWinsAmount = result.Sum(x => x.TotalWinsAmount),
                TotalGGR = result.Sum(x => x.TotalGGR),
                Bets = result
            };
        }

        public PlayersInfo GetPlayersInfoForDashboard(FilterDashboard filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewDashboard
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClientByCategory,
                ObjectTypeId = (int)ObjectTypes.ClientCategory
            });

            var partnerId = filter.PartnerId;
            if (!partnerAccess.HaveAccessForAllObjects)
            {
                if (partnerAccess.AccessibleObjects.Any())
                    partnerId = Convert.ToInt32(partnerAccess.AccessibleObjects.First());
                else
                    partnerId = -1;
            }
            var fDate = (long)filter.FromDate.Year * 1000000 + (long)filter.FromDate.Month * 10000 + (long)filter.FromDate.Day * 100 + (long)filter.FromDate.Hour;
            var tDate = (long)filter.ToDate.Year * 1000000 + (long)filter.ToDate.Month * 10000 + (long)filter.ToDate.Day * 100 + (long)filter.ToDate.Hour;

            var signUpsCountQuery = Db.Clients.Where(c => c.CreationTime >= filter.FromDate && c.CreationTime < filter.ToDate);
            var visitorsCountQuery = Db.ClientSessions.Where(s => s.StartTime >= filter.FromDate && s.StartTime < filter.ToDate && s.ProductId == Constants.PlatformProductId);
            var returnsCountQuery = Db.ClientSessions.Where(s => s.StartTime >= filter.FromDate && s.StartTime < filter.ToDate &&
                s.ProductId == Constants.PlatformProductId && s.Client.CreationTime < filter.FromDate);

            if (partnerId != null)
            {
                signUpsCountQuery = signUpsCountQuery.Where(c => c.PartnerId == partnerId.Value);
                visitorsCountQuery = visitorsCountQuery.Where(s => s.Client.PartnerId == partnerId.Value);
                returnsCountQuery = returnsCountQuery.Where(s => s.Client.PartnerId == partnerId.Value);
            }
            var signUpsCount = signUpsCountQuery.Count();
            var visitorsCount = visitorsCountQuery.Select(s => s.ClientId).Distinct().Count();
            var returnsCount = returnsCountQuery.Select(s => s.ClientId).Distinct().Count();


            var internetBets = Db.fn_ClientInfoForDashboard(fDate, tDate, partnerId ?? 0).Select(x => new
            {
                x.CurrencyId,
                x.MaxBet,
                x.MaxWin,
                x.TotalBetAmount,
                x.TotalBetCount,
                x.CashoutAmount
            }).ToList();

            var maximumWin = internetBets.Any() ? internetBets.Max(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.MaxWin ?? 0)) : 0;

            return new PlayersInfo
            {
                AverageBet = internetBets.Any() ? internetBets.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalBetAmount ?? 0)) / internetBets.Sum(x => x.TotalBetCount ?? 0) : 0,
                MaxBet = internetBets.Any() ? internetBets.Max(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.MaxBet ?? 0)) : 0,
                MaxWin = maximumWin,
                MaxWinBet = 0,
                SignUpsCount = signUpsCount,
                VisitorsCount = visitorsCount,
                ReturnsCount = returnsCount,
                TotalPlayersCount = 0,
                TotalCashoutAmount = internetBets.Any() ? internetBets.Max(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.CashoutAmount ?? 0)) : 0,
            };
        }

        public object GetMembersInfoForDashboard(FilterDashboard filter)
        {
            filter.AgentId = Identity.Id;
            var user = CacheManager.GetUserById(Identity.Id);
            if (user.Type == (int)UserTypes.AgentEmployee)
            {
                CheckPermission(Constants.Permissions.ViewDashboard);
                filter.AgentId = user.ParentId;
            }

            var fDate = (long)filter.FromDate.Year * 1000000 + (long)filter.FromDate.Month * 10000 + (long)filter.FromDate.Day * 100 + (long)filter.FromDate.Hour;
            var tDate = (long)filter.ToDate.Year * 1000000 + (long)filter.ToDate.Month * 10000 + (long)filter.ToDate.Day * 100 + (long)filter.ToDate.Hour;

            var signUpsCount = Db.Clients.Count(c => c.CreationTime >= filter.FromDate && c.CreationTime < filter.ToDate && c.User.Path.Contains("/" + filter.AgentId + "/"));

            return new
            {
                ClientsCount = signUpsCount
            };
        }


        public RealTimeInfo GetOnlineClients(FilterRealTime input)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var realTimeAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewRealTime
            });

            var clientCategoryAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClientByCategory,
                ObjectTypeId = (int)ObjectTypes.ClientCategory
            });

            input.CheckPermissionResuts = new List<CheckPermissionOutput<BllOnlineClient>>
                {
                    new CheckPermissionOutput<BllOnlineClient>
                    {
                        AccessibleObjects = realTimeAccess.AccessibleObjects,
                        HaveAccessForAllObjects = realTimeAccess.HaveAccessForAllObjects,
                        Filter = x => clientCategoryAccess.AccessibleObjects.AsEnumerable().Contains(x.Id.Value)
                    },
                    new CheckPermissionOutput<BllOnlineClient>
                    {
                        AccessibleObjects = clientCategoryAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientCategoryAccess.HaveAccessForAllObjects,
                        Filter = x => clientCategoryAccess.AccessibleObjects.AsEnumerable().Contains(x.CategoryId.Value)
                    },
                     new CheckPermissionOutput<BllOnlineClient>
                    {
                        AccessibleObjects = partnerAccess.AccessibleObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter =  x => input.PartnerId.HasValue && partnerAccess.AccessibleObjects.AsEnumerable().Contains(input.PartnerId.Value)
                    }
                };

            Func<IQueryable<BllOnlineClient>, IOrderedQueryable<BllOnlineClient>> orderBy;

            if (input.OrderBy.HasValue)
            {
                if (input.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<BllOnlineClient>(input.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<BllOnlineClient>(input.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = onlineClients => onlineClients.OrderBy(x => x.SessionTime);
            }

            var response = new RealTimeInfo
            {
                OnlineClients = input.FilterObjects(CacheManager.OnlineClients(CurrencyId).AsQueryable(), orderBy).ToList(),
                Count = input.SelectedObjectsCount(CacheManager.OnlineClients(CurrencyId).AsQueryable())
            };

            if (input.PartnerId != null)
            {
                try
                {
                    GetPermissionsToObject(new CheckPermissionInput
                    {
                        Permission = Constants.Permissions.ViewTotalsOnlineClients
                    });

                    var realTimeInfo = CacheManager.RealTimeInfo(Identity.CurrencyId).FirstOrDefault(c => input.PartnerId == c.PartnerId);
                    if (realTimeInfo != null)
                    {
                        response.TotalLoginsCount = realTimeInfo.LoginCount.Value;
                        response.TotalBetsCount = realTimeInfo.BetsCount.Value;
                        response.TotalBetsAmount = realTimeInfo.BetsAmount.Value;
                        response.TotalPlayersCount = realTimeInfo.PlayersCount.Value;
                        response.ApprovedDepositsCount = realTimeInfo.ApprovedDepositsCount.Value;
                        response.ApprovedDepositsAmount = realTimeInfo.ApprovedDepositsAmount.Value;
                        response.ApprovedWithdrawalsCount = realTimeInfo.ApprovedWithdrawalsCount.Value;
                        response.ApprovedWithdrawalsAmount = realTimeInfo.ApprovedWithdrawalsAmount.Value;
                        response.WonBetsCount = realTimeInfo.WonBetsCount.Value;
                        response.WonBetsAmount = realTimeInfo.WonBetsAmount.Value;
                        response.LostBetsCount = realTimeInfo.LostBetsCount.Value;
                        response.LostBetsAmount = realTimeInfo.LostBetsAmount.Value;
                    }
                }
                catch (Exception)
                {
                    //ignored
                }
            }

            return response;
        }
        public ClientReport GetClientsInfoList(FilterfnClientDashboard filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });
            var checkViewAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPlayerDashboard,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnClientReport>>
            {
                 new CheckPermissionOutput<fnClientReport>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnClientReport>
                {
                    AccessibleObjects = checkViewAccess.AccessibleObjects,
                    HaveAccessForAllObjects = checkViewAccess.HaveAccessForAllObjects,
                    Filter = x => checkViewAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId)
                },
                new CheckPermissionOutput<fnClientReport>
                {
                    AccessibleObjects = affiliateReferralAccess.AccessibleObjects,
                    HaveAccessForAllObjects = affiliateReferralAccess.HaveAccessForAllObjects,
                    Filter = x =>x.AffiliatePlatformId.HasValue && affiliateReferralAccess.AccessibleObjects.AsEnumerable().Contains(x.AffiliatePlatformId.Value)
                }
            };

            Func<IQueryable<fnClientReport>, IOrderedQueryable<fnClientReport>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnClientReport>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnClientReport>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = clientList => clientList.OrderByDescending(x => x.ClientId);
            }
            var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * 1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            var dbClientReport = filter.FilterObjects(Db.fn_ClientReport(fDate, tDate)).ToList();
            var entities = new List<ApiClientInfo>();
            dbClientReport.ForEach(x =>
            {
                var balance = CacheManager.GetClientCurrentBalance(x.ClientId);
                entities.Add(new ApiClientInfo
                {
                    ClientId = x.ClientId,
                    UserName = x.UserName,
                    PartnerId = x.PartnerId,
                    CurrencyId = x.CurrencyId,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    AffiliatePlatformId = x.AffiliatePlatformId,
                    AffiliateId = x.AffiliateId,
                    AffiliateReferralId = x.AffiliateReferralId,
                    TotalDepositAmount = x.TotalDepositAmount ?? 0,
                    DepositsCount = x.DepositsCount ?? 0,
                    TotalWithdrawalAmount = x.TotalWithdrawalAmount ?? 0,
                    WithdrawalsCount = x.WithdrawalsCount ?? 0,
                    TotalBetAmount = x.TotalBetAmount ?? 0,
                    BetsCount = x.BetsCount ?? 0,
                    TotalWinAmount = x.TotalWinAmount ?? 0,
                    WinsCount = x.WinsCount ?? 0,
                    TotalCreditCorrection = x.TotalCreditCorrection ?? 0,
                    CreditCorrectionsCount = x.CreditCorrectionsCount ?? 0,
                    TotalDebitCorrection = x.TotalDebitCorrection ?? 0,
                    DebitCorrectionsCount = x.DebitCorrectionsCount ?? 0,
                    GGR = x.GGR ?? 0,
                    NGR = (x.TotalDepositAmount ?? 0) + (x.TotalDebitCorrection?? 0) -
                          (x.TotalWithdrawalAmount ?? 0) - (x.TotalCreditCorrection ?? 0) - balance.AvailableBalance

                });
            });
            var clients = new PagedModel<ApiClientInfo>
            {
                Entities = entities,
                Count = filter.SelectedObjectsCount(Db.fn_ClientReport(fDate, tDate)),
            };
            return new ClientReport
            {
                Clients = clients,
                Totals = new ClientReportTotal
                {
                    TotalWithdrawalsAmount = entities.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalWithdrawalAmount)),
                    TotalDepositsAmount=entities.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalDepositAmount)),
                    TotalBetsAmount=entities.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalBetAmount)),
                    TotalWinsAmount=entities.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalWinAmount)),
                    TotalGGRs=entities.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.GGR)),
                    TotalNGRs=entities.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.NGR)),
                    TotalDebitCorrections=entities.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalDebitCorrection)),
                    TotalCreditCorrections=entities.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalCreditCorrection)),
                }
            };
        }

        #endregion

        #region Reporting

        #region BetShop Reports

        public BetShopBets GetBetshopBetsPagedModel(FilterBetShopBet filter, string currencyId, string permission)
        {
            var viewBetShopBetAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = permission,
                ObjectTypeId = (int)ObjectTypes.fnBetShopBet
            });

            var viewBetShopAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShop,
                ObjectTypeId = (int)ObjectTypes.BetShop
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnBetShopBet>>
            {
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleObjects = viewBetShopBetAccess.AccessibleObjects,
                    HaveAccessForAllObjects = viewBetShopBetAccess.HaveAccessForAllObjects,
                    Filter = x => viewBetShopBetAccess.AccessibleObjects.AsEnumerable().Contains(x.ObjectId)
                },
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleObjects = viewBetShopAccess.AccessibleObjects,
                    HaveAccessForAllObjects = viewBetShopAccess.HaveAccessForAllObjects,
                    Filter = x => viewBetShopAccess.AccessibleObjects.AsEnumerable().Contains(x.BetShopId)
                },
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                }
            };

            Func<IQueryable<fnBetShopBet>, IOrderedQueryable<fnBetShopBet>> orderBy = betShopBet => betShopBet.OrderByDescending(x => x.BetDocumentId);

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnBetShopBet>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnBetShopBet>(filter.FieldNameToOrderBy, false);
                }
            }
            var groupedBets = (from b in filter.FilterObjectsTotals(Db.fn_BetShopBet())
                               group b by b.CurrencyId
                                   into y
                               select new
                               {
                                   CurrencyId = y.Key,
                                   Count = y.Count(),
                                   BetAmount = y.Sum(x => x.BetAmount),
                                   WinAmount = y.Sum(x => x.WinAmount)
                               }).ToList();
            var totalCount = groupedBets.Sum(x => x.Count);
            filter.TakeCount = Math.Min(Math.Max(0, totalCount - filter.SkipCount * filter.TakeCount), filter.TakeCount);
            var query = filter.FilterObjects(Db.fn_BetShopBet(), orderBy);
            var entries = query.ToList();

            var toCurrency = string.IsNullOrEmpty(currencyId) ? CurrencyId : currencyId;
            foreach (var e in entries)
            {
                e.BetAmount = Math.Round(ConvertCurrency(e.CurrencyId, toCurrency, e.BetAmount), 2);
                e.WinAmount = Math.Round(ConvertCurrency(e.CurrencyId, toCurrency, e.WinAmount), 2);
                e.PossibleWin = Math.Round(ConvertCurrency(e.CurrencyId, toCurrency, e.PossibleWin), 2);
            }

            var result = new BetShopBets
            {
                Entities = entries,
                Count = totalCount,
                TotalBetAmount = groupedBets.Sum(e => Math.Round(ConvertCurrency(e.CurrencyId, toCurrency, e.BetAmount), 2)),
                TotalWinAmount = groupedBets.Sum(e => Math.Round(ConvertCurrency(e.CurrencyId, toCurrency, e.WinAmount), 2)),
                TotalProfit = groupedBets.Sum(e => Math.Round(ConvertCurrency(e.CurrencyId, toCurrency, e.BetAmount - e.WinAmount), 2))
            };

            return result;
        }

        public List<fnBetShopBet> GetBetshopBetsForCashier(DateTime fromDate, DateTime toDate, int cashDeskId,
            int cashierId, int? productId, int? state)
        {
            var fDate = fromDate.Year * 1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
            var tDate = toDate.Year * 1000000 + toDate.Month * 10000 + toDate.Day * 100 + toDate.Hour;
            var query =
                Db.fn_BetShopBet()
                    .Where(
                        x =>
                            x.Date >= fDate && x.Date < tDate && x.CashDeskId == cashDeskId &&
                            x.CashierId == cashierId);
            if (state != null)
                query = query.Where(x => x.State == state.Value);
            if (productId != null)
                query = query.Where(x => x.ProductId == productId.Value);
            return query.OrderByDescending(x => x.BetDocumentId)
                .Take(2000)
                .ToList();
        }

        public List<fnReportByBetShopOperation> GetReportByBetShopPayments(FilterReportByBetShopPayment filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var betShopAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShop,
                ObjectTypeId = (int)ObjectTypes.BetShop
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnReportByBetShopOperation>>
            {
                new CheckPermissionOutput<fnReportByBetShopOperation>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                 new CheckPermissionOutput<fnReportByBetShopOperation>
                {
                    AccessibleObjects = betShopAccess.AccessibleObjects,
                    HaveAccessForAllObjects = betShopAccess.HaveAccessForAllObjects,
                    Filter = x => betShopAccess.AccessibleObjects.AsEnumerable().Contains(x.Id)
                }
            };
            var result = filter.FilterObjects(Db.fn_ReportByBetShopOperation(filter.FromDate, filter.ToDate), d => d.OrderByDescending(x => x.Id)).ToList();
            return result;
        }

        public fnBetShopBet GetBetByBarcode(int cashDeskId, long barcode)
        {
            var result = new fnBetShopBet();
            var docId = barcode - 1000000000000;
            docId /= 10;
            var documents = Db.Documents.Include(x => x.Product).Include(x => x.CashDesk).Where(x => x.Id == docId || x.ParentId == docId).ToList();
            var bet = documents.FirstOrDefault(x => x.OperationTypeId == (int)OperationTypes.Bet);
            if (bet == null || bet.CashDeskId != cashDeskId)
                return null;
            var win = documents.FirstOrDefault(x => x.OperationTypeId == (int)OperationTypes.Win);
            var winAmount = documents.Where(d => d.OperationTypeId == (int)OperationTypes.Win ||
                d.OperationTypeId == (int)OperationTypes.CashOut ||
                d.OperationTypeId == (int)OperationTypes.Jackpot ||
                d.OperationTypeId == (int)OperationTypes.MultipleBonus).Sum(x => x.Amount);
            var lastWin = documents.Where(d => d.OperationTypeId == (int)OperationTypes.CashOut ||
                d.OperationTypeId == (int)OperationTypes.Jackpot ||
                d.OperationTypeId == (int)OperationTypes.MultipleBonus).OrderByDescending(x => x.Date).FirstOrDefault();
            if (lastWin == null)
                lastWin = documents.Where(d => d.OperationTypeId == (int)OperationTypes.Win).OrderByDescending(x => x.Date).FirstOrDefault();
            result.BetDocumentId = bet.Id;
            result.BetExternalTransactionId = null;
            result.BetShopCurrencyId = bet.CurrencyId;
            result.BetInfo = null;
            result.CashDeskId = bet.CashDeskId;
            result.BetTypeId = bet.TypeId ?? 1;
            result.PossibleWin = bet.PossibleWin ?? 0;
            result.ProductId = bet.ProductId ?? 0;
            result.GameProviderId = bet.Product.GameProviderId;
            result.TicketInfo = null;
            result.CashierId = bet.UserId;
            result.BetShopGroupId = 0;
            result.BetDate = bet.CreationTime;
            result.PayDate = null;
            result.BetShopId = bet.CashDesk.BetShopId;
            result.BetShopName = string.Empty;
            result.BetAmount = bet.Amount;
            result.CurrencyId = bet.CurrencyId;
            result.TicketNumber = bet.TicketNumber;
            result.PartnerId = 1;
            result.ProductName = bet.Product.NickName;
            result.RoundId = null;
            result.ProviderName = string.Empty;
            result.HasNote = false;
            result.State = lastWin == null ? bet.State : lastWin.State;
            result.WinDate = (win == null ? (DateTime?)null : win.CreationTime);
            result.WinAmount = winAmount;
            result.Date = bet.Date ?? 0;
            return result;
        }

        public BetShops GetReportByBetShops(FilterBetShopBet filter)
        {
            CheckPermission(Constants.Permissions.ViewBetShopsReport);

            var viewBetShopAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShop,
                ObjectTypeId = (int)ObjectTypes.BetShop
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnBetShopBet>>
            {
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleObjects = viewBetShopAccess.AccessibleObjects,
                    HaveAccessForAllObjects = viewBetShopAccess.HaveAccessForAllObjects,
                    Filter = x => viewBetShopAccess.AccessibleObjects.AsEnumerable().Contains(x.BetShopId)
                },
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                }
            };

            var entities =
                filter.FilterObjects(Db.fn_BetShopBet())
                    .GroupBy(x => new { x.BetShopId, x.BetShopGroupId, x.BetShopName, x.BetShopCurrencyId })
                    .Select(
                        x =>
                            new BetShopReport
                            {
                                BetAmount = x.Sum(y => y.BetAmount),
                                WinAmount = x.Sum(y => y.WinAmount),
                                BetShopId = x.Key.BetShopId,
                                BetShopGroupId = x.Key.BetShopGroupId,
                                BetShopName = x.Key.BetShopName,
                                CurrencyId = x.Key.BetShopCurrencyId
                            }).ToList();
            foreach (var e in entities)
            {
                e.BetAmount = ConvertCurrency(e.CurrencyId, CurrencyId, e.BetAmount);
                e.WinAmount = ConvertCurrency(e.CurrencyId, CurrencyId, e.WinAmount);
            }
            var result = new BetShops
            {
                Entities = entities,
                TotalBetAmount = entities.Sum(x => x.BetAmount),
                TotalWinAmount = entities.Sum(x => x.WinAmount),
                TotalProfit = entities.Sum(x => x.BetAmount - x.WinAmount)
            };
            return result;
        }

        public BetShopGames GetReportByBetShopGames(FilterBetShopBet filter)
        {
            CheckPermission(Constants.Permissions.ViewBetShopsReport);

            var viewBetShopAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShop,
                ObjectTypeId = (int)ObjectTypes.BetShop
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnBetShopBet>>
            {
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleObjects = viewBetShopAccess.AccessibleObjects,
                    HaveAccessForAllObjects = viewBetShopAccess.HaveAccessForAllObjects,
                    Filter = x => viewBetShopAccess.AccessibleObjects.AsEnumerable().Contains(x.BetShopId)
                },
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                }
            };

            var entities =
                filter.FilterObjects(Db.fn_BetShopBet())
                    .GroupBy(x => new { x.ProductId, x.ProductName, x.BetShopCurrencyId })
                    .Select(
                        x =>
                            new BetShopGame
                            {
                                BetAmount = x.Sum(y => y.BetAmount),
                                WinAmount = x.Sum(y => y.WinAmount),
                                Count = x.Count(),
                                GameId = x.Key.ProductId,
                                GameName = x.Key.ProductName,
                                CurrencyId = x.Key.BetShopCurrencyId
                            }).ToList();
            foreach (var e in entities)
            {
                e.BetAmount = ConvertCurrency(e.CurrencyId, CurrencyId, e.BetAmount);
                e.WinAmount = ConvertCurrency(e.CurrencyId, CurrencyId, e.WinAmount);
            }
            var result = new BetShopGames
            {
                TotalBetAmount = entities.Sum(x => x.BetAmount),
                TotalWinAmount = entities.Sum(x => x.WinAmount),
                TotalBetCount = entities.Sum(x => x.Count),
                Entities = entities
            };
            return result;
        }

        #endregion

        #region InternetReports

        public InternetBetsReport GetInternetBetsPagedModel(FilterInternetBet filter, string currencyId, bool checkPermission)
        {
            if (filter.AgentId.HasValue)
            {
                var user = CacheManager.GetUserById(filter.AgentId.Value);
                if (user == null)
                    throw CreateException(Identity.LanguageId, Constants.Errors.UserNotFound);
                if (user.Type == (int)UserTypes.AgentEmployee)
                {
                    CheckPermission(Constants.Permissions.ViewInternetBets);
                    filter.AgentId = user.ParentId;
                }
            }

            if (checkPermission)
            {
                var internetBetAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewInternetBets,
                    ObjectTypeId = (int)ObjectTypes.fnInternetBet
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
                var clientAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });
                var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliateReferral,
                    ObjectTypeId = (int)ObjectTypes.AffiliateReferral
                });

                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnInternetBet>>
                {
                    new CheckPermissionOutput<fnInternetBet>
                    {
                        AccessibleObjects = internetBetAccess.AccessibleObjects,
                        HaveAccessForAllObjects = internetBetAccess.HaveAccessForAllObjects,
                        Filter = x => internetBetAccess.AccessibleObjects.AsEnumerable().Contains(x.ObjectId)
                    },
                    new CheckPermissionOutput<fnInternetBet>
                    {
                        AccessibleObjects = partnerAccess.AccessibleObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                    },
                    new CheckPermissionOutput<fnInternetBet>
                    {
                        AccessibleObjects = clientCategoryAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientCategoryAccess.HaveAccessForAllObjects,
                        Filter = x => clientCategoryAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientCategoryId)
                    },
                    new CheckPermissionOutput<fnInternetBet>
                    {
                        AccessibleObjects = clientAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                        Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId)
                    },
                    new CheckPermissionOutput<fnInternetBet>
                    {
                        AccessibleObjects = affiliateReferralAccess.AccessibleObjects,
                        HaveAccessForAllObjects = affiliateReferralAccess.HaveAccessForAllObjects,
                        Filter = x =>x.AffiliateReferralId.HasValue && affiliateReferralAccess.AccessibleObjects.AsEnumerable().Contains(x.AffiliateReferralId.Value)
                    }
                };
            }
            else
                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnInternetBet>>();

            Func<IQueryable<fnInternetBet>, IOrderedQueryable<fnInternetBet>> orderBy;
            orderBy = clients => clients.OrderByDescending(x => x.BetDocumentId);
            if (filter.OrderBy.HasValue)
            {
                if (!string.IsNullOrEmpty(filter.FieldNameToOrderBy))
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnInternetBet>(filter.FieldNameToOrderBy, filter.OrderBy.Value);
                else
                    orderBy = clients => clients.OrderByDescending(x => x.BetDocumentId);
            }

            var totalBets = (from ib in filter.FilterObjects(Db.fn_InternetBet())
                             group ib by ib.CurrencyId into bets
                             select new
                             {
                                 CurrencyId = bets.Key,
                                 TotalBetsAmount = bets.Sum(b => b.BetAmount),
                                 TotalBetsCount = bets.Count(),
                                 TotalWinsAmount = bets.Sum(b => b.WinAmount),
                                 TotalProfit = bets.Sum(b => b.BetAmount - b.WinAmount),
                                 TotalPossibleWinsAmount = bets.Sum(b => b.PossibleWin)
                             }).ToList();

            var entries = filter.FilterObjects(Db.fn_InternetBet(), orderBy).ToList();
            var convertCurrency = !string.IsNullOrEmpty(currencyId) ? currencyId : CurrencyId;

            foreach (var entry in entries)
            {
                entry.BetAmount = Math.Round(ConvertCurrency(entry.CurrencyId, convertCurrency, entry.BetAmount), 2);
                entry.WinAmount = Math.Round(ConvertCurrency(entry.CurrencyId, convertCurrency, entry.WinAmount), 2);
                entry.PossibleWin = Math.Round(ConvertCurrency(entry.CurrencyId, convertCurrency, entry.PossibleWin ?? 0), 2);

            }

            var response = new InternetBetsReport
            {
                Entities = entries,
                Count = totalBets.Sum(x => x.TotalBetsCount),
                TotalBetAmount = totalBets.Sum(x => ConvertCurrency(x.CurrencyId, convertCurrency, x.TotalBetsAmount)),
                TotalWinAmount = totalBets.Sum(x => ConvertCurrency(x.CurrencyId, convertCurrency, x.TotalWinsAmount)),
                TotalPossibleWinAmount = totalBets.Sum(x => ConvertCurrency(x.CurrencyId, convertCurrency, x.TotalPossibleWinsAmount ?? 0)),
                TotalCurrencyCount = totalBets.Count,
                TotalPlayersCount = 0,
                TotalProvidersCount = 0,
                TotalProductsCount = 0
            };
            response.TotalGGR = response.TotalBetAmount - response.TotalWinAmount;
            return response;
        }

        public InternetBetsReport GetBetsForWebSite(FilterWebSiteBet filter)
        {
            var client = CacheManager.GetClientById(filter.ClientId);
            if (client == null)
                throw CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            var winDisplayType = (int)WinDisplayTypes.Win;
            var partnerWinDisplayType = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.WinDisplayType);
            if (!string.IsNullOrEmpty(partnerWinDisplayType) && int.TryParse(partnerWinDisplayType, out int configValue) && Enum.IsDefined(typeof(WinDisplayTypes), configValue))
                winDisplayType = Convert.ToInt32(partnerWinDisplayType);
            var totalBets = (from ib in filter.FilterObjects(Db.fn_InternetBet())
                             group ib by ib.ClientId into bets
                             select new
                             {
                                 TotalBetsAmount = bets.Sum(b => b.BetAmount),
                                 TotalBetsCount = bets.Count(),
                                 TotalWinsAmount = bets.Sum(b => b.WinAmount),
                                 TotalProfit = bets.Sum(b => b.BetAmount - b.WinAmount),
                                 TotalPossibleWinsAmount = bets.Sum(b => b.PossibleWin)
                             }).FirstOrDefault();

            var entries = filter.FilterObjects(Db.fn_InternetBet(), clients => clients.OrderByDescending(x => x.BetDocumentId)).ToList();
            foreach (var entry in entries)
            {
                entry.BetAmount = Math.Round(entry.BetAmount, 2);
                entry.WinAmount = winDisplayType == (int)WinDisplayTypes.Win ? Math.Round(entry.WinAmount, 2) : Math.Round(entry.WinAmount - entry.BetAmount, 2);
                entry.PossibleWin = winDisplayType == (int)WinDisplayTypes.Win || !entry.PossibleWin.HasValue || entry.PossibleWin == 0 ?
                                    Math.Round(entry.PossibleWin ?? 0, 2) : Math.Round(entry.PossibleWin ?? 0 - entry.BetAmount, 2);
            }

            var response = new InternetBetsReport
            {
                Entities = entries,
                Count = totalBets == null ? 0 : totalBets.TotalBetsCount,
                TotalBetAmount = totalBets == null ? 0 : totalBets.TotalBetsAmount,
                TotalWinAmount = totalBets == null ? 0 : (winDisplayType == (int)WinDisplayTypes.Win ? totalBets.TotalWinsAmount : (totalBets.TotalWinsAmount - totalBets.TotalBetsAmount)),
                TotalPossibleWinAmount = totalBets == null ? 0 : 
                    (winDisplayType == (int)WinDisplayTypes.Win ? totalBets.TotalPossibleWinsAmount : (totalBets.TotalPossibleWinsAmount - totalBets.TotalBetsAmount))
            };
            response.TotalGGR = response.TotalBetAmount - response.TotalWinAmount;
            return response;
        }

        public InternetBetsByClientReport GetInternetBetsByClientPagedModel(FilterInternetBet filter)
        {
            /*var internetBetAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewInternetBets,
                ObjectTypeId = (int)ObjectTypes.fnInternetBet
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
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });
            var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnInternetBet>>
            {
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = internetBetAccess.AccessibleObjects,
                    HaveAccessForAllObjects = internetBetAccess.HaveAccessForAllObjects,
                    Filter = x => internetBetAccess.AccessibleObjects.AsEnumerable().Contains(x.ObjectId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = clientCategoryAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientCategoryAccess.HaveAccessForAllObjects,
                    Filter = x => clientCategoryAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientCategoryId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = affiliateReferralAccess.AccessibleObjects,
                    HaveAccessForAllObjects = affiliateReferralAccess.HaveAccessForAllObjects,
                    Filter = x => x.AffiliateReferralId.HasValue && affiliateReferralAccess.AccessibleObjects.AsEnumerable().Contains(x.AffiliateReferralId.Value)
                }
            };

            Func<IQueryable<InternetBetByClient>, IOrderedQueryable<InternetBetByClient>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<InternetBetByClient>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<InternetBetByClient>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = internetBet => internetBet.OrderByDescending(x => x.ClientId);
            }

            var result = (from ib in
                              (from b in filter.FilterObjects(Db.fn_InternetBet())
                               group b by new { b.ClientId, b.ClientUserName, b.CurrencyId } into y
                               select new
                               {
                                   ClientId = y.Key.ClientId,
                                   UserName = y.Key.ClientUserName,
                                   TotalBetsCount = y.Count(),
                                   TotalBetsAmount = y.Sum(x => x.BetAmount),
                                   TotalWinsAmount = y.Sum(x => x.WinAmount),
                                   Currency = y.Key.CurrencyId,
                                   // State = y.Single(x => x.State),
                                   MaxBetAmount = y.Max(x => x.BetAmount)
                               })
                          from pr in Db.fn_InternetBetByClient()
                              .Where(m => m.ClientId == ib.ClientId).DefaultIfEmpty()
                          select new InternetBetByClient
                          {
                              ClientId = ib.ClientId,
                              UserName = ib.UserName,
                              // State = ib.State,
                              TotalBetsCount = ib.TotalBetsCount,
                              TotalBetsAmount = ib.TotalBetsAmount,
                              TotalWinsAmount = ib.TotalWinsAmount,
                              Currency = ib.Currency,
                              //GGR = ib.TotalBetsAmount - ib.TotalWinsAmount,
                              MaxBetAmount = ib.MaxBetAmount,
                              TotalDepositsCount = pr.TotalDepositsCount == null ? 0 : pr.TotalDepositsCount.Value,
                              TotalDepositsAmount = pr.TotalDepositsAmount == null ? 0 : pr.TotalDepositsAmount.Value,
                              TotalWithdrawalsCount = pr.TotalWithdrawalsCount == null ? 0 : pr.TotalWithdrawalsCount.Value,
                              TotalWithdrawalsAmount = pr.TotalWithdrawalsAmount == null ? 0 : pr.TotalWithdrawalsAmount.Value,
                              Balance = pr.Balance == null ? 0 : pr.Balance.Value
                          });

            var totalBets = (from ib in filter.FilterResultObjects(result)
                             group ib by ib.Currency into bets
                             select new
                             {
                                 CurrencyId = bets.Key,
                                 TotalBetCount = bets.Sum(b => b.TotalBetsCount),
                                 TotalBetAmount = bets.Sum(b => b.TotalBetsAmount),
                                 TotalWinAmount = bets.Sum(b => b.TotalWinsAmount),
                                 TotalBalance = bets.Sum(b => b.Balance),
                                 TotalDepositCount = bets.Sum(b => b.TotalDepositsCount),
                                 TotalDepositAmount = bets.Sum(b => b.TotalDepositsAmount),
                                 TotalWithdrawCount = bets.Sum(b => b.TotalWithdrawalsCount),
                                 TotalWithdrawAmount = bets.Sum(b => b.TotalWithdrawalsAmount),
                                 TotalPlayerCount = bets.Count()
                             }).ToList();


            var entries = filter.FilterResultObjects(result, orderBy).ToList();
            foreach (var e in entries)
            {
                e.TotalBetsAmount = ConvertCurrency(e.Currency, CurrencyId, e.TotalBetsAmount);
                e.TotalWinsAmount = ConvertCurrency(e.Currency, CurrencyId, e.TotalWinsAmount);
                e.MaxBetAmount = ConvertCurrency(e.Currency, CurrencyId, e.MaxBetAmount);
                e.TotalDepositsAmount = ConvertCurrency(e.Currency, CurrencyId, e.TotalDepositsAmount);
                e.TotalWithdrawalsAmount = ConvertCurrency(e.Currency, CurrencyId, e.TotalWithdrawalsAmount);
                e.Balance = ConvertCurrency(e.Currency, CurrencyId, e.Balance);
                e.GGR = e.TotalBetsAmount - e.TotalWinsAmount;
            }
            var response = new InternetBetsByClientReport
            {
                Entities = entries,
                Count = totalBets.Sum(x => x.TotalPlayerCount),
                TotalBetCount = totalBets.Sum(x => x.TotalBetCount),
                TotalBalance = totalBets.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalBalance)),
                TotalDepositCount = totalBets.Sum(x => x.TotalDepositCount),
                TotalWithdrawCount = totalBets.Sum(x => x.TotalWithdrawCount),
                TotalCurrencyCount = totalBets.Count,
                TotalBetAmount = totalBets.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalBetAmount)),
                TotalWinAmount = totalBets.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalWinAmount)),
                TotalDepositAmount = totalBets.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalDepositAmount)),
                TotalWithdrawAmount = totalBets.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalWithdrawAmount))
            };
            response.TotalGGR = response.TotalBetAmount - response.TotalWinAmount;*/
            return new InternetBetsByClientReport { Entities = new List<InternetBetByClient>() };
        }

        public ClientInternetGameReport GetClientReportByBetsTemp(int clientId, int providerId, DateTime? fromDate, DateTime? toDate)
        {
            var currentDate = DateTime.UtcNow;
            if (fromDate == null)
                fromDate = currentDate.AddDays(-1);
            if ((currentDate - fromDate.Value).TotalDays > 30)
                fromDate = currentDate.AddDays(-30);

            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

            var result = Db.fn_InternetBet().Where(x => x.ClientId == clientId && x.GameProviderId == providerId);
            var fDate = fromDate.Value.Year * 1000000 + fromDate.Value.Month * 10000 + fromDate.Value.Day * 100 + fromDate.Value.Hour;
            result = result.Where(x => x.Date >= fDate);

            if (toDate != null)
            {
                var tDate = toDate.Value.Year * 1000000 + toDate.Value.Month * 10000 + toDate.Value.Day * 100 + toDate.Value.Hour;
                result = result.Where(x => x.Date <= tDate);
            }
            var clientBets = result.OrderByDescending(x => x.BetDocumentId).Take(10000).ToList();

            var report = new ClientInternetGameReport
            {
                TotalRoundCount = clientBets.Where(x => x.RoundId != null).Distinct().Count(),
                TotalTransactionCount = clientBets.Count(),
                TotalBetCount = clientBets.Count(),
                TotalBetAmount = clientBets.Sum(x => x.BetAmount),
                TotalCanceledBetCount = clientBets.Where(x => x.State == (int)BetDocumentStates.Deleted).Count(),
                TotalCanceledBetAmount = clientBets.Where(x => x.State == (int)BetDocumentStates.Deleted).Sum(x => x.BetAmount),
                TotalWinCount = clientBets.Where(x => x.State == (int)BetDocumentStates.Won || x.State == (int)BetDocumentStates.Lost).Count(),
                TotalWinAmount = clientBets.Sum(x => x.WinAmount)
            };
            report.TotalBetCount -= report.TotalCanceledBetCount;
            report.TotalBetAmount -= report.TotalCanceledBetAmount;
            report.TotalWinAmount -= report.TotalCanceledBetAmount;
            return report;
        }

        public InternetGames GetReportByInternetGames(FilterInternetGame filter)
        {
            var internetBetAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewInternetBets,
                ObjectTypeId = (int)ObjectTypes.fnInternetBet
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnInternetGame>>
            {
                new CheckPermissionOutput<fnInternetGame>
                {
                    AccessibleObjects = internetBetAccess.AccessibleObjects,
                    HaveAccessForAllObjects = internetBetAccess.HaveAccessForAllObjects,
                    Filter = x => internetBetAccess.AccessibleObjects.AsEnumerable().Contains(x.ObjectId)
                },
                new CheckPermissionOutput<fnInternetGame>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                }
            };
            var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Value.Year * 1000000 + filter.ToDate.Value.Month * 10000 + filter.ToDate.Value.Day * 100 + filter.ToDate.Value.Hour;

            /*8/Func<IQueryable<fnInternetGame>, IOrderedQueryable<fnInternetGame>> orderBy;
            orderBy = x => x.OrderByDescending(y => y.ProductId);
            if (filter.OrderBy.HasValue)
            {
                if (!string.IsNullOrEmpty(filter.FieldNameToOrderBy))
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnInternetGame>(filter.FieldNameToOrderBy, filter.OrderBy.Value);
                else
                    orderBy = x => x.OrderByDescending(y => y.ProductId);
            }*/
            var objects = filter.FilterObjects(Db.fn_InternetGame(fDate, tDate)).ToList();

            var entities = new List<InternetGame>();
            foreach (var e in objects)
            {
                decimal? pps = null;
                if (filter.PartnerId != null)
                    pps = CacheManager.GetPartnerProductSettingByProductId(filter.PartnerId.Value, e.ProductId)?.Percent;
                var ba = ConvertCurrency(e.CurrencyId, CurrencyId, e.BetAmount ?? 0);
                var wa = ConvertCurrency(e.CurrencyId, CurrencyId, e.WinAmount ?? 0);
                entities.Add(new InternetGame
                {
                    BetAmount = ba,
                    WinAmount = wa,
                    Count = e.BetCount ?? 0,
                    GameId = e.ProductId,
                    GameName = e.ProductName,
                    CurrencyId = e.CurrencyId,
                    ProviderId = e.GameProviderId,
                    ProviderName = CacheManager.GetGameProviderById(e.GameProviderId.Value).Name,
                    SubproviderId = e.SubproviderId ?? e.GameProviderId,
                    SubproviderName = CacheManager.GetGameProviderById(e.SubproviderId ?? e.GameProviderId.Value).Name,
                    SupplierPercent = pps ?? 0,
                    SupplierFee = (ba - wa) * (pps ?? 0) / 100
                });
            }
            var result = new InternetGames
            {
                TotalBetAmount = entities.Sum(x => x.BetAmount),
                TotalWinAmount = entities.Sum(x => x.WinAmount),
                TotalBetCount = entities.Sum(x => x.Count),
                TotalSupplierFee = entities.Sum(x => x.SupplierFee),
                Entities = entities,
            };
            return result;
        }

        public CorrectionsReport GetReportByCorrections(FilterCorrection filter)
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

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewReportByCorrection,
                ObjectTypeId = (int)ObjectTypes.fnCorrection
            });
            var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnCorrection>>
            {
                new CheckPermissionOutput<fnCorrection>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId.Value)
                },
                new CheckPermissionOutput<fnCorrection>
                {
                    AccessibleObjects = affiliateReferralAccess.AccessibleObjects,
                    HaveAccessForAllObjects = affiliateReferralAccess.HaveAccessForAllObjects,
                    Filter = x => x.AffiliateReferralId.HasValue && affiliateReferralAccess.AccessibleObjects.AsEnumerable().Contains(x.AffiliateReferralId.Value)
                },
                new CheckPermissionOutput<fnCorrection>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId.Value)
                }
            };

            Func<IQueryable<fnCorrection>, IOrderedQueryable<fnCorrection>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnCorrection>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnCorrection>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = documents => documents.OrderByDescending(y => y.Id);
            }

            var totals = (from c in filter.FilterObjects(Db.fn_Correction(true))
                          group c by c.CurrencyId into corrections
                          select new
                          {
                              CurrencyId = corrections.Key,
                              TotalAmount = corrections.Sum(b => b.Amount),
                              TotalCount = corrections.Count()
                          }).ToList();

            var entities = filter.FilterObjects(Db.fn_Correction(true), orderBy).ToList();
            /*foreach (var e in entities)
            {
                e.Amount = ConvertCurrency(e.CurrencyId, CurrencyId, e.Amount);
            }*/
            return new CorrectionsReport
            {
                Entities = entities,
                Count = totals.Sum(x => x.TotalCount),
                TotalAmount = totals.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalAmount))
            };
        }


        public PagedModel<ObjectDataChangeHistory> GetBetShopLimitChangesReport(FilterReportByBetShopLimitChanges filter)
        {
            CheckPermissionToSaveObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShop,
                ObjectTypeId = (int)ObjectTypes.BetShop
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<ObjectDataChangeHistory>>();

            var checkP = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShop
            });

            filter.CheckPermissionResuts.Add(new CheckPermissionOutput<ObjectDataChangeHistory>
            {
                AccessibleObjects = checkP.AccessibleObjects,
                HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                Filter = x => checkP.AccessibleObjects.AsEnumerable().Contains(x.ObjectId)
            });
            Func<IQueryable<ObjectDataChangeHistory>, IOrderedQueryable<ObjectDataChangeHistory>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<ObjectDataChangeHistory>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<ObjectDataChangeHistory>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = betShotLimitChangedHistory => betShotLimitChangedHistory.OrderByDescending(x => x.Id);
            }

            return new PagedModel<ObjectDataChangeHistory>
            {
                Entities = filter.FilterObjects(Db.ObjectDataChangeHistories, orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.ObjectDataChangeHistories)
            };
        }

        #endregion

        #region Business Intelligence Reports

        public List<ReportByProvidersElement> GetReportByProviders(FilterReportByProvider filter)
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

            var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewReportByProvider
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnReportByProvider>>
            {
                new CheckPermissionOutput<fnReportByProvider>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId ?? 0)
                },
                new CheckPermissionOutput<fnReportByProvider>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId.Value)
                },
                new CheckPermissionOutput<fnReportByProvider>
                {
                    AccessibleObjects = affiliateReferralAccess.AccessibleObjects,
                    HaveAccessForAllObjects = affiliateReferralAccess.HaveAccessForAllObjects,
                    Filter = x => x.AffiliateReferralId.HasValue && affiliateReferralAccess.AccessibleObjects.AsEnumerable().Contains(x.AffiliateReferralId.Value)
                }
            };
            var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * 1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            var rep = filter.FilterObjects(Db.fn_ReportByProvider(fDate, tDate)).ToList();
            var result = rep.GroupBy(x => new { x.ProviderName, x.Currency, x.PartnerId })
                .Select(x => new ReportByProvidersElement
                {
                    PartnerId = x.Key.PartnerId ?? 0,
                    ProviderName = x.Key.ProviderName,
                    Currency = x.Key.Currency,
                    TotalBetsCount = x.Sum(y => y.TotalBetsCount) ?? 0,
                    TotalBetsAmount = Math.Round(x.Sum(y => ConvertCurrency(y.Currency, Identity.CurrencyId, y.TotalBetsAmount ?? 0)), 2),
                    TotalWinsAmount = Math.Round(x.Sum(y => ConvertCurrency(y.Currency, Identity.CurrencyId, y.TotalWinsAmount ?? 0)), 2),
                    TotalUncalculatedBetsCount = x.Sum(y => y.TotalUncalculatedBetsCount) ?? 0,
                    TotalUncalculatedBetsAmount = Math.Round(x.Sum(y => ConvertCurrency(y.Currency, Identity.CurrencyId, y.TotalUncalculatedBetsAmount ?? 0)), 2),
                    GGR = Math.Round(x.Sum(y => ConvertCurrency(y.Currency, Identity.CurrencyId, y.GGR ?? 0)), 2),
                    BetsCountPercent = x.Sum(y => y.TotalBetsCount) * 100 ?? 0,
                    BetsAmountPercent = Math.Round(x.Sum(y => ConvertCurrency(y.Currency, Identity.CurrencyId, y.TotalBetsAmount * 100 ?? 0)), 2),
                    GGRPercent = Math.Round(x.Sum(y => ConvertCurrency(y.Currency, Identity.CurrencyId, y.GGR * 100 ?? 0)), 2)
                }).ToList();

            var totalBetsCount = result.Sum(x => x.TotalBetsCount);
            var totalBetsAmount = result.Sum(x => x.TotalBetsAmount);
            var totalGgr = result.Sum(x => x.GGRPercent);
            foreach (var r in result)
            {
                r.BetsCountPercent = Math.Round(totalBetsCount == 0 ? 0 : r.BetsCountPercent / totalBetsCount, 2);
                r.BetsAmountPercent = Math.Round(totalBetsAmount == 0 ? 0 : r.BetsAmountPercent / totalBetsAmount, 2);
                r.GGRPercent = Math.Round(totalGgr == 0 ? 0 : r.GGRPercent / totalGgr, 2);
            }
            return result;
        }

        public List<fnReportByProduct> GetReportByProducts(FilterReportByProduct filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewReportByProduct
            });

            var clientCategoryAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClientByCategory,
                ObjectTypeId = (int)ObjectTypes.ClientCategory
            });

            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });

            var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnReportByProduct>>
            {
                new CheckPermissionOutput<fnReportByProduct>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnReportByProduct>
                {
                    AccessibleObjects = clientCategoryAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientCategoryAccess.HaveAccessForAllObjects,
                    Filter = x => clientCategoryAccess.AccessibleObjects.AsEnumerable().Contains(x.CategoryId)
                },
                new CheckPermissionOutput<fnReportByProduct>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId)
                },
                new CheckPermissionOutput<fnReportByProduct>
                {
                    AccessibleObjects = affiliateReferralAccess.AccessibleObjects,
                    HaveAccessForAllObjects = affiliateReferralAccess.HaveAccessForAllObjects,
                    Filter = x =>x.AffiliateReferralId.HasValue && affiliateReferralAccess.AccessibleObjects.AsEnumerable().Contains(x.AffiliateReferralId.Value)
                }
            };
            var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * 1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            var result = filter.FilterObjects(Db.fn_ReportByProduct(fDate, tDate)).ToList();
            return result;
        }

        #endregion

        #region Business Audit Reports

        public PagedModel<fnActionLog> GetReportByActionLogPaging(FilterReportByActionLog filter, bool checkPermission)
        {
            if (checkPermission)
            {
                CheckPermission(Constants.Permissions.ViewReportByUserLog);

                var checkP = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });
                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnActionLog>>
                {
                    new CheckPermissionOutput<fnActionLog>
                    {
                        AccessibleObjects = checkP.AccessibleObjects,
                        HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                        Filter = x => checkP.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                    }
                };
            }
            Func<IQueryable<fnActionLog>, IOrderedQueryable<fnActionLog>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnActionLog>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnActionLog>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = userLogs => userLogs.OrderByDescending(x => x.Id);
            }
            var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * 1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;

            return new PagedModel<fnActionLog>
            {
                Entities = filter.FilterObjects(Db.fn_ActionLog(Identity.LanguageId, fDate, tDate), orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_ActionLog(Identity.LanguageId, fDate, tDate))
            };
        }

        #endregion

        #region Accounting Reports

        public PartnerPaymentsSummaryReport GetPartnerPaymentsSummaryReport(FilterPartnerPaymentsSummary filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && !partnerAccess.AccessibleObjects.AsEnumerable().Contains(filter.PartnerId))
                filter.PartnerId = -1;

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPaymentSystems
            });

            var paymentsInfo = (from d in Db.fn_PaymentRequest()
                                where d.PartnerId == filter.PartnerId && d.CreationTime >= filter.FromDate && d.CreationTime < filter.ToDate &&
                                (d.Status == (int)PaymentRequestStates.Approved || d.Status == (int)PaymentRequestStates.ApprovedManually) && d.Type == filter.Type
                                group d by new { d.ClientId, d.FirstName, d.LastName, d.CurrencyId, d.PaymentSystemId } into y
                                select new
                                {
                                    ClientId = y.Key.ClientId,
                                    FirstName = y.Key.FirstName,
                                    LastName = y.Key.LastName,
                                    CurrencyId = y.Key.CurrencyId,
                                    PaymentSystemId = y.Key.PaymentSystemId,
                                    Amount = y.Sum(x => x.Amount)
                                }).ToList();
            var paymentMethods = (from pi in paymentsInfo
                                  group pi by new { pi.PaymentSystemId, pi.CurrencyId } into y
                                  select new PaymentMethod
                                  {
                                      PaymentSystemId = y.Key.PaymentSystemId,
                                      CurrencyId = y.Key.CurrencyId
                                  }).ToList();
            return new PartnerPaymentsSummaryReport
            {
                PaymentMethods = paymentMethods,
                PaymentsInfo = (from pi in paymentsInfo
                                group pi by new { pi.ClientId, pi.FirstName, pi.LastName, pi.CurrencyId } into y
                                select new PaymentInfo
                                {
                                    ClientId = y.Key.ClientId,
                                    FirstName = y.Key.FirstName,
                                    LastName = y.Key.LastName,
                                    CurrencyId = y.Key.CurrencyId,
                                    Payments = y.Select(x => new PaymentMethodElement
                                    {
                                        PaymentSystemId = x.PaymentSystemId,
                                        Amount = x.Amount
                                    }).ToList()
                                }).ToList()
            };
        }

        #endregion

        #region Session Reports

        public PagedModel<fnClientSession> GetClientSessions(FilterReportByClientSession filter, bool onlyPlatform)
        {
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });

            var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnClientSession>>
                {
                    new CheckPermissionOutput<fnClientSession>
                    {
                        AccessibleObjects = clientAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                        Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId)
                    },
                    new CheckPermissionOutput<fnClientSession>
                    {
                        AccessibleObjects = affiliateReferralAccess.AccessibleObjects,
                        HaveAccessForAllObjects = affiliateReferralAccess.HaveAccessForAllObjects,
                        Filter = x => x.AffiliateReferralId.HasValue && affiliateReferralAccess.AccessibleObjects.AsEnumerable().Contains(x.AffiliateReferralId.Value)
                    },
                    new CheckPermissionOutput<fnClientSession>
                    {
                        AccessibleObjects = partnerAccess.AccessibleObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x =>partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                    }
                };
            if (onlyPlatform)
                return new PagedModel<fnClientSession>
                {
                    Entities = filter.FilterObjects(Db.fn_ClientSession().Where(x => x.ProductId == Constants.PlatformProductId),
                    sessions => sessions.OrderByDescending(y => y.Id)).ToList(),
                    Count = filter.SelectedObjectsCount(Db.fn_ClientSession().Where(x => x.ProductId == Constants.PlatformProductId))
                };
            return new PagedModel<fnClientSession>
            {
                Entities = filter.FilterObjects(Db.fn_ClientSession().Where(x => x.ProductId != Constants.PlatformProductId),
                sessions => sessions.OrderByDescending(y => y.Id)).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_ClientSession().Where(x => x.ProductId != Constants.PlatformProductId))
            };
        }

        #endregion

        #endregion

        #region Export to excel

        public InternetGames ExportReportByInternetGames(FilterInternetGame filter)
        {
            var internetBetAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewInternetBets,
                ObjectTypeId = (int)ObjectTypes.fnInternetBet
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnInternetGame>>
            {
                new CheckPermissionOutput<fnInternetGame>
                {
                    AccessibleObjects = internetBetAccess.AccessibleObjects,
                    HaveAccessForAllObjects = internetBetAccess.HaveAccessForAllObjects,
                    Filter = x => internetBetAccess.AccessibleObjects.AsEnumerable().Contains(x.ObjectId)
                },
                new CheckPermissionOutput<fnInternetGame>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                }
            };
            var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Value.Year * 1000000 + filter.ToDate.Value.Month * 10000 + filter.ToDate.Value.Day * 100 + filter.ToDate.Value.Hour;
            filter.SkipCount = 0;
            filter.TakeCount = 0;
            var objects = filter.FilterObjects(Db.fn_InternetGame(fDate, tDate)).ToList();

            var entities = new List<InternetGame>();
            foreach (var e in objects)
            {
                entities.Add(new InternetGame
                {
                    BetAmount = ConvertCurrency(e.CurrencyId, CurrencyId, e.BetAmount ?? 0),
                    WinAmount = ConvertCurrency(e.CurrencyId, CurrencyId, e.WinAmount ?? 0),
                    Count = e.BetCount ?? 0,
                    GameId = e.ProductId,
                    GameName = e.ProductName,
                    CurrencyId = e.CurrencyId,
                    ProviderId = e.GameProviderId,
                    ProviderName = CacheManager.GetGameProviderById(e.GameProviderId.Value).Name,
                    SubproviderId = e.SubproviderId ?? e.GameProviderId,
                    SubproviderName = CacheManager.GetGameProviderById(e.SubproviderId ?? e.GameProviderId.Value).Name
                });
            }
            var result = new InternetGames
            {
                TotalBetAmount = entities.Sum(x => x.BetAmount),
                TotalWinAmount = entities.Sum(x => x.WinAmount),
                TotalBetCount = entities.Sum(x => x.Count),
                Entities = entities,
            };
            return result;
        }

        public List<fnClientSession> ExportClientSessions(FilterReportByClientSession filter)
        {
            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportClientSessions
            });
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });

            var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnClientSession>>
            {
                new CheckPermissionOutput<fnClientSession>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId)
                },
                new CheckPermissionOutput<fnClientSession>
                {
                    AccessibleObjects = affiliateReferralAccess.AccessibleObjects,
                    HaveAccessForAllObjects = affiliateReferralAccess.HaveAccessForAllObjects,
                    Filter = x => x.AffiliateReferralId.HasValue && affiliateReferralAccess.AccessibleObjects.AsEnumerable().Contains(x.AffiliateReferralId.Value)
                },
                new CheckPermissionOutput<fnClientSession>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x =>partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnClientSession>
                {
                    AccessibleObjects = exportAccess.AccessibleObjects,
                    HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                    Filter = x => exportAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
            };
            filter.TakeCount = 0;
            filter.SkipCount = 0;
            return filter.FilterObjects(Db.fn_ClientSession().Where(x => x.ProductId == Constants.PlatformProductId)).ToList();
        }

        public List<fnInternetBet> ExportInternetBet(FilterInternetBet filter)
        {
            var internetBetAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewInternetBets,
                ObjectTypeId = (int)ObjectTypes.fnInternetBet
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportInternetBet
            });

            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });

            var affiliateReferraltAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnInternetBet>>
            {
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = internetBetAccess.AccessibleObjects,
                    HaveAccessForAllObjects = internetBetAccess.HaveAccessForAllObjects,
                    Filter = x => internetBetAccess.AccessibleObjects.AsEnumerable().Contains(x.ObjectId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = exportAccess.AccessibleObjects,
                    HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                    Filter = x => exportAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = affiliateReferraltAccess.AccessibleObjects,
                    HaveAccessForAllObjects = affiliateReferraltAccess.HaveAccessForAllObjects,
                    Filter = x => x.AffiliateReferralId.HasValue && affiliateReferraltAccess.AccessibleObjects.AsEnumerable().Contains(x.AffiliateReferralId.Value)
                }
            };

            filter.TakeCount = 0;
            filter.SkipCount = 0;
            var result = filter.FilterObjects(Db.fn_InternetBet(), d => d.OrderByDescending(x => x.BetDocumentId)).ToList();
            foreach (var r in result)
            {
                r.BetAmount = ConvertCurrency(r.CurrencyId, CurrencyId, r.BetAmount);
                r.WinAmount = ConvertCurrency(r.CurrencyId, CurrencyId, r.WinAmount);
                r.PossibleWin = ConvertCurrency(r.CurrencyId, CurrencyId, r.PossibleWin ?? 0);
            }
            return result;
        }

        public List<fnReportByProduct> ExportProducts(FilterReportByProduct filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportProduct
            });

            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });

            var affiliateReferraltAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnReportByProduct>>
                {
                new CheckPermissionOutput<fnReportByProduct>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnReportByProduct>
                {
                    AccessibleObjects = exportAccess.AccessibleObjects,
                    HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                    Filter = x => exportAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnReportByProduct>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId)
                },
                new CheckPermissionOutput<fnReportByProduct>
                {
                    AccessibleObjects = affiliateReferraltAccess.AccessibleObjects,
                    HaveAccessForAllObjects = affiliateReferraltAccess.HaveAccessForAllObjects,
                    Filter = x =>x.AffiliateReferralId.HasValue && affiliateReferraltAccess.AccessibleObjects.AsEnumerable().Contains(x.AffiliateReferralId.Value)
                }
            };
            filter.TakeCount = 0;
            filter.SkipCount = 0;
            var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * 1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            var result = filter.FilterObjects(Db.fn_ReportByProduct(fDate, tDate)).ToList();
            return result;
        }

        public List<BetShopReport> ExportBetShops(FilterBetShopBet filter)
        {
            CheckPermission(Constants.Permissions.ViewBetShopsReport);

            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportBetShops
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnBetShopBet>>
            {
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleObjects = exportAccess.AccessibleObjects,
                    HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                    Filter = x => exportAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                }
            };

            filter.TakeCount = 0;
            filter.SkipCount = 0;

            var entities =
                filter.FilterObjects(Db.fn_BetShopBet(), d => d.OrderByDescending(x => x.BetDocumentId)).Select(x => new
                {
                    x.BetShopId,
                    x.BetShopCurrencyId,
                    x.BetShopName,
                    x.BetShopGroupId,
                    BetAmount = x.BetAmount,
                    WinAmount = x.WinAmount
                }).GroupBy(x => new { x.BetShopId, x.BetShopGroupId, x.BetShopName, x.BetShopCurrencyId })
                    .Select(
                        x =>
                            new BetShopReport
                            {
                                BetAmount = x.Sum(y => y.BetAmount),
                                WinAmount = x.Sum(y => y.WinAmount),
                                BetShopId = x.Key.BetShopId,
                                BetShopGroupId = x.Key.BetShopGroupId,
                                BetShopName = x.Key.BetShopName,
                                CurrencyId = x.Key.BetShopCurrencyId
                            }).ToList();

            return entities;
        }

        public List<ReportByProvidersElement> ExportProviders(FilterReportByProvider filter)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportProviders
            });
            filter.TakeCount = 0;
            filter.SkipCount = 0;
            var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * 1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            var result = filter.FilterObjects(Db.fn_ReportByProvider(fDate, tDate))
                .Select(x => new ReportByProvidersElement
                {
                    ProviderName = x.ProviderName,
                    Currency = x.Currency,
                    TotalBetsCount = x.TotalBetsCount ?? 0,
                    TotalBetsAmount = x.TotalBetsAmount ?? 0,
                    TotalWinsAmount = x.TotalWinsAmount ?? 0,
                    TotalUncalculatedBetsCount = x.TotalUncalculatedBetsCount ?? 0,
                    TotalUncalculatedBetsAmount = x.TotalUncalculatedBetsAmount ?? 0,
                    GGR = x.GGR ?? 0,
                    BetsCountPercent = x.TotalBetsCount * 100 ?? 0,
                    BetsAmountPercent = x.TotalBetsAmount * 100 ?? 0,
                    GGRPercent = x.GGR * 100 ?? 0
                }).ToList();

            var totalBetsCount = result.Sum(x => x.TotalBetsCount);
            var totalBetsAmount = result.Sum(x => x.TotalBetsAmount);
            var totalGGR = result.Sum(x => x.GGRPercent);
            foreach (var r in result)
            {
                r.BetsCountPercent = Math.Round(totalBetsCount == 0 ? 0 : r.BetsCountPercent / totalBetsCount, 2);
                r.BetsAmountPercent = Math.Round(totalBetsAmount == 0 ? 0 : r.BetsAmountPercent / totalBetsAmount, 2);
                r.GGRPercent = Math.Round(totalGGR == 0 ? 0 : r.GGRPercent / totalGGR, 2);
            }
            return result;
        }

        public List<fnBetShopReconing> ExportBetShopReconings(FilterfnBetShopReconing filter)
        {
            var checkP = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShopReconing,
                ObjectTypeId = (int)ObjectTypes.BetShopReconing
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnBetShopReconing>>
            {
                new CheckPermissionOutput<fnBetShopReconing>
                {
                    AccessibleObjects = checkP.AccessibleObjects,
                    HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                    Filter = x=> checkP.AccessibleObjects.AsEnumerable().Contains(x.ObjectId)
                }
            };

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            filter.CheckPermissionResuts.Add(new CheckPermissionOutput<fnBetShopReconing>
            {
                AccessibleObjects = partnerAccess.AccessibleObjects,
                HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
            });

            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportBetShopReconings
            });

            filter.CheckPermissionResuts.Add(new CheckPermissionOutput<fnBetShopReconing>
            {
                AccessibleObjects = exportAccess.AccessibleObjects,
                HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                Filter = x => exportAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
            });

            filter.TakeCount = 0;
            filter.SkipCount = 0;

            var result = filter.FilterObjects(Db.fn_BetShopReconing(), reconings => reconings.OrderBy(x => x.Id)).ToList();
            return result;
        }

        public List<InternetBetByClient> ExportInternetBetsByClient(FilterInternetBet filter)
        {
            var internetBetAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewInternetBets,
                ObjectTypeId = (int)ObjectTypes.fnInternetBet
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportInternetBetsByClient
            });

            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });

            var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnInternetBet>>
            {
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = internetBetAccess.AccessibleObjects,
                    HaveAccessForAllObjects = internetBetAccess.HaveAccessForAllObjects,
                    Filter = x => internetBetAccess.AccessibleObjects.AsEnumerable().Contains(x.ObjectId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = exportAccess.AccessibleObjects,
                    HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                    Filter = x => exportAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = affiliateReferralAccess.AccessibleObjects,
                    HaveAccessForAllObjects = affiliateReferralAccess.HaveAccessForAllObjects,
                    Filter = x => x.AffiliateReferralId.HasValue && affiliateReferralAccess.AccessibleObjects.AsEnumerable().Contains(x.AffiliateReferralId.Value)
                }
            };

            var result = (from ib in
                              (from b in filter.FilterObjects(Db.fn_InternetBet(), d => d.OrderByDescending(x => x.BetDocumentId))
                               group b by new { b.ClientId, b.ClientUserName, b.CurrencyId } into y
                               select new
                               {
                                   ClientId = y.Key.ClientId,
                                   UserName = y.Key.ClientUserName,
                                   TotalBetsCount = y.Count(),
                                   TotalBetsAmount = y.Sum(x => x.BetAmount),
                                   TotalWinsAmount = y.Sum(x => x.WinAmount),
                                   Currency = y.Key.CurrencyId,
                                   MaxBetAmount = y.Max(x => x.BetAmount)
                               })
                          from pr in Db.fn_InternetBetByClient()
                              .Where(m => m.ClientId == ib.ClientId).DefaultIfEmpty()
                          select new InternetBetByClient
                          {
                              ClientId = ib.ClientId,
                              UserName = ib.UserName,
                              TotalBetsCount = ib.TotalBetsCount,
                              TotalBetsAmount = ib.TotalBetsAmount,
                              TotalWinsAmount = ib.TotalWinsAmount,
                              Currency = ib.Currency,
                              //GGR = ib.TotalBetsAmount - ib.TotalWinsAmount,
                              MaxBetAmount = ib.MaxBetAmount,
                              TotalDepositsCount = pr.TotalDepositsCount ?? 0,
                              TotalDepositsAmount = pr.TotalDepositsAmount ?? 0,
                              TotalWithdrawalsCount = pr.TotalWithdrawalsCount ?? 0,
                              TotalWithdrawalsAmount = pr.TotalWithdrawalsAmount ?? 0,
                              Balance = pr.Balance == null ? 0 : pr.Balance.Value
                          });

            filter.TakeCount = 0;
            filter.SkipCount = 0;

            var entries = filter.FilterResultObjects(result).ToList();
            foreach (var e in entries)
            {
                e.GGR = e.TotalBetsAmount - e.TotalWinsAmount;
            }
            return entries;
        }

        public List<fnBetShopBet> ExportBetShopBets(FilterBetShopBet filter)
        {
            var betShopBetAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShopBets,
                ObjectTypeId = (int)ObjectTypes.fnBetShopBet
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportBetShopBets
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnBetShopBet>>
            {
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleObjects = betShopBetAccess.AccessibleObjects,
                    HaveAccessForAllObjects = betShopBetAccess.HaveAccessForAllObjects,
                    Filter = x => betShopBetAccess.AccessibleObjects.AsEnumerable().Contains(x.ObjectId)
                },
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                }
            };

            filter.TakeCount = 0;
            filter.SkipCount = 0;

            var result = filter.FilterObjects(Db.fn_BetShopBet(), bets => bets.OrderByDescending(y => y.BetDocumentId)).ToList();
            return result;
        }

        public List<fnReportByBetShopOperation> ExportByBetShopPayments(FilterReportByBetShopPayment filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportByBetShopPayments
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnReportByBetShopOperation>>
            {
                new CheckPermissionOutput<fnReportByBetShopOperation>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnReportByBetShopOperation>
                {
                    AccessibleObjects = exportAccess.AccessibleObjects,
                    HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                    Filter = x => exportAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                }
            };

            filter.TakeCount = 0;
            filter.SkipCount = 0;

            var result = filter.FilterObjects(Db.fn_ReportByBetShopOperation(filter.FromDate, filter.ToDate)).ToList();
            return result;
        }

        public List<BetshopSummaryReport> ExportBetshopSummary(FilterBetShopBet filter)
        {
            return new List<BetshopSummaryReport>();
        }

        public List<fnActionLog> ExportByActionLogs(FilterReportByActionLog filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportByUserLogs
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnActionLog>>
            {
                new CheckPermissionOutput<fnActionLog>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                }
            };

            filter.TakeCount = 0;
            filter.SkipCount = 0;
            var fDate = (long)filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = (long)filter.ToDate.Year * 1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            var result = filter.FilterObjects(Db.fn_ActionLog(Identity.LanguageId, fDate, tDate), reconings => reconings.OrderBy(x => x.Id)).ToList();
            return result;
        }

        public List<fnCorrection> ExportClientCorrections(FilterCorrection filter)
        {
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });

            var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportClientCorrections
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnCorrection>>
            {
                new CheckPermissionOutput<fnCorrection>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId.Value)
                },
                new CheckPermissionOutput<fnCorrection>
                {
                    AccessibleObjects = exportAccess.AccessibleObjects,
                    HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                    Filter = x => exportAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId.Value)
                },
                new CheckPermissionOutput<fnCorrection>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId.Value)
                },
                new CheckPermissionOutput<fnCorrection>
                {
                    AccessibleObjects = affiliateReferralAccess.AccessibleObjects,
                    HaveAccessForAllObjects = affiliateReferralAccess.HaveAccessForAllObjects,
                    Filter = x =>x.AffiliateReferralId.HasValue && affiliateReferralAccess.AccessibleObjects.AsEnumerable().Contains(x.AffiliateReferralId.Value)
                }
            };

            var result = filter.FilterObjects(Db.fn_Correction(true), documents => documents.OrderByDescending(y => y.Id)).ToList();
            return result;
        }

        public List<fnAccount> ExportClientAccounts(FilterfnAccount filter)
        {
            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportClientAccounts
            });

            filter.CheckPermissionResuts.Add(new CheckPermissionOutput<fnAccount>
            {
                AccessibleObjects = exportAccess.AccessibleObjects,
                HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                Filter = x => exportAccess.AccessibleObjects.AsEnumerable().Contains(x.ObjectId) //?? x.ObjectId is null
            });

            filter.TakeCount = 0;
            filter.SkipCount = 0;

            return filter.FilterObjects(Db.fn_Account(LanguageId)).ToList();
        }

        public List<ApiClientInfo> ExportClientsInfoList(FilterfnClientDashboard filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });

            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportPlayersDashboard
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnClientReport>>
            {
                new CheckPermissionOutput<fnClientReport>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId)
                },
                new CheckPermissionOutput<fnClientReport>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnClientReport>
                {
                    AccessibleObjects = exportAccess.AccessibleObjects,
                    HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                    Filter = x => exportAccess.AccessibleObjects.Contains(x.PartnerId)
                }
            };
            filter.TakeCount = 0;
            filter.SkipCount = 0;
            var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * 1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            var dbClientReport = filter.FilterObjects(Db.fn_ClientReport(fDate, tDate)).ToList();
            var entities = new List<ApiClientInfo>();
            dbClientReport.ForEach(x =>
            {
                var balance = CacheManager.GetClientCurrentBalance(x.ClientId);
                entities.Add(new ApiClientInfo
                {
                    ClientId = x.ClientId,
                    UserName = x.UserName,
                    PartnerId = x.PartnerId,
                    CurrencyId = x.CurrencyId,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    AffiliatePlatformId = x.AffiliatePlatformId,
                    AffiliateId = x.AffiliateId,
                    AffiliateReferralId = x.AffiliateReferralId,
                    TotalDepositAmount = x.TotalDepositAmount ?? 0,
                    DepositsCount = x.DepositsCount ?? 0,
                    TotalWithdrawalAmount = x.TotalWithdrawalAmount ?? 0,
                    WithdrawalsCount = x.WithdrawalsCount ?? 0,
                    TotalBetAmount = x.TotalBetAmount ?? 0,
                    BetsCount = x.BetsCount ?? 0,
                    TotalWinAmount = x.TotalWinAmount ?? 0,
                    WinsCount = x.WinsCount ?? 0,
                    TotalCreditCorrection = x.TotalCreditCorrection ?? 0,
                    CreditCorrectionsCount = x.CreditCorrectionsCount ?? 0,
                    TotalDebitCorrection = x.TotalDebitCorrection ?? 0,
                    DebitCorrectionsCount = x.DebitCorrectionsCount ?? 0,
                    GGR = x.GGR ?? 0,
                    NGR = (x.TotalDepositAmount ?? 0) + (x.TotalDebitCorrection?? 0) -
                          (x.TotalWithdrawalAmount ?? 0) - (x.TotalCreditCorrection ?? 0) - balance.AvailableBalance

                });
            });
            return entities;
        }

        #endregion

        #region Agent

        public List<fnAgent> GetReportByAgents(DateTime fromDate, DateTime toDate, int agentId)
        {
            var user = CacheManager.GetUserById(agentId);
            if (user.Type == (int)UserTypes.AgentEmployee)
                CheckPermission(Constants.Permissions.ViewUserReport);

            var fDate = fromDate.Year * (long)1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
            var tDate = toDate.Year * (long)1000000 + toDate.Month * 10000 + toDate.Day * 100 + toDate.Hour;
            var transactions = Db.fn_AgentProfitReport(fDate, tDate, agentId);
            var agents = Db.fn_Agent(agentId).ToList();
            var bets = Db.Bets.Where(x => x.BetDate >= fDate && x.BetDate < tDate && x.Client.User.Path.Contains("/" + agentId + "/")).GroupBy(x => x.Client.UserId).Select(x => new
            {
                UserId = x.Key,
                TotalBetAmount = x.Sum(y => y.BetAmount),
                TotalWinAmount = x.Sum(y => y.WinAmount),
                TotalProfit = x.Sum(y => y.State == (int)BetDocumentStates.Uncalculated ? 0 : y.BetAmount - y.WinAmount)
            }).ToList();
            foreach (var subAgent in agents)
            {
                var agentTransactions = transactions.Where(x => x.ToAgentId == subAgent.Id).ToList();
                subAgent.TotalTurnoverProfit = agentTransactions.Sum(x => x.TotalTurnoverProfit ?? 0);
                subAgent.TotalGGRProfit = agentTransactions.Sum(x => x.TotalGGRProfit ?? 0);
                var uBets = bets.FirstOrDefault(x => x.UserId == subAgent.Id);
                if (uBets != null)
                {
                    subAgent.TotalBetAmount = uBets.TotalBetAmount;
                    subAgent.TotalWinAmount = uBets.TotalWinAmount;
                    subAgent.TotalGGR = uBets.TotalProfit;
                }
            }
            return agents;
        }

        public List<fnAgent> GetAgentsReportByProductGroup(DateTime fromDate, DateTime toDate, int agentId, string productGroupName)
        {
            var productGroupId = CacheManager.GetProductGroupByName(Constants.SportGroupName).Id;
            var user = CacheManager.GetUserById(agentId);
            if (user.Type == (int)UserTypes.AgentEmployee)
                CheckPermission(Constants.Permissions.ViewUserReport);

            var fDate = fromDate.Year * (long)1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
            var tDate = toDate.Year * (long)1000000 + toDate.Month * 10000 + toDate.Day * 100 + toDate.Hour;
            var profitTransactions = Db.AgentProfits.Include(x => x.Agent).Where(x => x.Agent.Path.Contains("/" + agentId + "/") &&
                                      ((productGroupName == Constants.SportGroupName && x.ProductGroupId == productGroupId) ||
                                      (productGroupName != Constants.SportGroupName && x.ProductGroupId != productGroupId)) &&
                                      x.CreationDate > fDate && x.CalculationStartingDate <= tDate);

            var agents = Db.fn_Agent(agentId).ToList();

            foreach (var subAgent in agents)
            {
                var agentTransactions = profitTransactions.Where(x => x.AgentId == subAgent.Id).ToList();
                subAgent.TotalTurnoverProfit = agentTransactions.Where(x => x.Type == (int)AgentProfitTypes.Turnover).Sum(x => x.Profit);
                subAgent.DirectTurnoverProfit = agentTransactions.Where(x => x.Type == (int)AgentProfitTypes.Turnover && x.FromAgentId == null).Sum(x => x.Profit);

                subAgent.TotalGGRProfit = agentTransactions.Where(x => x.Type == (int)AgentProfitTypes.GGR).Sum(x => x.Profit);
                subAgent.DirectGGRProfit = agentTransactions.Where(x => x.Type == (int)AgentProfitTypes.GGR && x.FromAgentId == null).Sum(x => x.Profit);

                subAgent.TotalBetAmount = agentTransactions.Where(x => x.Type == (int)AgentProfitTypes.Turnover).Sum(x => x.TotalBetAmount);
                subAgent.DirectBetAmount = agentTransactions.Where(x => x.Type == (int)AgentProfitTypes.Turnover && x.FromAgentId == null).Sum(x => x.TotalBetAmount);

                subAgent.TotalWinAmount = agentTransactions.Where(x => x.Type == (int)AgentProfitTypes.Turnover).Sum(x => x.TotalWinAmount);
                subAgent.DirectWinAmount = agentTransactions.Where(x => x.Type == (int)AgentProfitTypes.Turnover && x.FromAgentId == null).Sum(x => x.TotalWinAmount);

                subAgent.TotalGGR = agentTransactions.Where(x => x.Type == (int)AgentProfitTypes.GGR).Sum(x => x.GGR);
            }

            return agents;
        }

        public PagedModel<fnAgentTransaction> GetAgentTransactions(FilterfnAgentTransaction filter, int agentId, bool checkPermission)
        {
            if (checkPermission)
                CheckPermission(Constants.Permissions.ViewReportByTransaction);
            var fDate = filter.FromDate.Year * (long)1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * (long)1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            return new PagedModel<fnAgentTransaction>
            {
                Entities = filter.FilterObjects(Db.fn_AgentTransaction(fDate, tDate, agentId), documents => documents.OrderByDescending(y => y.Id)).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_AgentTransaction(fDate, tDate, agentId))
            };
        }

        public List<fnOnlineUser> GetOnlineUsers(int? userId)
        {
            var query = Db.fn_OnlineUser(Identity.Id);
            if (userId.HasValue)
                query = query.Where(x => x.Id == userId);
            return query.ToList();
        }

        public PagedModel<spObjectChangeHistory> GetReportByObjectChangeHistory(FilterReportByObjectChangeHistory filter)
        {
            CheckPermissionToSaveObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewReportByObjectChangeHistory
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<spObjectChangeHistory>>
            {
                new CheckPermissionOutput<spObjectChangeHistory>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => x.PartnerId.HasValue && partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId.Value)
                }
            };
            Func<IQueryable<spObjectChangeHistory>, IOrderedQueryable<spObjectChangeHistory>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<spObjectChangeHistory>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<spObjectChangeHistory>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = history => history.OrderByDescending(x => x.Id);
            }
            var objectResult = Db.Procedures.sp_ObjectChangeHistoryAsync(filter.FromDate, filter.ToDate, filter.ObjectTypeId).Result;
            return new PagedModel<spObjectChangeHistory>
            {
                Entities = filter.FilterObjects(objectResult.AsQueryable(), orderBy).ToList(),
                Count = filter.SelectedObjectsCount(objectResult.AsQueryable())
            };
        }

        public PagedModel<spObjectChangeHistory> ExportObjectChangeHistory(FilterReportByObjectChangeHistory filter)
        {
            CheckPermissionToSaveObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportObjectChangeHistory
            });
            filter.TakeCount = 0;
            filter.SkipCount = 0;
            return GetReportByObjectChangeHistory(filter);
        }

        public List<ObjectChangeHistoryItem> GetClientChangeHistory(int? clientId, DateTime startDate, DateTime? endDate, double timeZone)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });

            var query = Db.ObjectChangeHistories.Join(Db.Clients, oh => oh.ObjectId, c => c.Id, (oh, c) => new ClientChangeHistoryModel
            {
                ObjectChangeHistory = oh,
                ClientItem = c
            })
                                                .Where(x => x.ObjectChangeHistory.ObjectTypeId == (int)ObjectTypes.Client &&
                                                            x.ObjectChangeHistory.ChangeDate >= startDate);
            if (endDate.HasValue)
                query = query.Where(x => x.ObjectChangeHistory.ChangeDate <= endDate);
            if (clientId.HasValue)
                query = query.Where(x => x.ObjectChangeHistory.ObjectId == clientId.Value);

            var checkPermissionResuts = new List<CheckPermissionOutput<ClientChangeHistoryModel>>
            {
                new CheckPermissionOutput<ClientChangeHistoryModel>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientItem.PartnerId)
                },
                new CheckPermissionOutput<ClientChangeHistoryModel>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientItem.Id)
                }
            };
            query = checkPermissionResuts.Where(
                        checkPermissionResut =>
                            checkPermissionResut != null && !checkPermissionResut.HaveAccessForAllObjects)
                        .Aggregate(query,
                            (current, checkPermissionResut) => current.Where(checkPermissionResut.Filter));

            var result = query.AsEnumerable();
            return result.Select(oh => new ObjectChangeHistoryItem
            {
                Id = oh.ObjectChangeHistory.Id,
                ObjectId = oh.ObjectChangeHistory.ObjectId,
                ObjectTypeId = oh.ObjectChangeHistory.ObjectTypeId,
                ChangeDate = oh.ObjectChangeHistory.ChangeDate,
                Comment = oh.ObjectChangeHistory.Comment != "System" && oh.ObjectChangeHistory.SessionId == null ? "Player" : oh.ObjectChangeHistory.Comment,
                FirstName = oh.ObjectChangeHistory.Session?.User?.FirstName,
                LastName = oh.ObjectChangeHistory.Session?.User?.LastName,
                ObjectChangedItems = new List<string>
                {
                    oh.ObjectChangeHistory.Object,
                    JsonConvert.SerializeObject(oh.ClientItem.ToClientInfo(timeZone))
                }
            }).ToList();
        }

        public List<ObjectChangeHistoryItem> GetObjectChangeHistory(int objectTypeId, int objectId)
        {
            return Db.ObjectChangeHistories.Where(oh => oh.ObjectId == objectId && oh.ObjectTypeId == objectTypeId)
                                                            .Select(oh => new ObjectChangeHistoryItem
                                                            {
                                                                Id = oh.Id,
                                                                ObjectId = oh.ObjectId,
                                                                ObjectTypeId = oh.ObjectTypeId,
                                                                ChangeDate = oh.ChangeDate,
                                                                Comment = oh.Comment != "System" && oh.SessionId == null ? "Player" : oh.Comment,
                                                                FirstName = oh.Session.User.FirstName,
                                                                LastName = oh.Session.User.LastName
                                                            }).OrderByDescending(x => x.Id).ToList();
        }

        public List<string> GetObjectHistoryElementById(long objectHistoryId)
        {
            var result = Db.ObjectChangeHistories.Where(oh => oh.Id == objectHistoryId).FirstOrDefault();
            if (result == null)
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var newResult = Db.ObjectChangeHistories.Where(oh => oh.Id > result.Id && oh.ObjectTypeId == result.ObjectTypeId &&
                oh.ObjectId == result.ObjectId).OrderBy(x => x.Id).FirstOrDefault();
            var newValue = string.Empty;
            if (newResult == null)
            {
                switch (result.ObjectTypeId)
                {
                    case (int)ObjectTypes.Client:
                        var client = Db.Clients.First(x => x.Id == result.ObjectId);
                        newValue = JsonConvert.SerializeObject(client.ToClientInfo());
                        break;
                    case (int)ObjectTypes.Trigger:
                        var trigger = Db.TriggerSettings.First(x => x.Id == result.ObjectId);
                        newValue = JsonConvert.SerializeObject(trigger.ToTriggerInfo());
                        break;
                    case (int)ObjectTypes.Bonus:
                        var bonus = Db.Bonus.First(x => x.Id == result.ObjectId);
                        newValue = JsonConvert.SerializeObject(bonus.ToBonusInfo());
                        break;
                    case (int)ObjectTypes.Product:
                        var product = Db.Products.First(x => x.Id == result.ObjectId);
                        newValue = JsonConvert.SerializeObject(product.ToProductInfo(LanguageId));
                        break;
                    case (int)ObjectTypes.PartnerProductSetting:
                        var partnerProduct = Db.PartnerProductSettings.First(x => x.Id == result.ObjectId);
                        newValue = JsonConvert.SerializeObject(partnerProduct);
                        break;
                    case (int)ObjectTypes.ClientIdentity:
                        var clientIdentity = Db.ClientIdentities.First(x => x.Id == result.ObjectId);
                        newValue = JsonConvert.SerializeObject(clientIdentity.ToClientIdentity());
                        break;
                    case (int)ObjectTypes.ClientSetting:
                        var c = CacheManager.GetClientById(Convert.ToInt32(result.ObjectId));
                        var clientSetting = Db.ClientSettings.Where(x => x.ClientId == result.ObjectId).ToList().Select(x => x.ToClientSettingInfo()).ToList();
                        var currentDate = DateTime.UtcNow;
                        var documents = Db.ClientIdentities.Where(x => x.ClientId == c.Id).ToList();
                        clientSetting.Add(new
                        {
                            Name = "Excluded",
                            StringValue = (c.State == (int)ClientStates.ForceBlock ||
                                                                                  c.State == (int)ClientStates.FullBlocked ||
                                                                                  c.State == (int)ClientStates.Disabled) ? "1" : "0"
                        });
                        clientSetting.Add(new { Name = "DocumentVerified", StringValue = (c.IsDocumentVerified) ? "1" : "0" });
                        clientSetting.Add(new
                        {
                            Name = "DocumentExpired",
                            StringValue = (!documents.Any(x => x.Status == (int)KYCDocumentStates.Approved) &&
                            documents.Any(x => x.Status == (int)KYCDocumentStates.Expired)) ? "1" : "0"
                        });
                        var partner = CacheManager.GetPartnerById(c.PartnerId);
                        clientSetting.Add(new
                        {
                            Name = "Younger",
                            StringValue = (c.BirthDate != DateTime.MinValue &&
                                (currentDate.Year - c.BirthDate.Year < partner.ClientMinAge ||
                                (currentDate.Year - c.BirthDate.Year == partner.ClientMinAge && currentDate.Month - c.BirthDate.Month < 0) ||
                                (currentDate.Year - c.BirthDate.Year == partner.ClientMinAge && currentDate.Month == c.BirthDate.Month && currentDate.Day < c.BirthDate.Day))) ? "1" : "0"
                        });
                        newValue = JsonConvert.SerializeObject(clientSetting);
                        break;
                    default:
                        break;
                }
            }
            else
                newValue = newResult.Object;

            return new List<string> { result.Object, newValue };
        }

        public BonusReport GetReportByBonus(FilterReportByBonus filter)
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
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBonuses
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnClientBonus>>
            {
                new CheckPermissionOutput<fnClientBonus>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnClientBonus>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId)
                }
            };
            Func<IQueryable<fnClientBonus>, IOrderedQueryable<fnClientBonus>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnClientBonus>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnClientBonus>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = clientBonuses => clientBonuses.OrderByDescending(x => x.BonusId);
            }
            var res = new PagedModel<fnClientBonus>
            {
                Entities = filter.FilterObjects(Db.fn_ClientBonus(LanguageId), orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_ClientBonus(LanguageId)),
            };
            var totalPrize = res.Entities.Sum(x => x.BonusPrize);
            var totalFinalAmount = res.Entities.Sum(x => x.FinalAmount ?? 0);

            return new BonusReport
            {
                Entities = res.Entities,
                Count = res.Count,
                TotalBonusPrize = totalPrize,
                TotalFinalAmount = totalFinalAmount
            };
        }

        public List<fnClientBonus> ExportReportByBonus(FilterReportByBonus filter)
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
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBonuses
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnClientBonus>>
            {
                new CheckPermissionOutput<fnClientBonus>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnClientBonus>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId)
                }
            };
            var clientBonuses = filter.FilterObjects(Db.fn_ClientBonus(LanguageId)).ToList();
            using (var clientBl = new ClientBll(this))
            {
                foreach (var cb in clientBonuses)
                {
                    cb.TriggerSettingItems = clientBl.GetClientBonusTriggers(cb.ClientId, cb.BonusId, cb.ReuseNumber ?? 1, true);
                }
                return clientBonuses;
            }
        }

        public PagedModel<fnClientIdentity> GetReportByClientIdentity(FilterReportByClientIdentity filter)
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
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBonuses
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnClientIdentity>>
            {
                new CheckPermissionOutput<fnClientIdentity>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnClientIdentity>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId)
                }
            };
            Func<IQueryable<fnClientIdentity>, IOrderedQueryable<fnClientIdentity>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnClientIdentity>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnClientIdentity>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = clientIdentities => clientIdentities.OrderByDescending(x => x.ExpirationDate);
            }
            return new PagedModel<fnClientIdentity>
            {
                Entities = filter.FilterObjects(Db.fn_ClientIdentity(), orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_ClientIdentity())
            };
        }
        public PagedModel<fnClientIdentity> ExportClientIdentities(FilterReportByClientIdentity filter)
        {
            CheckPermissionToSaveObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportClientIdentities
            });
            filter.TakeCount = 0;
            filter.SkipCount = 0;
            return GetReportByClientIdentity(filter);
        }
        #endregion
        
        #region Client
        public PagedModel<fnCorrection> GetClientCorrections(FilterCorrection filter, bool checkPermission = true)
        {
            if (checkPermission)
            {
                var clientAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });

                var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliateReferral,
                    ObjectTypeId = (int)ObjectTypes.AffiliateReferral
                });

                GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });

                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnCorrection>>
                {
                    new CheckPermissionOutput<fnCorrection>
                    {
                        AccessibleObjects = clientAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                        Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId.Value)
                    },
                    new CheckPermissionOutput<fnCorrection>
                    {
                        AccessibleObjects = affiliateReferralAccess.AccessibleObjects,
                        HaveAccessForAllObjects = affiliateReferralAccess.HaveAccessForAllObjects,
                        Filter = x => x.AffiliateReferralId.HasValue && affiliateReferralAccess.AccessibleObjects.Contains(x.AffiliateReferralId.Value)
                    }
                };
            }
            var result = new PagedModel<fnCorrection>
            {
                Entities = filter.FilterObjects(Db.fn_Correction(true), documents => documents.OrderByDescending(y => y.Id)).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_Correction(true))
            };
            return result;
        }


        public PagedModel<fnReportByClientExclusion> GetReportByClientExclusions(FilterClientExclusion filter)
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

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnReportByClientExclusion>>
                {
                    new CheckPermissionOutput<fnReportByClientExclusion>
                    {
                        AccessibleObjects = partnerAccess.AccessibleObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleObjects.Contains(x.PartnerId.Value)
                    },
                    new CheckPermissionOutput<fnReportByClientExclusion>
                    {
                        AccessibleObjects = clientAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                        Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId)
                    }
                };
            Func<IQueryable<fnReportByClientExclusion>, IOrderedQueryable<fnReportByClientExclusion>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnReportByClientExclusion>(filter.FieldNameToOrderBy, true);
                else
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnReportByClientExclusion>(filter.FieldNameToOrderBy, false);
            }
            else
                orderBy = clientExclusion => clientExclusion.OrderByDescending(x => x.ClientId);

            return new PagedModel<fnReportByClientExclusion>
            {
                Entities = filter.FilterObjects(Db.fn_ReportByClientExclusion(), orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_ReportByClientExclusion())
            };
        }

        #endregion

        public PagedModel<fnReportByPaymentSystem> GetReportByPaymentSystems(FilterReportByPaymentSystem filter, int type)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPaymentRequests
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnReportByPaymentSystem>>
            {
                new CheckPermissionOutput<fnReportByPaymentSystem>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                }
            };
            Func<IQueryable<fnReportByPaymentSystem>, IOrderedQueryable<fnReportByPaymentSystem>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnReportByPaymentSystem>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnReportByPaymentSystem>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = paymentRequests => paymentRequests.OrderByDescending(x => x.PaymentSystemId);
            }
            var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * 1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            return new PagedModel<fnReportByPaymentSystem>
            {
                Entities = filter.FilterObjects(Db.fn_ReportByPaymentSystem(fDate, tDate, type), orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_ReportByPaymentSystem(fDate, tDate, type))
            };
        }
        public PagedModel<fnReportByPaymentSystem> ExportReportByPaymentSystems(FilterReportByPaymentSystem filter, int type)
        {
            CheckPermissionToSaveObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPaymentRequests
            });
            filter.TakeCount = 0;
            filter.SkipCount = 0;
            return GetReportByPaymentSystems(filter, type);
        }

        public PagedModel<fnReportByAgentTransfer> GetReportByAgentTransfers(FilterReportByAgentTranfer filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            var userAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewUser,
                ObjectTypeId = (int)ObjectTypes.User
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewReportByCorrection
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnReportByAgentTransfer>>
            {
                new CheckPermissionOutput<fnReportByAgentTransfer>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnReportByAgentTransfer>
                {
                    AccessibleObjects = userAccess.AccessibleObjects,
                    HaveAccessForAllObjects = userAccess.HaveAccessForAllObjects,
                    Filter = x => userAccess.AccessibleObjects.AsEnumerable().Contains(x.UserId)
                }
            };
            Func<IQueryable<fnReportByAgentTransfer>, IOrderedQueryable<fnReportByAgentTransfer>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnReportByAgentTransfer>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnReportByAgentTransfer>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = agentTranfers => agentTranfers.OrderByDescending(x => x.UserId);
            }
            var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * 1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            return new PagedModel<fnReportByAgentTransfer>
            {
                Entities = filter.FilterObjects(Db.fn_ReportByAgentTransfer(fDate, tDate), orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_ReportByAgentTransfer(fDate, tDate))
            };
        }
        public PagedModel<fnReportByAgentTransfer> ExportReportByAgentTransfers(FilterReportByAgentTranfer filter)
        {
            CheckPermissionToSaveObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewReportByCorrection
            });
            filter.TakeCount = 0;
            filter.SkipCount = 0;
            return GetReportByAgentTransfers(filter);
        }
        public PagedModel<fnReportByUserTransaction> GetReportByUserTransactions(FilterReportByUserTransaction filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            var userAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewUser,
                ObjectTypeId = (int)ObjectTypes.User
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewReportByCorrection
            });
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewReportByTransaction
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnReportByUserTransaction>>
            {
                new CheckPermissionOutput<fnReportByUserTransaction>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnReportByUserTransaction>
                {
                    AccessibleObjects = userAccess.AccessibleObjects,
                    HaveAccessForAllObjects = userAccess.HaveAccessForAllObjects,
                    Filter = x => userAccess.AccessibleObjects.Contains(x.UserId)
                }
            };
            Func<IQueryable<fnReportByUserTransaction>, IOrderedQueryable<fnReportByUserTransaction>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnReportByUserTransaction>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnReportByUserTransaction>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = userTransactions => userTransactions.OrderByDescending(x => x.UserId);
            }
            var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * 1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            return new PagedModel<fnReportByUserTransaction>
            {
                Entities = filter.FilterObjects(Db.fn_ReportByUserTransaction(fDate, tDate), orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_ReportByUserTransaction(fDate, tDate))
            };
        }

        public PagedModel<fnReportByUserTransaction> ExportReportByUserTransactions(FilterReportByUserTransaction filter)
        {
            filter.TakeCount = 0;
            filter.SkipCount = 0;
            return GetReportByUserTransactions(filter);
        }
        public List<fnReportByPartner> GetReportByPartners(FilterReportByPartner filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewReportByPartner
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnReportByPartner>>
            {
                new CheckPermissionOutput<fnReportByPartner>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                }
            };
            Func<IQueryable<fnReportByPartner>, IOrderedQueryable<fnReportByPartner>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnReportByPartner>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnReportByPartner>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = agentTranfers => agentTranfers.OrderByDescending(x => x.PartnerId);
            }
            var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * 1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            return filter.FilterObjects(Db.fn_ReportByPartner(fDate, tDate), orderBy).ToList();
        }

        public List<fnReportByPartner> ExportReportByPartners(FilterReportByPartner filter)
        {
            CheckPermissionToSaveObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportReportByPartners
            });
            filter.TakeCount = 0;
            filter.SkipCount = 0;
            return GetReportByPartners(filter);
        }

        public List<SegmentByPaymentSystem> GetReportBySegment(FilterReportBySegment input)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            CheckPermission(Constants.Permissions.ViewPaymentRequests);
            CheckPermission(Constants.Permissions.ViewSegment);
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != input.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var fDate = input.FromDate.Year * 1000000 + input.FromDate.Month * 10000 + input.FromDate.Day * 100 + input.FromDate.Hour;
            var tDate = input.ToDate.Year * 1000000 + input.ToDate.Month * 10000 + input.ToDate.Day * 100 + input.ToDate.Hour;

            //return Db.PaymentRequests.Where(x => x.Client.PartnerId == input.PartnerId && x.PaymentSystemId == input.PaymentSystemId &&
            //                                     x.Type == input.Type && x.Status == input.Type && x.SegmentId != null &&
            //                                     x.Date >= fDate && x.Date < tDate)
            //                               .GroupBy(x => new { x.SegmentId, SegmentName = x.PaymentSegment.TagCount + x.PaymentSegment.TagName })
            //                               .Select(x => new SegmentByPaymentSystem
            //                               {
            //                                   SegmentId = x.Key.SegmentId.Value,
            //                                   SegmentName = x.Key.SegmentName,
            //                                   PaymentRequestsCount = x.Count(),
            //                                   TotalAmount = x.Sum(y => y.Amount)
            //                               }).ToList();
            return new List<SegmentByPaymentSystem>();

        }
        public PagedModel<Log> GetLogs(int? id, DateTime fromDate, DateTime? toDate, int takeCount, int skipCount)
        {
            CheckPermission(Constants.Permissions.ViewReportByLog);
            var context = new NDBConetxt( ConfigurationString);
            var parameters = new List<SqlParameter> { new SqlParameter("@fromDate", fromDate) };
            var sqlFields = "SELECT Id, Type, Caller, Message, CreationTime ";
            var sqlCount = "SELECT count(*) ";
            var sqlString = "FROM Log WHERE CreationTime>=@fromDate";
            if (id.HasValue)
            {
                sqlString += " AND Id>=@id";
                parameters.Add(new SqlParameter("@id", id.Value));
            }
            if (toDate.HasValue)
            {
                sqlString += " AND CreationTime<@toDate";
                parameters.Add(new SqlParameter("@toDate", toDate));
            }
            var result = context.Database.SqlQuery<Log>(sqlFields + sqlString + " ORDER BY Id DESC", parameters.ToArray()).Skip(skipCount * takeCount).Take(takeCount).ToList();
            var count = context.Database.SqlQuery<int>(sqlCount + sqlString, parameters.Select(x => ((ICloneable)x).Clone()).ToArray()).First();

            return new PagedModel<Log>
            {
                Count = count,
                Entities = result
            };
        }
    }
}