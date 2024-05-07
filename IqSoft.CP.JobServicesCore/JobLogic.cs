using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models.Job;
using IqSoft.CP.DAL.Models.Document;
using IqSoft.CP.DAL.Models.User;
using Renci.SshNet;
using IqSoft.CP.JobService.Hubs;
using IqSoft.CP.Common.Models.Enums;
using IqSoft.CP.BLL.Services;
using System.ServiceModel;
using System.Threading.Tasks;
using IqSoft.CP.DAL.Models.Report;
using Microsoft.AspNetCore.SignalR;
using IqSoft.CP.BLL.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using IqSoft.CP.BLL.Caching;

namespace IqSoft.CP.JobService
{
    public static class JobBll
    {
        public static long LastProcessedBetDocumentId = 0;
        public static int IqWalletId;
        static JobBll()
        {
            using var db = new IqSoftCorePlatformEntities();
            LastProcessedBetDocumentId = db.PartnerKeys.First(x => x.Name == Constants.PartnerKeys.LastProcessedBetDocumentId).NumericValue.Value;
            var paymentSystem = db.PaymentSystems.FirstOrDefault(x => x.Name == (Constants.PaymentSystems.IqWallet));
            IqWalletId = paymentSystem == null ? 0 : paymentSystem.Id;
        }

        public static Job GetJobById(int id)
        {
            using var db = new IqSoftCorePlatformEntities();
            var currentTime = DateTime.UtcNow;
            return db.Jobs.FirstOrDefault(x => x.Id == id && x.NextExecutionTime <= currentTime && x.State == (int)JobStates.Active);
        }

        public static Job SaveJob(Job job)
        {
            using var db = new IqSoftCorePlatformEntities();
            var currentTime = DateTime.UtcNow;
            var dbJob = db.Jobs.FirstOrDefault(x => x.Id == job.Id);

            if (dbJob == null)
            {
                dbJob = new Job { CreationTime = currentTime, Id = job.Id };
                db.Jobs.Add(dbJob);
            }
            job.CreationTime = dbJob.CreationTime;
            db.Entry(dbJob).CurrentValues.SetValues(job);
            db.SaveChanges();
            return dbJob;
        }

        public static JobResult SaveJobResult(JobResult jobResult)
        {
            using var db = new IqSoftCorePlatformEntities();
            var currentTime = DateTime.UtcNow;
            var dbJobResult = db.JobResults.FirstOrDefault(x => x.Id == jobResult.Id);
            if (dbJobResult == null)
            {
                dbJobResult = new JobResult { Id = jobResult.Id, ExecutionTime = currentTime };
                db.JobResults.Add(dbJobResult);
            }
            jobResult.ExecutionTime = dbJobResult.ExecutionTime;
            db.Entry(dbJobResult).CurrentValues.SetValues(jobResult);
            db.SaveChanges();
            return dbJobResult;
        }

        public static ResetBetShopDailyTicketNumberOutput ResetBetShopDailyTicketNumber(ResetBetShopDailyTicketNumberInput input)
        {
            using var db = new IqSoftCorePlatformEntities();
            var result = new ResetBetShopDailyTicketNumberOutput { Results = new List<ResetBetShopDailyTicketNumberOutputItem>() };
            var currentTime = DateTime.UtcNow;
            foreach (var dailyTicketNumberResetSetting in input.Settings)
            {
                if (dailyTicketNumberResetSetting.ResetTime > currentTime)
                    result.Results.Add(new ResetBetShopDailyTicketNumberOutputItem { PartnerId = dailyTicketNumberResetSetting.PartnerId, ResetResult = false });
                else
                {
                    var betShopIds =
                        db.BetShops.Where(x => x.PartnerId == dailyTicketNumberResetSetting.PartnerId)
                            .Select(x => x.Id)
                            .ToList();
                    foreach (var betShopId in betShopIds)
                    {
                        var r = Task.Run(() => db.Procedures.sp_GetBetShopLockAsync(betShopId)).Result;
                        var betShop = db.BetShops.FirstOrDefault(x => x.Id == betShopId);
                        betShop.DailyTicketNumber = 0;
                    }
                    result.Results.Add(new ResetBetShopDailyTicketNumberOutputItem { PartnerId = dailyTicketNumberResetSetting.PartnerId, ResetResult = true });
                }
            }
            db.SaveChanges();
            return result;
        }

        public static bool CloseAccountPeriod(ILog log, ClosePeriodInput input)
        {
            using var db = new IqSoftCorePlatformEntities();
            var currentTime = DateTime.UtcNow;
            if (input.EndTime > currentTime)
                return false;

            var startDate = input.EndTime.AddHours(-Constants.ClosePeriodPeriodicy);
            var groupedTransactions = (from t in db.Transactions
                                       where t.CreationTime >= startDate && t.CreationTime < input.EndTime
                                       group t by new { t.Type, t.AccountId } into trans
                                       select new { Amount = trans.Sum(t => t.Amount), TypeId = trans.Key.Type, AccountId = trans.Key.AccountId }).ToList();

            var closibleAccounts = groupedTransactions.Select(x => x.AccountId).Distinct().ToList();
            foreach (var closibleAccountId in closibleAccounts)
            {
                var lastPeriod =
                    db.AccountClosedPeriods.Where(x => x.AccountId == closibleAccountId)
                        .OrderByDescending(x => x.Date)
                        .FirstOrDefault();

                var totalCreditAmount =
                    groupedTransactions.FirstOrDefault(
                        x =>
                            x.AccountId == closibleAccountId &&
                            x.TypeId == (int)TransactionTypes.Credit);
                var totalDebitAmount =
                    groupedTransactions.FirstOrDefault(
                        x =>
                            x.AccountId == closibleAccountId &&
                            x.TypeId == (int)TransactionTypes.Debit);

                var closePeriod = new AccountClosedPeriod
                {
                    AccountId = closibleAccountId,
                    Date = input.EndTime,
                    TotalCreditAmount = totalCreditAmount == null ? 0 : totalCreditAmount.Amount,
                    TotalDebitAmount = totalDebitAmount == null ? 0 : totalDebitAmount.Amount,
                    FirstBalance = lastPeriod == null ? 0 : lastPeriod.LastBalance,
                    LastBalance = GetAccountBalanceByDate(closibleAccountId, input.EndTime, db)
                };
                db.AccountClosedPeriods.Add(closePeriod);
            }
            db.SaveChanges();
            return true;
        }

        private static decimal GetAccountBalanceByDate(long accountId, DateTime date, IqSoftCorePlatformEntities db)
        {
            var account = db.Accounts.Include(x => x.Type).FirstOrDefault(x => x.Id == accountId);
            if (account != null)
            {
                var accountBalance =
                    db.AccountBalances.Include(x => x.Account).Where(x => x.AccountId == account.Id && x.Date >= date)
                        .OrderBy(x => x.Date)
                        .FirstOrDefault();
                if (accountBalance != null)
                    return accountBalance.Balance;
                else
                    return account.Balance;
            }
            return 0;
        }

        public static bool CloseClientPeriod(ClosePeriodInput input)
        {
            using var db = new IqSoftCorePlatformEntities();
            var currentTime = DateTime.UtcNow;
            if (input.EndTime > currentTime)
                return false;
            var startDate = input.EndTime.AddHours(-Constants.ClosePeriodPeriodicy);
            var groupedDocuments = (from d in db.Documents
                                    where d.CreationTime >= startDate && d.CreationTime < input.EndTime && d.ClientId != null
                                    group d by new { d.ClientId, d.OperationTypeId } into docs
                                    select
                                        new
                                        {
                                            Amount = docs.Sum(t => t.Amount),
                                            OperationTypeId = docs.Key.OperationTypeId,
                                            ClientId = docs.Key.ClientId
                                        }).ToList();
            var clientIds = groupedDocuments.Select(x => x.ClientId).Distinct().ToList();
            foreach (var clientId in clientIds)
            {
                var lastPeriod =
                    db.ClientClosedPeriods.Where(x => x.ClientId == clientId)
                        .OrderByDescending(x => x.Date)
                        .FirstOrDefault();

                var totalDepositAmount = groupedDocuments.Where(
                    x =>
                        x.ClientId == clientId &&
                        (x.OperationTypeId == (int)OperationTypes.TransferFromPaymentSystemToClient ||
                         x.OperationTypeId == (int)OperationTypes.TransferFromBetShopToClient))
                    .Sum(x => x.Amount);
                var totalWithdrawAmountModel =
                    groupedDocuments.FirstOrDefault(
                        x =>
                            x.ClientId == clientId &&
                            x.OperationTypeId == (int)OperationTypes.TransferFromClientToPaymentSystem ||
                            x.OperationTypeId == (int)OperationTypes.ClientTransferToBetShop);
                var totalBetAmountModel =
                    groupedDocuments.FirstOrDefault(
                        x => x.ClientId == clientId && x.OperationTypeId == (int)OperationTypes.Bet);
                var totalBetAmount = totalBetAmountModel == null ? 0 : totalBetAmountModel.Amount;
                var totalWinAmountModel =
                    groupedDocuments.FirstOrDefault(
                        x => x.ClientId == clientId && x.OperationTypeId == (int)OperationTypes.Win);
                var totalWinAmount = totalWinAmountModel == null ? 0 : totalWinAmountModel.Amount;
                var totalCreditCorrectionModel =
                    groupedDocuments.FirstOrDefault(
                        x =>
                            x.ClientId == clientId &&
                            x.OperationTypeId == (int)OperationTypes.CreditCorrectionOnClient);
                var totalDebitCorrectionModel =
                    groupedDocuments.FirstOrDefault(
                        x =>
                            x.ClientId == clientId &&
                            x.OperationTypeId == (int)OperationTypes.DebitCorrectionOnClient);
                var closePeriod = new ClientClosedPeriod
                {
                    ClientId = clientId.Value,
                    Date = input.EndTime,
                    TotalBetAmount = totalBetAmount,
                    TotalWinAmount = totalWinAmount,
                    TotalGGR = (lastPeriod == null ? 0 : lastPeriod.TotalGGR) + totalBetAmount - totalWinAmount,
                    TotalNetGaming = (lastPeriod == null ? 0 : lastPeriod.TotalNetGaming) + totalBetAmount,
                    TotalCreditCorrection =
                        totalCreditCorrectionModel == null ? 0 : totalCreditCorrectionModel.Amount,
                    TotalDebitCorrection = totalDebitCorrectionModel == null ? 0 : totalDebitCorrectionModel.Amount,
                    TotalDepositAmount = totalDepositAmount,
                    TotalWithdrawAmount = totalWithdrawAmountModel == null ? 0 : totalWithdrawAmountModel.Amount
                };
                db.ClientClosedPeriods.Add(closePeriod);
            }
            db.SaveChanges();
            return true;
        }

        public static bool AddMoneyToPartnerAccount(ILog log, AddMoneyToPartnerAccountInput input)
        {
            using var partnerBl = new PartnerBll(new SessionIdentity(), log);
            return partnerBl.ChangePartnerAccountBalance(input.PartnerId, input.EndTime); //????
        }

        public static void ExpireUserSessions()
        {
            using var db = new IqSoftCorePlatformEntities();
            var currentTime = DateTime.UtcNow;
            db.UserSessions.Include(x => x.User)
               .Join(db.Partners, us => us.User.PartnerId, p => p.Id, (us, p) => new { UserSession = us, UserType = us.User.Type, Partner = p })
               .Where(x =>
                      x.UserSession.State != (int)SessionStates.Inactive && x.UserType == (int)UserTypes.AdminUser &&
                     (EF.Functions.DateDiffMinute(currentTime, x.UserSession.LastUpdateTime) >= x.Partner.UserSessionExpireTime)).Select(x => x.UserSession)
               .UpdateFromQuery(x => new UserSession { State = (int)SessionStates.Inactive, EndTime = currentTime, LogoutType = (int)LogoutTypes.Expired });
        }

        public static void ExpireClientSessions()
        {
            try
            {
                ExpireClientPlatformSessions();
                ExpireClientProductSessions();
            }
            catch
            {
            }
        }

        private static void ExpireClientPlatformSessions()
        {
            if (BaseHub.Caches.Any(x => x.Value.ProjectId == (int)ProjectTypes.MasterCache))
                BaseHub.CurrentContext.Clients.Client(BaseHub.Caches.First(x => x.Value.ProjectId == (int)ProjectTypes.MasterCache).Key)
                                              .SendAsync("ExpireClientPlatformSessions", 1);
        }

        private static void ExpireClientProductSessions()
        {
            if (BaseHub.Caches.Any(x => x.Value.ProjectId == (int)ProjectTypes.MasterCache))
                BaseHub.CurrentContext.Clients.Client(BaseHub.Caches.First(x => x.Value.ProjectId == (int)ProjectTypes.MasterCache).Key)
                                             .SendAsync("ExpireClientProductSessions", 1);
        }

        public static void ExpireClientVerificationKeys()
        {
            using var db = new IqSoftCorePlatformEntities();
            var currentTime = DateTime.UtcNow;

            var keyTypes = new List<int>
                {
                    (int)ClientInfoTypes.MobileVerificationKey,
                    (int)ClientInfoTypes.EmailVerificationKey,
                    (int)ClientInfoTypes.PasswordRecoveryMobileKey,
                    (int)ClientInfoTypes.PasswordRecoveryEmailKey
                };

            db.ClientInfoes.Where(x => x.State == (int)ClientInfoStates.Active && keyTypes.Contains(x.Type) &&
                                      (EF.Functions.DateDiffMinute(currentTime, x.CreationTime) >= x.Partner.VerificationKeyActiveMinutes || 
                                       EF.Functions.DateDiffMinute(currentTime, x.CreationTime) >= x.Client.Partner.VerificationKeyActiveMinutes))
                           .UpdateFromQuery(x => new ClientInfo { State = (int)ClientInfoStates.Expired });
        }

        public static void CalculateCashBackBonuses(ILog log)
        {
            using var bonusBl = new BonusService(new SessionIdentity(), log, 1200);
            bonusBl.CalculateCashBackBonus();
        }

        public static void AwardCashBackBonuses(DateTime lastExecutionTime, ILog log)
        {
            var clientList = new List<int>();
            using (var bonusBl = new BonusService(new SessionIdentity(), log, 1200))
            {
                clientList = bonusBl.AwardCashbackBonus(lastExecutionTime);
            }
            clientList.ForEach(clientId =>
            {
                BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, clientId));
                BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.NotAwardedCampaigns, clientId));
            });
        }

        public static void GiveAffiliateBonus(ILog log)
        {
            var clientList = new List<int>();
            using (var bonusBl = new BonusService(new SessionIdentity(), log))
            {
                clientList = bonusBl.GiveBonusToAffiliateManagers();
            }
            foreach (var clientId in clientList)
            {
                BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, clientId));
            }
        }

        public static void DeletePaymentExpiredActiveRequests(ILog log)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                var currentTime = DateTime.UtcNow.AddDays(-1);
                var lastUpdateTime = DateTime.UtcNow;
                var date = (long)currentTime.Year * 1000000 + (long)currentTime.Month * 10000 + (long)currentTime.Day * 100 + (long)currentTime.Hour;
                db.PaymentRequests.Where(x => x.Type == (int)PaymentRequestTypes.Deposit && x.Status == (int)PaymentRequestStates.Pending &&
                                              x.Date < date)
                                  .UpdateFromQuery(x => new PaymentRequest { Status = (int)PaymentRequestStates.Expired, LastUpdateTime =  lastUpdateTime });
            }
        }

        public static void UpdateCurrentRate(ILog log)
        {
            try
            {
                AuthResponse authResponse = null;
                var partnerKeyNames = new List<string>
                {
                    Constants.PartnerKeys.XeKey,
                    Constants.PartnerKeys.XeId,
                    Constants.PartnerKeys.XeApiUrl
                };
                using var db = new IqSoftCorePlatformEntities();
                var partnerKey = db.PartnerKeys.Where(x => partnerKeyNames.Contains(x.Name)).ToDictionary(x => x.Name, y => y.StringValue);
                var currentDate = DateTime.UtcNow;
                var requestHeaders = new Dictionary<string, string>
                {
                    {
                        "Authorization",
                        "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes
                                                    (partnerKey[Constants.PartnerKeys.XeId] + ":" + partnerKey[Constants.PartnerKeys.XeKey]))
                    }
               };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = HttpMethod.Get,
                    Url = partnerKey[Constants.PartnerKeys.XeApiUrl] + "account_info",
                    RequestHeaders = requestHeaders
                };
                authResponse = JsonConvert.DeserializeObject<AuthResponse>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (authResponse.Code == 0)
                {
                    var currenies = db.Currencies.ToList();
                    foreach (var dbCurr in currenies)
                    {
                        try
                        {
                            var request = new
                            {
                                from = dbCurr.Id,
                                to = Constants.DefaultCurrencyId,
                                amount = 1
                            };

                            httpRequestInput.Url = partnerKey[Constants.PartnerKeys.XeApiUrl] + "convert_from/?" + CommonFunctions.GetUriEndocingFromObject(request);
                            var response = JsonConvert.DeserializeObject<XeRateResponse>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                            var currency = new Currency
                            {
                                Id = dbCurr.Id,
                                CurrentRate = response.To[0].Mid,
                                Symbol = dbCurr.Symbol,
                                SessionId = dbCurr.SessionId,
                                Code = dbCurr.Code,
                                CreationTime = dbCurr.CreationTime,
                                LastUpdateTime = currentDate
                            };
                            var currencyRate = new CurrencyRate
                            {
                                CurrencyId = dbCurr.Id,
                                RateAfter = response.To[0].Mid,
                                RateBefore = dbCurr.CurrentRate,
                                SessionId = dbCurr.SessionId,
                                CreationTime = currentDate,
                                LastUpdateTime = currentDate
                            };
                            db.CurrencyRates.Add(currencyRate);
                            db.Entry(dbCurr).CurrentValues.SetValues(currency);
                        }
                        catch
                        {

                        }
                    }
                    db.SaveChanges();
                }
                else
                {
                    log.Info(authResponse);
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        public static void GroupNewBets(long lastId, ILog logger)
        {
            try
            {
                var bets = new List<Bet>();
                var deletedBets = new List<Bet>();
                using (var db = new IqSoftCorePlatformEntities())
                {
                    long newLast = lastId;
                    var currentTime = DateTime.UtcNow;
                    var documents = db.Documents.Include(x => x.Client).Include(x => x.CashDesk.BetShop).Where(x => x.Id > lastId).OrderBy(x => x.Id).Take(1000).ToList();

                    foreach (var d in documents)
                    {
                        if ((currentTime - d.CreationTime).TotalSeconds < 10) break;
                        if (d.OperationTypeId == (int)OperationTypes.Bet || d.OperationTypeId == (int)OperationTypes.Win ||
                        d.OperationTypeId == (int)OperationTypes.PayDepositFromBetshop || d.OperationTypeId == (int)OperationTypes.BetRollback ||
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
                                if (!string.IsNullOrEmpty(d.Info))
                                {
                                    try
                                    {
                                        bonusId = JsonConvert.DeserializeObject<DocumentInfo>(d.Info).BonusId;
                                    }
                                    catch { }
                                }

                                var newBet = new Bet
                                {
                                    PartnerId = d.ClientId != null ? d.Client.PartnerId : d.CashDesk.BetShop.PartnerId,
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
                                    LastUpdateTime = d.CreationTime
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
                                        if (bet.WinDocumentId == null)
                                        {
                                            bet.WinDocumentId = d.Id;
                                            bet.State = d.State;
                                            bet.CalculationTime = d.CreationTime;
                                            bet.CalculationDate = d.Date;
                                            bet.LastUpdateTime = d.CreationTime;
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
                            else if (d.OperationTypeId == (int)OperationTypes.PayDepositFromBetshop)
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
                                        PartnerId = d.ClientId != null ? d.Client.PartnerId : d.CashDesk.BetShop.PartnerId,
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
                        var key = db.PartnerKeys.First(x => x.Name == Constants.PartnerKeys.LastProcessedBetDocumentId);
                        key.NumericValue = newLast;
                        db.SaveChanges();
                        LastProcessedBetDocumentId = newLast;
                    }
                }
                using var clientBl = new ClientBll(new SessionIdentity(), logger);
                if (bets.Any())
                {
                    clientBl.GiveComplimentaryPoint(bets);
                    clientBl.IncreaseJackpot(bets);
                }
                if (deletedBets!= null && deletedBets.Any())
                    clientBl.CancelComplimentaryPoint(deletedBets);
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        public static void SendActiveMails(ILog log)
        {
            var activeMails = new List<Email>();

            using (var db = new IqSoftCorePlatformEntities())
            {
                activeMails = db.Emails.Include(x => x.MessageTemplate).Include(x => x.Partner).Where(x => x.Status == (int)EmailStates.Active).Take(60).ToList();
            }
            using var notificationBll = new NotificationBll(new SessionIdentity(), log);
            foreach (var email in activeMails)
            {
                try
                {
                    var status = notificationBll.SendEmail(email.Receiver, email.PartnerId, email.Receiver, email.Subject, email.Body, email.MessageTemplate?.ExternalTemplateId) ?
                        (int)EmailStates.Sent : (int)EmailStates.Failed;
                    notificationBll.UpdateEmailStatus(email.Id, status);
                }
                catch (FaultException<BllFnErrorType> ex)
                {
                    log.Error(JsonConvert.SerializeObject(new { ex.Detail.Id, ex.Detail.Message, ex.Detail.DecimalInfo }) + "_" + JsonConvert.SerializeObject(new { email.Id, email.Receiver }));
                    notificationBll.UpdateEmailStatus(email.Id, (int)EmailStates.Failed);
                }
                catch (Exception e)
                {
                    log.Error(e);
                    notificationBll.UpdateEmailStatus(email.Id, (int)EmailStates.Failed);
                }
            }
        }

        public static void SendPartnerDailyReport(ILog log)
        {
            var startTime = DateTime.UtcNow.Date.AddDays(-1);
            var endDate = startTime.AddHours(24);
            SendReport(startTime, endDate, "Daily", log);
        }
        public static void SendPartnerWeeklyReport(ILog log)
        {
            var startOfWeek = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek + 7));
            var endDate = startOfWeek.AddDays(7);
            SendReport(startOfWeek, endDate, "Weekly", log);
        }
        public static void SendPartnerMonthlyReport(ILog log)
        {
            var currentDate = DateTime.UtcNow;
            var month = currentDate.Month;
            var year = currentDate.Year;
            if (month == 1)
            {
                year -= 1;
                month = 12;
            }
            else month -= 1;
            var firstDayOfMonth = new DateTime(year, month, 1);
            var endDate = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59);
            SendReport(firstDayOfMonth, endDate, "Monthly", log);
        }

        private static void SendReport(DateTime startTime, DateTime endDate, string subject, ILog log)
        {
            Dictionary<int, string> partnerKeys;
            Dictionary<int, string> partners;
            Dictionary<int, string> messageTemplates;
            Dictionary<int, int> newPlayers;
            Dictionary<int, int> activePlayers;
            Dictionary<int, decimal> withdrawableBalance;
            Dictionary<int, decimal> bonusConversion;
            Dictionary<int, List<fnReportByPaymentSystem>> withdrawalReportByPaymentSystem;
            Dictionary<int, List<fnReportByPaymentSystem>> depositReportByPaymentSystem;
            Dictionary<int, List<fnReportByProvider>> reportByGameProvider;
            var fDate = startTime.Year * 1000000 + startTime.Month * 10000 + startTime.Day * 100 + startTime.Hour;
            var tDate = endDate.Year * 1000000 + endDate.Month * 10000 + endDate.Day * 100 + endDate.Hour;
            Dictionary<string, decimal> currencies;
            using (var db = new IqSoftCorePlatformEntities())
            {
                RelationalDatabaseFacadeExtensions.SetCommandTimeout(db.Database, 180);
                currencies = db.Currencies.ToDictionary(x => x.Id, x => x.CurrentRate);

                partnerKeys = db.WebSiteMenuItems.Where(x => x.Menu.Type == "Config" && x.Title == Constants.PartnerKeys.PartnerEmails)
                                                 .ToDictionary(x => x.Menu.PartnerId, x => x.Href);
                partners = db.Partners.Where(x => partnerKeys.ContainsKey(x.Id)).ToDictionary(x => x.Id, x => x.Name);
                newPlayers = db.Clients.Where(x => x.CreationTime >= startTime && partnerKeys.ContainsKey(x.PartnerId))
                                            .GroupBy(x => x.PartnerId)
                                            .ToDictionary(x => x.Key, x => x.Count());
                activePlayers = db.ClientSessions.Where(x => x.StartTime >= startTime && partnerKeys.ContainsKey(x.Client.PartnerId)&&
                                                             x.ProductId == Constants.PlatformProductId)
                                                 .Select(x => new { x.Client.PartnerId, x.ClientId }).Distinct()
                                                 .GroupBy(x => x.PartnerId)
                                                 .ToDictionary(x => x.Key, x => x.Count());

                withdrawableBalance = (from acc in db.Accounts
                                       join c in db.Clients on acc.ObjectId equals c.Id
                                       join p in db.Partners on c.PartnerId equals p.Id
                                       join crr in db.Currencies on acc.CurrencyId equals crr.Id
                                       where acc.ObjectTypeId == (int)ObjectTypes.Client &&
                                             acc.Type.Kind != (int)AccountTypeKinds.Booked &&
                                             acc.TypeId != (int)AccountTypes.ClientCoinBalance &&
                                             acc.TypeId != (int)AccountTypes.ClientCompBalance &&
                                             acc.TypeId != (int)AccountTypes.ClientBonusBalance &&
                                             partnerKeys.ContainsKey(c.PartnerId)
                                       select new
                                       {
                                           c.PartnerId,
                                           PartnerCurrencyId = p.CurrencyId,
                                           USDBalance = acc.Balance * crr.CurrentRate
                                       }).GroupBy(x => new { x.PartnerId, x.PartnerCurrencyId }).ToDictionary(x => x.Key.PartnerId, x => x.Sum(y => y.USDBalance) / currencies[x.Key.PartnerCurrencyId]);

                bonusConversion = db.Documents.Where(x => x.Date >= fDate && partnerKeys.ContainsKey(x.Client.PartnerId)&&
                                                            x.OperationTypeId == (int)OperationTypes.BonusWin)
                                                .Select(x => new
                                                {
                                                    x.Client.PartnerId,
                                                    PartnerCurrencyId = x.Client.Partner.CurrencyId,
                                                    USDAmount = x.Amount * x.Client.Currency.CurrentRate
                                                }).GroupBy(x => new { x.PartnerId, x.PartnerCurrencyId }).ToDictionary(x => x.Key.PartnerId, x => x.Sum(y => y.USDAmount) / currencies[x.Key.PartnerCurrencyId]);

                withdrawalReportByPaymentSystem = db.fn_ReportByPaymentSystem(fDate, tDate, (int)PaymentSettingTypes.Withdraw)
                    .Where(x => x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually)
                                          .GroupBy(x => x.PartnerId).ToDictionary(x => x.Key, x => x.ToList());

                depositReportByPaymentSystem = db.fn_ReportByPaymentSystem(fDate, tDate, (int)PaymentSettingTypes.Deposit)
                    .Where(x => x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually)
                                          .GroupBy(x => x.PartnerId).ToDictionary(x => x.Key, x => x.ToList());

                reportByGameProvider = db.fn_ReportByProvider(fDate, tDate).GroupBy(x => x.PartnerId ?? 0)
                    .ToDictionary(x => x.Key, x => x.ToList());

                var messageInfoType = (int)Enum.Parse(typeof(ClientInfoTypes), ("Partner" + subject + "Report"));
                messageTemplates = db.fn_MessageTemplate(Constants.DefaultLanguageId).Where(x => x.ClientInfoType == messageInfoType)
                    .ToDictionary(x => x.PartnerId, x => x.Text);
            }
            using var notificationBll = new NotificationBll(new SessionIdentity(), log);
            foreach (var partnerEmails in partnerKeys)
            {
                try
                {
                    var partner = CacheManager.GetPartnerById(partnerEmails.Key);
                    var fileName = string.Format("{0}_{1}.csv", partners[partnerEmails.Key], startTime.ToString("yyyyMMdd"));
                    var msgText = messageTemplates.ContainsKey(partnerEmails.Key) ? messageTemplates[partnerEmails.Key] : string.Empty;
                    msgText = msgText.Replace("{p}", partners[partnerEmails.Key])
                                     .Replace("{sd}", startTime.ToString())
                                     .Replace("{ed}", endDate.ToString());
                    var report = new PartnerReport
                    {
                        NewPlayers = !newPlayers.ContainsKey(partnerEmails.Key) ? 0 : newPlayers[partnerEmails.Key],
                        ActivePlayers = !activePlayers.ContainsKey(partnerEmails.Key) ? 0 : activePlayers[partnerEmails.Key],
                        WithdrawableBalance = !withdrawableBalance.ContainsKey(partnerEmails.Key) ? 0 : withdrawableBalance[partnerEmails.Key],
                        BonusConversion = !bonusConversion.ContainsKey(partnerEmails.Key) ? 0 : bonusConversion[partnerEmails.Key]
                    };
                    var gameReport = new PartnerGameReport
                    {
                        GameProvider = !reportByGameProvider.ContainsKey(partnerEmails.Key) ? null : reportByGameProvider[partnerEmails.Key].GroupBy(x => x.ProviderName)
                       .Select(x => new GameProviderReportItem
                       {
                           GameProviderName = x.Key,
                           TotalBetAmount = Math.Round(x.Sum(y => (y.TotalBetsAmount ?? 0) * currencies[y.Currency]/currencies[partner.CurrencyId]), 2),
                           TotalWinAmount = Math.Round(x.Sum(y => (y.TotalWinsAmount ?? 0)* currencies[y.Currency]/currencies[partner.CurrencyId]), 2),
                           GGR = Math.Round(x.Sum(y => y.GGR * currencies[y.Currency]/currencies[partner.CurrencyId]) ?? 0, 2)
                       }).ToList()
                    };

                    var withdrawalsReport = new PartnerWithdrawalsReport
                    {
                        Withdrawals = !withdrawalReportByPaymentSystem.ContainsKey(partnerEmails.Key) ? null : withdrawalReportByPaymentSystem[partnerEmails.Key]
                        .Select(x => new WithdrawPaymentSystemReportItem
                        {
                            PaymentSystemName = x.PaymentSystemName,
                            TotalAmount = x.TotalAmount.Value
                        }).ToList()
                    };
                    var depositsReport = new PartnerDepositsReport
                    {
                        Deposits = !depositReportByPaymentSystem.ContainsKey(partnerEmails.Key) ? null : depositReportByPaymentSystem[partnerEmails.Key]
                        .Select(x => new DepositPaymentSystemReportItem
                        {
                            PaymentSystemName = x.PaymentSystemName,
                            TotalAmount = Math.Round(x.TotalAmount.Value, 2)
                        }).ToList()
                    };

                    withdrawalsReport.TotalWithdrawalAmount = new List<TotalWithdrawalResult> {
                            new TotalWithdrawalResult { TotalAmount = withdrawalsReport.Withdrawals != null ? Math.Round(withdrawalsReport.Withdrawals.Sum(x => x.TotalAmount), 2) : 0 } };
                    depositsReport.TotalDepositAmount = new List<TotalDepositResult> {
                            new TotalDepositResult { TotalAmount = depositsReport.Deposits != null ? Math.Round(depositsReport.Deposits.Sum(x => x.TotalAmount), 2) : 0 } };
                    var totalAmounts = new GameProviderTotalItem
                    {
                        TotalBetAmount = gameReport.GameProvider != null ? Math.Round(gameReport.GameProvider.Sum(x => x.TotalBetAmount), 2) : 0,
                        TotalWinAmount = gameReport.GameProvider != null ? Math.Round(gameReport.GameProvider.Sum(x => x.TotalWinAmount), 2) : 0,
                        TotalGGR = gameReport.GameProvider != null ? Math.Round(gameReport.GameProvider.Sum(x => x.GGR)) : 0
                    };
                    var emails = partnerEmails.Value.Split(',').ToList();
                    var content = new List<string> { string.Format("{0} {1} Summary {2} - {3}", subject, partners[partnerEmails.Key], startTime, endDate) };
                    ExportExcelHelper.AddObjectToLine(new List<PartnerReport> { report }, content, false, false);
                    content.Add(string.Empty);
                    ExportExcelHelper.AddObjectToLine(new List<PartnerGameReport> { gameReport }, content, false, true);
                    content.Add(string.Empty);
                    ExportExcelHelper.AddObjectToLine(new List<GameProviderTotalItem> { totalAmounts }, content, true, false);
                    content.Add(string.Empty);
                    ExportExcelHelper.AddObjectToLine(new List<PartnerWithdrawalsReport> { withdrawalsReport }, content, false, true);
                    content.Add(string.Empty);
                    ExportExcelHelper.AddObjectToLine(new List<PartnerDepositsReport> { depositsReport }, content, false, true);

                    emails.ForEach(x =>
                    notificationBll.SendEmail("0", partnerEmails.Key, x, subject + " report", msgText, null, fileName, string.Join(Environment.NewLine, content)));
                }
                catch (FaultException<BllFnErrorType> ex)
                {
                    log.Error(JsonConvert.SerializeObject(new { ex.Detail.Id, ex.Detail.Message, ex.Detail.DecimalInfo }));
                }
                catch (Exception e)
                {
                    log.Error(e);
                }
            }
        }

        public static List<Bet> GetBetsForReport(out int totalCount)
        {
            var selectedItems = new List<Bet>();
            totalCount = 0;

            using (var db = new IqSoftCorePlatformEntities())
            {
                var lastConsideredBet = db.PartnerKeys.Where(pk => pk.PartnerId == 1 && pk.Name == Constants.PartnerKeys.LastConsideredBetId).FirstOrDefault();
                if (lastConsideredBet != null && lastConsideredBet.Id != 0)
                {
                    long toDate = Convert.ToInt64(DateTime.UtcNow.AddDays(-1).ToString("yyyyMMddHH"));
                    var bets = db.Bets.Include(x => x.CashDesk).Where(b => b.Id > lastConsideredBet.NumericValue && b.State != (int)BetDocumentStates.Uncalculated &&
                    b.BetDate < toDate).Take(5000).ToArray();
                    totalCount = bets.Length;
                    if (totalCount > 0)
                    {
                        var rand = new Random(Guid.NewGuid().GetHashCode());
                        foreach (var item in bets)
                        {
                            //if (rand.Next(1, 101) <= Constants.percentOfReportingBets)
                            if (item.CashDeskId != null && item.CashDeskId.Value > 0)
                            {
                                selectedItems.Add(item);
                            }
                        }
                        lastConsideredBet.NumericValue = bets[bets.Length - 1].Id;
                        db.SaveChanges();
                    }
                }
            }

            return selectedItems;
        }

        public static string SendBetsToControlSystem(List<Bet> bets)
        {
            /*string controlSystemUrl = CacheManager.GetPartnerSettingByKey(1, Constants.PartnerKeys.ControlSystemUrl).StringValue;

            var input = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = controlSystemUrl,
                PostData = JsonConvert.SerializeObject(new { Bets = bets.Select(x => x.ToPlatformBet()).ToList() })
            };
            var result = CommonFunctions.SendHttpRequest(input, timeout: 120000);*/
            return string.Empty;
        }

        public static void UpdateClientWageringBonus()
        {
            using var db = new IqSoftCorePlatformEntities();
            var currentDate = DateTime.UtcNow;
            var lostBonuses = db.ClientBonus.Where(x => x.Status == (int)BonusStatuses.Active && x.Bonus.ValidForSpending != null &&
            EF.Functions.DateDiffHour(currentDate, x.AwardingTime) >= x.Bonus.ValidForSpending).ToList();

            foreach (var lb in lostBonuses)
            {
                lb.Status = (int)BonusStatuses.Lost;
                db.SaveChanges();
                BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.ActiveBonusId, lb.ClientId));
                BroadcastRemoveCache(string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientBonus, lb.ClientId, lb.BonusId));
            }
            var dbClientBonuses = db.ClientBonus.Include(x => x.Bonus).Where(x => (x.Bonus.BonusType == (int)BonusTypes.CampaignWagerCasino ||
                                                             x.Bonus.BonusType == (int)BonusTypes.CampaignWagerSport) &&
                                                             x.Status == (int)BonusStatuses.Active).ToList();

            foreach (var clientBonus in dbClientBonuses)
            {
                var date = (long)clientBonus.AwardingTime.Value.Year * 1000000 + clientBonus.AwardingTime.Value.Month * 10000 +
                    clientBonus.AwardingTime.Value.Day * 100 + clientBonus.AwardingTime.Value.Hour;
                var bonusId = clientBonus.BonusId.ToString();
                if (db.Documents.Any(x => x.Date >= date && x.ClientId == clientBonus.ClientId && x.OperationTypeId == (int)OperationTypes.Bet &&
                    x.State == (int)BetDocumentStates.Uncalculated && x.Info != null && x.Info != ""))
                    continue;

                if (clientBonus.TurnoverAmountLeft == 0)
                {
                    clientBonus.Status = (int)BonusStatuses.Finished;
                    db.SaveChanges();
                    BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.ActiveBonusId, clientBonus.ClientId));
                    BroadcastRemoveCache(string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientBonus, clientBonus.ClientId, clientBonus.BonusId));
                }
                else
                {
                    var balance = db.Accounts.Where(x => x.ObjectTypeId == (int)ObjectTypes.Client && x.ObjectId == clientBonus.ClientId &&
                                                         x.TypeId == (int)AccountTypes.ClientBonusBalance).Sum(x => x.Balance);
                    if (balance == 0)
                    {
                        clientBonus.Status = (int)BonusStatuses.Finished;
                        db.SaveChanges();
                        BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.ActiveBonusId, clientBonus.ClientId));
                        BroadcastRemoveCache(string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientBonus, clientBonus.ClientId, clientBonus.BonusId));
                    }
                }
            }
        }

        public static void FinalizeWageringBonus(ILog log)
        {
            var clientList = new List<ClientBonu>();
            using (var documentBl = new DocumentBll(new SessionIdentity(), log))
            {
                clientList = documentBl.FinalizeWageringBonusDocument();
            }
            foreach (var clientB in clientList)
            {
                BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.ActiveBonusId, clientB.ClientId));
                BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, clientB.ClientId));
                BroadcastRemoveCache(string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientBonus, clientB.ClientId, clientB.BonusId));
            }
        }

        public static void SendActiveMerchantRequests(ILog log)
        {
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = HttpMethod.Post
            };

            using var db = new IqSoftCorePlatformEntities();
            var activeMerchantRequests = db.MerchantRequests.Where(x => x.Status == (int)MerchantRequestStates.Active).ToList();

            foreach (var merchantRequest in activeMerchantRequests)
            {
                try
                {
                    httpRequestInput.Url = merchantRequest.RequestUrl;
                    httpRequestInput.PostData = merchantRequest.Content;
                    ++merchantRequest.RetryCount;
                    merchantRequest.Response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                    merchantRequest.Status = (int)MerchantRequestStates.Success;
                }
                catch (Exception e)
                {
                    if (merchantRequest.RetryCount >= 20)
                        merchantRequest.Status = (int)MerchantRequestStates.Failed;
                    log.Error(e);
                }
            }
            db.SaveChanges();
        }

        public static void SendAffiliateReport(ILog log)
        {
            try
            {
                using var db = new IqSoftCorePlatformEntities();
                var currentTime = DateTime.UtcNow;
                var affiliatePlatforms = db.AffiliatePlatforms.Where(x => Constants.ReportingAffiliates.Contains(x.Name) && x.LastExecutionTime.HasValue &&
                                                                          x.StepInHours.HasValue && x.Status ==(int)BaseStates.Active &&
                                                                          (x.StepInHours ?? 0) <= EF.Functions.DateDiffHour(currentTime, x.LastExecutionTime.Value))
                                                              .Select(x => x.Id).ToList();
                if (affiliatePlatforms.Count <= 0)
                    return;

                using (var baseBll = new BaseBll(new SessionIdentity(), log))
                using (var affiliateService = new AffiliateService(baseBll))
                {
                    var clientsByAffiliate = db.Clients.Where(x => affiliatePlatforms.Contains(x.AffiliateReferral.AffiliatePlatform.Id))
                                                       .Select(x => new DAL.Models.Affiliates.AffiliatePlatformModel
                                                       {
                                                           PartnerId = x.PartnerId,
                                                           ClientId =x.Id,
                                                           AffiliateId = x.AffiliateReferral.AffiliatePlatform.Id,
                                                           AffiliateName = x.AffiliateReferral.AffiliatePlatform.Name,
                                                           ClickId = x.AffiliateReferral.RefId,
                                                           RegistrationDate = x.CreationTime,
                                                           CountryCode = x.Region.IsoCode,
                                                           Language = x.LanguageId,
                                                           CurrencyId = x.CurrencyId,
                                                           LastExecutionTime = x.AffiliateReferral.AffiliatePlatform.LastExecutionTime.Value,
                                                           KickOffTime = x.AffiliateReferral.AffiliatePlatform.KickOffTime,
                                                           StepInHours = x.AffiliateReferral.AffiliatePlatform.StepInHours.Value
                                                       })
                                                       .GroupBy(x => new { x.PartnerId, x.AffiliateName, x.AffiliateId, x.KickOffTime, x.LastExecutionTime, x.StepInHours })
                                                       .ToList();

                    foreach (var affClient in clientsByAffiliate)
                    {
                        var brand = CacheManager.GetPartnerSettingByKey(affClient.Key.PartnerId, affClient.Key.AffiliateName + Constants.PartnerKeys.AffiliateBrandId).StringValue;
                        var ftpUrl = CacheManager.GetPartnerSettingByKey(affClient.Key.PartnerId, affClient.Key.AffiliateName + Constants.PartnerKeys.AffiliateFtpUrl).StringValue;
                        var ftpUsername = CacheManager.GetPartnerSettingByKey(affClient.Key.PartnerId, affClient.Key.AffiliateName + Constants.PartnerKeys.AffiliateFtpUsername).StringValue;
                        var ftpPassword = CacheManager.GetPartnerSettingByKey(affClient.Key.PartnerId, affClient.Key.AffiliateName + Constants.PartnerKeys.AffiliateFtpPassword).StringValue;
                        var ftpModel = new FtpModel
                        {
                            Url = ftpUrl,
                            UserName = ftpUsername,
                            Password = ftpPassword
                        };
                        if (!Int32.TryParse(brand, out int brandId))
                            continue;

                        var fromDate = affClient.Key.KickOffTime.Value;
                        var fDate = fromDate.Year * (long)1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
                        var upToDate = affClient.Key.LastExecutionTime.AddHours(affClient.Key.StepInHours);
                        var tDate = upToDate.Year * (long)1000000 + upToDate.Month * 10000 + upToDate.Day * 100 + upToDate.Hour;
                        var dateString = fromDate.ToString("yyyy-MM-dd");

                        var newRegisteredClients = affClient.Where(x => x.RegistrationDate >= fromDate && x.RegistrationDate < upToDate)
                                                            .Select(x => new DAL.Models.Affiliates.RegistrationActivityModel
                                                            {
                                                                CustomerId = x.ClientId,
                                                                CountryCode = x.CountryCode,
                                                                BTag = x.ClickId,
                                                                RegistrationDate = x.RegistrationDate.ToString("yyyy-MM-dd"),
                                                                BrandId = brandId,
                                                                LanguageId = x.Language
                                                            }).ToList();
                        log.Info("NewClient: " + newRegisteredClients.Count);
                        var content = new StringBuilder();

                        switch (affClient.Key.AffiliateName)
                        {
                            case AffiliatePlatforms.MyAffiliates:
                            case AffiliatePlatforms.Netrefer:
                            case AffiliatePlatforms.Intelitics:
                                var affiliateClientActivies = affiliateService.GetClientActivity(affClient.ToList(), brandId, fromDate, tDate);
                                content.AppendLine(string.Join(",", typeof(AffiliateReportInput).GetProperties().Select(x => x.Name.ToUpper()).ToArray()));
                                if (newRegisteredClients.Any())
                                {
                                    content.AppendLine(newRegisteredClients.Aggregate(string.Empty, (current, item) => current +
                                    string.Format("{0},{1},{2},{3},{4},{5}", item.BrandId, item.CustomerId, item.BTag, item.CountryCode, item.RegistrationDate, item.LanguageId) + Environment.NewLine));
                                }
                                baseBll.SFTPUpload(content.ToString(), "/CasinoName_signups_" + dateString + ".csv", ftpModel);
                                content.Clear();
                                content.AppendLine(string.Join(",", typeof(DAL.Models.Affiliates.ClientActivityModel).GetProperties().Select(x => x.Name.ToUpper()).ToArray()));
                                if (affiliateClientActivies.Any())
                                {
                                    content.AppendLine(affiliateClientActivies.Aggregate(string.Empty, (current, item) => current +
                                    string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                                    item.BrandId, item.CustomerId, item.CurrencyId, item.BTag, item.ActivityDate,
                                    item.SportGrossRevenue, item.CasinoGrossRevenue, item.SportBonusBetsAmount, item.CasinoBonusBetsAmount,
                                    item.SportBonusWinsAmount, item.CasinoBonusWinsAmount, item.SportTotalWinAmount, item.CasinoTotalWinAmount,
                                    item.Deposits, item.Withdrawals, item.TotalTransactions) + Environment.NewLine));
                                }
                                baseBll.SFTPUpload(content.ToString(), "/CasinoName_activity_" + dateString + ".csv", ftpModel);
                                break;
                            case AffiliatePlatforms.DIM:
                                var dimClientActivies = affiliateService.GetDIMClientActivity(affClient.ToList(), brandId, fromDate, tDate);

                                if (newRegisteredClients.Any())
                                {
                                    content.AppendLine("ClickId,PIN,RegDate,RegCountry,RegLanguage,RegCategory");
                                    content.AppendLine(newRegisteredClients.Aggregate(string.Empty, (current, item) => current +
                                    string.Format("{0},{1},{2},{3},{4},{5}", item.BTag, item.CustomerId, item.RegistrationDate, item.CountryCode, item.LanguageId, 100) + Environment.NewLine));
                                }
                                baseBll.UploadFile(content.ToString(), "/NewAccounts_" + dateString + ".csv", ftpModel);
                                content.Clear();
                                if (dimClientActivies.Any(x => x.PaymentTransactions > 0))
                                {
                                    content.AppendLine("PIN,AmountDate,Amount,AmountCurrency,AmountCategory");
                                    content.AppendLine(dimClientActivies.Where(x => x.PaymentTransactions > 0).Aggregate(string.Empty, (current, item) => current +
                                    string.Format("{0},{1},{2},{3},{4}", item.CustomerId, item.ActivityDate, item.Deposits, item.CurrencyId, 100) + Environment.NewLine));
                                    baseBll.UploadFile(content.ToString(), "/Deposits_" + dateString + ".csv", ftpModel);
                                    content.Clear();
                                }
                                else
                                    baseBll.UploadFile(content.ToString(), "/Deposits_" + dateString + ".csv", ftpModel);

                                if (dimClientActivies.Any(x => x.TotalTransactions > 0))
                                {
                                    content.AppendLine("PIN,AmountDate,Amount,AmountCurrency,AmountCategory");
                                    content.AppendLine(dimClientActivies.Where(x => x.TotalTransactions > 0).Aggregate(string.Empty, (current, item) => current +
                                    string.Format("{0},{1},{2},{3},{4}", item.CustomerId, item.ActivityDate, item.PokerGrossRevenue, item.CurrencyId, "poker") + Environment.NewLine +
                                    string.Format("{0},{1},{2},{3},{4}", item.CustomerId, item.ActivityDate, item.MahjongGrossRevenue, item.CurrencyId, "mahjong") + Environment.NewLine +
                                    string.Format("{0},{1},{2},{3},{4}", item.CustomerId, item.ActivityDate, item.SportGrossRevenue + item.CasinoGrossRevenue - item.TotalConvertedBonusAmount,
                                    item.CurrencyId, "sport+casino") + Environment.NewLine));
                                    baseBll.UploadFile(content.ToString(), "/NetRevenues_" + dateString + ".csv", ftpModel);
                                }
                                else
                                    baseBll.UploadFile(content.ToString(), "/NetRevenues_" + dateString + ".csv", ftpModel);
                                break;
                            default:
                                break;
                        }
                        var affiliatePlaform = db.AffiliatePlatforms.FirstOrDefault(x => x.Id == affClient.Key.AffiliateId);
                        affiliatePlaform.LastExecutionTime = upToDate;
                        if (affiliatePlaform.KickOffTime.Value.AddHours(affiliatePlaform.PeriodInHours.Value) <= upToDate)
                        {
                            affiliatePlaform.KickOffTime = affiliatePlaform.KickOffTime.Value.AddHours(affiliatePlaform.PeriodInHours.Value);
                        }
                        db.SaveChanges();

                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        public static void CalculateAgentsGGRProfit(Job job, ILog log)
        {
            try
            {
                var toDate = job.NextExecutionTime;
                var currentTime = DateTime.UtcNow;
                var tDate = toDate.Year * (int)1000000 + toDate.Month * 10000 + 100;
                var fromDate = toDate.AddMonths(-1);
                var fDate = fromDate.Year * (int)1000000 + fromDate.Month * 10000 + 100;

                using var userBll = new UserBll(new SessionIdentity(), log);
                using var partnerBll = new PartnerBll(userBll);
                using var documentBll = new DocumentBll(userBll);
                var agentsProfit = userBll.GetAgentProfit(fDate, tDate);
                foreach (var agentProfit in agentsProfit)
                {
                    if (agentProfit.TotalProfit > 0)
                    {
                        try
                        {
                            var agent = userBll.GetUserById(agentProfit.AgentId);
                            var commissionTypes = partnerBll.GetPartnerKey(agent.PartnerId, Constants.PartnerKeys.PartnerCommissionType).StringValue.Split('|').Select(Int32.Parse).ToList();
                            var amount = agentProfit.TotalProfit.Value;
                            if (commissionTypes.Contains((int)CommissionTypes.GGRWithoutTurnover))
                            {
                                var transactions = userBll.GetAgentTurnoverProfit(fDate, tDate);
                                var item = transactions.FirstOrDefault(x => x.RecieverAgentId == agentProfit.RecieverAgentId && x.SenderAgentId == agentProfit.AgentId);
                                amount = Math.Max(0, amount - (item != null ? item.TotalProfit : 0));
                            }
                            if (((commissionTypes.Contains((int)CommissionTypes.GGRWithoutTurnover) ||
                                  commissionTypes.Contains((int)CommissionTypes.GGRWithTurnover)) && amount > 0) ||
                                (commissionTypes.Contains((int)CommissionTypes.PT) && amount != 0))
                            {
                                var userTranferInput = new UserTransferInput
                                {
                                    FromUserId = agentProfit.AgentId != agentProfit.RecieverAgentId ? agentProfit.AgentId : (int?)null,
                                    UserId = agentProfit.RecieverAgentId,
                                    Amount = amount,
                                    ExternalTransactionId = string.Format("2_{0}", tDate.ToString()),
                                    ProductId = agentProfit.ProductId
                                };
                                userBll.TransferToUser(userTranferInput, documentBll);
                                var dbAgentProfit = new AgentProfit
                                {
                                    AgentId = agentProfit.RecieverAgentId,
                                    FromAgentId = agentProfit.AgentId != agentProfit.RecieverAgentId ? agentProfit.AgentId : (int?)null,
                                    TotalBetAmount = agentProfit.TotalBetAmount.Value,
                                    TotalWinAmount = agentProfit.TotalWinAmount.Value,
                                    GGR = agentProfit.TotalBetAmount.Value - agentProfit.TotalWinAmount.Value,
                                    Profit = amount,
                                    Type = (int)AgentProfitTypes.GGR,
                                    ProductGroupId = agentProfit.ProductGroupId.Value,
                                    ProductId = agentProfit.ProductId,
                                    CreationTime = toDate,
                                    CalculationStartingTime = fromDate,
                                    CreationDate = tDate,
                                    CalculationStartingDate = fDate,
                                };
                                userBll.AddAgentProfit(dbAgentProfit);
                            }
                        }
                        catch (Exception e)
                        {
                            log.Error(e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        public static void CalculateAgentsTurnoverProfit(DateTime toDate, ILog log)
        {

            var currentTime = DateTime.UtcNow;
            var fromDate = toDate.AddHours(-1);

            var tDate = toDate.Year * (int)1000000 + toDate.Month * 10000 + toDate.Day * 100 + toDate.Hour;
            var fDate = fromDate.Year * (int)1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
            try
            {
                using (var userBll = new UserBll(new SessionIdentity(), log))
                {
                    using (var documentBll = new DocumentBll(userBll))
                    {
                        using (var clientBll = new ClientBll(userBll))
                        {
                            var agentsProfit = userBll.GetAgentTurnoverProfit(fDate, tDate);
                            foreach (var agentProfit in agentsProfit)
                            {
                                if (agentProfit.TotalProfit > 0)
                                {
                                    try
                                    {
                                        if (agentProfit.RecieverAgentId != null)
                                        {
                                            var userTranferInput = new UserTransferInput
                                            {
                                                FromUserId = agentProfit.SenderAgentId != agentProfit.RecieverAgentId ? agentProfit.SenderAgentId : (int?)null,
                                                UserId = agentProfit.RecieverAgentId,
                                                Amount = agentProfit.TotalProfit,
                                                ExternalTransactionId = string.Format("1_{0}", tDate.ToString()),
                                                ProductId = agentProfit.ProductId
                                            };
                                            userBll.TransferToUser(userTranferInput, documentBll);
                                        }
                                        else
                                        {
                                            var client = CacheManager.GetClientById(agentProfit.RecieverClientId.Value);
                                            var input = new ClientOperation
                                            {
                                                ClientId = agentProfit.RecieverClientId,
                                                Amount = agentProfit.TotalProfit,
                                                OperationTypeId = (int)OperationTypes.CommissionForClient,
                                                PartnerId = client.PartnerId,
                                                CurrencyId = client.CurrencyId,
                                                AccountTypeId = (int)AccountTypes.ClientUnusedBalance
                                            };
                                            clientBll.CreateDebitToClientFromJob(client.Id, input, documentBll);
                                        }
                                        var dbAgentProfit = new AgentProfit
                                        {
                                            AgentId = agentProfit.RecieverAgentId,
                                            ClientId = agentProfit.RecieverClientId,
                                            FromAgentId = agentProfit.SenderAgentId != agentProfit.RecieverAgentId ? agentProfit.SenderAgentId : (int?)null,
                                            TotalBetAmount = agentProfit.TotalBetAmount,
                                            TotalWinAmount = agentProfit.TotalWinAmount,
                                            GGR = agentProfit.TotalBetAmount - agentProfit.TotalWinAmount,
                                            Profit = agentProfit.TotalProfit,
                                            Type = (int)AgentProfitTypes.Turnover,
                                            ProductGroupId = agentProfit.ProductGroupId ?? Constants.PlatformProductId,
                                            ProductId = agentProfit.ProductId,
                                            CreationTime = currentTime,
                                            CreationDate = currentTime.Year * (int)1000000 + currentTime.Month * 10000 + currentTime.Day * 100 + currentTime.Hour,
                                            CalculationStartingTime = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, fromDate.Hour, 0, 0),
                                            CalculationStartingDate = fDate
                                        };
                                        userBll.AddAgentProfit(dbAgentProfit);
                                    }
                                    catch (Exception e)
                                    {
                                        log.Error(JsonConvert.SerializeObject(agentProfit), e);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        public static void TriggerMissedDepositCRM(Job job, ILog log)
        {
            using var db = new IqSoftCorePlatformEntities();
            using var notificationBl = new NotificationBll(new SessionIdentity(), log);
            var currentTime = DateTime.UtcNow;
            var triggerSettings = db.CRMSettings.Where(x => x.Type == (int)CRMSettingTypes.MissedDeposit &&
                                                           x.State == (int)BaseStates.Active && x.StartTime <= currentTime &&
                                                           currentTime <= x.FinishTime).ToList();

            foreach (var triggerSetting in triggerSettings)
            {
                var partner = db.Partners.FirstOrDefault(x => x.Id == triggerSetting.PartnerId);
                var condition = Convert.ToInt32(triggerSetting.Condition);

                var toDate = job.NextExecutionTime.AddHours(-condition);
                var fromDate = toDate.AddSeconds(-job.PeriodInSeconds);

                var clients = db.Clients.Where(x => x.PartnerId == triggerSetting.PartnerId &&
                    x.LastDepositDate >= fromDate && x.LastDepositDate < toDate).ToList();
                clients.AddRange(db.Clients.Where(x => x.PartnerId == triggerSetting.PartnerId &&
                    x.LastDepositDate == null && x.CreationTime >= fromDate && x.CreationTime < toDate).ToList());
                var messageTemplateNikeName = string.Format("{0}_{1}_{2}", ClientInfoTypes.MissedDepositEmail.ToString(),
                            triggerSetting.Sequence, triggerSetting.Condition);
                foreach (var client in clients)
                {
                    if (!string.IsNullOrEmpty(client.Email) &&
                        db.PaymentRequests.Count(x => x.ClientId == client.Id &&
                                                x.Type == (int)PaymentRequestTypes.Deposit &&
                                                (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually)) == triggerSetting.Sequence)
                    {
                        var messageTemplate = db.fn_MessageTemplate(client.LanguageId).Where(x => x.PartnerId == client.PartnerId &&
                            x.ClientInfoType == (int)ClientInfoTypes.MissedDepositEmail && x.NickName == messageTemplateNikeName).FirstOrDefault();

                        var messageTextTemplate = messageTemplate.Text.Replace("\\n", Environment.NewLine)
                                                   .Replace("{u}", client.UserName)
                                                   .Replace("{w}", partner.SiteUrl.Split(',')[0])
                                                   .Replace("{pc}", client.Id.ToString())
                                                   .Replace("{fn}", client.FirstName)
                                                   .Replace("{e}", client.Email)
                                                   .Replace("{m}", client.MobileNumber);// to be added
                        notificationBl.SaveEmailMessage(client.PartnerId, client.Id, client.Email, partner.Name, messageTextTemplate, messageTemplate.Id);
                    }
                }
                db.SaveChanges();
            }
        }

        public static void NotifyIdentityExpiration(ILog log)
        {
            using var db = new IqSoftCorePlatformEntities();
            using var notificationBl = new NotificationBll(new SessionIdentity(), log);
            var currentTime = DateTime.UtcNow;
            var finishTime = currentTime.AddMonths(1);
            var finishData = finishTime.Year * 10000 + finishTime.Month * 100 + finishTime.Day;

            var clientIdenities = db.ClientIdentities.Include(x => x.Client.Partner).Where(x => x.ExpirationDate < finishData && x.Status == (int)KYCDocumentStates.Approved).ToList();
            foreach (var ci in clientIdenities)
            {
                ci.Status = (int)KYCDocumentStates.CloseToExpiration;
                var oldClientIdentity = new
                {
                    Id = ci.Id,
                    ClientId = ci.ClientId,
                    UserId = (int?)null,
                    DocumentTypeId = ci.DocumentTypeId,
                    ImagePath = ci.ImagePath,
                    Status = ci.Status,
                    CreationTime = ci.CreationTime,
                    LastUpdateTime = ci.LastUpdateTime,
                    ExpirationTime = ci.ExpirationTime
                };
                notificationBl.SaveChangesWithHistory((int)ObjectTypes.ClientIdentity, ci.Id, JsonConvert.SerializeObject(oldClientIdentity), "System");
            }
            foreach (var c in clientIdenities)
            {
                var messageTemplate = db.fn_MessageTemplate(c.Client.LanguageId).Where(x => x.PartnerId == c.Client.PartnerId && x.ClientInfoType == (int)ClientInfoTypes.IdentityCloseToExpire).FirstOrDefault();
                if (messageTemplate != null)
                {
                    var messageTextTemplate = messageTemplate.Text.Replace("\\n", Environment.NewLine)
                                                   .Replace("{u}", c.Client.UserName)
                                                   .Replace("{w}", c.Client.Partner.SiteUrl.Split(',')[0])
                                                   .Replace("{pc}", c.Client.Id.ToString())
                                                   .Replace("{fn}", c.Client.FirstName)
                                                   .Replace("{e}", c.Client.Email)
                                                   .Replace("{m}", c.Client.MobileNumber);// to be added
                    notificationBl.SaveEmailMessage(c.Client.PartnerId, c.Client.Id, c.Client.Email, c.Client.Partner.Name, messageTextTemplate, messageTemplate.Id);
                }
            }
        }

        public static void CheckInactiveClients(ILog log)
        {
            using var db = new IqSoftCorePlatformEntities();
            using var clientBll = new ClientBll(new SessionIdentity(), log);
            using var notificationBl = new NotificationBll(clientBll.Identity, log);
            var partnerSetting = db.WebSiteMenuItems.Where(x => x.Menu.Type == Constants.WebSiteConfiguration.Config && x.Title == Constants.PartnerKeys.BlockForInactivity &&
                                           x.Href != "0").ToDictionary(x => x.Menu.PartnerId, x => Convert.ToInt32(x.Href));
            var currentDate = DateTime.UtcNow;
            foreach (var partner in partnerSetting)
            {
                var clients = db.Clients.Include(x => x.Partner)
                                        .Where(x => x.PartnerId == partner.Key && (x.State == (int)ClientStates.Active || x.State == (int)ClientStates.Suspended) &&
                                                    EF.Functions.DateDiffDay(currentDate, x.LastSession.EndTime) >= partner.Value &&
                                                    !x.ClientSettings.Any(y => y.Name == Constants.ClientSettings.BlockedForInactivity && y.NumericValue == 1))
                                        .ToList();
                foreach (var c in clients)
                {
                    clientBll.AddOrUpdateClientSetting(c.Id, Constants.ClientSettings.BlockedForInactivity, 1, string.Empty, null, null, "System");
                    BroadcastRemoveCache(string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSettings, c.Id, Constants.ClientSettings.BlockedForInactivity));
                    var messageTemplate = db.fn_MessageTemplate(c.LanguageId).Where(x => x.PartnerId == c.PartnerId && x.ClientInfoType == (int)ClientInfoTypes.ClientInactivityEmail).FirstOrDefault();
                    if (messageTemplate != null)
                    {
                        var messageTextTemplate = messageTemplate.Text.Replace("\\n", Environment.NewLine)
                                                       .Replace("{u}", c.UserName)
                                                       .Replace("{w}", c.Partner.SiteUrl.Split(',')[0])
                                                       .Replace("{pc}", c.Id.ToString())
                                                       .Replace("{fn}", c.FirstName)
                                                       .Replace("{e}", c.Email)
                                                       .Replace("{m}", c.MobileNumber);
                        notificationBl.SaveEmailMessage(c.PartnerId, c.Id, c.Email, c.Partner.Name, messageTextTemplate, messageTemplate.Id);
                    }
                }
            }
        }

        public static void CheckInactiveUsers()
        {
            using var db = new IqSoftCorePlatformEntities();
            var partnerSetting = db.WebSiteMenuItems.Where(x => x.Menu.Type == Constants.WebSiteConfiguration.Config && x.Title == Constants.PartnerKeys.BlockUserForInactivity &&
                                           x.Href != "0").ToDictionary(x => x.Menu.PartnerId, x => Convert.ToInt32(x.Href));
            var currentDate = DateTime.UtcNow;
            foreach (var partner in partnerSetting)
            {
                db.Users.Where(x => x.PartnerId == partner.Key && (x.State == (int)UserStates.Active || x.State == (int)UserStates.Suspended) &&
                                    EF.Functions.DateDiffDay(currentDate, x.Session.EndTime) >= partner.Value)
                        .UpdateFromQuery(x => new User { State = (int)UserStates.InactivityClosed });

            }
        }

        public static void CheckClientBlockedSessions(ILog log)
        {
            using var db = new IqSoftCorePlatformEntities();
            using var notificationBl = new NotificationBll(new SessionIdentity(), log);
            using var clientBl = new ClientBll(notificationBl);
            var currentTime = DateTime.UtcNow;
            var excludedClients = db.ClientSettings.Include(x => x.Client.Partner).
                Where(x => (x.Name == Constants.ClientSettings.SelfExcluded || x.Name == Constants.ClientSettings.SystemExcluded)
                && x.NumericValue == 1 && x.DateValue < currentTime).GroupBy(x => x.Client.PartnerId).ToList();
            foreach (var partnerClients in excludedClients)
            {
                var clientsGroupedByLang = partnerClients.GroupBy(x => x.Client.LanguageId);
                foreach (var clients in clientsGroupedByLang)
                {
                    var messageTemplate = db.fn_MessageTemplate(clients.Key).Where(y => y.PartnerId == partnerClients.Key &&
                          y.ClientInfoType == (int)ClientInfoTypes.SelfExclusionFinished).FirstOrDefault();
                    foreach (var c in clients)
                    {
                        if (messageTemplate != null)
                        {
                            var messageTextTemplate = messageTemplate.Text.Replace("\\n", Environment.NewLine)
                                                         .Replace("{u}", c.Client.UserName)
                                                         .Replace("{w}", c.Client.Partner.SiteUrl.Split(',')[0])
                                                         .Replace("{pc}", c.Client.Id.ToString())
                                                         .Replace("{fn}", c.Client.FirstName)
                                                         .Replace("{e}", c.Client.Email)
                                                         .Replace("{m}", c.Client.MobileNumber);
                            notificationBl.SaveEmailMessage(c.Client.PartnerId, c.Client.Id, c.Client.Email, c.Client.Partner.Name, messageTextTemplate, messageTemplate.Id);
                        }
                        var oldSettings = JsonConvert.SerializeObject(clientBl.GetClientsSettings(c.Client.Id, false).Select(x => new
                        {
                            x.Name,
                            StringValue = string.IsNullOrEmpty(x.StringValue) ? (x.NumericValue.HasValue ? x.NumericValue.Value.ToString() : String.Empty) : x.StringValue,
                            DateValue = x.DateValue ?? x.CreationTime,
                            LastUpdateTime = x.LastUpdateTime
                        }).ToList());
                        notificationBl.SaveChangesWithHistory((int)ObjectTypes.ClientSetting, c.Client.Id, oldSettings, "System");
                        c.NumericValue = 0;
                    }
                }
                db.SaveChanges();
            }
        }
        //use existing functions instead
        public static void CheckForceBlockedClients()
        {
            /*using (var db = new IqSoftCorePlatformEntities())
            {
                var currentTime = DateTime.UtcNow;
                var clientIds = db.ObjectActions.Where(x => x.ObjectTypeId == (int)ObjectTypes.Client &&
                                                            x.Type == (int)ObjectActionTypes.BlockClientForce &&
                                                            x.FinishTime < currentTime && x.State == (int)BaseStates.Active)
                                                .Select(x => x.ObjectId).ToList();
                if (clientIds.Count > 0)
                {
                    var clients = db.Clients.Where(x => x.State == (int)ClientStates.ForceBlock && clientIds.Contains(x.Id)).ToList();
                    clients.ForEach(c =>
                    {
                        var parentState = CacheManager.GetClientSettingByName(c.Id, Constants.ClientSettings.ParentState).NumericValue;
                        c.State = parentState == 0 || parentState == (int)ClientStates.ForceBlock ? (int)ClientStates.Active : (int)parentState;
                    });
                    db.SaveChanges();
                    db.ObjectActions.Where(x => x.ObjectTypeId == (int)ObjectTypes.Client && x.Type == (int)ObjectActionTypes.BlockClientForce &&
                    x.FinishTime < currentTime && x.State == (int)BaseStates.Active).UpdateFromQuery(x => new ObjectAction { State = (int)BaseStates.Inactive });
                }
            }*/
        }

        public static void CheckUserBlockedSessions()
        {
            using var db = new IqSoftCorePlatformEntities();
            var currentTime = DateTime.UtcNow;
            var userIds = db.ObjectActions.Where(x => x.ObjectTypeId == (int)ObjectTypes.User &&
            (x.Type == (int)UserStates.ForceBlock || x.Type == (int)UserStates.ForceBlockBySecurityCode) &&
            x.FinishTime < currentTime && x.State == (int)BaseStates.Active).Select(x => x.ObjectId).ToList();
            if (userIds.Count > 0)
            {
                db.Users.Where(x => (x.State == (int)UserStates.ForceBlock || x.State == (int)UserStates.ForceBlockBySecurityCode) && userIds.Contains(x.Id)).
                    UpdateFromQuery(x => new User { State = (int)UserStates.Active });
                db.ObjectActions.Where(x => x.ObjectTypeId == (int)ObjectTypes.User && x.Type == (int)ObjectActionTypes.BlockUserForce &&
                x.FinishTime < currentTime && x.State == (int)BaseStates.Active).UpdateFromQuery(x => new ObjectAction { State = (int)BaseStates.Inactive });
            }
        }

        public static void DeactivateExiredKYC(ILog log)
        {
            using var db = new IqSoftCorePlatformEntities();
            using var notificationBl = new NotificationBll(new SessionIdentity(), log);
            var currentTime = DateTime.UtcNow;
            var currentData = currentTime.Year * 10000 + currentTime.Month * 100 +
                                 currentTime.Day;
            var clientIdenities = db.ClientIdentities.Include(x => x.Client).Where(x => x.ExpirationDate < currentData && x.Status != (int)KYCDocumentStates.Expired).ToList();

            foreach (var ci in clientIdenities)
            {
                ci.Status = (int)KYCDocumentStates.Expired;
                var oldClientIdentity = new
                {
                    Id = ci.Id,
                    ClientId = ci.ClientId,
                    UserId = (int?)null,
                    DocumentTypeId = ci.DocumentTypeId,
                    ImagePath = ci.ImagePath,
                    Status = ci.Status,
                    CreationTime = ci.CreationTime,
                    LastUpdateTime = ci.LastUpdateTime,
                    ExpirationTime = ci.ExpirationTime
                };
                notificationBl.SaveChangesWithHistory((int)ObjectTypes.ClientIdentity, ci.Id, JsonConvert.SerializeObject(oldClientIdentity), "System");
            }
            foreach (var c in clientIdenities)
            {
                var messageTemplate = db.fn_MessageTemplate(c.Client.LanguageId).Where(x => x.PartnerId == c.Client.PartnerId && x.ClientInfoType == (int)ClientInfoTypes.IdentityExpired).FirstOrDefault();
                if (messageTemplate != null)
                {
                    var messageTextTemplate = messageTemplate.Text.Replace("\\n", Environment.NewLine)
                                                   .Replace("{u}", c.Client.UserName)
                                                   .Replace("{w}", c.Client.Partner.SiteUrl.Split(',')[0])
                                                   .Replace("{pc}", c.Client.Id.ToString())
                                                   .Replace("{fn}", c.Client.FirstName)
                                                   .Replace("{e}", c.Client.Email)
                                                   .Replace("{m}", c.Client.MobileNumber);// to be added
                    notificationBl.SaveEmailMessage(c.Client.PartnerId, c.Client.Id, c.Client.Email, c.Client.Partner.Name, messageTextTemplate, messageTemplate.Id);
                }
            }

            db.SaveChanges();
        }

        public static void InactivateImpossiblBonuses()
        {
            var currentDate = DateTime.UtcNow;
            using var db = new IqSoftCorePlatformEntities();
            var impossibleTriggerGroups = db.TriggerGroups.Where(x => (x.Type == (int)TriggerGroupType.All && x.TriggerGroupSettings.Any(y => y.Setting.FinishTime < currentDate)) ||
            (x.Type == (int)TriggerGroupType.Any && x.TriggerGroupSettings.All(y => y.Setting.FinishTime < currentDate))).Select(x => x.Id).ToList();
            db.Bonus.Where(x => x.Status && (x.FinishTime < currentDate ||
                               (x.MaxGranted.HasValue && x.TotalGranted >= x.MaxGranted) ||
                               (x.MaxReceiversCount.HasValue && x.TotalReceiversCount >= x.MaxReceiversCount) ||
                               (x.TriggerGroups.Any() && x.TriggerGroups.All(y => impossibleTriggerGroups.Contains(y.Id)))))
                    .UpdateFromQuery(x => new Bonu { Status = false });
        }

        public static void AwardClientCampaignBonus(ILog log)
        {
            using var db = new IqSoftCorePlatformEntities();
            var currentDate = DateTime.UtcNow;
            var lostBonuses = db.ClientBonus.Where(x => x.Status == (int)BonusStatuses.NotAwarded &&
            (!x.Bonus.Status || !x.Bonus.ValidForAwarding.HasValue ||
            EF.Functions.DateDiffHour(currentDate, x.CreationTime) >= x.Bonus.ValidForAwarding.Value ||
            x.Bonus.FinishTime < currentDate)).ToList();
            foreach (var b in lostBonuses)
            {
                b.Status = (int)BonusStatuses.Lost;
                b.FinalAmount = 0;
            }
            db.SaveChanges();
            var clientIds = lostBonuses.Select(x => x.ClientId).Distinct().ToList();
            foreach (var c in clientIds)
            {
                BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.NotAwardedCampaigns, c));
            }
            foreach (var lb in lostBonuses)
            {
                BroadcastRemoveCache(string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientBonus, lb.ClientId, lb.BonusId));
            }
            var notAwardedBonuses = db.ClientBonus.Include(x => x.Bonus).Include(x => x.Client.Partner).Where(x => x.Status == (int)BonusStatuses.NotAwarded).ToList();
            var bonusIds = notAwardedBonuses.Select(x => x.BonusId).Distinct().ToList();
            var currencies = db.Currencies.ToDictionary(x => x.Id, x => x.CurrentRate);
            var bonuses = db.Bonus.Where(x => bonusIds.Contains(x.Id)).Select(x => new
            {
                BonusId = x.Id,
                x.AccountTypeId,
                x.BonusType,
                x.Priority,
                x.Info,
                x.TurnoverCount,
                BonusMinAmount = x.MinAmount,
                BonusMaxAmount = x.MaxAmount,
                x.Sequence,
                x.IgnoreEligibility,
                x.ValidForAwarding,
                x.ValidForSpending,
                x.ReusingMaxCount,
                x.ResetOnWithdraw,
                x.PartnerId,
                PartnerCurrencyId = x.Partner.CurrencyId,
                Groups = x.TriggerGroups.Select(y => new
                {
                    GroupId = y.Id,
                    GroupType = y.Type,
                    TriggerSettings = y.TriggerGroupSettings.OrderBy(z => z.Order).Select(z => new
                    {
                        z.Setting.Id,
                        z.Setting.Type,
                        z.Setting.Percent,
                        z.Setting.MinAmount,
                        z.Setting.MaxAmount,
                        z.Order
                    })
                }),
                Triggers = x.TriggerGroups.SelectMany(y => y.TriggerGroupSettings).OrderBy(z => z.Order).Select(z => z.Setting.Id)
            }).ToList();

            var bonusTriggers = db.ClientBonusTriggers.Where(x => bonusIds.Contains(x.BonusId)).ToList();
            var awardedBonuses = new List<ClientBonu>();
            foreach (var clientBonus in notAwardedBonuses)
            {
                var bonus = bonuses.First(x => x.BonusId == clientBonus.BonusId);
                var partner = db.Partners.First(x => x.Id == bonus.PartnerId);

                var clientBonusTriggers = bonusTriggers.Where(x => x.BonusId == clientBonus.BonusId && x.ReuseNumber == clientBonus.ReuseNumber &&
                  (x.ClientId == clientBonus.ClientId || x.ClientId == null) &&
                  (x.BetCount == null || x.BetCount >= x.Trigger.MinBetCount) &&
                  (x.WageringAmount == null || x.WageringAmount >= BaseBll.ConvertCurrencyForJob(partner.CurrencyId, currencies[partner.CurrencyId],
                  clientBonus.Client.CurrencyId, currencies[clientBonus.Client.CurrencyId], x.Trigger.MinAmount.Value))).ToList();

                var clientBonusTriggerIds = clientBonusTriggers.Select(x => x.TriggerId).ToList();
                if (bonus.Groups.All(y =>
                                   (y.GroupType == (int)TriggerGroupType.Any && (!y.TriggerSettings.Any() || y.TriggerSettings.Any(z => clientBonusTriggerIds.Contains(z.Id)))) ||
                                   (y.GroupType == (int)TriggerGroupType.All && y.TriggerSettings.All(z => clientBonusTriggerIds.Contains(z.Id)))
                                 ))
                {
                    decimal? sourceAmount = null;
                    foreach (var t in bonus.Triggers)
                    {
                        sourceAmount = clientBonusTriggers.FirstOrDefault(x => x.TriggerId == t)?.SourceAmount;
                        if (sourceAmount != null)
                            break;
                    }
                    clientBonus.FinalAmount = sourceAmount ?? 0;
                    awardedBonuses.Add(clientBonus);
                }
            }
            var groupedBonuses = awardedBonuses.GroupBy(x => x.Client.PartnerId);
            var partnerKeys = db.WebSiteMenuItems.Where(x => x.Menu.Type == Constants.WebSiteConfiguration.Config &&
                                                             x.Title == Constants.PartnerKeys.ActiveBonusMaxCount)
                                                .ToDictionary(x => x.Menu.PartnerId, x => x.Href);

            foreach (var gb in groupedBonuses)
            {
                int count = 0;
                if (partnerKeys.ContainsKey(gb.Key) && !string.IsNullOrEmpty(partnerKeys[gb.Key]))
                    int.TryParse(partnerKeys[gb.Key], out count);
                var partnerBonuses = gb.OrderBy(x => x.Bonus.Priority ?? 0).ToList();
                foreach (var ab in partnerBonuses)
                {
                    try
                    {
                        if (count > 0 && db.ClientBonus.Count(x => x.ClientId == ab.ClientId && x.Status == (int)BonusStatuses.Active) >= count)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientAlreadyHasActiveBonus);

                        if (ab.Bonus.BonusType == (int)BonusTypes.CampaignCash)
                        {
                            ab.Status = (int)BonusStatuses.Finished;
                            db.SaveChanges();
                            BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.NotAwardedCampaigns, ab.ClientId));
                        }
                        else if (ab.Bonus.BonusType == (int)BonusTypes.CampaignFreeBet)
                        {
                            ab.Status = (int)BonusStatuses.Active;
                            ab.AwardingTime = DateTime.UtcNow;
                            ab.BonusPrize = ab.FinalAmount.Value;
                            ab.TurnoverAmountLeft = ab.BonusPrize;
                            ab.FinalAmount = null;
                            db.SaveChanges();
                            BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.NotAwardedCampaigns, ab.ClientId));
                        }
                        else if (ab.Bonus.BonusType == (int)BonusTypes.CompaignFreeSpin)
                        {
                            ab.Status = (int)BonusStatuses.Active;
                            ab.AwardingTime = DateTime.UtcNow;
                            ab.BonusPrize = 0;
                            ab.FinalAmount = null;
                            db.SaveChanges();
                            BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.NotAwardedCampaigns, ab.ClientId));
                        }
                        else if (ab.Bonus.BonusType == (int)BonusTypes.CampaignWagerCasino || ab.Bonus.BonusType == (int)BonusTypes.CampaignWagerSport)
                        {
                            using (var bonusService = new BonusService(new SessionIdentity(), log))
                            {
                                var bonusAmount = ab.FinalAmount.Value;
                                var partnerAmount = BaseBll.ConvertCurrencyForJob(ab.Client.CurrencyId, currencies[ab.Client.CurrencyId],
                                    ab.Client.Partner.CurrencyId, currencies[ab.Client.Partner.CurrencyId], bonusAmount);
                                bonusService.GiveWageringBonus(ab.Bonus, ab.Client, bonusAmount, ab.ReuseNumber ?? 1);
                            }
                            ab.Status = (int)BonusStatuses.Active;
                            ab.AwardingTime = DateTime.UtcNow;
                            ab.BonusPrize = ab.FinalAmount.Value;
                            ab.TurnoverAmountLeft = ab.BonusPrize * ab.Bonus.TurnoverCount;
                            ab.FinalAmount = null;
                            db.SaveChanges();
                            BroadcastRemoveCache(string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.Accounts, (int)ObjectTypes.Client, ab.ClientId, ab.Client.CurrencyId));
                            BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.ActiveBonusId, ab.ClientId));
                            BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, ab.ClientId));
                            BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.NotAwardedCampaigns, ab.ClientId));
                            BroadcastRemoveCache(string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientBonus, ab.ClientId, ab.BonusId));
                        }
                    }
                    catch (FaultException<BllFnErrorType> ex)
                    {
                        log.Error(JsonConvert.SerializeObject(new { Id = ex.Detail.Id, Message = ex.Detail.Message }) + "_" + JsonConvert.SerializeObject(new { ab.Id, ab.BonusId, ab.ClientId }));
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                };
            }
        }

        public static void UpdateJackpotFeed(ILog log)
        {
            try
            {
                using var db = new IqSoftCorePlatformEntities();
                var gameProviders = db.GameProviders.Select(x => new { x.Id, x.Name }).ToList();
                foreach (var gp in gameProviders)
                {
                    switch (gp.Name)
                    {
                        case Constants.GameProviders.BlueOcean:
                            var jackpodFeed = Integration.Products.Helpers.BlueOceanHelpers.GetJackpotFeed(Constants.MainPartnerId, log);
                            var providerProducts = db.Products.Where(x => x.GameProviderId == gp.Id).ToList();
                            var hasJackpotroducts = providerProducts.Where(x => jackpodFeed.ContainsKey(x.ExternalId)).ToList();
                            var ids = hasJackpotroducts.Select(x => x.Id).ToList();
                            db.Products.Where(x => x.GameProviderId == gp.Id && !ids.Contains(x.Id)).UpdateFromQuery(x => new Product { Jackpot = null });
                            var prods = db.Products.Where(x => x.GameProviderId == gp.Id && ids.Contains(x.Id)).ToList();
                            prods.ForEach(x =>
                            {
                                x.Jackpot = JsonConvert.SerializeObject(jackpodFeed[x.ExternalId]);
                            });
                            db.SaveChanges();
                            Parallel.ForEach(providerProducts, x =>
                                            {
                                                BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.Products, x.Id));
                                                BroadcastRemoveCache(string.Format("{0}_{1}_{2}", Constants.CacheItems.Products, gp.Id, x.ExternalId));
                                            });
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public static void ReconsiderDynamicSegments(ILog log)
        {
            try
            {
                using (var db = new IqSoftCorePlatformEntities())
                {
                    var triggers = db.JobTriggers.Where(x => x.Type == (int)JobTriggerTypes.ReconsiderSegments);
                    var allClientsIds = triggers.Select(x => x.ClientId).Distinct().ToList();
                    var fnClients = db.fn_Client().Where(x => allClientsIds.Contains(x.Id));
                    var partnerIds = fnClients.Select(x => x.PartnerId).Distinct().ToList();

                    if (triggers.Any())
                    {
                        var segments = db.Segments.Where(x => x.Mode == (int)PaymentSegmentModes.Dynamic &&
                                                              x.State == (int)SegmentStates.Active && partnerIds.Contains(x.PartnerId)).
                                                              ToList().Select(x => x.MapToSegmentModel()).GroupBy(x => x.PartnerId);

                        foreach (var partnerSegments in segments)
                        {
                            var query = fnClients.Where(x => x.PartnerId == partnerSegments.Key);
                            foreach (var s in partnerSegments)
                            {
                                var sQuery = query.Where(x => (!s.IsKYCVerified.HasValue || x.IsDocumentVerified == s.IsKYCVerified) && (!s.Gender.HasValue || x.Gender == s.Gender));

                                sQuery = CustomHelper.FilterByCondition(sQuery, s.ClientStatusObject, "State");
                                sQuery = CustomHelper.FilterByCondition(sQuery, s.ClientIdObject, "Id", isAndCondition: false);
                                sQuery = CustomHelper.FilterByCondition(sQuery, s.EmailObject, "Email", isAndCondition: false);
                                sQuery = CustomHelper.FilterByCondition(sQuery, s.FirstNameObject, "FirstName", isAndCondition: false);
                                sQuery = CustomHelper.FilterByCondition(sQuery, s.LastNameObject, "LastName", isAndCondition: false);
                                sQuery = CustomHelper.FilterByCondition(sQuery, s.AffiliateIdObject, "AffiliatePlatformId", isAndCondition: false);
                                sQuery = CustomHelper.FilterByCondition(sQuery, s.RegionObject, "RegionId", isAndCondition: false);
                                sQuery = CustomHelper.FilterByCondition(sQuery, s.MobileCodeObject, "MobileNumber", isAndCondition: false);

                                // query = CustomHelper.FilterByCondition(clients, s.SessionPeriod, "ClientSession.StartTime"); // must be clarified
                                sQuery = CustomHelper.FilterByCondition(sQuery, s.SignUpPeriodObject, "CreationTime");
                                var clientIds = sQuery.Select(x => x.Id).ToList();
                                if ((s.TotalDepositsAmount != null && s.TotalDepositsAmountObject.ConditionItems.Any()) ||
                                    (s.TotalDepositsCount != null && s.TotalDepositsCountObject.ConditionItems.Any()))
                                {
                                    var totalDeposits = db.PaymentRequests.Where(x => clientIds.Contains(x.ClientId) && x.Type == (int)PaymentRequestTypes.Deposit &&
                                                        (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually))
                                                        .GroupBy(x => x.ClientId)
                                                        .Select(x => new { ClientId = x.Key, Amount = x.Sum(y => y.Amount), Count = x.Count() });
                                    if (totalDeposits.Any())
                                    {
                                        totalDeposits = CustomHelper.FilterByCondition(totalDeposits, s.TotalDepositsAmountObject, "Amount");
                                        totalDeposits = CustomHelper.FilterByCondition(totalDeposits, s.TotalDepositsCountObject, "Count");
                                        var tmpClientIds = clientIds.Where(x => !totalDeposits.Any(y => y.ClientId == x))
                                                                    .Select(x => new { ClientId = x, Count = 0, Amount = 0 }).AsQueryable();
                                        tmpClientIds = CustomHelper.FilterByCondition(tmpClientIds, s.TotalDepositsAmountObject, "Amount");
                                        tmpClientIds = CustomHelper.FilterByCondition(tmpClientIds, s.TotalDepositsCountObject, "Count");
                                        clientIds = clientIds.Where(x => totalDeposits.Any(y => y.ClientId == x) ||
                                                                         tmpClientIds.Any(z => z.ClientId == x)).ToList();
                                    }
                                }
                                if ((s.TotalWithdrawalsAmount != null && s.TotalWithdrawalsAmountObject.ConditionItems.Any()) ||
                                    (s.TotalWithdrawalsCount != null && s.TotalWithdrawalsCountObject.ConditionItems.Any()))
                                {
                                    var totalWithdrawals = db.PaymentRequests.Where(x => clientIds.Contains(x.ClientId) && x.Type == (int)PaymentRequestTypes.Withdraw &&
                                                        (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually))
                                                        .GroupBy(x => x.ClientId)
                                                        .Select(x => new
                                                        {
                                                            ClientId = x.Key,
                                                            Amount = x.Sum(y => y.Amount),
                                                            Count = x.Count()
                                                        });
                                    if (totalWithdrawals.Any())
                                    {
                                        totalWithdrawals = CustomHelper.FilterByCondition(totalWithdrawals, s.TotalWithdrawalsAmountObject, "Amount");
                                        totalWithdrawals = CustomHelper.FilterByCondition(totalWithdrawals, s.TotalWithdrawalsCountObject, "Count");
                                        var tmpClientIds = clientIds.Where(x => !totalWithdrawals.Any(y => y.ClientId == x))
                                                                   .Select(x => new { ClientId = x, Count = 0, Amount = 0 }).AsQueryable();
                                        tmpClientIds = CustomHelper.FilterByCondition(tmpClientIds, s.TotalWithdrawalsAmountObject, "Amount");
                                        tmpClientIds = CustomHelper.FilterByCondition(tmpClientIds, s.TotalWithdrawalsCountObject, "Count");
                                        clientIds = clientIds.Where(x => totalWithdrawals.Any(y => y.ClientId == x) ||
                                                                         tmpClientIds.Any(z => z.ClientId == x)).ToList();
                                    }
                                }
                                if ((s.TotalBetsCount != null && s.TotalBetsCountObject.ConditionItems.Any()) ||
                                    (s.TotalBetsAmount != null && s.TotalBetsAmountObject.ConditionItems.Any()))
                                {
                                    var totalBets = db.Bets.Where(x => clientIds.Contains(x.ClientId.Value) && x.BetAmount > 0 && x.State != (int)BetDocumentStates.Returned)
                                                           .GroupBy(x => x.ClientId)
                                                           .Select(x => new { ClientId = x.Key, Count = x.Count(), BetAmount = x.Sum(y => y.BetAmount) });
                                    if (s.TotalBetsCount != null && s.TotalBetsCountObject.ConditionItems.Any())
                                    {
                                        totalBets = CustomHelper.FilterByCondition(totalBets, s.TotalBetsCountObject, "Count");
                                        var tmpClientIds = clientIds.Where(x => !totalBets.Any(y => y.ClientId == x))
                                                                    .Select(x => new { ClientId = x, Count = 0 }).AsQueryable();
                                        tmpClientIds = CustomHelper.FilterByCondition(tmpClientIds, s.TotalBetsCountObject, "Count");
                                        clientIds = clientIds.Where(x => totalBets.Any(y => y.ClientId == x) ||
                                        tmpClientIds.Any(z => z.ClientId == x)).ToList();
                                    }
                                    if (s.TotalBetsAmount != null && s.TotalBetsAmountObject.ConditionItems.Any())
                                    {
                                        totalBets = CustomHelper.FilterByCondition(totalBets, s.TotalBetsAmountObject, "BetAmount");
                                        var tmpClientIds = clientIds.Where(x => !totalBets.Any(y => y.ClientId == x))
                                                                    .Select(x => new { ClientId = x, BetAmount = 0 }).AsQueryable();
                                        tmpClientIds = CustomHelper.FilterByCondition(tmpClientIds, s.TotalBetsAmountObject, "BetAmount");
                                        clientIds = clientIds.Where(x => totalBets.Any(y => y.ClientId == x) ||
                                        tmpClientIds.Any(z => z.ClientId == x)).ToList();
                                    }
                                }
                                if (s.Profit != null && s.ProfitObject.ConditionItems.Any()) // must be clarified
                                {
                                    var profit = db.PaymentRequests.Where(x => clientIds.Contains(x.ClientId) &&
                                    (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually))
                                                           .GroupBy(x => x.ClientId)
                                                           .Select(x => new
                                                           {
                                                               ClientId = x.Key,
                                                               Profit = x.Where(y => y.Type == (int)PaymentRequestTypes.Deposit).Sum(y => y.Amount) -
                                                                x.Where(y => y.Type == (int)PaymentRequestTypes.Withdraw).Sum(y => y.Amount)
                                                           });
                                    profit = CustomHelper.FilterByCondition(profit, s.ProfitObject, "Profit");
                                    var tmpClientIds = clientIds.Where(x => !profit.Any(y => y.ClientId == x))
                                                                  .Select(x => new { ClientId = x, Profit = 0 }).AsQueryable();
                                    tmpClientIds = CustomHelper.FilterByCondition(tmpClientIds, s.ProfitObject, "Profit");
                                    clientIds = clientIds.Where(x => profit.Any(y => y.ClientId == x) ||
                                                                     tmpClientIds.Any(z => z.ClientId == x)).ToList();
                                }
                                if (s.Bonus != null && s.BonusObject.ConditionItems.Any()) // must be clarified
                                {; }

                                /// must be clarified
                                //if (s.SuccessDepositPaymentSystem != null && s.SuccessDepositPaymentSystem.ConditionItems.Any())
                                //{
                                //    var successDepositPayments = db.PaymentRequests.Where(x => clientIds.Contains(x.ClientId) && x.Type == (int)PaymentRequestTypes.Deposit &&
                                //                   (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually))
                                //                   .GroupBy(x => x.ClientId)
                                //                   .ToDictionary(k => k.Key,
                                //                                 v => v.Select(x => x.PaymentSystemId));
                                //    CustomHelper.FilterByCondition(successDepositPayments, s.TotalWithdrawalsAmount, "Value");
                                //    clientIds = clientIds.Where(x => successDepositPayments.Keys.Contains(x)).ToList();
                                //}
                                //if (s.SuccessWithdrawalPaymentSystem != null && s.SuccessWithdrawalPaymentSystem.ConditionItems.Any())
                                //{
                                //    var successWithdrawalPayments = db.PaymentRequests.Where(x => clientIds.Contains(x.ClientId) && x.Type == (int)PaymentRequestTypes.Withdraw &&
                                //                   (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually))
                                //                   .GroupBy(x => x.ClientId)
                                //                   .ToDictionary(k => k.Key,
                                //                                 v => v.Select(x => x.PaymentSystemId));
                                //    CustomHelper.FilterByCondition(successWithdrawalPayments, s.TotalWithdrawalsAmount, "Value");
                                //    clientIds = clientIds.Where(x => successWithdrawalPayments.Keys.Contains(x)).ToList();
                                //}
                                if (s.ComplimentaryPoint != null && s.ComplimentaryPointObject.ConditionItems.Any())
                                {
                                    var accounts = db.Accounts.Where(x => x.ObjectTypeId == (int)ObjectTypes.Client && clientIds.Contains((int)x.ObjectId) &&
                                                                          x.TypeId == (int)AccountTypes.ClientCompBalance)
                                                              .GroupBy(x => x.ObjectId)
                                                              .Select(x => new { ClientId = x.Key, Balance = x.Min(y => y.Balance) });
                                    if (accounts != null)
                                    {
                                        accounts = CustomHelper.FilterByCondition(accounts, s.ComplimentaryPointObject, "Balance");
                                        var tmpClientIds = clientIds.Where(x => !accounts.Any(y => y.Balance == x))
                                                                      .Select(x => new { ClientId = x, Count = 0 }).AsQueryable();
                                        tmpClientIds = CustomHelper.FilterByCondition(tmpClientIds, s.ComplimentaryPointObject, "Balance");

                                        clientIds = clientIds.Where(x => accounts.Any(y => y.ClientId == x) ||
                                                                         tmpClientIds.Any(z => z.ClientId == x)).ToList();
                                    }
                                }
                                if (s.SegmentId != null && s.SegmentIdObject.ConditionItems.Any())
                                {
                                    var cs = db.ClientClassifications.Where(x => clientIds.Contains(x.ClientId) &&
                                                                                 x.ProductId == Constants.PlatformProductId)
                                                                     .Select(x => new { ClientId = x.ClientId, SegmentId = x.SegmentId.Value });
                                    CustomHelper.FilterByCondition(cs, s.SegmentIdObject, "SegmentId", isAndCondition: false);
                                    clientIds = clientIds.Where(x => cs.Any(y => y.ClientId == x)).ToList();
                                }
                                if (s.SportBetsCount != null && s.SportBetsCountObject.ConditionItems.Any())
                                {
                                    var sportBets = db.Bets.Where(x => clientIds.Contains(x.ClientId.Value) &&
                                                                       x.ProductId == Constants.SportsbookProductId)
                                                           .GroupBy(x => x.ClientId)
                                                           .Select(x => new { ClientId = x.Key, Count = x.Count() });

                                    sportBets = CustomHelper.FilterByCondition(sportBets, s.SportBetsCountObject, "Count");
                                    var tmpClientIds = clientIds.Where(x => !sportBets.Any(y => y.ClientId == x))
                                                                   .Select(x => new { ClientId = x, Count = 0 }).AsQueryable();
                                    tmpClientIds = CustomHelper.FilterByCondition(tmpClientIds, s.SportBetsCountObject, "Count");
                                    clientIds = clientIds.Where(x => sportBets.Any(y => y.ClientId == x) ||
                                                                     tmpClientIds.Any(z => z.ClientId == x)).ToList();
                                }
                                if (s.CasinoBetsCount != null && s.CasinoBetsCountObject.ConditionItems.Any())
                                {
                                    var casinoBets = db.Bets.Where(x => clientIds.Contains(x.ClientId.Value) &&
                                                                        x.ProductId != Constants.SportsbookProductId)
                                                            .GroupBy(x => x.ClientId)
                                                            .Select(x => new { ClientId = x.Key, Count = x.Count() });
                                    casinoBets = CustomHelper.FilterByCondition(casinoBets, s.CasinoBetsCountObject, "Count");
                                    var tmpClientIds = clientIds.Where(x => !casinoBets.Any(y => y.ClientId == x))
                                                                .Select(x => new { ClientId = x, Count = 0 }).AsQueryable();
                                    tmpClientIds = CustomHelper.FilterByCondition(tmpClientIds, s.CasinoBetsCountObject, "Count");
                                    clientIds = clientIds.Where(x => casinoBets.Any(y => y.ClientId == x) ||
                                                                     tmpClientIds.Any(z => z.ClientId == x)).ToList();
                                }

                                var existings = db.ClientClassifications.Where(x => clientIds.Contains(x.ClientId) &&
                                    x.SegmentId == s.Id && x.ProductId == Constants.PlatformProductId).Select(x => x.ClientId);
                                var notExistings = clientIds.Except(existings);
                                var removables = query.Where(x => !clientIds.Contains(x.Id)).Select(x => x.Id).ToList();

                                var clientSegments = notExistings.Select(x => new ClientClassification
                                {
                                    ClientId = x,
                                    State = null,
                                    CategoryId = null,
                                    ProductId = Constants.PlatformProductId,
                                    SessionId = null,
                                    SegmentId = s.Id,
                                    LastUpdateTime = DateTime.UtcNow
                                }).ToList();
                                foreach (var cs in clientSegments)
                                {
                                    db.ClientClassifications.Add(cs);
                                    db.JobTriggers.AddIfNotExists(new JobTrigger
                                    {
                                        ClientId = cs.ClientId,
                                        Type = (int)JobTriggerTypes.FairSegmentTriggers,
                                        SegmentId = cs.SegmentId
                                    }, x => x.ClientId == cs.ClientId && x.Type == (int)JobTriggerTypes.FairSegmentTriggers);
                                }
                                db.ClientClassifications.Where(x => x.SegmentId == s.Id && removables.Contains(x.ClientId)).DeleteFromQuery();
                                db.SaveChanges();
                            }
                        }
                        db.JobTriggers.RemoveRange(triggers);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public static void CalculateCompPoints(ILog log)
        {
            try
            {
                List<ClientOperation> clients = new List<ClientOperation>();
                using (var db = new IqSoftCorePlatformEntities())
                {
                    var debitTriggers = db.JobTriggers.Include(x => x.Client).Where(x => x.Type == (int)JobTriggerTypes.AddComplimentaryPoint);
                    var creditTriggers = db.JobTriggers.Include(x => x.Client).Where(x => x.Type == (int)JobTriggerTypes.RemoveComplimentaryPoint);

                    clients = debitTriggers.GroupBy(x => new { x.ClientId, x.Client.PartnerId, x.Client.CurrencyId }).Select(x => new ClientOperation
                    {
                        ClientId = x.Key.ClientId,
                        PartnerId = x.Key.PartnerId,
                        CurrencyId = x.Key.CurrencyId,
                        Amount = x.Sum(y => y.Amount) ?? 0
                    }).ToList();

                    foreach (var item in creditTriggers)
                    {
                        var client = clients.FirstOrDefault(x => x.ClientId == item.ClientId);
                        if (client != null)
                            client.Amount -= (item.Amount ?? 0);
                        else
                            clients.Add(new ClientOperation { ClientId = item.ClientId, Amount = -(item.Amount ?? 0) });
                    }

                    db.JobTriggers.RemoveRange(debitTriggers);
                    db.JobTriggers.RemoveRange(creditTriggers);
                    db.SaveChanges();
                }
                using (var clientBl = new ClientBll(new SessionIdentity(), log))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        foreach (var client in clients)
                        {
                            if (client.Amount > 0)
                            {
                                client.OperationTypeId = (int)OperationTypes.ComplimentaryPointWin;
                                client.AccountTypeId = (int)AccountTypes.ClientCompBalance;
                                clientBl.CreateDebitToClientFromJob(client.ClientId.Value, client, documentBl);

                                var coinInput = new ClientOperation
                                {
                                    ClientId = client.ClientId,
                                    Amount = client.Amount,
                                    OperationTypeId = (int)OperationTypes.ComplimentaryPointWin,
                                    PartnerId = client.PartnerId,
                                    CurrencyId = client.CurrencyId,
                                    AccountTypeId = (int)AccountTypes.ClientCoinBalance
                                };
                                clientBl.CreateDebitToClientFromJob(client.ClientId.Value, coinInput, documentBl);
                            }
                            else
                            {
                                var cl = clientBl.GetClientById(client.ClientId.Value, false);
                                var compAccount = documentBl.GetOrCreateAccount(client.ClientId.Value, (int)ObjectTypes.Client, client.CurrencyId, (int)AccountTypes.ClientCompBalance);
                                if (compAccount != null && compAccount.Balance != 0)
                                {
                                    var compCorrectionInput = new ClientCorrectionInput
                                    {
                                        Amount = Math.Min(compAccount.Balance, Math.Abs(client.Amount)),
                                        AccountId = compAccount.Id,
                                        AccountTypeId = (int)AccountTypes.ClientCompBalance,
                                        CurrencyId = client.CurrencyId,
                                        ClientId = client.ClientId.Value,
                                        Info = "CompPointWinRollback"
                                    };
                                    clientBl.CreateCreditCorrectionFromJob(cl, compCorrectionInput, documentBl);
                                }
                                var coinAccount = documentBl.GetOrCreateAccount(client.ClientId.Value, (int)ObjectTypes.Client, client.CurrencyId, (int)AccountTypes.ClientCoinBalance);
                                if (coinAccount != null && compAccount.Balance != 0)
                                {
                                    var coinCorrectionInput = new ClientCorrectionInput
                                    {
                                        Amount = Math.Min(compAccount.Balance, Math.Abs(client.Amount)),
                                        AccountId = coinAccount.Id,
                                        AccountTypeId = (int)AccountTypes.ClientCoinBalance,
                                        CurrencyId = client.CurrencyId,
                                        ClientId = client.ClientId.Value,
                                        Info = "CoinPointWinRollback"
                                    };
                                    clientBl.CreateCreditCorrectionFromJob(cl, coinCorrectionInput, documentBl);
                                }
                            }
                            clientBl.AddClientJobTrigger(client.ClientId.Value, (int)JobTriggerTypes.ReconsiderSegments);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public static void FairSegmentTriggers(ILog log)
        {
            try
            {
                using var clientBl = new ClientBll(new SessionIdentity(), log);
                    clientBl.FairSegmentTriggers();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public static void GiveJackpotWin(ILog log)
        {
            try
            {
                using var bonusService = new BonusService(new SessionIdentity(), log);
                bonusService.GiveJackpotWin();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
        public static void BroadcastRemoveCache(string key)
        {
            BaseHub.CurrentContext?.Clients.Group("BaseHub").SendAsync("onRemoveKeyFromCache", key);
        }
    }
}