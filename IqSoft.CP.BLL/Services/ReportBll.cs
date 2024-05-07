using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web.UI.WebControls;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Helpers;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.Report;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Filters.Agent;
using IqSoft.CP.DAL.Filters.Clients;
using IqSoft.CP.DAL.Filters.Reporting;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Agents;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Dashboard;
using IqSoft.CP.DAL.Models.PlayersDashboard;
using IqSoft.CP.DAL.Models.RealTime;
using IqSoft.CP.DAL.Models.Report;
using IqSoft.CP.DataWarehouse;
using IqSoft.CP.DataWarehouse.Filters;
using log4net;
using Newtonsoft.Json;
using static IqSoft.CP.Common.Constants;
using fnClientSession = IqSoft.CP.DAL.fnClientSession;
using PaymentRequest = IqSoft.CP.DAL.PaymentRequest;

namespace IqSoft.CP.BLL.Services
{
    public class ReportBll : PermissionBll, IReportBll
    {
        protected IqSoftDataWarehouseEntities Dwh;

        #region Constructors

        public ReportBll(SessionIdentity identity, ILog log, int? timeout = null) : base(identity, log, timeout)
        {
            Dwh = new IqSoftDataWarehouseEntities();
            Dwh.Database.CommandTimeout = 300;
        }

        public ReportBll(BaseBll baseBl) : base(baseBl)
        {
            Dwh = new IqSoftDataWarehouseEntities();
            Dwh.Database.CommandTimeout = 300;
        }

        public void Dispose()
        {
            if (Dwh != null)
            {
                Dwh.Dispose();
                Dwh = null;
            }
            base.Dispose();
        }

        #endregion

        #region Dashboard And RealTime

        public ClientsInfo GetPlayersInfoForDashboard(FilterDashboard filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewDashboard
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClientByCategory,
                ObjectTypeId = ObjectTypes.ClientCategory
            });

            var partnerIds = filter.PartnerId.HasValue ? new List<int> { filter.PartnerId.Value } : new List<int>();
            if (!partnerAccess.HaveAccessForAllObjects)
            {
                if (filter.PartnerId != null && !partnerAccess.AccessibleIntegerObjects.Contains(filter.PartnerId.Value))
                    throw CreateException(LanguageId, Errors.DontHavePermission);

                if (!partnerIds.Any())
                {
                    if (partnerAccess.AccessibleIntegerObjects.Any())
                        partnerIds = partnerAccess.AccessibleIntegerObjects.ToList();
                    else
                        partnerIds = new List<int> { -1 };
                }
            }
            var q = Dwh.Gtd_Dashboard_Info.Where(x => x.Date >= filter.FromDay && x.Date < filter.ToDay);
            if (partnerIds.Any())
                q = q.Where(x => partnerIds.Contains(x.PartnerId));

            var info = q.GroupBy(x => 1).Select(x => new ClientsInfo
            {
                VisitorsCount = x.Sum(y => y.VisitorsCount),
                SignUpsCount = x.Sum(y => y.SignUpsCount),
                TotalPlayersCount = x.Sum(y => y.TotalPlayersCount),
                ReturnsCount = x.Sum(y => y.ReturnsCount),
                DepositsCount = x.Sum(y => y.DepositsCount),
                TotalBetsCount = x.Sum(y => y.BetsCount),
                TotalBetAmount = x.Sum(y => y.BetAmount),
                TotalBonusAmount = x.Sum(y => y.BonusAmount),
                TotalCashoutAmount = x.Sum(y => y.CashoutAmount),
                MaxBet = x.Max(y => y.MaxBet),
                MaxWin = x.Max(y => y.MaxWin)
            }).FirstOrDefault();
            if (info == null)
                return new ClientsInfo();

            info.AverageBet = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, info.TotalBetsCount == 0 ? 0 : info.TotalBetAmount / info.TotalBetsCount);
            info.MaxBet = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, info.MaxBet);
            info.MaxWin = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, info.MaxWin);
            info.TotalBonusAmount = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, info.TotalBonusAmount);
            info.TotalBetAmount = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, info.TotalBetAmount);
            info.TotalCashoutAmount = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, info.TotalCashoutAmount);
            
            info.DailyInfo = q.GroupBy(x => x.Date).Select(x => new ClientsDailyInfo
            {
                LongDate = x.Key,
                VisitorsCount = x.Sum(y => y.VisitorsCount),
                SignUpsCount = x.Sum(y => y.SignUpsCount),
                TotalPlayersCount = x.Sum(y => y.TotalPlayersCount),
                ReturnsCount = x.Sum(y => y.ReturnsCount),
                DepositsCount = x.Sum(y => y.DepositsCount),
                TotalBetsCount = x.Sum(y => y.BetsCount),
                TotalBetAmount = x.Sum(y => y.BetAmount),
                TotalBonusAmount = x.Sum(y => y.BonusAmount),
                TotalCashoutAmount = x.Sum(y => y.CashoutAmount),
                MaxBet = x.Max(y => y.MaxBet),
                MaxWin = x.Max(y => y.MaxWin)
            }).ToList();
            var totalDays = (filter.ToDate - filter.FromDate).TotalDays;
            for (int i = 0; i < totalDays; i++)
            {
                var date = filter.FromDate.AddDays(i);
                var day = (long)date.Year * 10000 + (long)date.Month * 100 + (long)date.Day;
                if(!info.DailyInfo.Any(x => x.LongDate == day))
                {
                    info.DailyInfo.Add(new ClientsDailyInfo { LongDate = day });
                }
            }
            info.DailyInfo = info.DailyInfo.OrderBy(x => x.LongDate).ToList();
            foreach (var di in info.DailyInfo)
            {
                di.Date = new DateTime((int)(di.LongDate / 10000), (int)((di.LongDate % 10000) / 100), (int)(di.LongDate % 100));
                di.AverageBet = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.TotalBetsCount == 0 ? 0 : di.TotalBetAmount / di.TotalBetsCount);
                di.MaxBet = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.MaxBet);
                di.MaxWin = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.MaxWin);
                di.TotalBonusAmount = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.TotalBonusAmount);
                di.TotalBetAmount = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.TotalBetAmount);
                di.TotalCashoutAmount = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.TotalCashoutAmount);
            }
            return info;
        }

        public List<CountryClientsInfo> GetTopRegistrationCountries(FilterDashboard filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewDashboard
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClientByCategory,
                ObjectTypeId = ObjectTypes.ClientCategory
            });

            var partnerIds = filter.PartnerId.HasValue ? new List<int> { filter.PartnerId.Value } : new List<int>();
            if (!partnerAccess.HaveAccessForAllObjects)
            {
                if (filter.PartnerId != null && !partnerAccess.AccessibleIntegerObjects.Contains(filter.PartnerId.Value))
                    throw CreateException(LanguageId, Errors.DontHavePermission);

                if (!partnerIds.Any())
                {
                    if (partnerAccess.AccessibleIntegerObjects.Any())
                        partnerIds = partnerAccess.AccessibleIntegerObjects.ToList();
                    else
                        partnerIds = new List<int> { -1 };
                }
            }
            var q = Dwh.Clients.Where(x => x.CreationTime >= filter.FromDate && x.CreationTime < filter.ToDate);
            if (partnerIds.Any())
                q = q.Where(x => partnerIds.Contains(x.PartnerId));

            var info = q.GroupBy(x => x.CountryId).Select(x => new CountryClientsInfo
            {
                CountryId = x.Key,
                TotalCount = x.Count()
            }).ToList();
            int totalCount = info.Sum(x => x.TotalCount);
            foreach(var i in info)
            {
                if (i.CountryId == null)
                    i.CountryId = Constants.DefaultRegionId;
                var r = CacheManager.GetRegionById(i.CountryId.Value, LanguageId);
                i.CountryCode = r?.IsoCode;
                i.CountryName = r?.Name;
                i.Percent = Math.Round(i.TotalCount * 100 / (double)totalCount, 2);
            }

            return info;
        }

        public List<CountryClientsInfo> GetTopVisitorCountries(FilterDashboard filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewDashboard
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClientByCategory,
                ObjectTypeId = ObjectTypes.ClientCategory
            });

            var partnerIds = filter.PartnerId.HasValue ? new List<int> { filter.PartnerId.Value } : new List<int>();
            if (!partnerAccess.HaveAccessForAllObjects)
            {
                if (filter.PartnerId != null && !partnerAccess.AccessibleIntegerObjects.Contains(filter.PartnerId.Value))
                    throw CreateException(LanguageId, Errors.DontHavePermission);

                if (!partnerIds.Any())
                {
                    if (partnerAccess.AccessibleIntegerObjects.Any())
                        partnerIds = partnerAccess.AccessibleIntegerObjects.ToList();
                    else
                        partnerIds = new List<int> { -1 };
                }
            }
            var q = Dwh.fn_ClientSession().Where(x => x.StartTime >= filter.FromDate && x.StartTime < filter.ToDate && x.ProductId == (int)Constants.PlatformProductId);
            if (partnerIds.Any())
                q = q.Where(x => partnerIds.Contains(x.PartnerId));

            var info = q.GroupBy(x => x.Country).Select(x => new CountryClientsInfo
            {
                CountryCode = x.Key,
                TotalCount = x.Count()
            }).ToList();
            int totalCount = info.Sum(x => x.TotalCount);
            foreach (var i in info)
            {
                if (string.IsNullOrEmpty(i.CountryCode))
                {
                    i.CountryId = Constants.DefaultRegionId;
                }
                else
                {
                    var r = CacheManager.GetRegionByCountryCode(i.CountryCode, LanguageId);
                    i.CountryId = r.Id;
                }
                
                i.CountryName = CacheManager.GetRegionById(i.CountryId.Value, LanguageId)?.Name;
                i.Percent = Math.Round(i.TotalCount * 100 / (double)totalCount, 2);
            }

            return info;
        }

        public BetsInfo GetBetsInfoForDashboard(FilterDashboard filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewDashboard
            });

            var partnerIds = filter.PartnerId.HasValue ? new List<int> { filter.PartnerId.Value } : new List<int>();
            if (!partnerAccess.HaveAccessForAllObjects)
            {
                if (filter.PartnerId != null && !partnerAccess.AccessibleIntegerObjects.Contains(filter.PartnerId.Value))
                    throw CreateException(LanguageId, Errors.DontHavePermission);

                if (!partnerIds.Any())
                {
                    if (partnerAccess.AccessibleIntegerObjects.Any())
                        partnerIds = partnerAccess.AccessibleIntegerObjects.ToList();
                    else
                        partnerIds = new List<int> { -1 };
                }
            }

            var q = Dwh.Gtd_Dashboard_Info.Where(x => x.Date >= filter.FromDay && x.Date < filter.ToDay);
            if (partnerIds.Any())
                q = q.Where(x => partnerIds.Contains(x.PartnerId));
            var info = q.GroupBy(x => 1).Select(b => new BetsInfo
            {
                TotalBetsCount = b.Sum(x => x.BetsCount),
                TotalBetsCountFromWebSite = b.Sum(x => x.DesktopBetsCount),
                TotalBetsCountFromMobile = b.Sum(x => x.MobileBetsCount),
                TotalBetsCountFromTablet = b.Sum(x => x.TabletBetsCount),

                TotalBetsAmount = b.Sum(x => x.BetAmount),
                TotalBetsFromWebSite = b.Sum(x => x.DesktopBetAmount),
                TotalBetsFromMobile = b.Sum(x => x.MobileBetAmount),
                TotalBetsFromTablet = b.Sum(x => x.TabletBetAmount),

                TotalGGR = b.Sum(x => x.GGR),
                TotalGGRFromWebSite = b.Sum(x => x.DesktopGGR),
                TotalGGRFromMobile = b.Sum(x => x.MobileGGR),
                TotalGGRFromTablet = b.Sum(x => x.TabletGGR),

                TotalNGR = b.Sum(x => x.NGR),
                TotalNGRFromWebSite = b.Sum(x => x.DesktopNGR),
                TotalNGRFromMobile = b.Sum(x => x.MobileNGR),
                TotalNGRFromTablet = b.Sum(x => x.TabletNGR),

                TotalPlayersCount = b.Sum(x => x.PlayersCount),
                TotalPlayersCountFromWebSite = b.Sum(x => x.DesktopPlayersCount),
                TotalPlayersCountFromMobile = b.Sum(x => x.MobilePlayersCount),
                TotalPlayersCountFromTablet = b.Sum(x => x.TabletPlayersCount)
            }).FirstOrDefault();
            if (info == null)
                return new BetsInfo();

            info.TotalBetsAmount = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, info.TotalBetsAmount);
            info.TotalBetsFromWebSite = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, info.TotalBetsFromWebSite);
            info.TotalBetsFromMobile = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, info.TotalBetsFromMobile);
            info.TotalBetsFromTablet = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, info.TotalBetsFromTablet);
            info.TotalGGR = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, info.TotalGGR);
            info.TotalGGRFromWebSite = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, info.TotalGGRFromWebSite);
            info.TotalGGRFromMobile = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, info.TotalGGRFromMobile);
            info.TotalGGRFromTablet = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, info.TotalGGRFromTablet);
            info.TotalNGR = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, info.TotalNGR);
            info.TotalNGRFromWebSite = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, info.TotalNGRFromWebSite);
            info.TotalNGRFromMobile = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, info.TotalNGRFromMobile);
            info.TotalNGRFromTablet = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, info.TotalNGRFromTablet);

            info.DailyInfo = q.GroupBy(x => x.Date).Select(b => new BetsDailyInfo
            {
                LongDate = b.Key,
                TotalBetsCount = b.Sum(x => x.BetsCount),
                TotalBetsCountFromWebSite = b.Sum(x => x.DesktopBetsCount),
                TotalBetsCountFromMobile = b.Sum(x => x.MobileBetsCount),
                TotalBetsCountFromTablet = b.Sum(x => x.TabletBetsCount),

                TotalBetsAmount = b.Sum(x => x.BetAmount),
                TotalBetsFromWebSite = b.Sum(x => x.DesktopBetAmount),
                TotalBetsFromMobile = b.Sum(x => x.MobileBetAmount),
                TotalBetsFromTablet = b.Sum(x => x.TabletBetAmount),

                TotalGGR = b.Sum(x => x.GGR),
                TotalGGRFromWebSite = b.Sum(x => x.DesktopGGR),
                TotalGGRFromMobile = b.Sum(x => x.MobileGGR),
                TotalGGRFromTablet = b.Sum(x => x.TabletGGR),

                TotalNGR = b.Sum(x => x.NGR),
                TotalNGRFromWebSite = b.Sum(x => x.DesktopNGR),
                TotalNGRFromMobile = b.Sum(x => x.MobileNGR),
                TotalNGRFromTablet = b.Sum(x => x.TabletNGR),

                TotalPlayersCount = b.Sum(x => x.PlayersCount),
                TotalPlayersCountFromWebSite = b.Sum(x => x.DesktopPlayersCount),
                TotalPlayersCountFromMobile = b.Sum(x => x.MobilePlayersCount),
                TotalPlayersCountFromTablet = b.Sum(x => x.TabletPlayersCount)
            }).ToList();
            var totalDays = (filter.ToDate - filter.FromDate).TotalDays;
            for (int i = 0; i < totalDays; i++)
            {
                var date = filter.FromDate.AddDays(i);
                var day = (long)date.Year * 10000 + (long)date.Month * 100 + (long)date.Day;
                if (!info.DailyInfo.Any(x => x.LongDate == day))
                {
                    info.DailyInfo.Add(new BetsDailyInfo { LongDate = day });
                }
            }
            info.DailyInfo = info.DailyInfo.OrderBy(x => x.LongDate).ToList();
            foreach (var di in info.DailyInfo)
            {
                di.Date = new DateTime((int)(di.LongDate / 10000), (int)((di.LongDate % 10000) / 100), (int)(di.LongDate % 100));
                di.TotalBetsAmount = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.TotalBetsAmount);
                di.TotalBetsFromWebSite = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.TotalBetsFromWebSite);
                di.TotalBetsFromMobile = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.TotalBetsFromMobile);
                di.TotalBetsFromTablet = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.TotalBetsFromTablet);
                di.TotalGGR = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.TotalGGR);
                di.TotalGGRFromWebSite = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.TotalGGRFromWebSite);
                di.TotalGGRFromMobile = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.TotalGGRFromMobile);
                di.TotalGGRFromTablet = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.TotalGGRFromTablet);
                di.TotalNGR = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.TotalNGR);
                di.TotalNGRFromWebSite = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.TotalNGRFromWebSite);
                di.TotalNGRFromMobile = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.TotalNGRFromMobile);
                di.TotalNGRFromTablet = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.TotalNGRFromTablet);
            }

            return info;
        }

        public ProvidersBetsInfo GetProviderBetsForDashboard(FilterDashboard filter)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewDashboard
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            var partnerIds = filter.PartnerId.HasValue ? new List<int> { filter.PartnerId.Value } : new List<int>();
            if (!partnerAccess.HaveAccessForAllObjects)
            {
                if (filter.PartnerId != null && !partnerAccess.AccessibleIntegerObjects.Contains(filter.PartnerId.Value))
                    throw CreateException(LanguageId, Errors.DontHavePermission);

                if (!partnerIds.Any())
                {
                    if (partnerAccess.AccessibleIntegerObjects.Any())
                        partnerIds = partnerAccess.AccessibleIntegerObjects.ToList();
                    else
                        partnerIds = new List<int> { -1 };
                }
            }

            var q = Dwh.Gtd_Provider_Bets.Where(x => x.Date >= filter.FromDay && x.Date < filter.ToDay);
            if (partnerIds.Any())
                q = q.Where(x => partnerIds.Contains(x.PartnerId));

            
            
            var dailyInfo = q.GroupBy(x => new { x.GameProviderId, x.SubProviderId, x.Date }).Select(x => new ProviderDailyInfo
            {
                LongDate = x.Key.Date,
                GameProviderId = x.Key.GameProviderId,
                SubProviderId = x.Key.SubProviderId,
                TotalBetsAmount = x.Sum(y => y.BetAmount),
                TotalWinsAmount = x.Sum(y => y.WinAmount),
                TotalGGR = x.Sum(y => y.GGR),
                TotalNGR = x.Sum(y => y.NGR),
                TotalBetsCount = x.Sum(y => y.BetsCount),
                TotalPlayersCount = x.Sum(y => y.PlayersCount),
            }).ToList();

            var info = dailyInfo.GroupBy(x => new { x.GameProviderId, x.SubProviderId }).Select(x => new ProviderBetsInfo
            {
                GameProviderId = x.Key.GameProviderId,
                SubProviderId = x.Key.SubProviderId,
                TotalBetsAmount = x.Sum(y => y.TotalBetsAmount),
                TotalWinsAmount = x.Sum(y => y.TotalWinsAmount),
                TotalGGR = x.Sum(y => y.TotalGGR),
                TotalNGR = x.Sum(y => y.TotalNGR),
                TotalBetsCount = x.Sum(y => y.TotalBetsCount),
                TotalPlayersCount = x.Sum(y => y.TotalPlayersCount)
            }).ToList();

            var totalDays = (filter.ToDate - filter.FromDate).TotalDays;
            foreach (var inf in info)
            {
                for (int i = 0; i < totalDays; i++)
                {
                    var date = filter.FromDate.AddDays(i);
                    var day = (long)date.Year * 10000 + (long)date.Month * 100 + (long)date.Day;
                    if (!dailyInfo.Any(x => x.GameProviderId == inf.GameProviderId && x.SubProviderId == inf.SubProviderId && x.LongDate == day))
                    {
                        dailyInfo.Add(new ProviderDailyInfo { GameProviderId = inf.GameProviderId, SubProviderId = inf.SubProviderId, LongDate = day });
                    }
                }
            }

            foreach (var i in dailyInfo)
            {
                i.Date = new DateTime((int)(i.LongDate / 10000), (int)((i.LongDate % 10000) / 100), (int)(i.LongDate % 100));
                i.GameProviderName = CacheManager.GetGameProviderById(i.GameProviderId).Name;
                i.SubProviderName = CacheManager.GetGameProviderById(i.SubProviderId).Name;
                i.TotalBetsAmount = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, i.TotalBetsAmount);
                i.TotalWinsAmount = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, i.TotalWinsAmount);
                i.TotalGGR = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, i.TotalGGR);
                i.TotalNGR = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, i.TotalNGR);
            }

            foreach (var i in info)
            {
                i.GameProviderName = CacheManager.GetGameProviderById(i.GameProviderId).Name;
                i.SubProviderName = CacheManager.GetGameProviderById(i.SubProviderId).Name;
                i.TotalBetsAmount = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, i.TotalBetsAmount);
                i.TotalWinsAmount = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, i.TotalWinsAmount);
                i.TotalGGR = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, i.TotalGGR);
                i.TotalNGR = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, i.TotalNGR);

                i.DailyInfo = dailyInfo.Where(x => x.GameProviderId == i.GameProviderId && x.SubProviderId == i.SubProviderId).OrderBy(x => x.LongDate).ToList();
            }

            return new ProvidersBetsInfo
            {
                TotalPlayersCount = info.Sum(x => x.TotalPlayersCount),
                TotalBetsAmount = info.Sum(x => x.TotalBetsAmount),
                TotalWinsAmount = info.Sum(x => x.TotalWinsAmount),
                TotalGGR = info.Sum(x => x.TotalGGR),
                TotalNGR = info.Sum(x => x.TotalNGR),
                Bets = info
            };
        }

        public List<PaymentRequestsInfo> GetPaymentRequestsForDashboard(FilterDashboard filter, int type)
        {
            var result = new List<PaymentRequestsInfo>();
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (type == (int)PaymentRequestTypes.Deposit)
                GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewDepositsTotals
                });
            else
                GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewWithdrawalsTotals
                });
            var partnerIds = filter.PartnerId.HasValue ? new List<int> { filter.PartnerId.Value } : new List<int>();
            if (!partnerAccess.HaveAccessForAllObjects)
            {
                if (filter.PartnerId != null && !partnerAccess.AccessibleIntegerObjects.Contains(filter.PartnerId.Value))
                    throw CreateException(LanguageId, Errors.DontHavePermission);

                if (!partnerIds.Any())
                {
                    if (partnerAccess.AccessibleIntegerObjects.Any())
                        partnerIds = partnerAccess.AccessibleIntegerObjects.ToList();
                    else
                        partnerIds = new List<int> { -1 };
                }
            }

            if (type == (int)PaymentRequestTypes.Deposit)
            {
                var q = Dwh.Gtd_Deposit_Info.Where(x => x.Date >= filter.FromDay && x.Date < filter.ToDay);
                if (partnerIds.Any())
                    q = q.Where(x => partnerIds.Contains(x.PartnerId));

                result = q.GroupBy(x => x.Status).Select(x => new PaymentRequestsInfo
                {
                    Status = x.Key,
                    TotalPlayersCount = x.Sum(y => y.PlayersCount),
                    TotalAmount = x.Sum(y => y.TotalAmount),
                    DailyInfo = x.GroupBy(y => y.Date).Select(y =>
                        new PaymentDailyInfo
                        {
                            LongDate = y.Key,
                            TotalAmount = y.Sum(z => z.TotalAmount),
                            TotalRequestsCount = y.Sum(z => z.RequestsCount),
                            TotalPlayersCount = y.Sum(z => z.PlayersCount)
                        }).ToList(),
                    PaymentRequests = x.GroupBy(y => y.PaymentSystemId).Select(y =>
                        new DAL.Models.Dashboard.PaymentInfo
                        {
                            PaymentSystemId = y.Key,
                            TotalAmount = y.Sum(z => z.TotalAmount),
                            TotalRequestsCount = y.Sum(z => z.RequestsCount),
                            TotalPlayersCount = y.Sum(z => z.PlayersCount),
                            DailyInfo = y.GroupBy(z => z.Date).Select(z =>
                               new PaymentDailyInfo
                               {
                                   LongDate = z.Key,
                                   TotalAmount = z.Sum(k => k.TotalAmount),
                                   TotalRequestsCount = z.Sum(k => k.RequestsCount),
                                   TotalPlayersCount = z.Sum(k => k.PlayersCount)
                               }).ToList()
                        }).ToList()
                }).ToList();
            }
            else
            {
                var q = Dwh.Gtd_Withdraw_Info.Where(x => x.Date >= filter.FromDay && x.Date < filter.ToDay);
                if (partnerIds.Any())
                    q = q.Where(x => partnerIds.Contains(x.PartnerId));

                result = q.GroupBy(x => x.Status).Select(x => new PaymentRequestsInfo
                {
                    Status = x.Key,
                    TotalPlayersCount = x.Sum(y => y.PlayersCount),
                    TotalAmount = x.Sum(y => y.TotalAmount),
                    DailyInfo = x.GroupBy(y => y.Date).Select(y =>
                        new PaymentDailyInfo
                        {
                            LongDate = y.Key,
                            TotalAmount = y.Sum(z => z.TotalAmount),
                            TotalRequestsCount = y.Sum(z => z.RequestsCount),
                            TotalPlayersCount = y.Sum(z => z.PlayersCount)
                        }).ToList(),
                    PaymentRequests = x.GroupBy(y => y.PaymentSystemId).Select(y =>
                        new DAL.Models.Dashboard.PaymentInfo
                        {
                            PaymentSystemId = y.Key,
                            TotalAmount = y.Sum(z => z.TotalAmount),
                            TotalRequestsCount = y.Sum(z => z.RequestsCount),
                            TotalPlayersCount = y.Sum(z => z.PlayersCount),
                            DailyInfo = y.GroupBy(z => z.Date).Select(z =>
                               new PaymentDailyInfo
                               {
                                   LongDate = z.Key,
                                   TotalAmount = z.Sum(k => k.TotalAmount),
                                   TotalRequestsCount = z.Sum(k => k.RequestsCount),
                                   TotalPlayersCount = z.Sum(k => k.PlayersCount)
                               }).ToList()
                        }).ToList()
                }).ToList();
            }
            var totalDays = (filter.ToDate - filter.FromDate).TotalDays;
            foreach (var r in result)
            {
                for (int i = 0; i < totalDays; i++)
                {
                    var date = filter.FromDate.AddDays(i);
                    var day = (long)date.Year * 10000 + (long)date.Month * 100 + (long)date.Day;
                    var di = r.DailyInfo.FirstOrDefault(x => x.LongDate == day);
                    if (di == null)
                    {
                        r.DailyInfo.Add(new PaymentDailyInfo { LongDate = day, Date = new DateTime(date.Year, date.Month, date.Day) });
                    }
                    else
                    {
                        di.Date = new DateTime(date.Year, date.Month, date.Day);
                        di.TotalAmount = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.TotalAmount);
                    }
                }
                r.TotalAmount = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, r.TotalAmount);
                r.DailyInfo = r.DailyInfo.OrderBy(x => x.LongDate).ToList();
                foreach (var pr in r.PaymentRequests)
                {
                    pr.PaymentSystemName = CacheManager.GetPaymentSystemById(pr.PaymentSystemId).Name;
                    pr.TotalAmount = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, pr.TotalAmount);
                    for (int i = 0; i < totalDays; i++)
                    {
                        var date = filter.FromDate.AddDays(i);
                        var day = (long)date.Year * 10000 + (long)date.Month * 100 + (long)date.Day;
                        var di = pr.DailyInfo.FirstOrDefault(x => x.LongDate == day);
                        if (di == null)
                        {
                            pr.DailyInfo.Add(new PaymentDailyInfo { LongDate = day, Date = new DateTime(date.Year, date.Month, date.Day) });
                        }
                        else
                        {
                            di.Date = new DateTime(date.Year, date.Month, date.Day);
                            di.TotalAmount = ConvertCurrency(Constants.DefaultCurrencyId, CurrencyId, di.TotalAmount);
                        }
                    }
                    pr.DailyInfo = pr.DailyInfo.OrderBy(x => x.LongDate).ToList();
                }
            }

            return result;
        }

        public RealTimeInfo GetOnlineClients(FilterRealTime input)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            var realTimeAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewRealTime
            });

            var clientCategoryAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClientByCategory,
                ObjectTypeId = ObjectTypes.ClientCategory
            });

            input.CheckPermissionResuts = new List<CheckPermissionOutput<BllOnlineClient>>
                {
                    new CheckPermissionOutput<BllOnlineClient>
                    {
                        AccessibleObjects = realTimeAccess.AccessibleObjects,
                        HaveAccessForAllObjects = realTimeAccess.HaveAccessForAllObjects,
                        Filter = x => clientCategoryAccess.AccessibleObjects.Contains(x.Id.Value)
                    },
                    new CheckPermissionOutput<BllOnlineClient>
                    {
                        AccessibleObjects = clientCategoryAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientCategoryAccess.HaveAccessForAllObjects,
                        Filter = x => clientCategoryAccess.AccessibleObjects.Contains(x.CategoryId.Value)
                    },
                     new CheckPermissionOutput<BllOnlineClient>
                    {
                        AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter =  x => input.PartnerId.HasValue && partnerAccess.AccessibleIntegerObjects.Contains(input.PartnerId.Value)
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

        private PagedModel<DashboardClientInfo> GetClientsInfo(FilterfnClientDashboard filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = ObjectTypes.Client
            });
            var checkViewAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPlayerDashboard,
                ObjectTypeId = ObjectTypes.Partner
            });
            var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliate,
                ObjectTypeId = ObjectTypes.Affiliate
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnClientReport>>
            {
                 new CheckPermissionOutput<fnClientReport>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnClientReport>
                {
                    AccessibleObjects = checkViewAccess.AccessibleObjects,
                    HaveAccessForAllObjects = checkViewAccess.HaveAccessForAllObjects,
                    Filter = x => checkViewAccess.AccessibleObjects.Contains(x.ClientId)
                },
                new CheckPermissionOutput<fnClientReport>
                {
                    AccessibleStringObjects = affiliateAccess.AccessibleStringObjects,
                    HaveAccessForAllObjects = affiliateAccess.HaveAccessForAllObjects,
                    Filter = x => string.IsNullOrEmpty(x.AffiliateId) && affiliateAccess.AccessibleStringObjects.Contains(x.AffiliateId)
                }
            };
            var fDate = (long)filter.FromDate.Year * 10000 + (long)filter.FromDate.Month * 100 + (long)filter.FromDate.Day;
            var tDate = (long)filter.ToDate.Year * 10000 + (long)filter.ToDate.Month * 100 + (long)filter.ToDate.Day;
            filter.FieldNameToOrderBy = "ClientId";
            filter.OrderBy = true;
            var dbClientReport = filter.FilterObjects(Dwh.fn_ClientReport(fDate, tDate), true).ToList();
            var entities = new List<DashboardClientInfo>();
            dbClientReport.ForEach(x =>
            {
                var balance = CacheManager.GetClientCurrentBalance(x.ClientId);
                entities.Add(new DashboardClientInfo
                {
                    ClientId = x.ClientId,
                    UserName = x.UserName,
                    PartnerId = x.PartnerId,
                    CurrencyId = x.CurrencyId,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email,
                    AffiliatePlatformId = x.AffiliatePlatformId,
                    AffiliateId = x.AffiliateId,
                    AffiliateReferralId = x.AffiliateReferralId,
                    TotalDepositAmount = x.TotalDepositAmount ?? 0,
                    DepositsCount = x.DepositsCount ?? 0,
                    TotalWithdrawalAmount = x.TotalWithdrawalAmount ?? 0,
                    WithdrawalsCount = x.WithdrawalsCount ?? 0,
                    TotalBetAmount = x.TotalBetAmount ?? 0,
                    TotalBetsCount = x.TotalBetsCount ?? 0,
                    SportBetsCount = x.SportBetsCount ?? 0,
                    TotalWinAmount = x.TotalWinAmount ?? 0,
                    WinsCount = x.WinsCount ?? 0,
                    TotalCreditCorrection = x.TotalCreditCorrection ?? 0,
                    CreditCorrectionsCount = x.CreditCorrectionsCount ?? 0,
                    TotalDebitCorrection = x.TotalDebitCorrection ?? 0,
                    DebitCorrectionsCount = x.DebitCorrectionsCount ?? 0,
                    GGR = x.GGR ?? 0,
                    NGR = 0,
                    RealBalance = Math.Floor(balance.Balances.Where(y => y.TypeId != (int)AccountTypes.ClientCompBalance &&
                                                                    y.TypeId != (int)AccountTypes.ClientCoinBalance &&
                                                                    y.TypeId != (int)AccountTypes.ClientBonusBalance).Sum(y => y.Balance) * 100) / 100,
                    BonusBalance = Math.Floor(balance.Balances.Where(y => y.TypeId == (int)AccountTypes.ClientBonusBalance).Sum(y => y.Balance) * 100) / 100,
                    ComplementaryBalance = Math.Floor(x.ComplementaryBalance ?? 0 * 100) / 100
                });
            });

            return new PagedModel<DashboardClientInfo>
            {
                Entities = entities,
                Count = filter.SelectedObjectsCount(Dwh.fn_ClientReport(fDate, tDate)),
            };
        }

        public ClientReport GetClientsInfoList(FilterfnClientDashboard filter)
        {
            var entities = GetClientsInfo(filter);

            return new ClientReport
            {
                Clients = entities
            };
        }

        public List<DashboardClientInfo> GetTopTurnoverClients(FilterDashboard filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = ObjectTypes.Client
            });
            var checkViewAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPlayerDashboard,
                ObjectTypeId = ObjectTypes.Partner
            });
            var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliate,
                ObjectTypeId = ObjectTypes.Affiliate
            });

           
            var fDate = (long)filter.FromDate.Year * 10000 + (long)filter.FromDate.Month * 100 + (long)filter.FromDate.Day;
            var tDate = (long)filter.ToDate.Year * 10000 + (long)filter.ToDate.Month * 100 + (long)filter.ToDate.Day;

            var query = Dwh.fn_ClientReport(fDate, tDate);
            if (!partnerAccess.HaveAccessForAllObjects)
                query = query.Where(x => partnerAccess.AccessibleObjects.Contains(x.PartnerId));

            var clients = query.OrderByDescending(x => x.TotalBetAmount * x.CurrentRate).Take(5).ToList();
            return clients.Select(x => new DashboardClientInfo
            {
                ClientId = x.ClientId,
                UserName = x.UserName,
                PartnerId = x.PartnerId,
                CurrencyId = x.CurrencyId,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                AffiliatePlatformId = x.AffiliatePlatformId,
                AffiliateId = x.AffiliateId,
                AffiliateReferralId = x.AffiliateReferralId,
                TotalDepositAmount = x.TotalDepositAmount ?? 0,
                DepositsCount = x.DepositsCount ?? 0,
                TotalWithdrawalAmount = x.TotalWithdrawalAmount ?? 0,
                WithdrawalsCount = x.WithdrawalsCount ?? 0,
                TotalBetAmount = x.TotalBetAmount ?? 0,
                TotalBetsCount = x.TotalBetsCount ?? 0,
                SportBetsCount = x.SportBetsCount ?? 0,
                TotalWinAmount = x.TotalWinAmount ?? 0,
                WinsCount = x.WinsCount ?? 0,
                TotalCreditCorrection = x.TotalCreditCorrection ?? 0,
                CreditCorrectionsCount = x.CreditCorrectionsCount ?? 0,
                TotalDebitCorrection = x.TotalDebitCorrection ?? 0,
                DebitCorrectionsCount = x.DebitCorrectionsCount ?? 0,
                GGR = x.GGR ?? 0,
                NGR = 0,
                ComplementaryBalance = Math.Floor(x.ComplementaryBalance ?? 0 * 100) / 100
            }).ToList();
        }

        public List<DashboardClientInfo> GetTopProfitableClients(FilterDashboard filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = ObjectTypes.Client
            });
            var checkViewAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPlayerDashboard,
                ObjectTypeId = ObjectTypes.Partner
            });
            var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliate,
                ObjectTypeId = ObjectTypes.Affiliate
            });


            var fDate = (long)filter.FromDate.Year * 10000 + (long)filter.FromDate.Month * 100 + (long)filter.FromDate.Day;
            var tDate = (long)filter.ToDate.Year * 10000 + (long)filter.ToDate.Month * 100 + (long)filter.ToDate.Day;

            var query = Dwh.fn_ClientReport(fDate, tDate);
            if (!partnerAccess.HaveAccessForAllObjects)
                query = query.Where(x => partnerAccess.AccessibleObjects.Contains(x.PartnerId));

            var clients = query.OrderByDescending(x => x.GGR * x.CurrentRate).Take(5).ToList();
            return clients.Select(x => new DashboardClientInfo
            {
                ClientId = x.ClientId,
                UserName = x.UserName,
                PartnerId = x.PartnerId,
                CurrencyId = x.CurrencyId,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                AffiliatePlatformId = x.AffiliatePlatformId,
                AffiliateId = x.AffiliateId,
                AffiliateReferralId = x.AffiliateReferralId,
                TotalDepositAmount = x.TotalDepositAmount ?? 0,
                DepositsCount = x.DepositsCount ?? 0,
                TotalWithdrawalAmount = x.TotalWithdrawalAmount ?? 0,
                WithdrawalsCount = x.WithdrawalsCount ?? 0,
                TotalBetAmount = x.TotalBetAmount ?? 0,
                TotalBetsCount = x.TotalBetsCount ?? 0,
                SportBetsCount = x.SportBetsCount ?? 0,
                TotalWinAmount = x.TotalWinAmount ?? 0,
                WinsCount = x.WinsCount ?? 0,
                TotalCreditCorrection = x.TotalCreditCorrection ?? 0,
                CreditCorrectionsCount = x.CreditCorrectionsCount ?? 0,
                TotalDebitCorrection = x.TotalDebitCorrection ?? 0,
                DebitCorrectionsCount = x.DebitCorrectionsCount ?? 0,
                GGR = x.GGR ?? 0,
                NGR = 0,
                ComplementaryBalance = Math.Floor(x.ComplementaryBalance ?? 0 * 100) / 100
            }).ToList();
        }

        public List<DashboardClientInfo> GetTopDamagingClients(FilterDashboard filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = ObjectTypes.Client
            });
            var checkViewAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPlayerDashboard,
                ObjectTypeId = ObjectTypes.Partner
            });
            var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliate,
                ObjectTypeId = ObjectTypes.Affiliate
            });


            var fDate = (long)filter.FromDate.Year * 10000 + (long)filter.FromDate.Month * 100 + (long)filter.FromDate.Day;
            var tDate = (long)filter.ToDate.Year * 10000 + (long)filter.ToDate.Month * 100 + (long)filter.ToDate.Day;

            var query = Dwh.fn_ClientReport(fDate, tDate);
            if (!partnerAccess.HaveAccessForAllObjects)
                query = query.Where(x => partnerAccess.AccessibleObjects.Contains(x.PartnerId));

            var clients = query.OrderBy(x => x.GGR * x.CurrentRate).Take(5).ToList();
            return clients.Select(x => new DashboardClientInfo
            {
                ClientId = x.ClientId,
                UserName = x.UserName,
                PartnerId = x.PartnerId,
                CurrencyId = x.CurrencyId,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                AffiliatePlatformId = x.AffiliatePlatformId,
                AffiliateId = x.AffiliateId,
                AffiliateReferralId = x.AffiliateReferralId,
                TotalDepositAmount = x.TotalDepositAmount ?? 0,
                DepositsCount = x.DepositsCount ?? 0,
                TotalWithdrawalAmount = x.TotalWithdrawalAmount ?? 0,
                WithdrawalsCount = x.WithdrawalsCount ?? 0,
                TotalBetAmount = x.TotalBetAmount ?? 0,
                TotalBetsCount = x.TotalBetsCount ?? 0,
                SportBetsCount = x.SportBetsCount ?? 0,
                TotalWinAmount = x.TotalWinAmount ?? 0,
                WinsCount = x.WinsCount ?? 0,
                TotalCreditCorrection = x.TotalCreditCorrection ?? 0,
                CreditCorrectionsCount = x.CreditCorrectionsCount ?? 0,
                TotalDebitCorrection = x.TotalDebitCorrection ?? 0,
                DebitCorrectionsCount = x.DebitCorrectionsCount ?? 0,
                GGR = x.GGR ?? 0,
                NGR = 0,
                ComplementaryBalance = Math.Floor(x.ComplementaryBalance ?? 0 * 100) / 100
            }).ToList();
        }

        #endregion

        #region Reporting

        #region BetShop Reports

        public DataWarehouse.Models.BetShopBets GetBetshopBetsPagedModel(FilterBetShopBet filter, string currencyId, string permission, bool checkPermission)
        {
            if (checkPermission)
            {
                var viewBetShopBetAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = permission,
                    ObjectTypeId = ObjectTypes.fnBetShopBet
                });
                var viewBetShopAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewBetShop,
                    ObjectTypeId = ObjectTypes.BetShop
                });
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });
                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnBetShopBet>>
            {
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleObjects = viewBetShopBetAccess.AccessibleObjects,
                    HaveAccessForAllObjects = viewBetShopBetAccess.HaveAccessForAllObjects,
                    Filter = x => viewBetShopBetAccess.AccessibleObjects.Contains(x.ObjectId)
                },
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleObjects = viewBetShopAccess.AccessibleObjects,
                    HaveAccessForAllObjects = viewBetShopAccess.HaveAccessForAllObjects,
                    Filter = x => viewBetShopAccess.AccessibleObjects.Contains(x.BetShopId)
                },
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                }
            };
            }

            if (!string.IsNullOrEmpty(filter.FieldNameToOrderBy))
            {
                if (!filter.OrderBy.HasValue)
                    filter.OrderBy = true;
            }
            else
            {
                filter.FieldNameToOrderBy = "BetDocumentId";
                filter.OrderBy = true;
            }
            var groupedBets = (from b in filter.FilterObjectsTotals(Dwh.fn_BetShopBet())
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
            var query = filter.FilterObjects(Dwh.fn_BetShopBet(), true);
            var entries = query.ToList();

            var toCurrency = string.IsNullOrEmpty(currencyId) ? CurrencyId : currencyId;
            foreach (var e in entries)
            {
                e.BetAmount = Math.Round(ConvertCurrency(e.CurrencyId, toCurrency, e.BetAmount), 2);
                e.WinAmount = Math.Round(ConvertCurrency(e.CurrencyId, toCurrency, e.WinAmount), 2);
                e.PossibleWin = Math.Round(ConvertCurrency(e.CurrencyId, toCurrency, e.PossibleWin), 2);
            }

            var result = new DataWarehouse.Models.BetShopBets
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
                Dwh.fn_BetShopBet()
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
                ObjectTypeId = ObjectTypes.Partner
            });

            var betShopAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShop,
                ObjectTypeId = ObjectTypes.BetShop
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnReportByBetShopOperation>>
            {
                new CheckPermissionOutput<fnReportByBetShopOperation>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                },
                 new CheckPermissionOutput<fnReportByBetShopOperation>
                {
                    AccessibleObjects = betShopAccess.AccessibleObjects,
                    HaveAccessForAllObjects = betShopAccess.HaveAccessForAllObjects,
                    Filter = x => betShopAccess.AccessibleObjects.Contains(x.Id)
                }
            };
            var result = filter.FilterObjects(Db.fn_ReportByBetShopOperation(filter.FromDate, filter.ToDate), d => d.OrderByDescending(x => x.Id)).ToList();
            return result;
        }

        public fnBetShopBet GetBetByBarcode(int cashDeskId, long barcode)
        {
            var result = new fnBetShopBet();
            var bc = Db.BetShopTickets.OrderByDescending(x => x.Id).FirstOrDefault(x => x.BarCode == barcode);
            if(bc == null)
                return null;
            var documents = Db.Documents.Include(x => x.Product).Include(x => x.CashDesk).Where(x => x.Id == bc.DocumentId || x.ParentId == bc.DocumentId).ToList();
            var bet = documents.FirstOrDefault(x => x.OperationTypeId == (int)OperationTypes.Bet);
            if (bet == null)
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
                ObjectTypeId = ObjectTypes.BetShop
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnBetShopBet>>
            {
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleObjects = viewBetShopAccess.AccessibleObjects,
                    HaveAccessForAllObjects = viewBetShopAccess.HaveAccessForAllObjects,
                    Filter = x => viewBetShopAccess.AccessibleObjects.Contains(x.BetShopId)
                },
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                }
            };
            var entities =
                filter.FilterObjects(Dwh.fn_BetShopBet(), false)
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
            return new BetShops
            {
                Entities = entities,
                TotalBetAmount = entities.Sum(x => x.BetAmount),
                TotalWinAmount = entities.Sum(x => x.WinAmount),
                TotalProfit = entities.Sum(x => x.BetAmount - x.WinAmount)
            };
        }

        public BetShopGames GetReportByBetShopGames(FilterBetShopBet filter)
        {
            CheckPermission(Constants.Permissions.ViewBetShopsReport);

            var viewBetShopAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShop,
                ObjectTypeId = ObjectTypes.BetShop
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnBetShopBet>>
            {
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleObjects = viewBetShopAccess.AccessibleObjects,
                    HaveAccessForAllObjects = viewBetShopAccess.HaveAccessForAllObjects,
                    Filter = x => viewBetShopAccess.AccessibleObjects.Contains(x.BetShopId)
                },
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                }
            };

            var entities = filter.FilterObjects(Dwh.fn_BetShopBet(), false)
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
                                CurrencyId = x.Key.BetShopCurrencyId,
                                OriginalBetAmount = x.Sum(y => y.BetAmount),
                                OriginalWinAmount = x.Sum(y => y.WinAmount)
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
                TotalOriginalBetAmount = entities.Sum(x => x.BetAmount),
                TotalOriginalWinAmount = entities.Sum(x => x.WinAmount),
                TotalBetCount = entities.Sum(x => x.Count),
                Entities = entities
            };
            return result;
        }

        #endregion

        #region InternetReports

        public DataWarehouse.Models.InternetBetsReport GetInternetBetsPagedModel(FilterInternetBet filter, string currencyId, bool checkPermission)
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
                    ObjectTypeId = ObjectTypes.fnInternetBet
                });
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });
                var clientCategoryAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClientByCategory,
                    ObjectTypeId = ObjectTypes.ClientCategory
                });
                var clientAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = ObjectTypes.Client
                });
                var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliate,
                    ObjectTypeId = ObjectTypes.Affiliate
                });

                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnInternetBet>>
                {
                    new CheckPermissionOutput<fnInternetBet>
                    {
                        AccessibleObjects = internetBetAccess.AccessibleObjects,
                        HaveAccessForAllObjects = internetBetAccess.HaveAccessForAllObjects,
                        Filter = x => internetBetAccess.AccessibleObjects.Contains(x.ObjectId)
                    },
                    new CheckPermissionOutput<fnInternetBet>
                    {
                        AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                    },
                    new CheckPermissionOutput<fnInternetBet>
                    {
                        AccessibleObjects = clientCategoryAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientCategoryAccess.HaveAccessForAllObjects,
                        Filter = x => clientCategoryAccess.AccessibleObjects.Contains(x.ClientCategoryId)
                    },
                    new CheckPermissionOutput<fnInternetBet>
                    {
                        AccessibleObjects = clientAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                        Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId)
                    },
                    new CheckPermissionOutput<fnInternetBet>
                    {
                        AccessibleStringObjects = affiliateAccess.AccessibleStringObjects,
                        HaveAccessForAllObjects = affiliateAccess.HaveAccessForAllObjects,
                        Filter = x => !string.IsNullOrEmpty(x.AffiliateId) && affiliateAccess.AccessibleStringObjects.Contains(x.AffiliateId)
                    }
                };
            }
            else
                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnInternetBet>>();

            if (filter.ClientId != null)
            {
                if (((filter.ToDate ?? DateTime.UtcNow) - filter.FromDate).TotalDays > 366)
                    throw CreateException(Constants.DefaultLanguageId, Constants.Errors.InvalidDataRange);
            }
            else
            {
                if (((filter.ToDate ?? DateTime.UtcNow) - filter.FromDate).TotalDays > 50)
                    throw CreateException(Constants.DefaultLanguageId, Constants.Errors.InvalidDataRange);
            }
            bool orderByDate = false;
            if (filter.OrderBy.HasValue)
            {
                if (!string.IsNullOrEmpty(filter.FieldNameToOrderBy))
                {
                    if (!filter.OrderBy.Value)
                        orderByDate = true;
                }
                else
                    filter.FieldNameToOrderBy = "BetDocumentId";
            }
            else
            {
                filter.FieldNameToOrderBy = "BetDocumentId";
                filter.OrderBy = true;
            }
            var query = Dwh.fn_InternetBet();
            filter.CreateQuery(ref query, false);
            var totalBets = query.GroupBy(x => x.CurrencyId).Select(x => new TotalValues
            {
                CurrencyId = x.Key,
                TotalBetsAmount = x.Sum(b => b.BetAmount),
                TotalBetsCount = x.Sum(b => 1),
                TotalWinsAmount = x.Sum(b => b.WinAmount),
                TotalPossibleWinsAmount = x.Sum(b => b.PossibleWin)
            }).ToList();
            query = Dwh.fn_InternetBet();
            filter.CreateQuery(ref query, true, orderByDate);
            var entries = query.ToList();
            var convertCurrency = !string.IsNullOrEmpty(currencyId) ? currencyId : CurrencyId;
            foreach (var entry in entries)
            {
                entry.OriginalBetAmount = Math.Round(entry.BetAmount, 2);
                entry.OriginalWinAmount = Math.Round(entry.WinAmount, 2);
                entry.OriginalBonusAmount = Math.Round(entry.BonusAmount ?? 0, 2);
                entry.OriginalBonusWinAmount = Math.Round(entry.BonusWinAmount ?? 0, 2);
                entry.BetAmount = Math.Round(ConvertCurrency(entry.CurrencyId, convertCurrency, entry.BetAmount), 2);
                entry.WinAmount = Math.Round(ConvertCurrency(entry.CurrencyId, convertCurrency, entry.WinAmount), 2);
                entry.BonusAmount = Math.Round(ConvertCurrency(entry.CurrencyId, convertCurrency, entry.BonusAmount ?? 0), 2);
                entry.BonusWinAmount = Math.Round(ConvertCurrency(entry.CurrencyId, convertCurrency, entry.BonusWinAmount ?? 0), 2);
                entry.PossibleWin = Math.Round(ConvertCurrency(entry.CurrencyId, convertCurrency, entry.PossibleWin ?? 0), 2);

            }

            var response = new DataWarehouse.Models.InternetBetsReport
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

        public DataWarehouse.Models.InternetBetsReport GetBetsForWebSite(FilterWebSiteBet filter)
        {
            var client = CacheManager.GetClientById(filter.ClientId);
            if (client == null)
                throw CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            var winDisplayType = (int)WinDisplayTypes.Win;
            var partnerWinDisplayType = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.WinDisplayType);
            if (!string.IsNullOrEmpty(partnerWinDisplayType) && int.TryParse(partnerWinDisplayType, out int configValue) && Enum.IsDefined(typeof(WinDisplayTypes), configValue))
                winDisplayType = Convert.ToInt32(partnerWinDisplayType);
            var totalBets = (from ib in filter.FilterObjects(Dwh.fn_InternetBet(), false)
                             group ib by ib.ClientId into bets
                             select new
                             {
                                 TotalBetsAmount = bets.Sum(b => b.BetAmount),
                                 TotalBetsCount = bets.Count(),
                                 TotalWinsAmount = bets.Sum(b => b.WinAmount),
                                 TotalProfit = bets.Sum(b => b.BetAmount - b.WinAmount),
                                 TotalPossibleWinsAmount = bets.Sum(b => b.PossibleWin)
                             }).FirstOrDefault();

            filter.FieldNameToOrderBy = "BetDocumentId";
            filter.OrderBy = true;
            var entries = filter.FilterObjects(Dwh.fn_InternetBet(), true).ToList();
            foreach (var entry in entries)
            {
                entry.BetAmount = Math.Round(entry.BetAmount, 2);
                entry.WinAmount = winDisplayType == (int)WinDisplayTypes.Win ? Math.Round(entry.WinAmount, 2) : Math.Round(entry.WinAmount - entry.BetAmount, 2);
                entry.PossibleWin = winDisplayType == (int)WinDisplayTypes.Win || !entry.PossibleWin.HasValue || entry.PossibleWin == 0 ?
                                    Math.Round(entry.PossibleWin ?? 0, 2) : Math.Round(entry.PossibleWin ?? 0 - entry.BetAmount, 2);
            }

            var response = new DataWarehouse.Models.InternetBetsReport
            {
                Entities = entries,
                Count = totalBets == null ? 0 : totalBets.TotalBetsCount,
                TotalBetAmount = totalBets == null ? 0 : totalBets.TotalBetsAmount,
                TotalWinAmount = totalBets == null ? 0 : (winDisplayType == (int)WinDisplayTypes.Win ? totalBets.TotalWinsAmount : (totalBets.TotalWinsAmount - totalBets.TotalBetsAmount)),
                TotalPossibleWinAmount = totalBets == null ? 0 : (winDisplayType == (int)WinDisplayTypes.Win ? totalBets.TotalPossibleWinsAmount : (totalBets.TotalPossibleWinsAmount - totalBets.TotalBetsAmount)),
            };
            response.TotalGGR = response.TotalBetAmount - response.TotalWinAmount;
            return response;
        }

        public InternetBetsByClientReport GetInternetBetsByClientPagedModel(FilterInternetBet filter)
        {
            /*var internetBetAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewInternetBets,
                ObjectTypeId = ObjectTypes.fnInternetBet
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            var clientCategoryAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClientByCategory,
                ObjectTypeId = ObjectTypes.ClientCategory
            });
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = ObjectTypes.Client
            });
            var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = ObjectTypes.AffiliateReferral
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnInternetBet>>
            {
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = internetBetAccess.AccessibleObjects,
                    HaveAccessForAllObjects = internetBetAccess.HaveAccessForAllObjects,
                    Filter = x => internetBetAccess.AccessibleObjects.Contains(x.ObjectId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = clientCategoryAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientCategoryAccess.HaveAccessForAllObjects,
                    Filter = x => clientCategoryAccess.AccessibleObjects.Contains(x.ClientCategoryId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = affiliateReferralAccess.AccessibleObjects,
                    HaveAccessForAllObjects = affiliateReferralAccess.HaveAccessForAllObjects,
                    Filter = x => x.AffiliateReferralId.HasValue && affiliateReferralAccess.AccessibleObjects.Contains(x.AffiliateReferralId.Value)
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

            var result = Dwh.fn_InternetBet().Where(x => x.ClientId == clientId && x.GameProviderId == providerId);
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
                ObjectTypeId = ObjectTypes.fnInternetBet
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            /*var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliate,
                ObjectTypeId = ObjectTypes.Affiliate
            });*/
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnInternetGame>>
            {
                new CheckPermissionOutput<fnInternetGame>
                {
                    AccessibleObjects = internetBetAccess.AccessibleObjects,
                    HaveAccessForAllObjects = internetBetAccess.HaveAccessForAllObjects,
                    Filter = x => internetBetAccess.AccessibleObjects.Contains(x.ObjectId)
                },
                new CheckPermissionOutput<fnInternetGame>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                },
                /*new CheckPermissionOutput<fnInternetGame>
                {
                    AccessibleObjects = affiliateAccess.AccessibleObjects,
                    HaveAccessForAllObjects = affiliateAccess.HaveAccessForAllObjects,
                    Filter = x => string.IsNullOrEmpty(x.AffiliateId) && affiliateAccess.AccessibleStringObjects.Contains(x.AffiliateId)
                }*/
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
            var objects = filter.FilterObjects(Dwh.fn_InternetGame(fDate, tDate), false).ToList();

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
                    ProviderName =  CacheManager.GetGameProviderById(e.GameProviderId.Value).Name,
                    SubproviderId = e.SubproviderId ?? e.GameProviderId,
                    SubproviderName = CacheManager.GetGameProviderById(e.SubproviderId ?? e.GameProviderId.Value).Name,
                    SupplierPercent = pps ?? 0,
                    SupplierFee = (ba - wa) * (pps ?? 0) / 100,
                    OriginalBetAmount = e.BetAmount ?? 0,
                    OriginalWinAmount = e.WinAmount ?? 0
                });
            }
            var result = new InternetGames
            {
                TotalBetAmount = entities.Sum(x => x.BetAmount),
                TotalWinAmount = entities.Sum(x => x.WinAmount),
                TotalOriginalBetAmount = entities.Sum(x => x.OriginalBetAmount),
                TotalOriginalWinAmount = entities.Sum(x => x.OriginalWinAmount),
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
                ObjectTypeId = ObjectTypes.Client
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewReportByCorrection,
                ObjectTypeId = ObjectTypes.fnCorrection
            });
            var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliate,
                ObjectTypeId = ObjectTypes.Affiliate
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
                    AccessibleStringObjects = affiliateAccess.AccessibleStringObjects,
                    HaveAccessForAllObjects = affiliateAccess.HaveAccessForAllObjects,
                    Filter = x => !string.IsNullOrEmpty(x.AffiliateId) && affiliateAccess.AccessibleStringObjects.Contains(x.AffiliateId)
                },
                new CheckPermissionOutput<fnCorrection>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId.Value)
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
                ObjectTypeId = ObjectTypes.BetShop
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
                Filter = x => checkP.AccessibleObjects.Contains(x.ObjectId)
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
                ObjectTypeId = ObjectTypes.Client
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliate,
                ObjectTypeId = ObjectTypes.Affiliate
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewReportByProvider
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnReportByProvider>>
            {
                new CheckPermissionOutput<fnReportByProvider>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId ?? 0)
                },
                new CheckPermissionOutput<fnReportByProvider>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId.Value)
                },
                new CheckPermissionOutput<fnReportByProvider>
                {
                    AccessibleStringObjects = affiliateAccess.AccessibleStringObjects,
                    HaveAccessForAllObjects = affiliateAccess.HaveAccessForAllObjects,
                    Filter = x => string.IsNullOrEmpty(x.AffiliateId) && affiliateAccess.AccessibleStringObjects.Contains(x.AffiliateId)
                }
            };
            var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * 1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            var rep = filter.FilterObjects(Dwh.fn_ReportByProvider(fDate, tDate), false).ToList();
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
                ObjectTypeId = ObjectTypes.Partner
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewReportByProduct
            });

            var clientCategoryAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClientByCategory,
                ObjectTypeId = ObjectTypes.ClientCategory
            });

            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = ObjectTypes.Client
            });

            var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliate,
                ObjectTypeId = ObjectTypes.Affiliate
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnReportByProduct>>
            {
                new CheckPermissionOutput<fnReportByProduct>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnReportByProduct>
                {
                    AccessibleObjects = clientCategoryAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientCategoryAccess.HaveAccessForAllObjects,
                    Filter = x => clientCategoryAccess.AccessibleObjects.Contains(x.CategoryId)
                },
                new CheckPermissionOutput<fnReportByProduct>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId)
                },
                new CheckPermissionOutput<fnReportByProduct>
                {
                    AccessibleStringObjects = affiliateAccess.AccessibleStringObjects,
                    HaveAccessForAllObjects = affiliateAccess.HaveAccessForAllObjects,
                    Filter = x => string.IsNullOrEmpty(x.AffiliateId) && affiliateAccess.AccessibleStringObjects.Contains(x.AffiliateId)
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

                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });
                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnActionLog>>
                {
                    new CheckPermissionOutput<fnActionLog>
                    {
                        AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
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
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && !partnerAccess.AccessibleIntegerObjects.Contains(filter.PartnerId))
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
                                select new DAL.Models.Report.PaymentInfo
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
                ObjectTypeId = ObjectTypes.Client
            });

            var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliate,
                ObjectTypeId = ObjectTypes.Affiliate
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnClientSession>>
                {
                    new CheckPermissionOutput<fnClientSession>
                    {
                        AccessibleObjects = clientAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                        Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId)
                    },
                    new CheckPermissionOutput<fnClientSession>
                    {
                        AccessibleStringObjects = affiliateAccess.AccessibleStringObjects,
                        HaveAccessForAllObjects = affiliateAccess.HaveAccessForAllObjects,
                        Filter = x => !string.IsNullOrEmpty(x.AffiliateId) && affiliateAccess.AccessibleStringObjects.Contains(x.AffiliateId)
                    },
                    new CheckPermissionOutput<fnClientSession>
                    {
                        AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x =>partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
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

        #region Client

        public PagedModel<fnCorrection> GetClientCorrections(FilterCorrection filter, bool checkPermission = true)
        {
            if (checkPermission)
            {
                var clientAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = ObjectTypes.Client
                });

                var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliate,
                    ObjectTypeId = ObjectTypes.Affiliate
                });

                GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
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
                        AccessibleStringObjects = affiliateAccess.AccessibleStringObjects,
                        HaveAccessForAllObjects = affiliateAccess.HaveAccessForAllObjects,
                        Filter = x => !string.IsNullOrEmpty(x.AffiliateId) && affiliateAccess.AccessibleStringObjects.Contains(x.AffiliateId)
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
                ObjectTypeId = ObjectTypes.Client
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnReportByClientExclusion>>
                {
                    new CheckPermissionOutput<fnReportByClientExclusion>
                    {
                        AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId.Value)
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
                orderBy = QueryableUtilsHelper.OrderByFunc<fnReportByClientExclusion>(filter.FieldNameToOrderBy, filter.OrderBy.Value);
            else
                orderBy = clientExclusion => clientExclusion.OrderByDescending(x => x.ClientId);

            return new PagedModel<fnReportByClientExclusion>
            {
                Entities = filter.FilterObjects(Db.fn_ReportByClientExclusion(), orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_ReportByClientExclusion())
            };
        }

		public PagedModel<fnReportByClientGame> GetClientGamePagedModel(FilterClientGame filter, string currencyId, bool checkPermission)
		{
			var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
			{
				Permission = Constants.Permissions.ViewPartner,
				ObjectTypeId = ObjectTypes.Partner
			});

			var clientAccess = GetPermissionsToObject(new CheckPermissionInput
			{
				Permission = Constants.Permissions.ViewClient,
				ObjectTypeId = ObjectTypes.Client
			});

			GetPermissionsToObject(new CheckPermissionInput
			{
				Permission = Constants.Permissions.ViewReportByProduct
			});

			filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnReportByClientGame>>
			{
				new CheckPermissionOutput<fnReportByClientGame>
				{
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
					HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
					Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
				},
				new CheckPermissionOutput<fnReportByClientGame>
				{
					AccessibleObjects = clientAccess.AccessibleObjects,
					HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
					Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId)
				}
			};
			var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * 1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
			return new PagedModel<fnReportByClientGame>
			{
				Entities = filter.FilterObjects(Db.fn_ReportByClientGame(fDate, tDate)).ToList(),
			    Count = filter.SelectedObjectsCount(Db.fn_ReportByClientGame(fDate, tDate))
			};
		}

        public PagedModel<fnDuplicateClient> GetDuplicateClients(FilterfnDuplicateClient filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = ObjectTypes.Client
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnDuplicateClient>>
            {
                new CheckPermissionOutput<fnDuplicateClient>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnDuplicateClient>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId)
                }
            };

            var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * 1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            return new PagedModel<fnDuplicateClient>
            {
                Entities = filter.FilterObjects(Dwh.fn_DuplicateClient(fDate, tDate), false).ToList(),
                Count = filter.SelectedObjectsCount(Dwh.fn_DuplicateClient(fDate, tDate), false)
            };
        }      

        #endregion

        public PagedModel<fnDocument> GetfnDocuments(FilterfnDocument filter)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewReportByTransaction
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = ObjectTypes.Client
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnDocument>>
            {
                new CheckPermissionOutput<fnDocument>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnDocument>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId.Value)
                },
            };
            filter.FieldNameToOrderBy = "Id";
            filter.OrderBy = true;
            var entities = filter.FilterObjects(Dwh.fn_Document(), true).ToList();
            entities.ForEach(x => x.ConvertedAmount = ConvertCurrency(x.CurrencyId, CurrencyId, x.Amount));
            return new PagedModel<fnDocument>
            {
                Entities = entities,
                Count = filter.SelectedObjectsCount(Dwh.fn_Document(), false)
            };
        }
        public List<fnDocument> ExportfnDocuments(FilterfnDocument filter)
        {
            filter.TakeCount = 0;
            filter.SkipCount = 0;
            return GetfnDocuments(filter).Entities.ToList();
        }
        public PagedModel<fnReportByPaymentSystem> GetReportByPaymentSystems(FilterReportByPaymentSystem filter, int type)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPaymentRequests
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnReportByPaymentSystem>>
            {
                new CheckPermissionOutput<fnReportByPaymentSystem>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
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
            var fDate = filter.FromDate.Year * (long)100000000 + filter.FromDate.Month * 1000000 +
                filter.FromDate.Day * 10000 + filter.FromDate.Hour * 100 + filter.FromDate.Minute;
            var tDate = filter.ToDate.Year * (long)100000000 + filter.ToDate.Month * 1000000 +
                filter.ToDate.Day * 10000 + filter.ToDate.Hour * 100 + filter.ToDate.Minute;

            return new PagedModel<fnReportByPaymentSystem>
            {
                Entities = filter.FilterObjects(Db.fn_ReportByPaymentSystem(fDate, tDate, type), orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_ReportByPaymentSystem(fDate, tDate, type))
            };
        }
        public PagedModel<fnUserSession> GetUserSessions(FilterReportByUserSession filter)
        {
            var userAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewUser,
                ObjectTypeId = ObjectTypes.User
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnUserSession>>
            {
                    new CheckPermissionOutput<fnUserSession>
                    {
                        AccessibleObjects = userAccess.AccessibleObjects,
                        HaveAccessForAllObjects = userAccess.HaveAccessForAllObjects,
                        Filter = x => userAccess.AccessibleObjects.Contains(x.UserId.Value)
                    },
                    new CheckPermissionOutput<fnUserSession>
                    {
                        AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                    }
            };
            Func<IQueryable<fnUserSession>, IOrderedQueryable<fnUserSession>> orderBy;
            if (filter.OrderBy.HasValue)
            {
                if (!string.IsNullOrEmpty(filter.FieldNameToOrderBy))
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnUserSession>(filter.FieldNameToOrderBy, filter.OrderBy.Value);
                else
                    orderBy = userSessions => userSessions.OrderByDescending(x => x.Id);
            }
            else
                orderBy = userSessions => userSessions.OrderByDescending(x => x.Id);


            return new PagedModel<fnUserSession>
            {
                Entities = filter.FilterObjects(Db.fn_UserSession(), orderBy),
                Count = filter.SelectedObjectsCount(Db.fn_UserSession())
            };
        }

        public PagedModel<fnReportByUserTransaction> GetReportByUserTransactions(FilterReportByUserTransaction filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var userAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewUser,
                ObjectTypeId = ObjectTypes.User
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
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
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
        public List<fnReportByPartner> GetReportByPartners(FilterReportByPartner filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewReportByPartner
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnReportByPartner>>
            {
                new CheckPermissionOutput<fnReportByPartner>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                }
            };
            Func<IQueryable<fnReportByPartner>, IOrderedQueryable<fnReportByPartner>> orderBy;

            if (!string.IsNullOrEmpty(filter.FieldNameToOrderBy))
            {
                if (!filter.OrderBy.HasValue)
                    filter.OrderBy = true;
            }
            else
            {
                filter.FieldNameToOrderBy = "PartnerId";
                filter.OrderBy = true;
            }
            var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * 1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            return filter.FilterObjects(Dwh.fn_ReportByPartner(fDate, tDate), true).ToList();
        }
        public List<SegmentByPaymentSystem> GetReportBySegment(FilterReportBySegment input)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            CheckPermission(Constants.Permissions.ViewPaymentRequests);
            CheckPermission(Constants.Permissions.ViewSegment);
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != input.PartnerId))
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
            var context = new DbContext("name=IqSoftCorePlatformLogger");
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
        public List<ShiftInfo> GetShifts(DateTime startTime, DateTime endTime, int cashDeskId, int? cashierId)
        {
            var response = new List<ShiftInfo>();
            var shifts = Db.CashDeskShifts.Include(x => x.User).Include(x => x.CashDesk.BetShop).Where(x =>
                            (x.EndTime == null || (x.StartTime >= startTime && x.EndTime <= endTime)) &&
                            (cashierId == null || x.CashierId == cashierId) && x.CashDeskId == cashDeskId).ToList();
            foreach (var s in shifts)
            {
                var info = new ShiftInfo
                {
                    Id = s.Id,
                    Number = s.Number,
                    CashierFirstName = s.User.FirstName,
                    CashierLastName = s.User.LastName,
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
                ObjectTypeId = ObjectTypes.BetShopReconing
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            var betShopAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShop,
                ObjectTypeId = ObjectTypes.BetShop
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnBetShopReconing>>
            {
                new CheckPermissionOutput<fnBetShopReconing>
                {
                    AccessibleObjects = checkP.AccessibleObjects,
                    HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                    Filter = x=> checkP.AccessibleObjects.Contains(x.ObjectId)
                },
                new CheckPermissionOutput<fnBetShopReconing>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnBetShopReconing>
                {
                    AccessibleObjects = betShopAccess.AccessibleObjects,
                    HaveAccessForAllObjects = betShopAccess.HaveAccessForAllObjects,
                    Filter = x => betShopAccess.AccessibleObjects.Contains(x.BetShopId)
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
                ObjectTypeId = ObjectTypes.BetShop
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnCashDeskTransaction>>
            {
                new CheckPermissionOutput<fnCashDeskTransaction>
                {
                    AccessibleObjects = viewCashDeskTransactionsAccess.AccessibleObjects,
                    HaveAccessForAllObjects = viewCashDeskTransactionsAccess.HaveAccessForAllObjects,
                    Filter = x => viewCashDeskTransactionsAccess.AccessibleObjects.Contains(x.ObjectId)
                },
                new CheckPermissionOutput<fnCashDeskTransaction>
                {
                    AccessibleObjects = betShopAccess.AccessibleObjects,
                    HaveAccessForAllObjects = betShopAccess.HaveAccessForAllObjects,
                    Filter = x => betShopAccess.AccessibleObjects.Contains(x.BetShopId)
                },
                new CheckPermissionOutput<fnCashDeskTransaction>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
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

        #endregion

        #region Export to excel

        public InternetGames ExportReportByInternetGames(FilterInternetGame filter)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportInternetBet
            });
            filter.SkipCount = 0;
            filter.TakeCount = 0;
            return GetReportByInternetGames(filter);
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
                ObjectTypeId = ObjectTypes.Client
            });

            var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = ObjectTypes.AffiliateReferral
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnClientSession>>
            {
                new CheckPermissionOutput<fnClientSession>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId)
                },
                new CheckPermissionOutput<fnClientSession>
                {
                    AccessibleObjects = affiliateReferralAccess.AccessibleObjects,
                    HaveAccessForAllObjects = affiliateReferralAccess.HaveAccessForAllObjects,
                    Filter = x => x.AffiliateReferralId.HasValue && affiliateReferralAccess.AccessibleObjects.Contains(x.AffiliateReferralId.Value)
                },
                new CheckPermissionOutput<fnClientSession>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x =>partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnClientSession>
                {
                    AccessibleObjects = exportAccess.AccessibleObjects,
                    HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                    Filter = x => exportAccess.AccessibleObjects.Contains(x.PartnerId)
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
                ObjectTypeId = ObjectTypes.fnInternetBet
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportInternetBet
            });

            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = ObjectTypes.Client
            });

            var affiliateReferraltAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = ObjectTypes.AffiliateReferral
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnInternetBet>>
            {
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = internetBetAccess.AccessibleObjects,
                    HaveAccessForAllObjects = internetBetAccess.HaveAccessForAllObjects,
                    Filter = x => internetBetAccess.AccessibleObjects.Contains(x.ObjectId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = exportAccess.AccessibleObjects,
                    HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                    Filter = x => exportAccess.AccessibleObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = affiliateReferraltAccess.AccessibleObjects,
                    HaveAccessForAllObjects = affiliateReferraltAccess.HaveAccessForAllObjects,
                    Filter = x => x.AffiliateReferralId.HasValue && affiliateReferraltAccess.AccessibleObjects.Contains(x.AffiliateReferralId.Value)
                }
            };

            filter.TakeCount = 0;
            filter.SkipCount = 0;

            var query = Dwh.fn_InternetBet();
            filter.FieldNameToOrderBy = "BetDocumentId";
            filter.OrderBy = true;
            filter.CreateQuery(ref query, true);

            var result = query.ToList();
            foreach (var r in result)
            {
                r.OriginalBetAmount = Math.Round(r.BetAmount, 2);
                r.OriginalWinAmount = Math.Round(r.WinAmount, 2);
                r.OriginalBonusAmount = Math.Round(r.BonusAmount ?? 0, 2);
                r.BetAmount = ConvertCurrency(r.CurrencyId, CurrencyId, r.BetAmount);
                r.WinAmount = ConvertCurrency(r.CurrencyId, CurrencyId, r.WinAmount);
                r.PossibleWin = ConvertCurrency(r.CurrencyId, CurrencyId, r.PossibleWin ?? 0);
                r.BonusAmount = Math.Round(ConvertCurrency(r.CurrencyId, CurrencyId, r.BonusAmount ?? 0), 2);
            }
            return result;
        }

        public List<fnReportByProduct> ExportProducts(FilterReportByProduct filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportProduct
            });

            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = ObjectTypes.Client
            });

            var affiliateReferraltAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = ObjectTypes.AffiliateReferral
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnReportByProduct>>
                {
                new CheckPermissionOutput<fnReportByProduct>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnReportByProduct>
                {
                    AccessibleObjects = exportAccess.AccessibleObjects,
                    HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                    Filter = x => exportAccess.AccessibleObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnReportByProduct>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId)
                },
                new CheckPermissionOutput<fnReportByProduct>
                {
                    AccessibleObjects = affiliateReferraltAccess.AccessibleObjects,
                    HaveAccessForAllObjects = affiliateReferraltAccess.HaveAccessForAllObjects,
                    Filter = x =>x.AffiliateReferralId.HasValue && affiliateReferraltAccess.AccessibleObjects.Contains(x.AffiliateReferralId.Value)
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
                ObjectTypeId = ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnBetShopBet>>
            {
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleObjects = exportAccess.AccessibleObjects,
                    HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                    Filter = x => exportAccess.AccessibleObjects.Contains(x.PartnerId)
                }
            };

            filter.TakeCount = 0;
            filter.SkipCount = 0;
            filter.FieldNameToOrderBy = "BetDocumentId";
            filter.OrderBy = true;
            var entities = filter.FilterObjects(Dwh.fn_BetShopBet(), true).Select(x => new
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
            var result = filter.FilterObjects(Dwh.fn_ReportByProvider(fDate, tDate), false)
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
                ObjectTypeId = ObjectTypes.BetShopReconing
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnBetShopReconing>>
            {
                new CheckPermissionOutput<fnBetShopReconing>
                {
                    AccessibleObjects = checkP.AccessibleObjects,
                    HaveAccessForAllObjects = checkP.HaveAccessForAllObjects,
                    Filter = x=> checkP.AccessibleObjects.Contains(x.ObjectId)
                }
            };

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            filter.CheckPermissionResuts.Add(new CheckPermissionOutput<fnBetShopReconing>
            {
                AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
            });

            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportBetShopReconings
            });

            filter.CheckPermissionResuts.Add(new CheckPermissionOutput<fnBetShopReconing>
            {
                AccessibleObjects = exportAccess.AccessibleObjects,
                HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                Filter = x => exportAccess.AccessibleObjects.Contains(x.PartnerId)
            });

            filter.TakeCount = 0;
            filter.SkipCount = 0;

            var result = filter.FilterObjects(Db.fn_BetShopReconing(), reconings => reconings.OrderBy(x => x.Id)).ToList();
            return result;
        }

        public List<InternetBetByClient> ExportInternetBetsByClient(FilterInternetBet filter)
        {
            /*var internetBetAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewInternetBets,
                ObjectTypeId = ObjectTypes.fnInternetBet
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportInternetBetsByClient
            });

            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = ObjectTypes.Client
            });

            var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = ObjectTypes.AffiliateReferral
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnInternetBet>>
            {
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = internetBetAccess.AccessibleObjects,
                    HaveAccessForAllObjects = internetBetAccess.HaveAccessForAllObjects,
                    Filter = x => internetBetAccess.AccessibleObjects.Contains(x.ObjectId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = exportAccess.AccessibleObjects,
                    HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                    Filter = x => exportAccess.AccessibleObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId)
                },
                new CheckPermissionOutput<fnInternetBet>
                {
                    AccessibleObjects = affiliateReferralAccess.AccessibleObjects,
                    HaveAccessForAllObjects = affiliateReferralAccess.HaveAccessForAllObjects,
                    Filter = x => x.AffiliateReferralId.HasValue && affiliateReferralAccess.AccessibleObjects.Contains(x.AffiliateReferralId.Value)
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
            }*/
            return new List<InternetBetByClient>();
        }
        
        public List<fnBetShopBet> ExportBetShopBets(FilterBetShopBet filter)
        {
            var betShopBetAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBetShopBets,
                ObjectTypeId = ObjectTypes.fnBetShopBet
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
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
                    Filter = x => betShopBetAccess.AccessibleObjects.Contains(x.ObjectId)
                },
                new CheckPermissionOutput<fnBetShopBet>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                }
            };

            filter.TakeCount = 0;
            filter.SkipCount = 0;
            filter.FieldNameToOrderBy = "BetDocumentId";
            filter.OrderBy = true;
            var result = filter.FilterObjects(Dwh.fn_BetShopBet(), true).ToList();
            return result;
        }

        public List<fnReportByBetShopOperation> ExportByBetShopPayments(FilterReportByBetShopPayment filter)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportByBetShopPayments
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnReportByBetShopOperation>>
            {
                new CheckPermissionOutput<fnReportByBetShopOperation>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnReportByBetShopOperation>
                {
                    AccessibleObjects = exportAccess.AccessibleObjects,
                    HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                    Filter = x => exportAccess.AccessibleObjects.Contains(x.PartnerId)
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
                ObjectTypeId = ObjectTypes.Partner
            });
            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportByUserLogs
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnActionLog>>
            {
                new CheckPermissionOutput<fnActionLog>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
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
                ObjectTypeId = ObjectTypes.Client
            });

            var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = ObjectTypes.AffiliateReferral
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
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
                    Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId.Value)
                },
                new CheckPermissionOutput<fnCorrection>
                {
                    AccessibleObjects = exportAccess.AccessibleObjects,
                    HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                    Filter = x => exportAccess.AccessibleObjects.Contains(x.PartnerId.Value)
                },
                new CheckPermissionOutput<fnCorrection>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId.Value)
                },
                new CheckPermissionOutput<fnCorrection>
                {
                    AccessibleObjects = affiliateReferralAccess.AccessibleObjects,
                    HaveAccessForAllObjects = affiliateReferralAccess.HaveAccessForAllObjects,
                    Filter = x =>x.AffiliateReferralId.HasValue && affiliateReferralAccess.AccessibleObjects.Contains(x.AffiliateReferralId.Value)
                }
            };
            return filter.FilterObjects(Db.fn_Correction(true), documents => documents.OrderByDescending(y => y.Id)).ToList();
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
                Filter = x => exportAccess.AccessibleObjects.Contains(x.ObjectId) //?? x.ObjectId is null
            });

            filter.TakeCount = 0;
            filter.SkipCount = 0;

            return filter.FilterObjects(Db.fn_Account(LanguageId)).ToList();
        }

        public List<DashboardClientInfo> ExportClientsInfoList(FilterfnClientDashboard filter)
        {
            filter.TakeCount = 0;
            filter.SkipCount = 0;
           
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportPlayersDashboard
            });
          
            return GetClientsInfo(filter).Entities.ToList();
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

        public PagedModel<fnReportByUserTransaction> ExportReportByUserTransactions(FilterReportByUserTransaction filter)
        {
            filter.TakeCount = 0;
            filter.SkipCount = 0;
            return GetReportByUserTransactions(filter);
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
            var bets = (from b in Dwh.Bets
                        join c in Dwh.Clients on b.ClientId equals c.Id
                        join u in Dwh.Users on c.UserId equals u.Id
                        where b.BetDate >= fDate && b.BetDate < tDate && u.Path.Contains("/" + agentId + "/")
                        group b by c.UserId into g
                        select new
                        {
                            UserId = g.Key,
                            TotalBetAmount = g.Sum(y => y.BetAmount),
                            TotalWinAmount = g.Sum(y => y.WinAmount),
                            TotalProfit = g.Sum(y => y.State == (int)BetDocumentStates.Uncalculated ? 0 : y.BetAmount - y.WinAmount)
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
            var profitTransactions = Db.AgentProfits.Include(x => x.User).Where(x => x.User.Path.Contains("/" + agentId + "/") &&
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
            Func<IQueryable<fnAgentTransaction>, IOrderedQueryable<fnAgentTransaction>> orderBy;
            if (filter.OrderBy.HasValue)
                orderBy = QueryableUtilsHelper.OrderByFunc<fnAgentTransaction>(filter.FieldNameToOrderBy, filter.OrderBy.Value);
            else
                orderBy = transaction => transaction.OrderByDescending(x => x.Id);
            return new PagedModel<fnAgentTransaction>
            {
                Entities = filter.FilterObjects(Db.fn_AgentTransaction(fDate, tDate, agentId), orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_AgentTransaction(fDate, tDate, agentId))
            };
        }

        public PagedModel<AgentReportItem> GetAgentsReport(FilterfnUser filter, bool checkPermission)
        {
            if (checkPermission)
                CheckPermission(Constants.Permissions.ViewReportByAgents);
            var fDate = filter.FromDate.Year * (long)1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * (long)1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            using (var userBl = new UserBll(this))
            {
                var agents = new PagedModel<AgentReportItem>();
                if (filter.ParentId == null)
                {
                    filter.IdentityId = Identity.Id;
                    filter.Types = new List<int> { (int)UserTypes.CompanyAgent };
                    var users = userBl.GetUsersPagedModel(filter, checkPermission);
                    agents.Count = users.Count;
                    agents.Entities = users.Entities.Select(x => x.ToAgentReportItem()).ToList();
                }
                else
                {
                    var users = userBl.GetSubAgents(filter.ParentId.Value, null, null, true, string.Empty);
                    agents.Count = users.Count;
                    agents.Entities = users.Select(x => x.ToAgentReportItem()).ToList();
                }

                var agentsGGRProfit = userBl.GetAgentProfit(fDate, tDate).Where(x => x.TotalProfit > 0).ToList();
                var agentsTurnoverProfit = userBl.GetAgentTurnoverProfit(fDate, tDate).Where(x => x.TotalProfit > 0).ToList();
                var corrections = userBl.GetAgentTransfers(fDate, tDate);

                foreach (var agent in agents.Entities)
                {
                    var ggrItems = agentsGGRProfit.Where(x => x.RecieverAgentId == agent.AgentId).ToList();
                    var turnoverItems = agentsTurnoverProfit.Where(x => x.RecieverAgentId == agent.AgentId).ToList();
                    var correctionItems = corrections.Where(x => (x.OperationTypeId >= (int)OperationTypes.DebitCorrectionOnUser && x.OperationTypeId <= (int)OperationTypes.CreditCorrectionOnUser &&
                        (x.Creator == agent.AgentId || x.UserId == agent.AgentId)) ||
                        (x.OperationTypeId >= (int)OperationTypes.DebitCorrectionOnClient && x.OperationTypeId <= (int)OperationTypes.CreditCorrectionOnClient &&
                        x.Creator == agent.AgentId)).ToList();
                    var dQuery = correctionItems.Where(x => (x.OperationTypeId == (int)OperationTypes.DebitCorrectionOnUser && x.UserId == agent.AgentId) ||
                        ((x.OperationTypeId == (int)OperationTypes.CreditCorrectionOnUser || 
                        x.OperationTypeId == (int)OperationTypes.CreditCorrectionOnClient) && x.Creator == agent.AgentId));
                    var wQuery = correctionItems.Where(x => ((x.OperationTypeId == (int)OperationTypes.DebitCorrectionOnUser || 
                        x.OperationTypeId == (int)OperationTypes.DebitCorrectionOnClient) && x.Creator == agent.AgentId) ||
                        (x.OperationTypeId == (int)OperationTypes.CreditCorrectionOnUser && x.UserId == agent.AgentId));

                    agent.TotalDepositCount = dQuery.Count();
                    agent.TotalDepositAmount = dQuery.Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                    agent.TotalWithdrawCount = wQuery.Count();
                    agent.TotalWithdrawAmount = wQuery.Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                    agent.TotalBetsCount = turnoverItems.Select(x => x.TotalBetsCount).DefaultIfEmpty(0).Sum();
                    agent.TotalUnsettledBetsCount = turnoverItems.Select(x => x.TotalUnsettledBetsCount).DefaultIfEmpty(0).Sum();
                    agent.TotalDeletedBetsCount = turnoverItems.Select(x => x.TotalDeletedBetsCount).DefaultIfEmpty(0).Sum();

                    agent.TotalBetAmount = ggrItems.Select(x => x.TotalBetAmount ?? 0).DefaultIfEmpty(0).Sum();
                    agent.TotalWinAmount = ggrItems.Select(x => x.TotalBetAmount ?? 0).DefaultIfEmpty(0).Sum();
                    agent.TotalProfit = agent.TotalBetAmount - agent.TotalWinAmount;
                    agent.TotalProfitPercent = agent.TotalBetAmount == 0 ? 0 : agent.TotalProfit * 100 / agent.TotalBetAmount;

                    agent.TotalGGRCommission = ggrItems.Select(x => x.TotalProfit ?? 0).DefaultIfEmpty(0).Sum();
                    agent.TotalTurnoverCommission = turnoverItems.Select(x => x.TotalProfit).DefaultIfEmpty(0).Sum();
                }

                return agents;
            }
        }

        public PagedModel<fnAffiliateTransaction> GetAffiliateTransactions(FilterfnAffiliateTransaction filter, int affiliateId)
        {
            var fDate = filter.FromDate.Year * (long)1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * (long)1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            Func<IQueryable<fnAffiliateTransaction>, IOrderedQueryable<fnAffiliateTransaction>> orderBy;
            if (filter.OrderBy.HasValue)
                orderBy = QueryableUtilsHelper.OrderByFunc<fnAffiliateTransaction>(filter.FieldNameToOrderBy, filter.OrderBy.Value);
            else
                orderBy = transaction => transaction.OrderByDescending(x => x.Id);
            return new PagedModel<fnAffiliateTransaction>
            {
                Entities = filter.FilterObjects(Db.fn_AffiliateTransaction(fDate, tDate, affiliateId), orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_AffiliateTransaction(fDate, tDate, affiliateId))
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
                ObjectTypeId = ObjectTypes.Partner
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<spObjectChangeHistory>>
            {
                new CheckPermissionOutput<spObjectChangeHistory>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                }
            };
            Func<IQueryable<spObjectChangeHistory>, IOrderedQueryable<spObjectChangeHistory>> orderBy;
            if (filter.OrderBy.HasValue)
                orderBy = QueryableUtilsHelper.OrderByFunc<spObjectChangeHistory>(filter.FieldNameToOrderBy, filter.OrderBy.Value);
            else
                orderBy = history => history.OrderByDescending(x => x.Id);
            return new PagedModel<spObjectChangeHistory>
            {
                Entities = filter.FilterObjects(Db.sp_ObjectChangeHistory(filter.FromDate, filter.ToDate, filter.ObjectTypeId).AsQueryable(), orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.sp_ObjectChangeHistory(filter.FromDate, filter.ToDate, filter.ObjectTypeId).AsQueryable())
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
                ObjectTypeId = ObjectTypes.Partner
            });
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = ObjectTypes.Client
            });

            var query = Db.ObjectChangeHistories.Join(Db.Clients, oh => oh.ObjectId, c => c.Id, (oh, c) => new ClientChangeHistoryModel
            {
                ObjectChangeHistory = oh,
                ClientItem = c
            }).Where(x => x.ObjectChangeHistory.ObjectTypeId == (int)ObjectTypes.Client &&
                          x.ObjectChangeHistory.ChangeDate >= startDate);
            if (endDate.HasValue)
                query = query.Where(x => x.ObjectChangeHistory.ChangeDate <= endDate);
            if (clientId.HasValue)
                query = query.Where(x => x.ObjectChangeHistory.ObjectId == clientId.Value);

            var checkPermissionResuts = new List<CheckPermissionOutput<ClientChangeHistoryModel>>
            {
                new CheckPermissionOutput<ClientChangeHistoryModel>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.ClientItem.PartnerId)
                },
                new CheckPermissionOutput<ClientChangeHistoryModel>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientItem.Id)
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
                FirstName = oh.ObjectChangeHistory.UserSession?.User?.FirstName,
                LastName = oh.ObjectChangeHistory.UserSession?.User?.LastName,
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
                                                                FirstName = oh.UserSession.User.FirstName,
                                                                LastName = oh.UserSession.User.LastName
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
                        var referralType = CacheManager.GetClientSettingByName(client.Id, Constants.ClientSettings.ReferralType);
                        newValue = JsonConvert.SerializeObject(client.ToClientInfo(null, (int?)referralType?.NumericValue));
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
                    case (int)ObjectTypes.ClientLimit:
                        using (var clientBll = new ClientBll(this))
                        {
                            newValue = JsonConvert.SerializeObject(clientBll.GetClientLimitSettings((int)result.ObjectId, false, true));
                        }
                        break;
                    case (int)ObjectTypes.ClientSetting:
                        var c = CacheManager.GetClientById(Convert.ToInt32(result.ObjectId));
                        using (var clientBll = new ClientBll(this))
                        {
                            var clientSetting = clientBll.GetClientSettings(c.Id, false);
                            newValue = JsonConvert.SerializeObject(clientSetting);
                        }
                        break;
                    case (int)ObjectTypes.Announcement:
                        using (var contentBll = new ContentBll(this))
                        {
                            newValue = JsonConvert.SerializeObject(contentBll.GetAnnouncementById((int)result.ObjectId));
                        }
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
                ObjectTypeId = ObjectTypes.Client
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBonuses
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnClientBonus>>
            {
                new CheckPermissionOutput<fnClientBonus>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnClientBonus>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId)
                }
            };
            Func<IQueryable<fnClientBonus>, IOrderedQueryable<fnClientBonus>> orderBy;
            if (filter.OrderBy.HasValue)
                orderBy = QueryableUtilsHelper.OrderByFunc<fnClientBonus>(filter.FieldNameToOrderBy, filter.OrderBy.Value);
            else
                orderBy = clientBonuses => clientBonuses.OrderByDescending(x => x.BonusId);
            
            var totals = (from b in filter.FilterObjects(Db.fn_ClientBonus(LanguageId))
                          group b by b.CurrencyId into bonuses
                          select new
                          {
                              CurrencyId = bonuses.Key,
                              TotalPrize = bonuses.Sum(b => b.BonusPrize),
                              TotalFinalAmount = bonuses.Sum(b => b.FinalAmount ?? 0),
                              TotalCount = bonuses.Count()
                          }).ToList();

            return new BonusReport
            {
                Entities = filter.FilterObjects(Db.fn_ClientBonus(LanguageId), orderBy).ToList(),
                Count = totals.Sum(x => x.TotalCount),
                TotalBonusPrize = totals.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalPrize)),
                TotalFinalAmount = totals.Sum(x => ConvertCurrency(x.CurrencyId, CurrencyId, x.TotalFinalAmount))
            };
        }

        public List<fnClientBonus> ExportReportByBonus(FilterReportByBonus filter)
        {
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = ObjectTypes.Client
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBonuses
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnClientBonus>>
            {
                new CheckPermissionOutput<fnClientBonus>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnClientBonus>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId)
                }
            };
            Func<IQueryable<fnClientBonus>, IOrderedQueryable<fnClientBonus>> orderBy;

            if (filter.OrderBy.HasValue)
                orderBy = QueryableUtilsHelper.OrderByFunc<fnClientBonus>(filter.FieldNameToOrderBy, filter.OrderBy.Value);
            else
                orderBy = c => c.OrderByDescending(x => x.BonusId);

            filter.TakeCount = 0;
            filter.SkipCount = 0;
            var clientBonuses = filter.FilterObjects(Db.fn_ClientBonus(LanguageId), orderBy).ToList();
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
                ObjectTypeId = ObjectTypes.Client
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBonuses
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnClientIdentity>>
            {
                new CheckPermissionOutput<fnClientIdentity>
                {
                    AccessibleIntegerObjects = partnerAccess.AccessibleIntegerObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleIntegerObjects.Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnClientIdentity>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.Contains(x.ClientId)
                }
            };
            Func<IQueryable<fnClientIdentity>, IOrderedQueryable<fnClientIdentity>> orderBy;

            if (filter.OrderBy.HasValue)
                orderBy = QueryableUtilsHelper.OrderByFunc<fnClientIdentity>(filter.FieldNameToOrderBy, filter.OrderBy.Value);
            else
                orderBy = clientIdentities => clientIdentities.OrderByDescending(x => x.ExpirationDate);
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

        public object GetAgentMemberPaymentsForDashboard(FilterDashboard filter)
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
                                            (pr.Status == (int)PaymentRequestStates.Approved || pr.Status == (int)PaymentRequestStates.ApprovedManually)
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

        public object GetAffiliateMemberPaymentsForDashboard(FilterDashboard filter)
        {
            var fDate = (long)filter.FromDate.Year * 100000000 + (long)filter.FromDate.Month * 1000000 +
                (long)filter.FromDate.Day * 10000 + (long)filter.FromDate.Hour * 100 + (long)filter.FromDate.Minute;
            var tDate = (long)filter.ToDate.Year * 100000000 + (long)filter.ToDate.Month * 1000000 +
                (long)filter.ToDate.Day * 10000 + (long)filter.ToDate.Hour * 100 + (long)filter.FromDate.Minute;

            var paymentRequestFilter = new FilterPaymentRequest
            {
                FromDate = fDate,
                ToDate = tDate,
                AffiliateId = Identity.Id,
                PartnerId = Identity.PartnerId
            };
            var partner = CacheManager.GetPartnerById(Identity.PartnerId);
            var query = paymentRequestFilter.FilterObjects(Db.PaymentRequests).Where(x => x.Status == (int)PaymentRequestStates.Approved ||
                                                                                          x.Status == (int)PaymentRequestStates.ApprovedManually);
            var firstTimeDepositors = (from c in Db.Clients
                                       join pr in Db.PaymentRequests on c.Id equals pr.ClientId
                                       where c.AffiliateReferral.AffiliateId == Identity.Id.ToString() && c.AffiliateReferral.AffiliatePlatformId == filter.PartnerId * 100 &&
                                             c.AffiliateReferral.Type == (int)AffiliateReferralTypes.InternalAffiliatePlatform &&
                                             c.FirstDepositDate >= filter.FromDate && c.FirstDepositDate < filter.ToDate &&
                                             pr.Type == (int)PaymentRequestTypes.Deposit &&
                                            (pr.Status == (int)PaymentRequestStates.Approved || pr.Status == (int)PaymentRequestStates.ApprovedManually)
                                       group pr by new { pr.ClientId, pr.Client.CurrencyId, pr.Client.Currency.CurrentRate } into grp
                                       select new
                                       {
                                           ClientId = grp.Key.ClientId,
                                           CurrencyId = grp.Key.CurrencyId,
                                           CurrencyRate = grp.Key.CurrentRate,
                                           FirstDeposit = grp.OrderBy(x => x.Id).FirstOrDefault()
                                       }).ToList();
            return new
            {
                FirstDepositAmount = firstTimeDepositors.Select(x => ConvertCurrency(x.CurrencyId, partner.CurrencyId, x.FirstDeposit.Amount)).DefaultIfEmpty(0).Sum(),
                FirstDepositCount = firstTimeDepositors.Count,
                TotalDepositAmount = ConvertCurrency(Constants.DefaultCurrencyId, partner.CurrencyId,
                    query.Where(x => x.Type == (int)PaymentRequestTypes.Deposit).Select(x => x.Amount * x.Currency.CurrentRate).DefaultIfEmpty(0).Sum()),
                TotalDepositsCount = query.Where(x => x.Type == (int)PaymentRequestTypes.Deposit).Count(),
                TotalWithdrawAmount = ConvertCurrency(Constants.DefaultCurrencyId, partner.CurrencyId,
                    query.Where(x => x.Type == (int)PaymentRequestTypes.Withdraw).Select(x => x.Amount * x.Currency.CurrentRate).DefaultIfEmpty(0).Sum()),
                TotalWithdrawsCount = query.Where(x => x.Type == (int)PaymentRequestTypes.Withdraw).Count()
            };
        }

        public object GetAgentMembersInfoForDashboard(FilterDashboard filter)
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

        public object GetAffiliateMembersInfoForDashboard(FilterDashboard filter)
        {
            var fDate = (long)filter.FromDate.Year * 1000000 + (long)filter.FromDate.Month * 10000 + (long)filter.FromDate.Day * 100 + (long)filter.FromDate.Hour;
            var tDate = (long)filter.ToDate.Year * 1000000 + (long)filter.ToDate.Month * 10000 + (long)filter.ToDate.Day * 100 + (long)filter.ToDate.Hour;

            var signUpsCount = Db.Clients.Count(c => c.CreationTime >= filter.FromDate && c.CreationTime < filter.ToDate &&
                c.AffiliateReferral.AffiliateId == Identity.Id.ToString() && c.AffiliateReferral.AffiliatePlatformId == filter.PartnerId * 100 &&
                c.AffiliateReferral.Type == (int)AffiliateReferralTypes.InternalAffiliatePlatform);

            return new
            {
                ClientsCount = signUpsCount
            };
        }

        #endregion
    }
}