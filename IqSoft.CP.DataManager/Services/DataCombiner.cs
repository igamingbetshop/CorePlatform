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
                var bets = new List<Bet>();
                var deletedBets = new List<Bet>();
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    long newLast = lastId;
                    var currentTime = DateTime.UtcNow;
                    var documents = db.Documents.Where(x => x.Id > lastId).OrderBy(x => x.Id).Take(10000).ToList();

                    foreach (var d in documents)
                    {
                        if ((currentTime - d.CreationTime).TotalSeconds < 10) break;
                        if (d.OperationTypeId == (int)OperationTypes.Bet || d.OperationTypeId == (int)OperationTypes.Win ||
                        d.OperationTypeId == (int)OperationTypes.PayWinFromBetshop || d.OperationTypeId == (int)OperationTypes.BetRollback ||
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
                    }
                    if (bets.Any())
                        db.Bets.AddRange(bets);
                    db.SaveChanges();

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
    }
}
