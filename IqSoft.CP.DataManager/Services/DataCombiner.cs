using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.Document;
using IqSoft.CP.Common.Models.Bonus;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.DataWarehouse;
using System.Web.UI.WebControls;

namespace IqSoft.CP.DataManager.Services
{
    public class DataCombiner
    {
        public static long LastProcessedBetDocumentId = 0;
        static DataCombiner()
        {
            using (var db = new IqSoftDataWarehouseEntities())
            {
                LastProcessedBetDocumentId = Convert.ToInt64(db.Settings.First(x => x.Name == Constants.PartnerKeys.LastProcessedBetDocumentId).NumericValue.Value);
            }
        }

        public static void GroupNewBets(long lastId, ILog logger)
        {
            try
            {
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var documents = db.Documents.Where(x => x.Id > lastId).OrderBy(x => x.Id).Take(10000).ToList();
                    var newLast = GroupDocuments(documents, db);

                    if (newLast > lastId)
                    {
                        var key = db.Settings.First(x => x.Name == Constants.PartnerKeys.LastProcessedBetDocumentId);
                        key.NumericValue = newLast;
                        db.SaveChanges();
                        LastProcessedBetDocumentId = newLast;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        public static int CleanBets(ILog logger)
        {
            try
            {
                var currentTime = DateTime.UtcNow;
                var startTime = currentTime.AddDays(-40);
                var endTime = currentTime.AddHours(-2);

                var fromDate = (long)startTime.Year * 1000000 + startTime.Month * 10000 + startTime.Day * 100 + startTime.Hour;
                var toDate = (long)endTime.Year * 1000000 + endTime.Month * 10000 + endTime.Day * 100 + endTime.Hour;
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    db.Database.CommandTimeout = 300;

                    var documents = db.Documents.Where(x => x.Date > fromDate && x.Date < toDate && x.Considered == false).OrderBy(x => x.Id).Take(10000).ToList();
                    var newLast = GroupDocuments(documents, db);
                    Program.DbLogger.Info("CleanBets_Finished");
                    return documents.Count;
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
                return 0;
            }
        }

        public static void CalculateDashboardInfo(ILog logger, long date)
        {
            try
            {
                var currentTime = DateTime.UtcNow;
                var currentDate = date == 0 ? (long)currentTime.Year * 10000 + (long)currentTime.Month * 100 + (long)currentTime.Day : date;
                var currentDay = date == 0 ? new DateTime(currentTime.Year, currentTime.Month, currentTime.Day) : new DateTime((int)(date/10000), (int)((date%10000)/100), (int)(date % 100));
                var yesterday = currentDay.AddDays(-1);
                var yesterdayDate = (long)yesterday.Year * 10000 + (long)yesterday.Month * 100 + (long)yesterday.Day;
                var toDay = currentDay.AddDays(1);
                var fDate = (long)currentDay.Year * 1000000 + (long)currentDay.Month * 10000 + (long)currentDay.Day * 100 + (long)currentDay.Hour;
                var tDate = (long)toDay.Year * 1000000 + (long)toDay.Month * 10000 + (long)toDay.Day * 100 + (long)toDay.Hour;

                logger.Info("CalculateDashboardInfo_" + date + "_" + fDate + "_" + tDate);
                
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    db.Database.CommandTimeout = 300;

                    #region TotalPlayers

                    var players = db.Clients.GroupBy(x => x.PartnerId).Select(x => new { PartnerId = x.Key, Count = x.Count() }).ToList();

                    #endregion

                    #region Visitors

                    var signups = db.Clients.Where(c => c.CreationTime >= currentDay && c.CreationTime < toDay).GroupBy(x => x.PartnerId).Select(x => new { PartnerId = x.Key, Count = x.Count() }).ToList();
                    foreach (var su in signups)
                    {
                        var dayInfo = db.Gtd_Dashboard_Info.FirstOrDefault(x => x.Date == currentDate && x.PartnerId == su.PartnerId);
                        if (dayInfo == null)
                        {
                            dayInfo = new Gtd_Dashboard_Info
                            {
                                Date = currentDate,
                                PartnerId = su.PartnerId,
                            };
                            db.Gtd_Dashboard_Info.Add(dayInfo);
                            db.SaveChanges();
                        }
                        dayInfo.SignUpsCount = su.Count;
                        dayInfo.TotalPlayersCount = players.FirstOrDefault(x => x.PartnerId == su.PartnerId)?.Count ?? 0;
                    }
                    
                    #endregion

                    #region Signups

                    var visitors = (from cs in db.ClientSessions
                                    join c in db.Clients on cs.ClientId equals c.Id
                                    where cs.StartTime >= currentDay && cs.StartTime < toDay && cs.ProductId == Constants.PlatformProductId
                                    group c by c.PartnerId into g
                                    select new
                                    {
                                        PartnerId = g.Key,
                                        Count = g.Select(x => x.Id).Distinct().Count()
                                    }).ToList();
                    foreach (var v in visitors)
                    {
                        var dayInfo = db.Gtd_Dashboard_Info.FirstOrDefault(x => x.Date == currentDate && x.PartnerId == v.PartnerId);
                        if (dayInfo == null)
                        {
                            dayInfo = new Gtd_Dashboard_Info
                            {
                                Date = currentDate,
                                PartnerId = v.PartnerId,
                            };
                            db.Gtd_Dashboard_Info.Add(dayInfo);
                            db.SaveChanges();
                        }
                        dayInfo.VisitorsCount = v.Count;
                        dayInfo.TotalPlayersCount = players.FirstOrDefault(x => x.PartnerId == v.PartnerId)?.Count ?? 0;
                    }
                    #endregion

                    #region Returns
                    
                    var returns = (from cs in db.ClientSessions
                                   join c in db.Clients on cs.ClientId equals c.Id
                                   where cs.StartTime >= currentDay && cs.StartTime < toDay && cs.ProductId == Constants.PlatformProductId
                                   && c.CreationTime < currentDay
                                   group c by c.PartnerId into g
                                   select new
                                   {
                                       PartnerId = g.Key,
                                       Count = g.Select(x => x.Id).Distinct().Count()
                                   }).ToList();
                    foreach (var r in returns)
                    {
                        var dayInfo = db.Gtd_Dashboard_Info.FirstOrDefault(x => x.Date == currentDate && x.PartnerId == r.PartnerId);
                        if (dayInfo == null)
                        {
                            dayInfo = new Gtd_Dashboard_Info
                            {
                                Date = currentDate,
                                PartnerId = r.PartnerId,
                            };
                            db.Gtd_Dashboard_Info.Add(dayInfo);
                            db.SaveChanges();
                        }
                        dayInfo.ReturnsCount = r.Count;
                        dayInfo.TotalPlayersCount = players.FirstOrDefault(x => x.PartnerId == r.PartnerId)?.Count ?? 0;
                    }

                    #endregion

                    #region Bets

                    var currencies = db.Currencies.ToList().ToDictionary(x => x.Id, x => x.CurrentRate);
                    var bets = db.fn_PartnerDeviceBets(fDate, tDate).ToList();
                    var playersCount = db.fn_DeviceClientsCount(fDate, tDate).ToList();
                    var totalPlayersCount = db.fn_PartnerClientsCount(fDate, tDate).ToList();

                    var gBets = bets.GroupBy(x => x.PartnerId).Select(g => new
                    {
                        PartnerId = g.Key,
                        MaxBet = g.Max(x => x.MaxBet * currencies[x.CurrencyId]),
                        MaxWin = g.Max(x => x.MaxWin * currencies[x.CurrencyId]),
                        CashoutAmount = g.Sum(x => x.CashoutAmount * currencies[x.CurrencyId]),

                        BetAmount = g.Sum(x => x.TotalBet * currencies[x.CurrencyId]),
                        DesktopBetAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Desktop).Sum(x => x.TotalBet * currencies[x.CurrencyId]),
                        MobileBetAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Mobile).Sum(x => x.TotalBet * currencies[x.CurrencyId]),
                        TabletBetAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Tablet).Sum(x => x.TotalBet * currencies[x.CurrencyId]),
                        BetShopBetAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.BetShop).Sum(x => x.TotalBet * currencies[x.CurrencyId]),
                        TerminalBetAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Terminal).Sum(x => x.TotalBet * currencies[x.CurrencyId]),
                        ApplicationBetAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Application).Sum(x => x.TotalBet * currencies[x.CurrencyId]),

                        BonusBetAmount = g.Sum(x => x.TotalBonusBet * currencies[x.CurrencyId]),
                        DesktopBonusBetAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Desktop).Sum(x => x.TotalBonusBet * currencies[x.CurrencyId]),
                        MobileBonusBetAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Mobile).Sum(x => x.TotalBonusBet * currencies[x.CurrencyId]),
                        TabletBonusBetAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Tablet).Sum(x => x.TotalBonusBet * currencies[x.CurrencyId]),
                        BetShopBonusBetAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.BetShop).Sum(x => x.TotalBonusBet * currencies[x.CurrencyId]),
                        TerminalBonusBetAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Terminal).Sum(x => x.TotalBonusBet * currencies[x.CurrencyId]),
                        ApplicationBonusBetAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Application).Sum(x => x.TotalBonusBet * currencies[x.CurrencyId]),

                        WinAmount = g.Sum(x => x.TotalWin * currencies[x.CurrencyId]),
                        DesktopWinAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Desktop).Sum(x => x.TotalWin * currencies[x.CurrencyId]),
                        MobileWinAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Mobile).Sum(x => x.TotalWin * currencies[x.CurrencyId]),
                        TabletWinAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Tablet).Sum(x => x.TotalWin * currencies[x.CurrencyId]),
                        BetShopWinAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.BetShop).Sum(x => x.TotalWin * currencies[x.CurrencyId]),
                        TerminalWinAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Terminal).Sum(x => x.TotalWin * currencies[x.CurrencyId]),
                        ApplicationWinAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Application).Sum(x => x.TotalWin * currencies[x.CurrencyId]),

                        BonusWinAmount = g.Sum(x => x.TotalBonusWin * currencies[x.CurrencyId]),
                        DesktopBonusWinAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Desktop).Sum(x => x.TotalBonusWin * currencies[x.CurrencyId]),
                        MobileBonusWinAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Mobile).Sum(x => x.TotalBonusWin * currencies[x.CurrencyId]),
                        TabletBonusWinAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Tablet).Sum(x => x.TotalBonusWin * currencies[x.CurrencyId]),
                        BetShopBonusWinAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.BetShop).Sum(x => x.TotalBonusWin * currencies[x.CurrencyId]),
                        TerminalBonusWinAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Terminal).Sum(x => x.TotalBonusWin * currencies[x.CurrencyId]),
                        ApplicationBonusWinAmount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Application).Sum(x => x.TotalBonusWin * currencies[x.CurrencyId]),

                        BetsCount = g.Sum(x => x.Count),
                        DesktopBetsCount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Desktop).Sum(x => x.Count),
                        MobileBetsCount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Mobile).Sum(x => x.Count),
                        TabletBetsCount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Tablet).Sum(x => x.Count),
                        BetShopBetsCount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.BetShop).Sum(x => x.Count),
                        TerminalBetsCount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Terminal).Sum(x => x.Count),
                        ApplicationBetsCount = g.Where(x => x.DeviceTypeId == (int)DeviceTypes.Application).Sum(x => x.Count),

                        PlayersCount = totalPlayersCount.FirstOrDefault(x => x.PartnerId == g.Key)?.PlayersCount ?? 0,
                        DesktopPlayersCount = playersCount.Where(x => x.PartnerId == g.Key && x.DeviceTypeId == (int)DeviceTypes.Desktop).Sum(x => x.PlayersCount),
                        MobilePlayersCount = playersCount.Where(x => x.PartnerId == g.Key && x.DeviceTypeId == (int)DeviceTypes.Mobile).Sum(x => x.PlayersCount),
                        TabletPlayersCount = playersCount.Where(x => x.PartnerId == g.Key && x.DeviceTypeId == (int)DeviceTypes.Tablet).Sum(x => x.PlayersCount),
                        BetShopPlayersCount = playersCount.Where(x => x.PartnerId == g.Key && x.DeviceTypeId == (int)DeviceTypes.BetShop).Sum(x => x.PlayersCount),
                        TerminalPlayersCount = playersCount.Where(x => x.PartnerId == g.Key && x.DeviceTypeId == (int)DeviceTypes.Terminal).Sum(x => x.PlayersCount),
                        ApplicationPlayersCount = playersCount.Where(x => x.PartnerId == g.Key && x.DeviceTypeId == (int)DeviceTypes.Application).Sum(x => x.PlayersCount)
                    });


                    foreach (var gBet in gBets)
                    {
                        var dayInfo = db.Gtd_Dashboard_Info.FirstOrDefault(x => x.Date == currentDate && x.PartnerId == gBet.PartnerId);
                        if (dayInfo == null)
                        {
                            dayInfo = new Gtd_Dashboard_Info
                            {
                                Date = currentDate,
                                PartnerId = gBet.PartnerId,
                            };
                            db.Gtd_Dashboard_Info.Add(dayInfo);
                            db.SaveChanges();
                        }
                        dayInfo.MaxBet = gBet.MaxBet ?? 0;
                        dayInfo.MaxWin = gBet.MaxWin ?? 0;
                        dayInfo.CashoutAmount = gBet.CashoutAmount ?? 0;

                        dayInfo.BetAmount = gBet.BetAmount ?? 0;
                        dayInfo.DesktopBetAmount = gBet.DesktopBetAmount ?? 0;
                        dayInfo.MobileBetAmount = gBet.MobileBetAmount ?? 0;
                        dayInfo.TabletBetAmount = gBet.TabletBetAmount ?? 0;
                        dayInfo.BetShopBetAmount = gBet.BetShopBetAmount ?? 0;
                        dayInfo.TerminalBetAmount = gBet.TerminalBetAmount ?? 0;
                        dayInfo.ApplicationBetAmount = gBet.ApplicationBetAmount ?? 0;

                        dayInfo.GGR = (gBet.BetAmount - gBet.WinAmount) ?? 0;
                        dayInfo.DesktopGGR = (gBet.DesktopBetAmount - gBet.DesktopWinAmount) ?? 0;
                        dayInfo.MobileGGR = (gBet.MobileBetAmount - gBet.MobileWinAmount) ?? 0;
                        dayInfo.TabletGGR = (gBet.TabletBetAmount - gBet.TabletWinAmount) ?? 0;
                        dayInfo.BetShopGGR = (gBet.BetShopBetAmount - gBet.BetShopWinAmount) ?? 0;
                        dayInfo.TerminalGGR = (gBet.TerminalBetAmount - gBet.TerminalWinAmount) ?? 0;
                        dayInfo.ApplicationGGR = (gBet.ApplicationBetAmount - gBet.ApplicationWinAmount) ?? 0;

                        dayInfo.NGR = dayInfo.GGR + (gBet.BonusWinAmount - gBet.BonusBetAmount) ?? 0;
                        dayInfo.DesktopNGR = dayInfo.DesktopGGR + (gBet.DesktopBonusWinAmount - gBet.DesktopBonusBetAmount) ?? 0;
                        dayInfo.MobileNGR = dayInfo.MobileGGR + (gBet.MobileBonusWinAmount - gBet.MobileBonusBetAmount) ?? 0;
                        dayInfo.TabletNGR = dayInfo.TabletGGR + (gBet.TabletBonusWinAmount - gBet.TabletBonusBetAmount) ?? 0;
                        dayInfo.BetShopNGR = dayInfo.BetShopGGR + (gBet.BetShopBonusWinAmount - gBet.BetShopBonusBetAmount) ?? 0;
                        dayInfo.TerminalNGR = dayInfo.TerminalGGR + (gBet.TerminalBonusWinAmount - gBet.TerminalBonusBetAmount) ?? 0;
                        dayInfo.ApplicationNGR = dayInfo.ApplicationGGR + (gBet.ApplicationBonusWinAmount - gBet.ApplicationBonusBetAmount) ?? 0;

                        dayInfo.BetsCount = gBet.BetsCount ?? 0;
                        dayInfo.DesktopBetsCount = gBet.DesktopBetsCount ?? 0;
                        dayInfo.MobileBetsCount = gBet.MobileBetsCount ?? 0;
                        dayInfo.TabletBetsCount = gBet.TabletBetsCount ?? 0;
                        dayInfo.BetShopBetsCount = gBet.BetShopBetsCount ?? 0;
                        dayInfo.TerminalBetsCount = gBet.TerminalBetsCount ?? 0;
                        dayInfo.ApplicationBetsCount = gBet.ApplicationBetsCount ?? 0;

                        dayInfo.PlayersCount = gBet.PlayersCount;
                        dayInfo.DesktopPlayersCount = gBet.DesktopPlayersCount ?? 0;
                        dayInfo.MobilePlayersCount = gBet.MobilePlayersCount ?? 0;
                        dayInfo.TabletPlayersCount = gBet.TabletPlayersCount ?? 0;
                        dayInfo.BetShopPlayersCount = gBet.BetShopPlayersCount ?? 0;
                        dayInfo.TerminalPlayersCount = gBet.TerminalPlayersCount ?? 0;
                        dayInfo.ApplicationPlayersCount = gBet.ApplicationPlayersCount ?? 0;

                        dayInfo.TotalPlayersCount = players.FirstOrDefault(x => x.PartnerId == gBet.PartnerId)?.Count ?? 0;
                    }

                    #endregion

                    #region Bonuses

                    var bonuses = (from cb in db.ClientBonus
                                   join c in db.Clients on cb.ClientId equals c.Id
                                   join crr in db.Currencies on c.CurrencyId equals crr.Id
                                   join b in db.Bonus on cb.BonusId equals b.Id
                                   where cb.CreationTime >= currentDay && cb.CreationTime < toDay && b.Type != (int)BonusTypes.CampaignFreeSpin
                                   group cb by new { c.PartnerId, c.CurrencyId, crr.CurrentRate } into g
                                   select new
                                   {
                                       PartnerId = g.Key.PartnerId,
                                       CurrencyId = g.Key.CurrencyId,
                                       CurrentRate = g.Key.CurrentRate,
                                       BonusPrize = g.Sum(x => x.BonusPrize)
                                   }).ToList();
                    var gBonuses = bonuses.GroupBy(x => x.PartnerId).Select(g => new
                    {
                        PartnerId = g.Key,
                        BonusAmount = g.Sum(x => x.BonusPrize * x.CurrentRate)
                    });

                    foreach (var gBonus in gBonuses)
                    {
                        var dayInfo = db.Gtd_Dashboard_Info.FirstOrDefault(x => x.Date == currentDate && x.PartnerId == gBonus.PartnerId);
                        if (dayInfo == null)
                        {
                            dayInfo = new Gtd_Dashboard_Info
                            {
                                Date = currentDate,
                                PartnerId = gBonus.PartnerId,
                            };
                            db.Gtd_Dashboard_Info.Add(dayInfo);
                            db.SaveChanges();
                        }
                        dayInfo.BonusAmount = gBonus.BonusAmount;
                        dayInfo.TotalPlayersCount = players.FirstOrDefault(x => x.PartnerId == gBonus.PartnerId)?.Count ?? 0;
                    }

                    #endregion

                    db.SaveChanges();
                    if (date == 0 && (currentTime - currentDay).TotalMinutes < 10)
                    {
                        AddJobTrigger("CalculateDashboardInfo", yesterdayDate, null, db);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }
        public static void CalculateProviderBets(ILog logger, long date)
        {
            try
            {
                var currentTime = DateTime.UtcNow;
                var currentDate = date == 0 ? (long)currentTime.Year * 10000 + (long)currentTime.Month * 100 + (long)currentTime.Day : date;
                var currentDay = date == 0 ? new DateTime(currentTime.Year, currentTime.Month, currentTime.Day) : new DateTime((int)(date / 10000), (int)((date % 10000) / 100), (int)(date % 100));
                var yesterday = currentDay.AddDays(-1);
                var yesterdayDate = (long)yesterday.Year * 10000 + (long)yesterday.Month * 100 + (long)yesterday.Day;
                var toDay = currentDay.AddDays(1);
                var fDate = (long)currentDay.Year * 1000000 + (long)currentDay.Month * 10000 + (long)currentDay.Day * 100 + (long)currentDay.Hour;
                var tDate = (long)toDay.Year * 1000000 + (long)toDay.Month * 10000 + (long)toDay.Day * 100 + (long)toDay.Hour;
                
                logger.Info("CalculateProviderBets_" + date + "_" + fDate + "_" + tDate);
                
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    db.Database.CommandTimeout = 300;

                    #region Bets

                    var currencies = db.Currencies.ToList().ToDictionary(x => x.Id, x => x.CurrentRate);
                    var bets = db.fn_PartnerProviderBets(fDate, tDate).ToList();
                    var playersCount = db.fn_ProviderClientsCount(fDate, tDate).ToList();
                    var gBets = bets.GroupBy(x => new { x.PartnerId, x.GameProviderId, x.SubproviderId }).Select(g => new
                    {
                        PartnerId = g.Key.PartnerId,
                        GameProviderId = g.Key.GameProviderId,
                        SubProviderId = g.Key.SubproviderId,
                        BetAmount = g.Sum(x => x.BetAmount * currencies[x.CurrencyId]),
                        BonusBetAmount = g.Sum(x => x.BonusBetAmount * currencies[x.CurrencyId]),
                        WinAmount = g.Sum(x => x.WinAmount * currencies[x.CurrencyId]),
                        BonusWinAmount = g.Sum(x => x.BonusWinAmount * currencies[x.CurrencyId]),
                        BetsCount = g.Sum(x => x.Count)
                    });

                    foreach (var gBet in gBets)
                    {
                        var dayInfo = db.Gtd_Provider_Bets.FirstOrDefault(x => x.Date == currentDate &&
                            x.PartnerId == gBet.PartnerId && x.GameProviderId == gBet.GameProviderId && x.SubProviderId == gBet.SubProviderId);
                        if (dayInfo == null)
                        {
                            dayInfo = new Gtd_Provider_Bets
                            {
                                Date = currentDate,
                                PartnerId = gBet.PartnerId,
                                GameProviderId = gBet.GameProviderId.Value,
                                SubProviderId = gBet.SubProviderId ?? gBet.GameProviderId.Value
                            };
                            db.Gtd_Provider_Bets.Add(dayInfo);
                            db.SaveChanges();
                        }

                        dayInfo.BetAmount = gBet.BetAmount ?? 0;
                        dayInfo.WinAmount = gBet.WinAmount ?? 0;
                        dayInfo.GGR = (gBet.BetAmount - gBet.WinAmount) ?? 0;
                        dayInfo.NGR = dayInfo.GGR + (gBet.BonusWinAmount - gBet.BonusBetAmount) ?? 0;
                        dayInfo.BetsCount = gBet.BetsCount ?? 0;
                        dayInfo.PlayersCount = playersCount.FirstOrDefault(x => x.PartnerId == gBet.PartnerId && 
                            x.GameProviderId == gBet.GameProviderId && x.SubProviderId == gBet.SubProviderId)?.PlayersCount ?? 0;
                    }

                    #endregion

                    db.SaveChanges();
                    if (date == 0 && (currentTime - currentDay).TotalMinutes < 10)
                    {
                        AddJobTrigger("CalculateProviderBets", yesterdayDate, null, db);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }
        public static void CalculatePaymentInfo(ILog logger, long date)
        {
            try
            {
                var currentTime = DateTime.UtcNow;
                var currentDate = date == 0 ? (long)currentTime.Year * 10000 + (long)currentTime.Month * 100 + (long)currentTime.Day : date;
                var currentDay = date == 0 ? new DateTime(currentTime.Year, currentTime.Month, currentTime.Day) : new DateTime((int)(date / 10000), (int)((date % 10000) / 100), (int)(date % 100));
                var yesterday = currentDay.AddDays(-1);
                var yesterdayDate = (long)yesterday.Year * 10000 + (long)yesterday.Month * 100 + (long)yesterday.Day;
                var toDay = currentDay.AddDays(1);
                var fDate = (long)currentDay.Year * 100000000 + (long)currentDay.Month * 1000000 + (long)currentDay.Day * 10000 + (long)currentDay.Hour * 100 + (long)currentDay.Minute;
                var tDate = (long)toDay.Year * 100000000 + (long)toDay.Month * 1000000 + (long)toDay.Day * 10000 + (long)toDay.Hour * 100 + (long)toDay.Minute;
                
                logger.Info("CalculatePaymentInfo_" + date + "_" + fDate + "_" + tDate);
                
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    db.Database.CommandTimeout = 300;

                    #region Requests

                    var requests = (from pr in db.PaymentRequests
                                    join c in db.Clients on pr.ClientId equals c.Id
                                    join crr in db.Currencies on pr.CurrencyId equals crr.Id
                                    where pr.Date >= fDate && pr.Date < tDate
                                    group pr by new { pr.Status, pr.PaymentSystemId, pr.Type, c.PartnerId, c.CurrencyId, crr.CurrentRate } into g
                                    select new
                                    {
                                        PartnerId = g.Key.PartnerId,
                                        PaymentSystemId = g.Key.PaymentSystemId,
                                        Status = g.Key.Status,
                                        Type = g.Key.Type,
                                        CurrencyId = g.Key.CurrencyId,
                                        CurrentRate = g.Key.CurrentRate,
                                        Amount = g.Sum(x => x.Amount),
                                        RequestsCount = g.Count(),
                                        PlayersCount = g.Select(x => x.ClientId).Distinct().Count()
                                    }).ToList();

                    var gRequests = requests.GroupBy(x => new { x.Status, x.PaymentSystemId, x.Type, x.PartnerId, }).Select(g => new
                    {
                        PartnerId = g.Key.PartnerId,
                        PaymentSystemId = g.Key.PaymentSystemId,
                        Status = g.Key.Status,
                        Type = g.Key.Type,
                        TotalAmount = g.Sum(x => x.Amount * x.CurrentRate),
                        TotalRequestsCount = g.Sum(x => x.RequestsCount),
                        TotalPlayersCount = g.Sum(x => x.PlayersCount)
                    });

                    var depositIds = new List<int>();
                    var withdrawIds = new List<int>();

                    foreach (var gRequest in gRequests)
                    {
                        if (gRequest.Type == (int)PaymentRequestTypes.Deposit || gRequest.Type == (int)PaymentRequestTypes.ManualDeposit)
                        {
                            var dayInfo = db.Gtd_Deposit_Info.FirstOrDefault(x => x.Date == currentDate &&
                            x.PartnerId == gRequest.PartnerId && x.PaymentSystemId == gRequest.PaymentSystemId && x.Status == gRequest.Status);
                            if (dayInfo == null)
                            {
                                dayInfo = new Gtd_Deposit_Info
                                {
                                    Date = currentDate,
                                    PartnerId = gRequest.PartnerId,
                                    PaymentSystemId = gRequest.PaymentSystemId,
                                    Status = gRequest.Status
                                };
                                db.Gtd_Deposit_Info.Add(dayInfo);
                                db.SaveChanges();
                            }
                            dayInfo.TotalAmount = gRequest.TotalAmount;
                            dayInfo.RequestsCount = gRequest.TotalRequestsCount;
                            dayInfo.PlayersCount = gRequest.TotalPlayersCount;
                            depositIds.Add(dayInfo.Id);
                        }
                        else
                        {
                            var dayInfo = db.Gtd_Withdraw_Info.FirstOrDefault(x => x.Date == currentDate &&
                            x.PartnerId == gRequest.PartnerId && x.PaymentSystemId == gRequest.PaymentSystemId && x.Status == gRequest.Status);
                            if (dayInfo == null)
                            {
                                dayInfo = new Gtd_Withdraw_Info
                                {
                                    Date = currentDate,
                                    PartnerId = gRequest.PartnerId,
                                    PaymentSystemId = gRequest.PaymentSystemId,
                                    Status = gRequest.Status
                                };
                                db.Gtd_Withdraw_Info.Add(dayInfo);
                                db.SaveChanges();
                            }
                            dayInfo.TotalAmount = gRequest.TotalAmount;
                            dayInfo.RequestsCount = gRequest.TotalRequestsCount;
                            dayInfo.PlayersCount = gRequest.TotalPlayersCount;
                            withdrawIds.Add(dayInfo.Id);
                        }
                    }

                    #endregion

                    db.SaveChanges();
                    db.Gtd_Deposit_Info.Where(x => x.Date == currentDate && !depositIds.Contains(x.Id)).DeleteFromQuery();
                    db.Gtd_Withdraw_Info.Where(x => x.Date == currentDate && !withdrawIds.Contains(x.Id)).DeleteFromQuery();
                    if (date == 0 && (currentTime - currentDay).TotalMinutes < 10)
                    {
                        AddJobTrigger("CalculatePaymentInfo", yesterdayDate, null, db);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }
        public static void CalculateClientInfo(ILog logger, long date, int clientId)
        {
            try
            {
                var currentTime = DateTime.UtcNow;
                var currentDate = date == 0 ? (long)currentTime.Year * 10000 + (long)currentTime.Month * 100 + (long)currentTime.Day : date;
                var currentDay = date == 0 ? new DateTime(currentTime.Year, currentTime.Month, currentTime.Day) : new DateTime((int)(date / 10000), (int)((date % 10000) / 100), (int)(date % 100));
                var yesterday = currentDay.AddDays(-1);
                var yesterdayDate = (long)yesterday.Year * 10000 + (long)yesterday.Month * 100 + (long)yesterday.Day;
                var toDay = currentDay.AddDays(1);
                var fDate = (long)currentDay.Year * 1000000 + (long)currentDay.Month * 10000 + (long)currentDay.Day * 100 + (long)currentDay.Hour;
                var tDate = (long)toDay.Year * 1000000 + (long)toDay.Month * 10000 + (long)toDay.Day * 100 + (long)toDay.Hour;

                logger.Info("CalculateClientInfo_" + date + "_" + clientId + "_" + fDate + "_" + tDate);

                using (var db = new IqSoftDataWarehouseEntities())
                {
                    db.Database.CommandTimeout = 300;

                    #region Bets

                    var bets = clientId > 0 ? db.fn_ClientBets(fDate, tDate).Where(x => x.ClientId == clientId).ToList() : db.fn_ClientBets(fDate, tDate).ToList();
                    
                    foreach (var bet in bets)
                    {
                        var dayInfo = db.Gtd_Client_Info.FirstOrDefault(x => x.Date == currentDate && x.ClientId == bet.ClientId);
                        if (dayInfo == null)
                        {
                            dayInfo = new Gtd_Client_Info
                            {
                                Date = currentDate,
                                ClientId = bet.ClientId.Value,
                            };
                            db.Gtd_Client_Info.Add(dayInfo);
                            db.SaveChanges();
                        }
                        dayInfo.TotalBetAmount = bet.BetAmount ?? 0;
                        dayInfo.TotalBetCount = bet.TotalBetsCount ?? 0;
                        dayInfo.SportBetCount = bet.SportBetsCount ?? 0;
                        dayInfo.TotalWinAmount = bet.WinAmount ?? 0;
                        dayInfo.TotalWinCount = bet.WinCount ?? 0;
                        dayInfo.GGR = dayInfo.TotalBetAmount - dayInfo.TotalWinAmount;
                    }

                    #endregion

                    #region PaymentRequests

                    var dQuery = db.PaymentRequests.Where(x => x.Date >= fDate * 100 && x.Date < tDate * 100 && x.ClientId != null &&
                        (x.Type == (int)PaymentRequestTypes.Deposit || x.Type == (int)PaymentRequestTypes.ManualDeposit) &&
                        (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually));
                    if (clientId > 0)
                        dQuery = dQuery.Where(x => x.ClientId == clientId);
                    var deposits = dQuery.GroupBy(x => new { ClientId = x.ClientId.Value }).Select(x => new { 
                        ClientId = x.Key.ClientId,
                        Amount = x.Sum(y => y.Amount),
                        Count = x.Count()
                    }).ToList();

                    foreach (var d in deposits)
                    {
                        var dayInfo = db.Gtd_Client_Info.FirstOrDefault(x => x.Date == currentDate && x.ClientId == d.ClientId);
                        if (dayInfo == null)
                        {
                            dayInfo = new Gtd_Client_Info
                            {
                                Date = currentDate,
                                ClientId = d.ClientId,
                            };
                            db.Gtd_Client_Info.Add(dayInfo);
                            db.SaveChanges();
                        }

                        dayInfo.TotalDepositAmount = d.Amount;
                        dayInfo.TotalDepositCount = d.Count;
                    }

                    var wQuery = db.PaymentRequests.Where(x => x.Date >= fDate * 100 && x.Date < tDate * 100 && x.ClientId != null &&
                        x.Type == (int)PaymentRequestTypes.Withdraw &&
                        (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually));
                    if (clientId > 0)
                        wQuery = wQuery.Where(x => x.ClientId == clientId);
                    var withdraws = wQuery.GroupBy(x => new { ClientId = x.ClientId.Value }).Select(x => new {
                        ClientId = x.Key.ClientId,
                        Amount = x.Sum(y => y.Amount),
                        Count = x.Count()
                    }).ToList();

                    foreach (var w in withdraws)
                    {
                        var dayInfo = db.Gtd_Client_Info.FirstOrDefault(x => x.Date == currentDate && x.ClientId == w.ClientId);
                        if (dayInfo == null)
                        {
                            dayInfo = new Gtd_Client_Info
                            {
                                Date = currentDate,
                                ClientId = w.ClientId,
                            };
                            db.Gtd_Client_Info.Add(dayInfo);
                            db.SaveChanges();
                        }

                        dayInfo.TotalWithdrawalAmount = w.Amount;
                        dayInfo.TotalWithdrawalCount = w.Count;
                    }

                    #endregion

                    #region Corrections

                    var dcQuery = db.Documents.Where(x => x.Date >= fDate && x.Date < tDate && x.OperationTypeId == (int)OperationTypes.DebitCorrectionOnClient && x.ClientId != null);

                    if (clientId > 0)
                        dcQuery = dcQuery.Where(x => x.ClientId == clientId);
                    var dCorrections = dcQuery.GroupBy(x => new { ClientId = x.ClientId.Value }).Select(x => new {
                        ClientId = x.Key.ClientId,
                        Amount = x.Sum(y => y.Amount),
                        Count = x.Count()
                    }).ToList();

                    foreach (var d in dCorrections)
                    {
                        var dayInfo = db.Gtd_Client_Info.FirstOrDefault(x => x.Date == currentDate && x.ClientId == d.ClientId);
                        if (dayInfo == null)
                        {
                            dayInfo = new Gtd_Client_Info
                            {
                                Date = currentDate,
                                ClientId = d.ClientId,
                            };
                            db.Gtd_Client_Info.Add(dayInfo);
                            db.SaveChanges();
                        }

                        dayInfo.TotalDebitCorrectionAmount = d.Amount;
                        dayInfo.TotalDebitCorrectionCount = d.Count;
                    }


                    var ccQuery = db.Documents.Where(x => x.Date >= fDate && x.Date < tDate && x.OperationTypeId == (int)OperationTypes.CreditCorrectionOnClient && x.ClientId != null);

                    if (clientId > 0)
                        ccQuery = ccQuery.Where(x => x.ClientId == clientId);
                    var cCorrections = ccQuery.GroupBy(x => new { ClientId = x.ClientId.Value }).Select(x => new {
                        ClientId = x.Key.ClientId,
                        Amount = x.Sum(y => y.Amount),
                        Count = x.Count()
                    }).ToList();

                    foreach (var d in cCorrections)
                    {
                        var dayInfo = db.Gtd_Client_Info.FirstOrDefault(x => x.Date == currentDate && x.ClientId == d.ClientId);
                        if (dayInfo == null)
                        {
                            dayInfo = new Gtd_Client_Info
                            {
                                Date = currentDate,
                                ClientId = d.ClientId,
                            };
                            db.Gtd_Client_Info.Add(dayInfo);
                            db.SaveChanges();
                        }

                        dayInfo.TotalCreditCorrectionAmount = d.Amount;
                        dayInfo.TotalCreditCorrectionCount = d.Count;
                    }

                    #endregion

                    db.SaveChanges();
                    if (date == 0)
                    {
                        AddJobTrigger("CalculateClientInfo", yesterdayDate, null, db);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        public static void ExecuteInfoFunctions(ILog logger)
        {
            try
            {
                var jobTriggers = new List<JobTrigger>();
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    jobTriggers = db.JobTriggers.ToList();
                }
                foreach (var jt in jobTriggers)
                {
                    switch (jt.FunctionName)
                    {
                        case "CalculateDashboardInfo":
                            CalculateDashboardInfo(logger, jt.Date);
                            break;
                        case "CalculateProviderBets":
                            CalculateProviderBets(logger, jt.Date);
                            break;
                        case "CalculatePaymentInfo":
                            CalculatePaymentInfo(logger, jt.Date);
                            break;
                        case "CalculateClientInfo":
                            CalculateClientInfo(logger, jt.Date, jt.ClientId ?? 0);
                            break;
                        default:
                            break;
                    }
                }
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    db.JobTriggers.DeleteFromQuery();
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        private static long GroupDocuments(List<Document> documents, IqSoftDataWarehouseEntities db)
        {
            long newLast = 0;
            var bets = new List<Bet>();
            var deletedBets = new List<Bet>();
            var currentTime = DateTime.UtcNow;
            foreach (var d in documents)
            {
                if ((currentTime - d.CreationTime).TotalSeconds < 10) 
                    break;
                if (d.Considered == true)
                    continue;

                if (d.OperationTypeId == (int)OperationTypes.Bet || 
                    d.OperationTypeId == (int)OperationTypes.Win ||
                d.OperationTypeId == (int)OperationTypes.PayWinFromBetshop || 
                d.OperationTypeId == (int)OperationTypes.BetRollback ||
                d.OperationTypeId == (int)OperationTypes.WinRollback || d.OperationTypeId == (int)OperationTypes.CashOut ||
                d.OperationTypeId == (int)OperationTypes.Jackpot || d.OperationTypeId == (int)OperationTypes.MultipleBonus ||
                d.OperationTypeId == (int)OperationTypes.RecalculationDebit)
                {
                    int? selectionsCount = null;
                    if (!string.IsNullOrEmpty(d.TicketInfo))
                    {
                        try
                        {
                            selectionsCount = JsonConvert.DeserializeObject<TicketInfo>(d.TicketInfo).SelectionsCount;
                        }
                        catch
                        {
                        }
                    }
                    if (d.OperationTypeId == (int)OperationTypes.Bet)
                    {
                        int? bonusId = null;
                        bool? fromBonusBalance = null;
                        decimal? bonusAmount = null;
                        if (!string.IsNullOrEmpty(d.Info))
                        {
                            try
                            {
                                var info = JsonConvert.DeserializeObject<DocumentInfo>(d.Info);
                                bonusId = info.BonusId;
                                fromBonusBalance = info.FromBonusBalance;
                                if (bonusId > 0 && fromBonusBalance == true)
                                    bonusAmount = info.BonusAmount;
                            }
                            catch { }
                        }

                        var newBet = new Bet
                        {
                            PartnerId = d.PartnerId ?? 0,
                            BetDocumentId = d.Id,
                            CurrencyId = d.CurrencyId,
                            ProductId = d.ProductId ?? Constants.PlatformProductId,
                            BetAmount = d.Amount,
                            State = (int)BetDocumentStates.Uncalculated,
                            TypeId = d.TypeId ?? 0,
                            CashDeskId = d.CashDeskId,
                            ClientId = d.ClientId,
                            TicketNumber = d.TicketNumber,
                            UserId = d.UserId,
                            DeviceTypeId = d.DeviceTypeId ?? (int)DeviceTypes.Desktop,
                            BetTime = d.CreationTime,
                            BetDate = d.Date ?? 0,
                            BonusId = bonusId,
                            SelectionsCount = selectionsCount,
                            Coefficient = d.Amount <= 0 ? 0 : Math.Round((d.PossibleWin ?? 0) / d.Amount, 2),
                            LastUpdateTime = d.CreationTime,
                            BonusAmount = bonusAmount,
                            AccountId = d.AccountId
                        };
                        bets.Add(newBet);
                    }
                    else if (d.OperationTypeId == (int)OperationTypes.Win || d.OperationTypeId == (int)OperationTypes.CashOut ||
                        d.OperationTypeId == (int)OperationTypes.Jackpot || d.OperationTypeId == (int)OperationTypes.MultipleBonus ||
                        d.OperationTypeId == (int)OperationTypes.RecalculationDebit)
                    {
                        var bet = db.Bets.FirstOrDefault(x => x.BetDocumentId == d.ParentId);
                        if (bet == null)
                            bet = bets.FirstOrDefault(x => x.BetDocumentId == d.ParentId);

                        if (bet != null)
                        {
                            if (d.OperationTypeId == (int)OperationTypes.Win || d.OperationTypeId == (int)OperationTypes.CashOut)
                            {
                                decimal? bonusWinAmount = null;
                                if (!string.IsNullOrEmpty(d.Info))
                                {
                                    try
                                    {
                                        var info = JsonConvert.DeserializeObject<BonusTicketInfo>(d.Info);
                                        if (info.ToBonusBalance)
                                            bonusWinAmount = info.BonusAmount;
                                    }
                                    catch { }
                                }

                                if (bet.WinDocumentId == null)
                                {
                                    bet.WinDocumentId = d.Id;
                                    bet.State = d.State;
                                    bet.CalculationTime = d.CreationTime;
                                    bet.CalculationDate = d.Date;
                                    bet.LastUpdateTime = d.CreationTime;
                                    bet.Rake = d.PossibleWin;
                                    bet.BonusWinAmount = bonusWinAmount;
                                }
                                else
                                {
                                    bet.LastUpdateTime = d.CreationTime;
                                    if (bonusWinAmount != null)
                                        bet.BonusWinAmount = (bet.BonusWinAmount ?? 0) + bonusWinAmount;
                                }
                            }
                            if (d.OperationTypeId == (int)OperationTypes.RecalculationDebit)
                            {
                                bet.WinDocumentId = d.Id;
                                bet.State = d.State;
                                bet.CalculationTime = d.CreationTime;
                                bet.CalculationDate = d.Date;
                                bet.LastUpdateTime = d.CreationTime;
                            }
                            else if (d.OperationTypeId == (int)OperationTypes.Jackpot)
                            {
                                if (bet.JackpotDocumentId == null)
                                {
                                    bet.JackpotDocumentId = d.Id;
                                    bet.LastUpdateTime = d.CreationTime;
                                }
                            }
                            else if (d.OperationTypeId == (int)OperationTypes.MultipleBonus)
                            {
                                if (bet.BonusDocumentId == null)
                                {
                                    bet.BonusDocumentId = d.Id;
                                    bet.LastUpdateTime = d.CreationTime;
                                }
                            }
                            bet.WinAmount += d.Amount;
                        }
                    }
                    else if (d.OperationTypeId == (int)OperationTypes.PayWinFromBetshop)
                    {
                        var bet = db.Bets.FirstOrDefault(x => x.BetDocumentId == d.ParentId);
                        if (bet == null)
                            bet = bets.FirstOrDefault(x => x.BetDocumentId == d.ParentId);

                        if (bet != null)
                        {
                            bet.PayDocumentId = d.Id;
                            bet.State = (int)BetDocumentStates.Paid;
                            bet.PayTime = d.CreationTime;
                            bet.PayDate = d.Date;
                            bet.LastUpdateTime = d.CreationTime;
                        }
                    }
                    else if (d.OperationTypeId == (int)OperationTypes.BetRollback)
                    {
                        var bet = db.Bets.FirstOrDefault(x => x.BetDocumentId == d.ParentId);
                        if (bet == null)
                            bet = bets.FirstOrDefault(x => x.BetDocumentId == d.ParentId);

                        if (bet != null)
                        {
                            bet.WinAmount = bet.BetAmount;
                            bet.State = (int)BetDocumentStates.Deleted;
                            bet.LastUpdateTime = d.CreationTime;
                            var deletedBet = new Bet
                            {
                                PartnerId = d.PartnerId ?? 0,
                                BetDocumentId = d.Id,
                                CurrencyId = d.CurrencyId,
                                ProductId = d.ProductId ?? Constants.PlatformProductId,
                                BetAmount = d.Amount,
                                State = (int)BetDocumentStates.Deleted,
                                TypeId = d.TypeId ?? 0,
                                ClientId = d.ClientId,
                                BonusId = Int32.TryParse(d.Info, out int bonusId) ? (int?)bonusId : null,
                            };
                            deletedBets.Add(deletedBet);
                        }
                    }
                    else if (d.OperationTypeId == (int)OperationTypes.WinRollback)
                    {
                        var bet = db.Bets.FirstOrDefault(x => x.WinDocumentId == d.ParentId);
                        if (bet == null)
                            bet = bets.FirstOrDefault(x => x.WinDocumentId == d.ParentId);

                        if (bet != null && bet.State != (int)BetDocumentStates.Deleted)
                        {
                            bet.WinDocumentId = null;
                            bet.WinAmount = 0;
                            bet.State = (int)BetDocumentStates.Uncalculated;
                            bet.CalculationTime = null;
                            bet.CalculationDate = null;
                            bet.LastUpdateTime = d.CreationTime;
                        }
                    }
                }
                newLast = d.Id;
                d.Considered = true;
            }
            if (bets.Any())
                db.Bets.AddRange(bets);
            db.SaveChanges();
            return newLast;
        }

        private static void AddJobTrigger(string functionName, long date, int? clientId, IqSoftDataWarehouseEntities db)
        {
            db.JobTriggers.Add(new JobTrigger
            {
                FunctionName = functionName,
                Date = date,
                ClientId = clientId
            });
            db.SaveChanges();
        }
    }
}
