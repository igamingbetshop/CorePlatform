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

namespace IqSoft.CP.DataManager.Services
{
    public class DataCombiner
    {
        static DataCombiner()
        {
        }

        public static int GroupNewBets(ILog logger)
        {
            try
            {
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var rows = db.Opt_Document_Considered.OrderBy(x => x.Id).Take(10000).ToList();
                    if (rows.Any())
                    {
                        var dIds = rows.Select(x => x.DocumentId).ToList();
                        var documents = db.Documents.Where(x => dIds.Contains(x.Id)).ToList();
                        var newLast = GroupDocuments(documents, db);
                        db.Opt_Document_Considered.DeleteRangeByKey(rows);
                        Program.DbLogger.Info("GroupNewBets_Finished");
                        return documents.Count;
                    }
                    else
                        return 0;
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
                return 1;
            }
        }

        public static void CalculateDashboardInfo(ILog logger, long date)
        {
            try
            {
                var currentTime = DateTime.UtcNow;
                var currentDate = date == 0 ? (long)currentTime.Year * 10000 + (long)currentTime.Month * 100 + (long)currentTime.Day : date;
                var currentDay = date == 0 ? new DateTime(currentTime.Year, currentTime.Month, currentTime.Day) : 
                    new DateTime((int)(date/10000), (int)((date%10000)/100), (int)(date % 100));
                var yesterday = currentDay.AddDays(-1);
                var yesterdayDate = (long)yesterday.Year * 10000 + (long)yesterday.Month * 100 + (long)yesterday.Day;
                var toDay = currentDay.AddDays(1);
                var fDate = (long)currentDay.Year * 1000000 + (long)currentDay.Month * 10000 + (long)currentDay.Day * 100 + (long)currentDay.Hour;
                var tDate = (long)toDay.Year * 1000000 + (long)toDay.Month * 10000 + (long)toDay.Day * 100 + (long)toDay.Hour;
                var fDateWithMinutes = fDate * 100 + currentDay.Minute;
                var tDateWithMinutes = tDate * 100 + currentDay.Minute;

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

                    #region Requests

                    var gRequests = (from pr in db.PaymentRequests
                                    join c in db.Clients on pr.ClientId equals c.Id
                                    where pr.Date >= fDateWithMinutes && pr.Date < tDateWithMinutes && 
                                        (pr.Status == (int)PaymentRequestStates.Approved || pr.Status == (int)PaymentRequestStates.ApprovedManually) &&
                                        (pr.Type == (int)PaymentRequestTypes.Deposit || pr.Type == (int)PaymentRequestTypes.ManualDeposit)
                                    group pr by new { c.PartnerId } into g
                                    select new
                                    {
                                        PartnerId = g.Key.PartnerId,
                                        FTD = g.Where(x => x.DepositCount == 0).Count(),
                                        DepositsCount = g.Count()
                                    }).ToList();

                    foreach (var gRequest in gRequests)
                    {
                        var dayInfo = db.Gtd_Dashboard_Info.FirstOrDefault(x => x.Date == currentDate && x.PartnerId == gRequest.PartnerId);
                        if (dayInfo == null)
                        {
                            dayInfo = new Gtd_Dashboard_Info
                            {
                                Date = currentDate,
                                PartnerId = gRequest.PartnerId,
                            };
                            db.Gtd_Dashboard_Info.Add(dayInfo);
                            db.SaveChanges();
                        }
                        dayInfo.DepositsCount = gRequest.DepositsCount;
                        dayInfo.FTDCount = gRequest.FTD;
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
                        var subProviderId = gBet.SubProviderId ?? gBet.GameProviderId.Value;

                        var dayInfo = db.Gtd_Provider_Bets.FirstOrDefault(x => x.Date == currentDate &&
                            x.PartnerId == gBet.PartnerId && x.GameProviderId == gBet.GameProviderId && x.SubProviderId == subProviderId);

                        if (dayInfo == null)
                        {
                            dayInfo = new Gtd_Provider_Bets
                            {
                                Date = currentDate,
                                PartnerId = gBet.PartnerId,
                                GameProviderId = gBet.GameProviderId.Value,
                                SubProviderId = subProviderId
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

                    var clientsBets = clientId > 0 ? db.fn_ClientBets(fDate, tDate).Where(x => x.ClientId == clientId).ToList() : db.fn_ClientBets(fDate, tDate).ToList();

                    var bwQuery = db.Documents.Where(x => x.OperationTypeId == (int)OperationTypes.BonusWin && x.Date >= fDate && x.Date < tDate);
                    if (clientId > 0)
                        bwQuery = bwQuery.Where(x => x.ClientId == clientId);

                    var bonusWins = bwQuery.GroupBy(x => new { ClientId = x.ClientId.Value }).Select(x => new {
                        ClientId = x.Key.ClientId,
                        Amount = x.Sum(y => y.Amount),
                        Count = x.Count()
                    }).ToList();


                    foreach (var clientBet in clientsBets)
                    {
                        var balance = db.AccountBalances.Where(x => x.ObjectTypeId == (int)ObjectTypes.Client &&
                            x.ObjectId == clientBet.ClientId && x.TypeId == (int)AccountTypes.ClientCoinBalance).OrderByDescending(x => x.Id).FirstOrDefault();
                        var dayInfo = db.Gtd_Client_Info.FirstOrDefault(x => x.Date == currentDate && x.ClientId == clientBet.ClientId);
                        if (dayInfo == null)
                        {
                            dayInfo = new Gtd_Client_Info
                            {
                                Date = currentDate,
                                ClientId = clientBet.ClientId.Value
                            };
                            db.Gtd_Client_Info.Add(dayInfo);
                            db.SaveChanges();
                        }
                        dayInfo.TotalBetAmount = clientBet.BetAmount ?? 0;
                        dayInfo.TotalBetCount = clientBet.TotalBetsCount ?? 0;
                        dayInfo.SportBetCount = clientBet.SportBetsCount ?? 0;
                        dayInfo.TotalWinAmount = clientBet.WinAmount ?? 0;
                        dayInfo.TotalWinCount = clientBet.WinCount ?? 0;
                        dayInfo.ComplementaryBalance = balance == null ? 0 : balance.Balance;
                        dayInfo.GGR = dayInfo.TotalBetAmount - dayInfo.TotalWinAmount;
                        dayInfo.NGR = dayInfo.GGR + ((clientBet.BonusWinAmount - clientBet.BonusBetAmount) ?? 0) - 
                            (bonusWins.FirstOrDefault(x => x.ClientId == clientBet.ClientId.Value)?.Amount ?? 0);
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
                        var balance = db.AccountBalances.Where(x => x.ObjectTypeId == (int)ObjectTypes.Client &&
                            x.ObjectId == d.ClientId && x.TypeId == (int)AccountTypes.ClientCoinBalance).OrderByDescending(x => x.Id).FirstOrDefault();
                        var dayInfo = db.Gtd_Client_Info.FirstOrDefault(x => x.Date == currentDate && x.ClientId == d.ClientId);
                        if (dayInfo == null)
                        {
                            dayInfo = new Gtd_Client_Info
                            {
                                Date = currentDate,
                                ClientId = d.ClientId
                            };
                            db.Gtd_Client_Info.Add(dayInfo);
                            db.SaveChanges();
                        }
                        dayInfo.ComplementaryBalance = balance == null ? 0 : balance.Balance;
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
                        var balance = db.AccountBalances.Where(x => x.ObjectTypeId == (int)ObjectTypes.Client &&
                            x.ObjectId == w.ClientId && x.TypeId == (int)AccountTypes.ClientCoinBalance).OrderByDescending(x => x.Id).FirstOrDefault();
                        var dayInfo = db.Gtd_Client_Info.FirstOrDefault(x => x.Date == currentDate && x.ClientId == w.ClientId);
                        if (dayInfo == null)
                        {
                            dayInfo = new Gtd_Client_Info
                            {
                                Date = currentDate,
                                ClientId = w.ClientId
                            };
                            db.Gtd_Client_Info.Add(dayInfo);
                            db.SaveChanges();
                        }
                        dayInfo.ComplementaryBalance = balance == null ? 0 : balance.Balance;
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
                        var balance = db.AccountBalances.Where(x => x.ObjectTypeId == (int)ObjectTypes.Client &&
                            x.ObjectId == d.ClientId && x.TypeId == (int)AccountTypes.ClientCoinBalance).OrderByDescending(x => x.Id).FirstOrDefault();
                        var dayInfo = db.Gtd_Client_Info.FirstOrDefault(x => x.Date == currentDate && x.ClientId == d.ClientId);
                        if (dayInfo == null)
                        {
                            dayInfo = new Gtd_Client_Info
                            {
                                Date = currentDate,
                                ClientId = d.ClientId
                            };
                            db.Gtd_Client_Info.Add(dayInfo);
                            db.SaveChanges();
                        }
                        dayInfo.ComplementaryBalance = balance == null ? 0 : balance.Balance;
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
                        var balance = db.AccountBalances.Where(x => x.ObjectTypeId == (int)ObjectTypes.Client &&
                            x.ObjectId == d.ClientId && x.TypeId == (int)AccountTypes.ClientCoinBalance).OrderByDescending(x => x.Id).FirstOrDefault();
                        var dayInfo = db.Gtd_Client_Info.FirstOrDefault(x => x.Date == currentDate && x.ClientId == d.ClientId);
                        if (dayInfo == null)
                        {
                            dayInfo = new Gtd_Client_Info
                            {
                                Date = currentDate,
                                ClientId = d.ClientId
                            };
                            db.Gtd_Client_Info.Add(dayInfo);
                            db.SaveChanges();
                        }
                        dayInfo.ComplementaryBalance = balance == null ? 0 : balance.Balance;
                        dayInfo.TotalCreditCorrectionAmount = d.Amount;
                        dayInfo.TotalCreditCorrectionCount = d.Count;
                    }

                    #endregion

                    #region Bonuses

                    var bQuery = db.Documents.Where(x => x.Date >= fDate && x.Date < tDate && x.OperationTypeId == (int)OperationTypes.WageringBonus);
                    if (clientId > 0)
                        bQuery = bQuery.Where(x => x.ClientId == clientId);

                    var bonuses = bQuery.GroupBy(x => new { ClientId = x.ClientId.Value }).Select(x => new {
                        ClientId = x.Key.ClientId,
                        Amount = x.Sum(y => y.Amount),
                        Count = x.Count()
                    }).ToList();

                    foreach (var b in bonuses)
                    {
                        var balance = db.AccountBalances.Where(x => x.ObjectTypeId == (int)ObjectTypes.Client &&
                            x.ObjectId == b.ClientId && x.TypeId == (int)AccountTypes.ClientCoinBalance).OrderByDescending(x => x.Id).FirstOrDefault();
                        var dayInfo = db.Gtd_Client_Info.FirstOrDefault(x => x.Date == currentDate && x.ClientId == b.ClientId);
                        if (dayInfo == null)
                        {
                            dayInfo = new Gtd_Client_Info
                            {
                                Date = currentDate,
                                ClientId = b.ClientId
                            };
                            db.Gtd_Client_Info.Add(dayInfo);
                            db.SaveChanges();
                        }
                        dayInfo.ComplementaryBalance = balance == null ? 0 : balance.Balance;
                        dayInfo.TotalBonusAmount = b.Amount;
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

        public static bool ExecuteInfoFunctions(ILog logger)
        {
            try
            {
                JobTrigger jobTrigger = null;
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    jobTrigger = db.JobTriggers.FirstOrDefault();
                }
                if(jobTrigger != null)
                {
                    switch (jobTrigger.FunctionName)
                    {
                        case "CalculateDashboardInfo":
                            CalculateDashboardInfo(logger, jobTrigger.Date);
                            break;
                        case "CalculateProviderBets":
                            CalculateProviderBets(logger, jobTrigger.Date);
                            break;
                        case "CalculatePaymentInfo":
                            CalculatePaymentInfo(logger, jobTrigger.Date);
                            break;
                        case "CalculateClientInfo":
                            CalculateClientInfo(logger, jobTrigger.Date, jobTrigger.ClientId ?? 0);
                            break;
                        default:
                            break;
                    }
                    using (var db = new IqSoftDataWarehouseEntities())
                    {
                        db.JobTriggers.DeleteByKey(jobTrigger);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                logger.Error(e);
                return true;
            }
        }

        private static long GroupDocuments(List<Document> documents, IqSoftDataWarehouseEntities db)
        {
            long newLast = 0;
            var bets = new List<Bet>();
            var deletedBets = new List<Bet>();
            var newConsideredDocuments = new List<long>();
            var datesToReconsider = new List<DateTime>();
            var currentTime = DateTime.UtcNow;
            var currentDate = currentTime.Date;

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
                        if (newBet.BetTime.Date < currentDate && !datesToReconsider.Contains(newBet.BetTime.Date))
                            datesToReconsider.Add(newBet.BetTime.Date);
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
                            if (bet.BetTime.Date < currentDate && !datesToReconsider.Contains(bet.BetTime.Date))
                                datesToReconsider.Add(bet.BetTime.Date);
                        }
                        else
                            continue;
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

                            if (bet.BetTime.Date < currentDate && !datesToReconsider.Contains(bet.BetTime.Date))
                                datesToReconsider.Add(bet.BetTime.Date);
                        }
                        else
                            continue;
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
                            if (bet.BetTime.Date < currentDate && !datesToReconsider.Contains(bet.BetTime.Date))
                                datesToReconsider.Add(bet.BetTime.Date);
                        }
                        else
                            continue;
                    }
                    else if (d.OperationTypeId == (int)OperationTypes.WinRollback)
                    {
                        var bet = db.Bets.FirstOrDefault(x => x.WinDocumentId == d.ParentId);
                        if (bet == null)
                            bet = bets.FirstOrDefault(x => x.WinDocumentId == d.ParentId);

                        if (bet != null)
                        {
                            if (bet.State != (int)BetDocumentStates.Deleted)
                            {
                                bet.WinDocumentId = null;
                                bet.WinAmount = 0;
                                bet.State = (int)BetDocumentStates.Uncalculated;
                                bet.CalculationTime = null;
                                bet.CalculationDate = null;
                                bet.LastUpdateTime = d.CreationTime;
                            }
                            if (bet.BetTime.Date < currentDate && !datesToReconsider.Contains(bet.BetTime.Date))
                                datesToReconsider.Add(bet.BetTime.Date);
                        }
                        else if(!db.Documents.Any(x => x.Id == d.ParentId && x.Considered == true))
                            continue;
                    }
                }
                newLast = d.Id;
                {
                    d.Considered = true;
                    newConsideredDocuments.Add(d.Id);
                }
            }
            if (bets.Any())
                db.Bets.AddRange(bets);
            db.SaveChanges();

            db.Opt_Document_Considered.Where(x => newConsideredDocuments.Contains(x.DocumentId)).DeleteFromQuery();
            foreach(var date in datesToReconsider)
            {
                var d = (long)date.Year * 10000 + (long)date.Month * 100 + (long)date.Day;
                AddJobTrigger("CalculateDashboardInfo", d, null, db);
                AddJobTrigger("CalculateProviderBets", d, null, db);
                AddJobTrigger("CalculatePaymentInfo", d, null, db);
                AddJobTrigger("CalculateClientInfo", d, null, db);
            }

            return newLast;
        }

        private static void AddJobTrigger(string functionName, long date, int? clientId, IqSoftDataWarehouseEntities db)
        {
            var existing = db.JobTriggers.FirstOrDefault(x => x.FunctionName == functionName && x.Date == date && x.ClientId == clientId);
            if (existing == null)
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
}
