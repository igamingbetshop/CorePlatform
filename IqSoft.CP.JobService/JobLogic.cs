using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using IqSoft.CP.DAL.Models.User;
using IqSoft.CP.JobService.Hubs;
using IqSoft.CP.Common.Models.Enums;
using IqSoft.CP.BLL.Services;
using System.ServiceModel;
using System.Threading.Tasks;
using IqSoft.CP.DAL.Models.Report;
using IqSoft.CP.BLL.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.BLL.Caching;
using static IqSoft.CP.Common.Constants;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.DataWarehouse;
using System.IO;
using System.Reflection;
using System.Data.Entity.Infrastructure;
using System.Security.Cryptography;
using Product = IqSoft.CP.DAL.Product;
using User = IqSoft.CP.DAL.User;
using ClientBonu = IqSoft.CP.DAL.ClientBonu;
using PaymentRequest = IqSoft.CP.DAL.PaymentRequest;
using JobTrigger = IqSoft.CP.DAL.JobTrigger;
using Bonu = IqSoft.CP.DAL.Bonu;
using IqSoft.CP.Common.Models.Partner;

namespace IqSoft.CP.JobService
{
    public static class JobBll
    {
        public static long LastProcessedBetDocumentId = 0;

        public static int IqWalletId;
        
        static JobBll()
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                LastProcessedBetDocumentId = Convert.ToInt64(db.PartnerKeys.First(x => x.Name == Constants.PartnerKeys.LastProcessedBetDocumentId).NumericValue.Value);
                var paymentSystem = db.PaymentSystems.FirstOrDefault(x => x.Name == Constants.PaymentSystems.IqWallet);
                IqWalletId = paymentSystem == null ? 0 : paymentSystem.Id;
            }
        }

        public static Job GetJobById(int id)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                var currentTime = DateTime.UtcNow;
                return db.Jobs.FirstOrDefault(x => x.Id == id && x.NextExecutionTime <= currentTime && x.State == (int)JobStates.Active);
            }
        }

        public static Job SaveJob(Job job)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
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
        }

        public static JobResult SaveJobResult(JobResult jobResult)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
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
        }

        public static ResetBetShopDailyTicketNumberOutput ResetBetShopDailyTicketNumber(ResetBetShopDailyTicketNumberInput input)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
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
                            db.sp_GetBetShopLock(betShopId);
                            var betShop = db.BetShops.FirstOrDefault(x => x.Id == betShopId);
                            betShop.DailyTicketNumber = 0;
                        }
                        result.Results.Add(new ResetBetShopDailyTicketNumberOutputItem { PartnerId = dailyTicketNumberResetSetting.PartnerId, ResetResult = true });
                    }
                }
                db.SaveChanges();
                return result;
            }
        }

        public static bool CloseAccountPeriod(ClosePeriodInput input)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
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
        }

        private static decimal GetAccountBalanceByDate(long accountId, DateTime date, IqSoftCorePlatformEntities db)
        {
            var account = db.Accounts.Include(x => x.AccountType).FirstOrDefault(x => x.Id == accountId);
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
            using (var db = new IqSoftCorePlatformEntities())
            {
                var currentTime = DateTime.UtcNow;
                if (input.EndTime > currentTime)
                    return false;
                var startDate = input.EndTime.AddHours(-Constants.ClosePeriodPeriodicy);
                var groupedDocuments = (from d in db.Documents
                                        where d.CreationTime >= startDate && d.CreationTime < input.EndTime && d.ClientId != null
                                        group d by new { d.ClientId, d.OperationTypeId }
                                        into docs
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
                            (x.OperationTypeId == (int)OperationTypes.Deposit ||
                             x.OperationTypeId == (int)OperationTypes.TransferFromBetShopToClient))
                        .Sum(x => x.Amount);
                    var totalWithdrawAmountModel =
                        groupedDocuments.FirstOrDefault(
                            x =>
                                x.ClientId == clientId &&
                                x.OperationTypeId == (int)OperationTypes.Withdraw ||
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
        }

        public static bool AddMoneyToPartnerAccount(ILog log, AddMoneyToPartnerAccountInput input)
        {
            using (var partnerBl = new PartnerBll(new SessionIdentity(), log))
            {
                return partnerBl.ChangePartnerAccountBalance(input.PartnerId, input.EndTime); //????
            }
        }

        public static void ExpireClientVerificationStatus()
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                var currentTime = DateTime.UtcNow;
                var partnerSetting = db.WebSiteMenuItems.Where(x => x.WebSiteMenu.Type == Constants.WebSiteConfiguration.Config &&
                                                               x.Title == Constants.PartnerKeys.VerificationValidPeriodInMonth && x.Href != null)
                                                  .Select(x => new { x.WebSiteMenu.PartnerId, x.Href }).ToList()
                                                  .Select(x => new PartnerSetting { PartnerId = x.PartnerId, Type = Convert.ToInt32(x.Href) }).ToList();
                var partnerIds = partnerSetting.Select(x => x.PartnerId).ToList();
                var clientSettings = db.ClientSettings.Where(x => x.Name.StartsWith(Constants.ClientSettings.VerificationServiceName) &&
                                                                  x.DateValue != null && partnerIds.Contains(x.Client.PartnerId))
                                                      .GroupBy(x=>x.Client.PartnerId).ToList();
                var clientIds = new List<int>();
                foreach(var cs in clientSettings)
                {
                    var expirationPeriod = partnerSetting.FirstOrDefault(x=> x.PartnerId == cs.Key).Type;
                    clientIds.AddRange(cs.Where(x => x.DateValue.Value.AddMonths(expirationPeriod) < currentTime)
                                         .Select(x => { x.NumericValue = (int)VerificationStatuses.EXPIRED; return x.ClientId; }));
                }
                db.SaveChanges();
                db.Clients.Where(x => x.IsDocumentVerified && clientIds.Contains(x.Id)).UpdateFromQuery(x => new DAL.Client { IsDocumentVerified = false });
                clientIds.ForEach(clientId =>
                {
                    CacheManager.RemoveClientFromCache(clientId);
                    BroadcastRemoveClientFromCache(clientId.ToString());
                });
            }
        }

        public static void ExpireUserSessions()
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                var currentTime = DateTime.UtcNow;
                db.UserSessions.Include(x => x.User)
                   .Join(db.Partners, us => us.User.PartnerId, p => p.Id, (us, p) => new { UserSession = us, UserType = us.User.Type, Partner = p })
                   .Where(x =>
                          x.UserSession.State != (int)SessionStates.Inactive && x.UserType== (int)UserTypes.AdminUser &&
                         (x.UserSession.LastUpdateTime < DbFunctions.AddMinutes(currentTime, -x.Partner.UserSessionExpireTime))).Select(x => x.UserSession)
                   .UpdateFromQuery(x => new UserSession { State = (int)SessionStates.Inactive, EndTime = currentTime, LogoutType = (int)LogoutTypes.Expired });
            }
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
                BaseHub._connectedClients.Client(BaseHub.Caches.First(x => x.Value.ProjectId == (int)ProjectTypes.MasterCache).Key).ExpireClientPlatformSessions(1);
        }

        private static void ExpireClientProductSessions()
        {
            if (BaseHub.Caches.Any(x => x.Value.ProjectId == (int)ProjectTypes.MasterCache))
                BaseHub._connectedClients.Client(BaseHub.Caches.First(x => x.Value.ProjectId == (int)ProjectTypes.MasterCache).Key).ExpireClientProductSessions(1);
        }

        public static void ExpireClientVerificationKeys()
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                var currentTime = DateTime.UtcNow;
                var keyTypes = new List<int>
                {
                    (int)ClientInfoTypes.MobileVerificationKey,
                    (int)ClientInfoTypes.EmailVerificationKey,
                    (int)ClientInfoTypes.PasswordRecoveryMobileKey,
                    (int)ClientInfoTypes.PasswordRecoveryEmailKey
                };
                var keys = db.ClientInfoes.Where(x => x.State == (int)ClientInfoStates.Active && keyTypes.Contains(x.Type) &&
                                                    (x.CreationTime < DbFunctions.AddMinutes(currentTime, -x.Partner.VerificationKeyActiveMinutes) ||
                                                      x.CreationTime < DbFunctions.AddMinutes(currentTime, -x.Client.Partner.VerificationKeyActiveMinutes)))
                                          .UpdateFromQuery(x => new ClientInfo { State = (int)ClientInfoStates.Expired });
            }
        }

        public static void CalculateCashBackBonuses(ILog log)
        {
            using (var bonusBl = new BonusService(new SessionIdentity(), log, 1200))
            {
                bonusBl.CalculateCashBackBonus();
            }
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

        public static void GiveAffiliateCommission(DateTime lastExecutionTime, ILog log)
        {
            using (var affiliateService = new AffiliateService(new SessionIdentity(), log))
            {
                affiliateService.GiveCommission(lastExecutionTime, log);
            }
        }

        public static void GiveFixedFeeCommission(ILog log)
        {
            using (var bonusBl = new BonusService(new SessionIdentity(), log))
            {
                bonusBl.GiveFixedFeeCommission();
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
                                  .UpdateFromQuery(x => new PaymentRequest { Status = (int)PaymentRequestStates.Expired, LastUpdateTime = lastUpdateTime });
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
                using (var db = new IqSoftCorePlatformEntities())
                {
                    var partnerKey = db.PartnerKeys.Where(x => partnerKeyNames.Contains(x.Name)).ToDictionary(x => x.Name, y => y.StringValue);
                    var currentDate = DateTime.UtcNow;
                    Dictionary<string, string> requestHeaders = new Dictionary<string, string>
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
                        RequestMethod = Constants.HttpRequestMethods.Get,
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
                                var beforeRate = dbCurr.CurrentRate;
                                db.Currencies.Where(x => x.Id == dbCurr.Id).UpdateFromQuery(x => new DAL.Currency { CurrentRate = response.To[0].Mid, LastUpdateTime = currentDate });
                                db.CurrencyRates.Add(new CurrencyRate
                                {
                                    CurrencyId = dbCurr.Id,
                                    RateAfter = response.To[0].Mid,
                                    RateBefore = dbCurr.CurrentRate,
                                    SessionId = dbCurr.SessionId,
                                    CreationTime = currentDate,
                                    LastUpdateTime = currentDate
                                });
                            }
                            catch (Exception ex)
                            {
                                log.Error(ex);
                            }
                        }
                        db.SaveChanges();
                    }
                    else
                    {
                        log.Info(authResponse);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }
        
        public static void SendActiveMails(ILog log)
        {
            var activeMails = new List<Email>();

            using (var db = new IqSoftCorePlatformEntities())
            {
                activeMails = db.Emails.Include(x => x.MessageTemplate).Include(x => x.Partner).Where(x => x.Status == (int)EmailStates.Active).Take(60).ToList();
            }
            using (var notificationBll = new NotificationBll(new SessionIdentity(), log))
            {
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
                        notificationBll.UpdateEmailStatus(email.Id, (int)EmailStates.Failed);
                        log.Error(e);
                    }
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
                db.Database.CommandTimeout = 180;
                currencies = db.Currencies.ToDictionary(x => x.Id, x => x.CurrentRate);

                partnerKeys = db.WebSiteMenuItems.Where(x => x.WebSiteMenu.Type == "Config" && x.Title == Constants.PartnerKeys.PartnerEmails)
                                                 .ToDictionary(x => x.WebSiteMenu.PartnerId, x => x.Href);
                partners = db.Partners.Where(x => partnerKeys.Keys.Contains(x.Id)).ToDictionary(x => x.Id, x => x.Name);
                newPlayers = db.Clients.Where(x => x.CreationTime >= startTime && partnerKeys.Keys.Contains(x.PartnerId))
                                            .GroupBy(x => x.PartnerId)
                                            .ToDictionary(x => x.Key, x => x.Count());
                activePlayers = db.ClientSessions.Where(x => x.StartTime >= startTime && partnerKeys.Keys.Contains(x.Client.PartnerId) &&
                                                             x.ProductId == Constants.PlatformProductId)
                                                 .Select(x => new { x.Client.PartnerId, x.ClientId }).Distinct()
                                                 .GroupBy(x => x.PartnerId)
                                                 .ToDictionary(x => x.Key, x => x.Count());

                withdrawableBalance = (from acc in db.Accounts
                                       join c in db.Clients on acc.ObjectId equals c.Id
                                       join p in db.Partners on c.PartnerId equals p.Id
                                       join crr in db.Currencies on acc.CurrencyId equals crr.Id
                                       where acc.ObjectTypeId == (int)ObjectTypes.Client &&
                                             acc.AccountType.Kind != (int)AccountTypeKinds.Booked &&
                                             acc.TypeId != (int)AccountTypes.ClientCoinBalance &&
                                             acc.TypeId != (int)AccountTypes.ClientCompBalance &&
                                             acc.TypeId != (int)AccountTypes.ClientBonusBalance &&
                                             partnerKeys.Keys.Contains(c.PartnerId)
                                       select new
                                       {
                                           c.PartnerId,
                                           PartnerCurrencyId = p.CurrencyId,
                                           USDBalance = acc.Balance * crr.CurrentRate
                                       }).GroupBy(x => new { x.PartnerId, x.PartnerCurrencyId }).ToDictionary(x => x.Key.PartnerId, x => x.Sum(y => y.USDBalance) / currencies[x.Key.PartnerCurrencyId]);

                bonusConversion = db.Documents.Where(x => x.Date >= fDate && partnerKeys.Keys.Contains(x.Client.PartnerId) &&
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

                using (var dwh = new IqSoftDataWarehouseEntities())
                {
                    reportByGameProvider = dwh.fn_ReportByProvider(fDate, tDate).GroupBy(x => x.PartnerId ?? 0)
                        .ToDictionary(x => x.Key, x => x.ToList());
                }
                var messageInfoType = (int)Enum.Parse(typeof(ClientInfoTypes), ("Partner" + subject + "Report"));
                messageTemplates = db.fn_MessageTemplate(Constants.DefaultLanguageId).Where(x => x.ClientInfoType == messageInfoType && x.State == (int)MessageTemplateStates.Active)
                    .ToDictionary(x => x.PartnerId, x => x.Text);
            }
            using (var notificationBll = new NotificationBll(new SessionIdentity(), log))
            {
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
                                TotalBetAmount = Math.Round(x.Sum(y => (y.TotalBetsAmount ?? 0) * currencies[y.Currency] / currencies[partner.CurrencyId]), 2),
                                TotalWinAmount = Math.Round(x.Sum(y => (y.TotalWinsAmount ?? 0) * currencies[y.Currency] / currencies[partner.CurrencyId]), 2),
                                GGR = Math.Round(x.Sum(y => y.GGR * currencies[y.Currency] / currencies[partner.CurrencyId]) ?? 0, 2)
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
                        ExportExcelHelper.AddObjectToLine(new List<PartnerReport> { report }, null, content, false, false);
                        content.Add(string.Empty);
                        ExportExcelHelper.AddObjectToLine(new List<PartnerGameReport> { gameReport }, null, content, false, true);
                        content.Add(string.Empty);
                        ExportExcelHelper.AddObjectToLine(new List<GameProviderTotalItem> { totalAmounts }, null, content, true, false);
                        content.Add(string.Empty);
                        ExportExcelHelper.AddObjectToLine(new List<PartnerWithdrawalsReport> { withdrawalsReport }, null, content, false, true);
                        content.Add(string.Empty);
                        ExportExcelHelper.AddObjectToLine(new List<PartnerDepositsReport> { depositsReport }, null, content, false, true);

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
        }

        public static void UpdateClientWageringBonus()
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                var currentDate = DateTime.UtcNow;
                var lostBonuses = db.ClientBonus.Where(x => x.Status == (int)ClientBonusStatuses.Active && x.Bonu.ValidForSpending != null &&
                    DbFunctions.AddHours(x.AwardingTime, x.Bonu.ValidForSpending) < currentDate).ToList();

                foreach (var lb in lostBonuses)
                {
                    lb.Status = (int)ClientBonusStatuses.Lost;
                    db.SaveChanges();
                    BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.ActiveBonusId, lb.ClientId));
                    BroadcastRemoveCache(string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientBonus, lb.ClientId, lb.BonusId));
                }
                var dbClientBonuses = db.ClientBonus.Include(x => x.Bonu).Where(x => (x.Bonu.Type == (int)BonusTypes.CampaignWagerCasino ||
                                                                 x.Bonu.Type == (int)BonusTypes.CampaignWagerSport) &&
                                                                 x.Status == (int)ClientBonusStatuses.Active).ToList();

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
                        clientBonus.Status = (int)ClientBonusStatuses.Finished;
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
                            clientBonus.Status = (int)ClientBonusStatuses.Finished;
                            db.SaveChanges();
                            BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.ActiveBonusId, clientBonus.ClientId));
                            BroadcastRemoveCache(string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientBonus, clientBonus.ClientId, clientBonus.BonusId));
                        }
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
                RequestMethod = Constants.HttpRequestMethods.Post
            };

            using (var db = new IqSoftCorePlatformEntities())
            {
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
        }    
       
        public static void SendAffiliateReport(ILog log)
        {
            try
            {
                using (var db = new IqSoftCorePlatformEntities())
                {
                    var currentTime = DateTime.UtcNow;
                    var affiliatePlatforms = db.AffiliatePlatforms.Where(x => Constants.ReportingAffiliates.Contains(x.Name) && x.LastExecutionTime.HasValue &&
                                                                              x.StepInHours.HasValue && x.Status == (int)BaseStates.Active &&
                                                                              currentTime >= DbFunctions.AddHours(x.LastExecutionTime.Value, x.StepInHours.Value))
                                                                  .Select(x => x.Id).ToList();
					if (affiliatePlatforms.Count <= 0)
                        return;
                    using (var baseBll = new BaseBll(new SessionIdentity(), log))
                    using (var affiliateService = new AffiliateService(baseBll))
                    {
                        var clientsByAffiliate = db.Clients.Where(x => affiliatePlatforms.Contains(x.AffiliateReferral.AffiliatePlatform.Id) &&
                                                                       x.CreationTime < DbFunctions.AddHours(x.AffiliateReferral.AffiliatePlatform.LastExecutionTime.Value, x.AffiliateReferral.AffiliatePlatform.StepInHours.Value)
                                                                 )
                                                           .Select(x => new DAL.Models.Affiliates.AffiliatePlatformModel
                                                           {
                                                               PartnerId = x.PartnerId,
                                                               ClientId =x.Id,
                                                               ClientName =x.UserName,
                                                               ClientStatus = x.State,
                                                               AffiliateId = x.AffiliateReferral.AffiliatePlatform.Id,
                                                               AffiliateName = x.AffiliateReferral.AffiliatePlatform.Name,
                                                               ClickId = x.AffiliateReferral.RefId,
                                                               RegistrationIp = x.RegistrationIp,
                                                               RegistrationDate = x.CreationTime,
                                                               FirstDepositDate = x.FirstDepositDate,
                                                               CountryCode = x.Region.IsoCode,
                                                               Language = x.LanguageId,
                                                               CurrencyId = x.CurrencyId,
                                                               LastExecutionTime = x.AffiliateReferral.AffiliatePlatform.LastExecutionTime.Value,
                                                               KickOffTime = x.AffiliateReferral.AffiliatePlatform.KickOffTime,
                                                               StepInHours = x.AffiliateReferral.AffiliatePlatform.StepInHours.Value
                                                           })
                                                           .GroupBy(x => new { x.PartnerId, x.AffiliateName, x.AffiliateId, x.KickOffTime, x.LastExecutionTime, x.StepInHours })
                                                           .ToList();
                        var currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                        foreach (var affClient in clientsByAffiliate)
                        {
                            var partner = CacheManager.GetPartnerById(affClient.Key.PartnerId);
                            string[] paths = { Path.GetDirectoryName(currentPath), "AdminWebApi", "AffiliateFiles", affClient.Key.AffiliateName, partner.Name };
                            var localPath = Path.Combine(paths);
                            if (!Directory.Exists(localPath))
                                Directory.CreateDirectory(localPath);
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
                            var upToDate = affClient.Key.LastExecutionTime.Value.AddHours(affClient.Key.StepInHours.Value);
                            var tDate = upToDate.Year * (long)1000000 + upToDate.Month * 10000 + upToDate.Day * 100 + upToDate.Hour;
                            var dateString = fromDate.ToString("yyyy-MM-dd");

                            var newRegisteredClients = affClient.Where(x => x.RegistrationDate >= fromDate && x.RegistrationDate < upToDate)
                                                                .Select(x => new DAL.Models.Affiliates.RegistrationActivityModel
                                                                {
                                                                    CustomerId = x.ClientId,
                                                                    CustomerName = x.ClientName,
                                                                    CountryCode = x.CountryCode,
                                                                    BTag = x.ClickId,
                                                                    RegistrationDate = x.RegistrationDate.ToString("yyyy-MM-dd"),
                                                                    BrandId = brandId,
                                                                    LanguageId = x.Language,
                                                                    RegistrationIp = x.RegistrationIp
                                                                }).ToList();
                            var content = new StringBuilder();

                            switch (affClient.Key.AffiliateName)
                            {
                                case AffiliatePlatforms.Netrefer:
                                    var date = fromDate.ToString("ddMMyyyy");
                                    var netreferClientActivies = affiliateService.GetNetreferClientActivity(affClient.ToList(), brandId, fromDate, upToDate);
                                    content.AppendLine("CustomerID,CountryID,BTag,RegistrationDate,BrandID,RegistrationIp,CustomerSourceID");

                                    if (newRegisteredClients.Any())
                                    {
                                        content.AppendLine(newRegisteredClients.Aggregate(string.Empty, (current, item) => current +
                                        string.Format("{0},{1},{2},{3},{4},{5},{6}", item.CustomerId, item.CountryCode, item.BTag, item.RegistrationDate,
                                                                                     item.BrandId, item.RegistrationIp, 1) + Environment.NewLine));
                                    }
                                    baseBll.SFTPUpload(content.ToString(), $"/REG_BTAG_{date}.csv", "/Customers/pending", ftpModel);
                                    content.Clear();
                                    content.AppendLine("CustomerId,ActivityDate,ProductId,GrossRevenue,Bonuses," +
                                                       "Adjustments,Deposit,Turnover,BrandID,AdjustmentTypeID,Withdrawals," +
                                                       "Payout,CustomerSourceID,Transactions");
                                    if (netreferClientActivies.Any())
                                    {
                                        content.AppendLine(netreferClientActivies.Aggregate(string.Empty, (current, item) => current +
                                        string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                                        item.CustomerId, item.ActivityDate, item.ProductId, item.GrossRevenue,
                                        item.Bonuses, item.Adjustments, item.Deposit, item.Turnover,
                                        item.BrandID, 1, item.Withdrawals, item.Payout, 1, item.Transactions) + Environment.NewLine));
                                    }
                                    baseBll.SFTPUpload(content.ToString(), $"/ACTIVITY_{date}.csv", "/Activity/pending", ftpModel);
                                    baseBll.WriteToFile(content.ToString(), Path.Combine(localPath, $"{partner.Name}_activity_{dateString}.csv"));
                                    break;
                                case AffiliatePlatforms.Intelitics:
                                    var affClientActivies = affiliateService.GetClientActivity(affClient.ToList(), brandId, fromDate, upToDate);
                                    content.AppendLine(string.Join(",", typeof(AffiliateReportInput).GetProperties().Select(x => x.Name.ToUpper()).ToArray()));
                                    if (newRegisteredClients.Any())
                                    {
                                        content.AppendLine(newRegisteredClients.Aggregate(string.Empty, (current, item) => current +
                                        string.Format("{0},{1},{2},{3},{4},{5}", item.BrandId, item.CustomerId, item.BTag, item.CountryCode,
                                                                                 item.RegistrationDate, item.LanguageId) + Environment.NewLine));
                                    }
                                    baseBll.SFTPUpload(content.ToString(), $"/{partner.Name}_signups_{dateString}.csv", string.Empty, ftpModel);
                                    content.Clear();
                                    content.AppendLine("BrandId,CustomerId,CurrencyId,BTag,ActivityDate,SportGrossRevenue,CasinoGrossRevenue," +
                                                       "SportBonusBetsAmount,CasinoBonusBetsAmount,SportBonusWinsAmount," +
                                                       "CasinoBonusWinsAmount,SportTotalWinAmount,CasinoTotalWinAmount,GrossGamingRevenue," +
                                                       "TotalDepositsAmount,TotalWithdrawalsAmounts,TotalDepositCount,TotalBetsCount");
                                    if (affClientActivies.Any())
                                    {
                                        content.AppendLine(affClientActivies.Aggregate(string.Empty, (current, item) => current +
                                        string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17}",
                                        item.BrandId, item.CustomerId, item.CurrencyId, item.BTag, item.ActivityDate,
                                        item.SportGrossRevenue, item.CasinoGrossRevenue, item.SportBonusBetsAmount, item.CasinoBonusBetsAmount,
                                        item.SportBonusWinsAmount, item.CasinoBonusWinsAmount, item.SportTotalWinsAmount, item.CasinoTotalWinsAmount,
                                        item.SportGrossRevenue + item.CasinoGrossRevenue, item.TotalDepositsAmount, item.TotalWithdrawalsAmount, item.TotalDepositsCount, item.TotalBetsCount) + Environment.NewLine));
                                    }
                                    var fileName = $"/{partner.Name}_activity_{dateString}.csv";
                                    baseBll.SFTPUpload(content.ToString(), fileName, string.Empty, ftpModel);
                                    baseBll.WriteToFile(content.ToString(), Path.Combine(localPath, fileName));
                                    break;
                                case AffiliatePlatforms.MyAffiliates:
                                    var affiliateClientActivies = affiliateService.GetMyAffiliateClientActivity(affClient.ToList(), brandId, fromDate, upToDate);
                                    /*content.AppendLine(string.Join(",", typeof(AffiliateReportInput).GetProperties().Select(x => x.Name.ToUpper()).ToArray()));
                                    if (newRegisteredClients.Any())
                                    {
                                        content.AppendLine(newRegisteredClients.Aggregate(string.Empty, (current, item) => current +
                                        string.Format("{0},{1},{2},{3},{4},{5}", item.BrandId, item.CustomerId, item.BTag, item.CountryCode, 
                                                                                 item.RegistrationDate, item.LanguageId) + Environment.NewLine));
                                    }
                                    baseBll.SFTPUpload(content.ToString(), $"/{partner.Name}_signups_{ dateString}.csv", ftpModel);
                                    content.Clear();*/
                                    content.AppendLine("Brand,PlayerId,Currency,BTag,TransactionDate,SportRevenue,CasinoRevenue," +
                                                       "SportWinAmount,CasinoWinAmount," +
                                                       "BonusSportBetsAmount,BonusCasinoBetsAmount,BonusSportWinsAmount,BonusCasinoWinsAmount,TotalBetCount," +
                                                       "FirstDepositAmount,DepositsAmount,WithdrawalsAmounts,DepositCount,ManualDepositAmount,ChargeBack," +
                                                       "AvailableBalance,BonusBalance,NGR," +
                                                       "CashBonusWinning,CorrectionCreditFrom,CorrectionDebitTo," +
                                                       "FreeBetAmount,FreeSpinAmount,FreeBetWinning,FreeSpinWinning");
                                    if (affiliateClientActivies.Any())
                                    {
                                        content.AppendLine(affiliateClientActivies.Aggregate(string.Empty, (current, item) => current +
                                        string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}," +
                                                      "{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29}",
                                        partner.Name, item.CustomerId, item.CurrencyId, item.BTag, item.ActivityDate,
                                        item.SportGrossRevenue, item.CasinoGrossRevenue, item.SportTotalWinsAmount, item.CasinoTotalWinsAmount,
                                        item.SportBonusBetsAmount, item.CasinoBonusBetsAmount, item.SportBonusWinsAmount,
                                        item.CasinoBonusWinsAmount, item.TotalBetsCount,
                                        item.FirstDepositAmount, item.TotalDepositsAmount, item.TotalWithdrawalsAmount, item.TotalDepositsCount, item.ManualDepositAmount,
                                        item.ChargeBack, item.AvailableBalance, item.BonusBalance,
                                        item.NGR, item.ConvertedBonusAmount, item.CreditCorrectionOnClient, item.DebitCorrectionOnClient, 0, 0, 0, 0) +
                                        Environment.NewLine));
                                    }
                                    fileName = $"/{partner.Name}_activity_{dateString}.csv";
                                    baseBll.SFTPUpload(content.ToString(), fileName, string.Empty, ftpModel);
                                    baseBll.WriteToFile(content.ToString(), Path.Combine(localPath, fileName));
                                    break;
                                case AffiliatePlatforms.VipAffiliate:
                                    affiliateClientActivies = affiliateService.GetMyAffiliateClientActivity(affClient.ToList(), brandId, fromDate, upToDate);
                                    content.AppendLine("Brand,PlayerId,Currency,BTag,TransactionDate,SportRevenue,CasinoRevenue," +
                                                       "SportTotalBetAmount,CasinoTotalBetAmount,SportWinAmount,CasinoWinAmount," +
                                                       "BonusSportBetsAmount,BonusCasinoBetsAmount,BonusSportWinsAmount,BonusCasinoWinsAmount,TotalBetCount," +
                                                       "FirstDepositAmount,DepositsAmount,WithdrawalsAmounts,DepositCount,ManualDepositAmount,ChargeBack," +
                                                       "AvailableBalance,BonusBalance,NGR," +
                                                       "CashBonusWinning,CorrectionCreditFrom,CorrectionDebitTo," +
                                                       "FreeBetAmount,FreeSpinAmount,FreeBetWinning,FreeSpinWinning");
                                    if (affiliateClientActivies.Any())
                                    {
                                        content.AppendLine(affiliateClientActivies.Aggregate(string.Empty, (current, item) => current +
                                        string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}," +
                                                      "{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31}",
                                        partner.Name, item.CustomerId, item.CurrencyId, item.BTag, item.ActivityDate,
                                        item.SportGrossRevenue, item.CasinoGrossRevenue,
                                        item.SportTotalBetsAmount, item.CasinoTotalBetsAmount,
                                        item.SportTotalWinsAmount, item.CasinoTotalWinsAmount,
                                        item.SportBonusBetsAmount, item.CasinoBonusBetsAmount, item.SportBonusWinsAmount,
                                        item.CasinoBonusWinsAmount, item.TotalBetsCount,
                                        item.FirstDepositAmount, item.TotalDepositsAmount, item.TotalWithdrawalsAmount, item.TotalDepositsCount, item.ManualDepositAmount,
                                        item.ChargeBack, item.AvailableBalance, item.BonusBalance,
                                        item.NGR, item.ConvertedBonusAmount, item.CreditCorrectionOnClient, item.DebitCorrectionOnClient, 0, 0, 0, 0) +
                                        Environment.NewLine));
                                    }
                                    fileName = $"/{partner.Name}_activity_{dateString}.csv";
                                    baseBll.SFTPUpload(content.ToString(), fileName, string.Empty, ftpModel);
                                    baseBll.WriteToFile(content.ToString(), localPath + fileName);
                                    break;
                                case AffiliatePlatforms.DIM:
                                    var dimClientActivies = affiliateService.GetDIMClientActivity(affClient.ToList(), brandId, fromDate, upToDate);

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
                                        string.Format("{0},{1},{2},{3},{4}", item.CustomerId, item.ActivityDate, item.TotalDepositsAmount, item.CurrencyId, 100) + Environment.NewLine));
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
                                case AffiliatePlatforms.Affilka:
                                    var affilkaClientActivies = JsonConvert.SerializeObject(affiliateService.GetAffilkaClientActivity(affClient.ToList(), fromDate, upToDate));
                                    var httpInput = new HttpRequestInput
                                    {
                                        RequestMethod = Constants.HttpRequestMethods.Post,
                                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                                        Url = $"{ftpUrl}/activity",
                                        RequestHeaders = new Dictionary<string, string> {
                                            { "X-Authorization-Key", ftpUsername },
                                            { "X-Authorization-Sign", CommonFunctions.ComputeHMACSha512(affilkaClientActivies, ftpPassword).ToLower() }
                                        },
                                        PostData = affilkaClientActivies
                                    };
                                    CommonFunctions.SendHttpRequest(httpInput, out _);
                                    baseBll.WriteToFile(content.ToString(), Path.Combine(localPath, $"{partner.Name}_activity_{dateString}.csv"));
                                    log.Debug("Affilka: " + affilkaClientActivies);
                                    break;
                                case AffiliatePlatforms.IncomeAccess:
									if (newRegisteredClients.Any())
									{
										content.AppendLine("REG_DATE,BTAG,PLAYER_ID,USERNAME,COUNTRY");
										content.AppendLine(newRegisteredClients.Aggregate(string.Empty, (current, item) => current +
										$"{item.RegistrationDate}, {item.BTag}, {item.CustomerId}, {item.CustomerName}, {item.CountryCode}" + Environment.NewLine));
									}
									baseBll.SFTPUpload(content.ToString(), $"/registration_report_{dateString}.csv", string.Empty, ftpModel);
									content.Clear();
									var incomeAccessClientActivies = affiliateService.GetIncomeAccessClientActivity(affClient.ToList(), brandId, fromDate, upToDate);
									if (incomeAccessClientActivies.Any())
									{
										content.AppendLine("TRANSACTION_DATE,PLAYER_ID,DEPOSITS,CHARGEBACKS," +
                                                           "CASINO_BETS,CASINO_REVENUE,CASINO_STAKE,CASINO_BONUS," +
														   "LIVE_CASINO_BETS,LIVE_CASINO_REVENUE,LIVE_CASINO_STAKE,LIVE_CASINO_BONUS," +
														   "PARIPLAY_CASINO_BETS,PARIPLAY_CASINO_REVENUE,PARIPLAY_CASINO_STAKE,PARIPLAY_CASINO_BONUS," +
                                                           "SPORTSBOOK_BETS,SPORTSBOOK_REVENUE,SPORTSBOOK_STAKE,SPORTSBOOK_BONUS,USERNAME");
										content.AppendLine(incomeAccessClientActivies.Aggregate(string.Empty, (current, item) => current +
										$"{item.ActivityDate}, {item.CustomerId}, {item.TotalDepositsAmount}, {item.ChargeBack}," +
                                        $" 0, 0, 0, 0," +
                                        $" 0, 0, 0, 0," +
                                        $" 0, 0, 0, 0," +
                                        $" {item.TotalBetsCount}, {item.SportGrossRevenue}, {item.SportTotalBetsAmount}, {item.SportBonusBetsAmount}, {item.UserName}"
                                        + Environment.NewLine));
									}
									baseBll.SFTPUpload(content.ToString(), $"/sales_report_{dateString}.csv", string.Empty, ftpModel);
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
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        public static void CalculateAgentsProfit(Job job, ILog log)
        {
            try
            {
                var toDate = job.NextExecutionTime;                
                var currentTime = DateTime.UtcNow;
                var tDate = toDate.Year * 1000000 + toDate.Month * 10000 + toDate.Day * 100 + 10;
                var fromDate = toDate.AddDays(-7);
                var fDate = fromDate.Year * 1000000 + fromDate.Month * 10000 + fromDate.Day* 100 + 10;

                using (var userBll = new UserBll(new SessionIdentity(), log))
                {
                    using (var partnerBll = new PartnerBll(userBll))
                    {
                        using (var documentBll = new DocumentBll(userBll))
                        {
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
                                        if (commissionTypes.Contains((int)AgentCommissionTypes.GGRWithoutTurnover))
                                        {
                                            var transactions = userBll.GetAgentTurnoverProfit(fDate, tDate);
                                            var item = transactions.FirstOrDefault(x => x.RecieverAgentId == agentProfit.RecieverAgentId && x.SenderAgentId == agentProfit.AgentId);
                                            amount = Math.Max(0, amount - (item != null ? item.TotalProfit : 0));
                                        }
                                        if (((commissionTypes.Contains((int)AgentCommissionTypes.GGRWithoutTurnover) ||
                                              commissionTypes.Contains((int)AgentCommissionTypes.GGRWithTurnover)) && amount > 0) ||
                                            (commissionTypes.Contains((int)AgentCommissionTypes.PT) && amount != 0))
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
                                                ProductGroupId = agentProfit.ProductGroupId ?? Constants.PlatformProductId,
                                                ProductId = agentProfit.ProductId,
                                                CreationTime = currentTime,
                                                CalculationStartingTime = new DateTime(fromDate.Year, fromDate.Month, 1),
                                                CreationDate = currentTime.Year * (int)1000000 + currentTime.Month * 10000 + currentTime.Day * 100 + currentTime.Hour,
                                                CalculationStartingDate = fDate
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
                log.Error(fDate + "_" + tDate);
            }
        }

        public static void TriggerMissedDepositCRM(Job job, ILog log)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                using (var notificationBl = new NotificationBll(new SessionIdentity(), log)) //++++
                {
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
                                     x.ClientInfoType == (int)ClientInfoTypes.MissedDepositEmail && x.NickName == messageTemplateNikeName && 
                                     x.State == (int)MessageTemplateStates.Active).FirstOrDefault();
                                if (messageTemplate != null)
                                {
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
                        }
                        db.SaveChanges();
                    }
                }
            }
        }

        public static void NotifyIdentityExpiration(ILog log)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                using (var notificationBl = new NotificationBll(new SessionIdentity(), log))//++++
                {
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
                        var messageTemplate = db.fn_MessageTemplate(c.Client.LanguageId).Where(x => x.PartnerId == c.Client.PartnerId &&
                                                                       x.ClientInfoType == (int)ClientInfoTypes.IdentityCloseToExpire && 
                                                                       x.State == (int)MessageTemplateStates.Active).FirstOrDefault();
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
            }
        }

        public static void CheckInactiveClients(ILog log)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                using (var clientBll = new ClientBll(new SessionIdentity(), log))
                {
                    using (var notificationBl = new NotificationBll(clientBll.Identity, log))
                    {
                        var partnerSetting = db.WebSiteMenuItems.Where(x => x.WebSiteMenu.Type == Constants.WebSiteConfiguration.Config && x.Title == Constants.PartnerKeys.BlockForInactivity &&
                                                       x.Href != "0").ToDictionary(x => x.WebSiteMenu.PartnerId, x => Convert.ToInt32(x.Href));
                        var currentDate = DateTime.UtcNow;
                        foreach (var partner in partnerSetting)
                        {
                            var clients = db.Clients.Include(x => x.Partner).Where(x => x.PartnerId == partner.Key && (x.State == (int)ClientStates.Active || x.State == (int)ClientStates.Suspended) &&
                                                                x.ClientSession.EndTime < DbFunctions.AddDays(currentDate, -partner.Value) &&
                                                                !x.ClientSettings.Any(y => y.Name == Constants.ClientSettings.BlockedForInactivity && y.NumericValue == 1)).ToList();
                            foreach (var c in clients)
                            {
                                clientBll.AddOrUpdateClientSetting(c.Id, Constants.ClientSettings.BlockedForInactivity, 1, string.Empty, null, null, "System");
                                BroadcastRemoveCache(string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSettings, c.Id, Constants.ClientSettings.BlockedForInactivity));
                                var messageTemplate = db.fn_MessageTemplate(c.LanguageId).Where(x => x.PartnerId == c.PartnerId &&
                                                                 x.ClientInfoType == (int)ClientInfoTypes.ClientInactivityEmail && 
                                                                 x.State == (int)MessageTemplateStates.Active).FirstOrDefault();
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
                }
            }
        }

        public static void RestrictUnverifiedClients(ILog log)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                var partnerSettings = db.WebSiteMenuItems.Where(x => x.WebSiteMenu.Type == Constants.WebSiteConfiguration.Config && 
                                                                     x.Title == Constants.PartnerKeys.RestrictUnverifiedClients && x.Href != "0")
                                                         .ToDictionary(x => x.WebSiteMenu.PartnerId, x => Convert.ToInt32(x.Href));
                var currentDate = DateTime.UtcNow;
                var clientIds = new List<int>();
                foreach (var partner in partnerSettings)
                {
                    var clients = db.Clients.Where(x => x.PartnerId == partner.Key && x.State == (int)ClientStates.Active && !x.IsDocumentVerified &&
                                                        x.CreationTime < DbFunctions.AddHours(currentDate, -partner.Value)).ToList();
                    clients.ForEach(x =>
                    {
                        x.State = (int)ClientStates.Suspended;
                        clientIds.Add(x.Id);
                    });
                    db.SaveChanges();
                }
                clientIds.ForEach(x =>
                {
                    CacheManager.RemoveClientFromCache(x);
                    BroadcastRemoveClientFromCache(x.ToString());
                });
            }
        }

        public static void CheckInactiveUsers()
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                var partnerSetting = db.WebSiteMenuItems.Where(x => x.WebSiteMenu.Type == Constants.WebSiteConfiguration.Config && x.Title == Constants.PartnerKeys.BlockUserForInactivity &&
                                               x.Href != "0").ToDictionary(x => x.WebSiteMenu.PartnerId, x => Convert.ToInt32(x.Href));
                var currentDate = DateTime.UtcNow;
                foreach (var partner in partnerSetting)
                {
                    db.Users.Where(x => x.PartnerId == partner.Key && (x.State == (int)UserStates.Active || x.State == (int)UserStates.Suspended) &&
                                        x.UserSession.EndTime < DbFunctions.AddDays(currentDate, -partner.Value))
                            .UpdateFromQuery(x => new User { State = (int)UserStates.InactivityClosed });

                }
            }
        }

        public static void ApplyClientRestriction(ILog log)
        {
            var restrictionKeys = typeof(DAL.Models.Clients.ClientCustomSettings).GetProperties()
                                                                                 .Where(x => !x.Name.StartsWith("System") && x.Name.Contains("Limit"))
                                                                                 .Select(x => x.Name).ToList();
            var currentTime = DateTime.UtcNow;
            try
            {
                using (var db = new IqSoftCorePlatformEntities())
                {
                    var clientSettings = db.ClientSettings.Where(x => restrictionKeys.Contains(x.Name) && x.DateValue.HasValue && x.LastUpdateTime != x.DateValue)
                                                          .GroupBy(x => x.Client.PartnerId);
                    foreach (var setting in clientSettings)
                    {
                        var limitFutureUpdateInDays = CacheManager.GetConfigKey(setting.Key, Constants.PartnerKeys.LimitFutureUpdateInDays);
                        if (int.TryParse(limitFutureUpdateInDays, out int updateInDays) && updateInDays > 0)
                        {
                            var updatingSetting = setting.Where(x => currentTime >= x.DateValue.Value.AddDays(updateInDays) && decimal.TryParse(x.StringValue, out _)).ToList();
                            updatingSetting.ForEach(x =>
                            {
                                x.LastUpdateTime = x.DateValue;
                                x.NumericValue = Convert.ToDecimal(x.StringValue);
                                BroadcastRemoveCache(string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSettings, x.ClientId, x.Name));
                            });
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Info(ex);
            }
        }

        public static void CheckClientBlockedSessions(ILog log)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                using (var notificationBl = new NotificationBll(new SessionIdentity(), log))
                {
                    using (var clientBl = new ClientBll(notificationBl))
                    {
                        var currentTime = DateTime.UtcNow;
                        var excludedClients = db.ClientSettings.Include(x => x.Client.Partner).
                            Where(x => (x.Name == Constants.ClientSettings.SelfExcluded || x.Name == Constants.ClientSettings.SystemExcluded)
                            && x.NumericValue == 1 && x.DateValue < currentTime).GroupBy(x => x.Client.PartnerId).ToList();
                        foreach (var partnerClients in excludedClients)
                        {
                            var clientsGroupedByLang = partnerClients.GroupBy(x => x.Client.LanguageId);
                            var verificationPlatform = CacheManager.GetConfigKey(partnerClients.Key, Constants.PartnerKeys.VerificationPlatform);
                            foreach (var clients in clientsGroupedByLang)
                            {
                                var messageTemplate = db.fn_MessageTemplate(clients.Key).Where(y => y.PartnerId == partnerClients.Key &&
                                                                       y.ClientInfoType == (int)ClientInfoTypes.SelfExclusionFinished && 
                                                                       y.State == (int)MessageTemplateStates.Active).FirstOrDefault();

                                foreach (var c in clients)
                                {
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(verificationPlatform) && int.TryParse(verificationPlatform, out int verificationPatformId))
                                        {
                                            switch (verificationPatformId)
                                            {
                                                case (int)VerificationPlatforms.Insic:
                                                    InsicHelpers.PlayerUnexcluded(c.Client.PartnerId, c.Client.Id, null, log);
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                    catch(Exception e)
                                    { log.Info(e); }
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
                                    var oldSettings = JsonConvert.SerializeObject(clientBl.GetClientSettings(c.Client.Id, false));
                                    notificationBl.SaveChangesWithHistory((int)ObjectTypes.ClientSetting, c.Client.Id, oldSettings, "System");
                                    c.NumericValue = 0;
                                }
                            }
                            db.SaveChanges();
                        }
                   // add here insic broadcast
                    }
                }
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
            using (var db = new IqSoftCorePlatformEntities())
            {
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
        }

        public static void DeactivateExiredKYC(ILog log)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                using (var notificationBl = new NotificationBll(new SessionIdentity(), log))
                {
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
                        var messageTemplate = db.fn_MessageTemplate(c.Client.LanguageId).Where(x => x.PartnerId == c.Client.PartnerId &&
                                                                             x.ClientInfoType == (int)ClientInfoTypes.IdentityExpired && 
                                                                             x.State == (int)MessageTemplateStates.Active).FirstOrDefault();
                        if (messageTemplate != null)
                        {
                            var documentTypeName = CacheManager.GetEnumerations(nameof(KYCDocumentTypes), c.Client.LanguageId)
                                                               .FirstOrDefault(x => x.Value == c.DocumentTypeId)?.Text;
                            var messageTextTemplate = messageTemplate.Text.Replace("\\n", Environment.NewLine)
                                                           .Replace("{u}", c.Client.UserName)
                                                           .Replace("{w}", c.Client.Partner.SiteUrl.Split(',')[0])
                                                           .Replace("{pc}", c.Client.Id.ToString())
                                                           .Replace("{it}", documentTypeName)
                                                           .Replace("{fn}", c.Client.FirstName)
                                                           .Replace("{e}", c.Client.Email)
                                                           .Replace("{m}", c.Client.MobileNumber);
                            notificationBl.SaveEmailMessage(c.Client.PartnerId, c.Client.Id, c.Client.Email, c.Client.Partner.Name, messageTextTemplate, messageTemplate.Id);
                        }
                    }

                    db.SaveChanges();
                }
            }
        }

        public static void InactivateImpossiblBonuses()
        {
            var currentDate = DateTime.UtcNow;
            using (var db = new IqSoftCorePlatformEntities())
            {
                var impossibleTriggerGroups = db.TriggerGroups.Where(x => (x.Type == (int)TriggerGroupType.All && x.TriggerGroupSettings.Any(y => y.TriggerSetting.FinishTime < currentDate)) ||
                (x.Type == (int)TriggerGroupType.Any && x.TriggerGroupSettings.All(y => y.TriggerSetting.FinishTime < currentDate))).Select(x => x.Id).ToList();
                db.Bonus.Where(x => x.Status == (int)BonusStatuses.Active && (x.FinishTime < currentDate ||
                                   (x.MaxGranted.HasValue && x.TotalGranted >= x.MaxGranted) ||
                                   (x.MaxReceiversCount.HasValue && x.TotalReceiversCount >= x.MaxReceiversCount) ||
                                   (x.TriggerGroups.Any() && x.TriggerGroups.All(y => impossibleTriggerGroups.Contains(y.Id)))))
                        .UpdateFromQuery(x => new Bonu { Status = (int)BonusStatuses.Inactive });
            }
        }

        public static void AwardClientCampaignBonus(ILog log)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                var currentDate = DateTime.UtcNow;
                var lostBonuses = db.ClientBonus.Where(x => x.Status == (int)ClientBonusStatuses.NotAwarded &&
                (x.Bonu.Status != (int)BonusStatuses.Active || !x.Bonu.ValidForAwarding.HasValue || 
                    DbFunctions.AddHours(x.CreationTime, x.Bonu.ValidForAwarding.Value) < currentDate ||
                x.Bonu.FinishTime < currentDate)).ToList();
                foreach (var b in lostBonuses)
                {
                    b.Status = (int)ClientBonusStatuses.Lost;
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
                var notAwardedBonuses = db.ClientBonus.Include(x => x.Bonu).Include(x => x.Client.Partner).Where(x => x.Status == (int)ClientBonusStatuses.NotAwarded).ToList();
                var bonusIds = notAwardedBonuses.Select(x => x.BonusId).Distinct().ToList();
                var currencies = db.Currencies.ToDictionary(x => x.Id, x => x.CurrentRate);
                var bonuses = db.Bonus.Where(x => bonusIds.Contains(x.Id)).Select(x => new
                {
                    BonusId = x.Id,
                    x.FinalAccountTypeId,
                    x.Type,
                    x.Priority,
                    x.Info,
                    x.TurnoverCount,
                    BonusMinAmount = x.MinAmount,
                    BonusMaxAmount = x.MaxAmount,
                    x.Sequence,
                    x.WinAccountTypeId,
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
                            z.TriggerSetting.Id,
                            z.TriggerSetting.Type,
                            z.TriggerSetting.Percent,
                            z.TriggerSetting.MinAmount,
                            z.TriggerSetting.MaxAmount,
                            z.Order
                        })
                    }),
                    Triggers = x.TriggerGroups.SelectMany(y => y.TriggerGroupSettings).OrderBy(z => z.Order).Select(z => z.TriggerSetting.Id)
                }).ToList();

                var bonusTriggers = db.ClientBonusTriggers.Where(x => bonusIds.Contains(x.BonusId)).ToList();
                var awardedBonuses = new List<ClientBonu>();
                foreach (var clientBonus in notAwardedBonuses)
                {
                    var bonus = bonuses.First(x => x.BonusId == clientBonus.BonusId);
                    var partner = db.Partners.First(x => x.Id == bonus.PartnerId);

                    var clientBonusTriggers = bonusTriggers.Where(x => x.BonusId == clientBonus.BonusId && x.ReuseNumber == clientBonus.ReuseNumber &&
                      (x.ClientId == clientBonus.ClientId || x.ClientId == null) &&
                      (x.BetCount == null || x.BetCount >= x.TriggerSetting.MinBetCount) &&
                      (x.WageringAmount == null || x.WageringAmount >= BaseBll.ConvertCurrency(partner.CurrencyId, clientBonus.Client.CurrencyId, x.TriggerSetting.MinAmount.Value))).ToList();

                    var clientBonusTriggerIds = clientBonusTriggers.Select(x => x.TriggerId).ToList();
                    if (bonus.Groups.All(y =>
                                       (y.GroupType == (int)TriggerGroupType.Any && (!y.TriggerSettings.Any() || y.TriggerSettings.Any(z => clientBonusTriggerIds.Contains(z.Id)))) ||
                                       (y.GroupType == (int)TriggerGroupType.All && y.TriggerSettings.All(z => clientBonusTriggerIds.Contains(z.Id)))))
                    {
                        decimal? sourceAmount = null;
                        foreach (var t in bonus.Triggers)
                        {
                            sourceAmount = clientBonusTriggers.FirstOrDefault(x => x.TriggerId == t)?.SourceAmount;
                            if (sourceAmount != null)
                                break;
                        }
                        clientBonus.PossibleBonusPrize = sourceAmount ?? clientBonus.BonusPrize;
                        awardedBonuses.Add(clientBonus);
                    }
                }
                var groupedBonuses = awardedBonuses.GroupBy(x => x.Client.PartnerId);
                var partnerKeys = db.WebSiteMenuItems.Where(x => x.WebSiteMenu.Type == Constants.WebSiteConfiguration.Config &&
                                                                 x.Title == Constants.PartnerKeys.ActiveBonusMaxCount)
                                                    .ToDictionary(x => x.WebSiteMenu.PartnerId, x => x.Href);

                foreach (var gb in groupedBonuses)
                {
                    int count = 1;
                    if (partnerKeys.ContainsKey(gb.Key) && !string.IsNullOrEmpty(partnerKeys[gb.Key]))
                        int.TryParse(partnerKeys[gb.Key], out count);
                    var partnerBonuses = gb.OrderBy(x => x.Bonu.Priority ?? 0).ToList();
                    foreach (var ab in partnerBonuses)
                    {
                        try
                        {
                            if (ab.Bonu.Type == (int)BonusTypes.CampaignCash)
                            {
                                ab.FinalAmount = ab.PossibleBonusPrize;
                                ab.Status = (int)ClientBonusStatuses.Finished;
                                db.SaveChanges();
                                BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.NotAwardedCampaigns, ab.ClientId));
                            }
                            else if (ab.Bonu.Type == (int)BonusTypes.CampaignFreeBet)
                            {
                                ab.Status = (int)ClientBonusStatuses.Active;
                                ab.AwardingTime = DateTime.UtcNow;
                                ab.BonusPrize = ab.PossibleBonusPrize.Value;
                                ab.TurnoverAmountLeft = ab.BonusPrize;
                                db.SaveChanges();
                                BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.NotAwardedCampaigns, ab.ClientId));
                            }
                            else if (ab.Bonu.Type == (int)BonusTypes.CampaignFreeSpin)
                            {
                                ab.Status = (int)ClientBonusStatuses.Active;
                                ab.AwardingTime = DateTime.UtcNow;
                                ab.BonusPrize = 0;
                                db.SaveChanges();
                                BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.NotAwardedCampaigns, ab.ClientId));
                            }
                            else if (ab.Bonu.Type == (int)BonusTypes.CampaignWagerCasino || ab.Bonu.Type == (int)BonusTypes.CampaignWagerSport)
                            {
                                if (count > 0 && db.ClientBonus.Count(x => x.ClientId == ab.ClientId &&
                                   (x.Status == (int)ClientBonusStatuses.Active || x.Status == (int)ClientBonusStatuses.Finished) &&
                                   (x.Bonu.Type == (int)BonusTypes.CampaignWagerCasino || x.Bonu.Type == (int)BonusTypes.CampaignWagerSport)) >= count)
                                   throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientAlreadyHasActiveBonus,
                                       info: "AwardClientCampaignBonus_" + ab.Id + "_" + ab.ClientId);

                                var bonusAmount = ab.PossibleBonusPrize.Value;
                                using (var bonusService = new BonusService(new SessionIdentity(), log))
                                {
                                    var partnerAmount = BaseBll.ConvertCurrency(ab.Client.CurrencyId, ab.Client.Partner.CurrencyId, bonusAmount);
                                    bonusService.GiveWageringBonus(ab.Bonu, ab.Client, bonusAmount, ab.ReuseNumber ?? 1);
                                }
                                ab.Status = (int)ClientBonusStatuses.Active;
                                ab.AwardingTime = DateTime.UtcNow;
                                ab.BonusPrize = bonusAmount;
                                ab.TurnoverAmountLeft = ab.BonusPrize * ab.Bonu.TurnoverCount;
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
                            log.Error(JsonConvert.SerializeObject(new { Id = ex.Detail.Id, Message = ex.Detail.Message, Info = ex.Detail.Info }) + 
                                "_" + JsonConvert.SerializeObject(new { ab.Id, ab.BonusId, ab.ClientId }));
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex);
                        }
                    };
                }
            }
        }

        public static void UpdateJackpotFeed(ILog log)
        {
            try
            {
                using (var db = new IqSoftCorePlatformEntities())
                {
                    var gameProviders = db.GameProviders.Select(x => new { x.Id, x.Name }).ToList();
                    foreach (var gp in gameProviders)
                    {
                        switch (gp.Name)
                        {
                            case Constants.GameProviders.BlueOcean:
                                var jackpodFeed = Integration.Products.Helpers.BlueOceanHelpers.GetJackpotFeed(Constants.MainPartnerId, log);
                                var providerProducts = db.Products.Where(x => x.GameProviderId == gp.Id).ToList();
                                var hasJackpotroducts = providerProducts.Where(x => jackpodFeed.Keys.Contains(x.ExternalId)).ToList();
                                var ids = hasJackpotroducts.Select(x => x.Id).ToList();
                                db.Products.Where(x => x.GameProviderId == gp.Id && !ids.Contains(x.Id)).UpdateFromQuery(x => new Product { Jackpot = null });
                                var prods = db.Products.Where(x => x.GameProviderId == gp.Id && ids.Contains(x.Id)).ToList();
                                prods.ForEach(x =>
                                {
                                    x.Jackpot = JsonConvert.SerializeObject(jackpodFeed[x.ExternalId]);
                                });
                                db.SaveChanges();//keep jackpots separately
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public static void FulfillDepositAction(ILog log)
        {
            try
            {
                var depositTriggers = new List<JobTrigger>();
                using (var transactionScope = CommonFunctions.CreateTransactionScope())
                {
                    using (var db = new IqSoftCorePlatformEntities())
                    {
                        using (var clientBl = new ClientBll(new SessionIdentity(), log))
                        using (var documentBl = new DocumentBll(clientBl))
                        using (var bonusService = new BonusService(clientBl))
                        using (var notificationBl = new NotificationBll(clientBl))
                        {
                            depositTriggers = db.JobTriggers.Include(x => x.PaymentRequest)
                                                            .Where(x => x.Type == (int)JobTriggerTypes.Deposit && x.PaymentRequestId.HasValue).ToList();
                            foreach (var trigger in depositTriggers)
                            {
                                var client = CacheManager.GetClientById(trigger.ClientId);
                                var depCount = CacheManager.GetClientDepositCount(trigger.ClientId);

                                var isInternalAffiliate = clientBl.GiveAffiliateCommission(client, trigger.PaymentRequestId.Value, 
                                    trigger.PaymentRequest.PartnerPaymentSettingId.Value, depCount, trigger.Amount ?? 0, documentBl);

                                var parameters = string.IsNullOrEmpty(trigger.PaymentRequest.Parameters) ? new Dictionary<string, string>() :
                                JsonConvert.DeserializeObject<Dictionary<string, string>>(trigger.PaymentRequest.Parameters);
                                if (!parameters.ContainsKey(nameof(trigger.PaymentRequest.BonusRefused)) ||
                                    !Convert.ToBoolean(parameters[nameof(trigger.PaymentRequest.BonusRefused)]))
                                    clientBl.CheckDepositBonus(trigger.PaymentRequest, bonusService);
                                
                                clientBl.ChangeClientDepositInfo(trigger.ClientId, depCount, trigger.PaymentRequest.Amount, trigger.PaymentRequest.LastUpdateTime);
                                notificationBl.SendDepositNotification(client.Id, trigger.PaymentRequest.Status, trigger.Amount ?? 0, string.Empty);
								var partnerKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CRMPlarforms).StringValue;
								if (!isInternalAffiliate || !string.IsNullOrWhiteSpace(partnerKey))
                                    notificationBl.DepositAffiliateNotification(client, trigger.PaymentRequest.Amount, trigger.PaymentRequest.Id, depCount, partnerKey);
								clientBl.AddClientJobTrigger(trigger.ClientId, (int)JobTriggerTypes.ReconsiderSegments);
                            }
                        }
                        db.JobTriggers.RemoveRange(depositTriggers);
                        db.SaveChanges();
                        transactionScope.Complete();
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
            var segId = 0;
            try
            {
                using (var db = new IqSoftCorePlatformEntities())
                {
                    using (var dwh = new IqSoftDataWarehouseEntities())
                    {
                        var triggers = db.JobTriggers.Where(x => x.Type == (int)JobTriggerTypes.ReconsiderSegments);
                        var allClientsIds = triggers.Select(x => x.ClientId).Distinct().ToList();
                        var fnClients = db.fn_Client().Where(x => allClientsIds.Contains(x.Id));
                        var partnerIds = fnClients.Select(x => x.PartnerId).Distinct().ToList();

                        if (triggers.Any())
                        {
                            var segments = db.Segments.Include(x=>x.Partner).Where(x => x.Mode == (int)PaymentSegmentModes.Dynamic &&
                                                                  x.State == (int)SegmentStates.Active && partnerIds.Contains(x.PartnerId))
                                                      .ToList().Select(x => x.MapToSegmentModel(0)).GroupBy(x => x.PartnerId);

                            foreach (var partnerSegments in segments)
                            {
                                var query = fnClients.Where(x => x.PartnerId == partnerSegments.Key);
                                foreach (var s in partnerSegments)
                                {
                                    segId = s.Id ?? 0;
                                    var sQuery = query.Where(x => (!s.IsKYCVerified.HasValue || x.IsDocumentVerified == s.IsKYCVerified) && (!s.Gender.HasValue || x.Gender == s.Gender));

                                    sQuery = CustomHelper.FilterByCondition(sQuery, s.ClientStatusSet, "State", isAndCondition: true);
                                    sQuery = CustomHelper.FilterByCondition(sQuery, s.ClientIdSet, "Id", isAndCondition: true);
                                    sQuery = CustomHelper.FilterByCondition(sQuery, s.EmailSet, "Email", isAndCondition: true);
                                    sQuery = CustomHelper.FilterByCondition(sQuery, s.FirstNameSet, "FirstName", isAndCondition: true);
                                    sQuery = CustomHelper.FilterByCondition(sQuery, s.LastNameSet, "LastName", isAndCondition: true);
                                    sQuery = CustomHelper.FilterByCondition(sQuery, s.AffiliateIdSet, "AffiliateId", isAndCondition: true);
                                    sQuery = CustomHelper.FilterByCondition(sQuery, s.RegionSet, "RegionId", isAndCondition: true);
                                    sQuery = CustomHelper.FilterByCondition(sQuery, s.MobileCodeSet, "MobileNumber", isAndCondition: true);

                                    // query = CustomHelper.FilterByCondition(clients, s.SessionPeriod, "ClientSession.StartTime"); // must be clarified
                                    if (s.SignUpPeriod != null && s.SignUpPeriodObject.ConditionItems.Any())
                                        sQuery = CustomHelper.FilterByCondition(sQuery, s.SignUpPeriodObject, "CreationTime");

                                    var clientIds = sQuery.Select(x => x.Id).ToList();
                                    if (s.SessionPeriod != null && s.SessionPeriodObject.ConditionItems.Any())
                                    {
                                        var sessionPeriodQuery = dwh.ClientSessions.Where(x => clientIds.Contains(x.ClientId) && x.ProductId == Constants.PlatformProductId)
                                                                   .Select(x => new
                                                                   {
                                                                       x.ClientId,
                                                                       SessionPeriod = DbFunctions.DiffMinutes(x.StartTime, x.EndTime ?? DateTime.UtcNow)
                                                                   });
                                        CustomHelper.FilterByCondition(sessionPeriodQuery, s.SessionPeriodObject, "SessionPeriod");
                                        var tmpIds = sessionPeriodQuery.Select(x => x.ClientId).ToList();
                                        clientIds = clientIds.Where(x => tmpIds.Contains(x)).ToList();
                                    }

                                    if ((s.TotalDepositsAmount != null && s.TotalDepositsAmountObject.ConditionItems.Any()) ||
                                        (s.TotalDepositsCount != null && s.TotalDepositsCountObject.ConditionItems.Any()))
                                    {
                                        var dep = db.PaymentRequests.Where(x => clientIds.Contains(x.ClientId.Value) &&
                                                            (x.Type == (int)PaymentRequestTypes.Deposit || x.Type == (int)PaymentRequestTypes.ManualDeposit) &&
                                                            (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually))
                                                            .GroupBy(x => new { x.ClientId, x.CurrencyId } )
                                                            .Select(x => new { x.Key.ClientId, Currency = x.Key.CurrencyId, Amount = x.Sum(y => y.Amount), Count = x.Count() })
                                                            .ToList().Select(x=>new { x.ClientId, Amount = BaseBll.ConvertCurrency(x.Currency, s.CurrencyId, x.Amount), x.Count});
                                        var depQuery = dep.AsQueryable();
                                        var queryIds = clientIds.Where(x => !dep.Any(y => y.ClientId == x))
                                                                .Select(x => new { ClientId = x, Count = 0, Amount = 0 }).AsQueryable();
                                        queryIds = CustomHelper.FilterByCondition(queryIds, s.TotalDepositsAmountObject, "Amount");
                                        queryIds = CustomHelper.FilterByCondition(queryIds, s.TotalDepositsCountObject, "Count");
                                        if (dep.Any())
                                        {
                                            depQuery = CustomHelper.FilterByCondition(depQuery, s.TotalDepositsAmountObject, "Amount");
                                            depQuery = CustomHelper.FilterByCondition(depQuery, s.TotalDepositsCountObject, "Count");
                                        }
                                        var tmpIds = queryIds.ToList();
                                        dep = depQuery.ToList();
                                        clientIds = clientIds.Where(x => dep.Any(y => y.ClientId == x) ||
                                                                         tmpIds.Any(z => z.ClientId == x)).ToList();
                                    }
                                    if ((s.TotalWithdrawalsAmount != null && s.TotalWithdrawalsAmountObject.ConditionItems.Any()) ||
                                        (s.TotalWithdrawalsCount != null && s.TotalWithdrawalsCountObject.ConditionItems.Any()))
                                    {
                                        var withdIds = db.PaymentRequests.Where(x => clientIds.Contains(x.ClientId.Value) && x.Type == (int)PaymentRequestTypes.Withdraw &&
                                                            (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually))
                                                            .GroupBy(x =>new { x.ClientId, x.CurrencyId })
                                                            .Select(x => new { x.Key.ClientId, Currency = x.Key.CurrencyId, Amount = x.Sum(y => y.Amount), Count = x.Count() })
                                                            .ToList().Select(x => new { x.ClientId, Amount = BaseBll.ConvertCurrency(x.Currency, s.CurrencyId, x.Amount), x.Count }); 
                                        var withdQuery = withdIds.AsQueryable();
                                        var queryIds = clientIds.Where(x => !withdIds.Any(y => y.ClientId == x))
                                                                .Select(x => new { ClientId = x, Count = 0, Amount = 0 }).AsQueryable();
                                        queryIds = CustomHelper.FilterByCondition(queryIds, s.TotalWithdrawalsAmountObject, "Amount");
                                        queryIds = CustomHelper.FilterByCondition(queryIds, s.TotalWithdrawalsCountObject, "Count");

                                        if (withdIds.Any())
                                        {
                                            withdQuery = CustomHelper.FilterByCondition(withdQuery, s.TotalWithdrawalsAmountObject, "Amount");
                                            withdQuery = CustomHelper.FilterByCondition(withdQuery, s.TotalWithdrawalsCountObject, "Count");
                                        }
                                        var tmpIds = queryIds.ToList();
                                        withdIds = withdQuery.ToList();
                                        clientIds = clientIds.Where(x => withdIds.Any(y => y.ClientId == x) ||
                                                                         tmpIds.Any(z => z.ClientId == x)).ToList();
                                    }
                                    if ((s.TotalBetsCount != null && s.TotalBetsCountObject.ConditionItems.Any()) ||
                                        (s.TotalBetsAmount != null && s.TotalBetsAmountObject.ConditionItems.Any()))
                                    {

                                        var totalBets = dwh.Bets.Where(x => clientIds.Contains(x.ClientId.Value) && x.BetAmount > 0 && x.State != (int)BetDocumentStates.Deleted)
                                                               .GroupBy(x =>new { x.ClientId, x.CurrencyId })
                                                               .Select(x => new { x.Key.ClientId, Currency = x.Key.CurrencyId, Count = x.Count(), BetAmount = x.Sum(y => y.BetAmount) })
                                                               .ToList().Select(x => new { x.ClientId, BetAmount = BaseBll.ConvertCurrency(x.Currency, s.CurrencyId, x.BetAmount), x.Count }); 
                                        var betsQuery = totalBets.AsQueryable();
                                        var queryIds = clientIds.Where(x => !totalBets.Any(y => y.ClientId == x))
                                                               .Select(x => new { ClientId = x, Count = 0, BetAmount = 0 }).AsQueryable();

                                        if (s.TotalBetsCount != null && s.TotalBetsCountObject.ConditionItems.Any())
                                        {
                                            queryIds = CustomHelper.FilterByCondition(queryIds, s.TotalBetsCountObject, "Count");
                                            if (totalBets.Any())
                                                betsQuery = CustomHelper.FilterByCondition(betsQuery, s.TotalBetsCountObject, "Count");
                                        }
                                        if (s.TotalBetsAmount != null && s.TotalBetsAmountObject.ConditionItems.Any())
                                        {
                                            queryIds = CustomHelper.FilterByCondition(queryIds, s.TotalBetsAmountObject, "BetAmount");
                                            if (totalBets.Any())
                                                betsQuery = CustomHelper.FilterByCondition(betsQuery, s.TotalBetsAmountObject, "BetAmount");
                                        }
                                        var tmpIds = queryIds.ToList();
                                        totalBets = betsQuery.ToList();
                                        clientIds = clientIds.Where(x => totalBets.Any(y => y.ClientId == x) ||
                                                                         tmpIds.Any(z => z.ClientId == x)).ToList();
                                    }                                   

                                    if (s.ComplimentaryPoint != null && s.ComplimentaryPointObject.ConditionItems.Any())
                                    {
                                        var accountsQuery = db.Accounts.Where(x => x.ObjectTypeId == (int)ObjectTypes.Client && clientIds.Contains((int)x.ObjectId) &&
                                                                                   x.TypeId == (int)AccountTypes.ClientCompBalance)
                                                                       .GroupBy(x => x.ObjectId)
                                                                       .Select(x => new { ClientId = x.Key, Balance = x.Select(y => y.Balance).DefaultIfEmpty(0).Sum() });
                                        var accounts = accountsQuery?.ToList();
                                        var queryIds = clientIds.Where(x => accounts == null || !accounts.Any(y => y.ClientId == x))
                                                                       .Select(x => new { ClientId = x, Balance = 0 }).AsQueryable();
                                        queryIds = CustomHelper.FilterByCondition(queryIds, s.ComplimentaryPointObject, "Balance");
                                        if (accountsQuery != null)
                                            accountsQuery = CustomHelper.FilterByCondition(accountsQuery, s.ComplimentaryPointObject, "Balance");

                                        var tmpIds = queryIds.ToList();
                                        accounts = accountsQuery.ToList();
                                        clientIds = clientIds.Where(x => accounts.Any(y => y.ClientId == x) ||
                                                                         tmpIds.Any(z => z.ClientId == x)).ToList();
                                    }
                                    if (s.SegmentId != null && s.SegmentIdSet.ConditionItems.Any())
                                    {
                                        var cs = db.ClientClassifications.Where(x => clientIds.Contains(x.ClientId) &&
                                                                                     x.ProductId == Constants.PlatformProductId)
                                                                         .Select(x => new { x.ClientId, SegmentId = x.SegmentId.Value });
                                        CustomHelper.FilterByCondition(cs, s.SegmentIdSet, "SegmentId", isAndCondition: true);
                                        clientIds = clientIds.Where(x => cs.Any(y => y.ClientId == x)).ToList();
                                    }
                                    if (s.SportBetsCount != null && s.SportBetsCountObject.ConditionItems.Any())
                                    {
                                        var sportBetsQuery = dwh.Bets.Where(x => clientIds.Contains(x.ClientId.Value) &&
                                                                           x.ProductId == Constants.SportsbookProductId)
                                                               .GroupBy(x => x.ClientId)
                                                               .Select(x => new { ClientId = x.Key, Count = x.Count() });
                                        var sportBets = sportBetsQuery.ToList();
                                        var queryIds = clientIds.Where(x => !sportBets.Any(y => y.ClientId == x))
                                                                       .Select(x => new { ClientId = x, Count = 0 }).AsQueryable();
                                        queryIds = CustomHelper.FilterByCondition(queryIds, s.SportBetsCountObject, "Count");
                                        if (sportBets.Any())
                                            sportBetsQuery = CustomHelper.FilterByCondition(sportBetsQuery, s.SportBetsCountObject, "Count");

                                        var tmpIds = queryIds.ToList();
                                        sportBets = sportBetsQuery.ToList();
                                        clientIds = clientIds.Where(x => sportBets.Any(y => y.ClientId == x) ||
                                                                         tmpIds.Any(z => z.ClientId == x)).ToList();
                                    }
                                    if (s.CasinoBetsCount != null && s.CasinoBetsCountObject.ConditionItems.Any())
                                    {
                                        var casinoBetsQuery = dwh.Bets.Where(x => clientIds.Contains(x.ClientId.Value) &&
                                                                            x.ProductId != Constants.SportsbookProductId)
                                                                .GroupBy(x => x.ClientId)
                                                                .Select(x => new { ClientId = x.Key, Count = x.Count() });
                                        var casinoBets = casinoBetsQuery.ToList();
                                        var queryIds = clientIds.Where(x => !casinoBets.Any(y => y.ClientId == x))
                                                                       .Select(x => new { ClientId = x, Count = 0 }).AsQueryable();
                                        if (casinoBets.Any())
                                            casinoBetsQuery = CustomHelper.FilterByCondition(casinoBetsQuery, s.CasinoBetsCountObject, "Count");
                                        var tmpIds = queryIds.ToList();
                                        casinoBets = casinoBetsQuery.ToList();
                                        clientIds = clientIds.Where(x => casinoBets.Any(y => y.ClientId == x) ||
                                                                         tmpIds.Any(z => z.ClientId == x)).ToList();
                                    }
                                    if (s.SuccessDepositPaymentSystemList != null && s.SuccessDepositPaymentSystemList.Any())

                                    {
                                        var successDepositPayments = db.PaymentRequests.Where(x => x.ClientId.HasValue && clientIds.Contains(x.ClientId.Value) &&
                                                   (x.Type == (int)PaymentRequestTypes.Deposit || x.Type == (int)PaymentRequestTypes.ManualDeposit) &&
                                                   (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually))
                                                   .GroupBy(x => x.ClientId)
                                                   .Select(x => new { ClientId = x.Key, PaymentSystemIds = x.Select(y => y.PaymentSystemId).Distinct().ToList() }).ToList();
                                        var tmpClientIds = successDepositPayments.Where(x => s.SuccessDepositPaymentSystemList.All( y=> x.PaymentSystemIds.Contains(y))).Select(x => x.ClientId).ToList();
                                        clientIds = clientIds.Where(x => tmpClientIds.Contains(x)).ToList();
                                    }
                                    if (s.SuccessWithdrawalPaymentSystemList != null && s.SuccessWithdrawalPaymentSystemList.Any())
                                    {
                                        var successWithdrawalPayments = db.PaymentRequests.Where(x => x.ClientId.HasValue && clientIds.Contains(x.ClientId.Value) &&
                                                        x.Type == (int)PaymentRequestTypes.Withdraw &&
                                                       (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually))
                                                       .GroupBy(x => x.ClientId)
                                                       .Select(x => new { ClientId = x.Key, PaymentSystemIds = x.Select(y => y.PaymentSystemId).Distinct().ToList() }).ToList();

                                        var tmpClientIds = successWithdrawalPayments.Where(x =>s.SuccessWithdrawalPaymentSystemList.All(y=> x.PaymentSystemIds.Contains(y))).Select(x => x.ClientId).ToList();
                                        clientIds = clientIds.Where(x => tmpClientIds.Contains(x)).ToList();
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

                                    db.ClientClassifications.Where(x => x.SegmentId == s.Id && removables.Any(y => y == x.ClientId)).DeleteFromQuery();
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
                                    db.SaveChanges();

                                    foreach (var r in removables)
                                        CacheManager.RemoveClientClassifications(r);
                                    foreach (var cs in clientSegments)
                                        CacheManager.RemoveClientClassifications(cs.ClientId);
                                }
                            }
                            db.JobTriggers.RemoveRange(triggers);
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"SegmentId: {segId},  " + ex);
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
								CacheManager.RemoveClientBalance(client.ClientId.Value);
								var partnerSetting = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.CharactersEnabled);
                                if (partnerSetting == "1")
                                {
                                    var dbClient = db.Clients.Find(client.ClientId.Value);
                                    if(dbClient.CharacterId != null)
									{
										var compPoints = db.Accounts.FirstOrDefault(x => x.ObjectId == client.ClientId && x.ObjectTypeId == (int)ObjectTypes.Client &&
																						 x.CurrencyId == client.CurrencyId && x.TypeId == (int)AccountTypes.ClientCompBalance);
										if (compPoints != null && compPoints.Balance != 0)
                                        {
                                            var characters = CacheManager.GetCharacters(dbClient.PartnerId, dbClient.LanguageId);
											var currentCharachter = characters.FirstOrDefault(x => x.Id == dbClient.CharacterId);
											var nextCharacter = characters.Where(x => x.ParentId == currentCharachter.ParentId && x.Order > currentCharachter.Order)
                                                                          .OrderBy(y => y.Order).FirstOrDefault();
											if (nextCharacter != null && compPoints.Balance >= nextCharacter.CompPoints)
											{
												dbClient.CharacterId = nextCharacter.Id;
												db.SaveChanges();
												CacheManager.RemoveClientFromCache(client.ClientId.Value);
											}
										}
									}
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public static void SettleBets(ILog log)
        {
            var unsettleBets = new List<long>();
            using (var db = new IqSoftCorePlatformEntities())
            {
                var toTime = DateTime.UtcNow.AddMinutes(-10);
                var fromTime = toTime.AddMinutes(-20);
                unsettleBets = db.Documents.Where(x => x.OperationTypeId == (int)OperationTypes.Bet && x.State == (int)BetDocumentStates.Uncalculated &&
                    x.CreationTime >= fromTime && x.CreationTime < toTime && x.Product.CategoryId == (int)ProiductCategories.Slots).Select(x => x.Id).ToList();
            }
            if (unsettleBets.Any())
            {
                using (var clientBl = new ClientBll(new SessionIdentity(), log))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        foreach (var b in unsettleBets)
                        {
                            try
                            {
                                var betDocument = documentBl.GetDocumentById(b);
                                betDocument.State = (int)BetDocumentStates.Lost;
                                var client = CacheManager.GetClientById(betDocument.ClientId ?? 0);
                                var input = new ListOfOperationsFromApi
                                {
                                    CurrencyId = betDocument.CurrencyId,
                                    GameProviderId = betDocument.GameProviderId ?? 0,
                                    OperationTypeId = (int)OperationTypes.Win,
                                    ExternalOperationId = null,
                                    ProductId = betDocument.ProductId,
                                    TransactionId = betDocument.Id + "_lost_internal",
                                    CreditTransactionId = betDocument.Id,
                                    State = (int)BetDocumentStates.Lost,
                                    Info = string.Empty,
                                    OperationItems = new List<OperationItemFromProduct>()
                                };

                                input.OperationItems.Add(new OperationItemFromProduct
                                {
                                    Client = client,
                                    Amount = 0
                                });
                                clientBl.CreateDebitsToClients(input, betDocument, documentBl);
                            }
                            catch (FaultException<BllFnErrorType> ex)
                            {
                                log.Error(JsonConvert.SerializeObject(ex));
                            }
                        }
                    }
                }
            }
        }

        public static void FairSegmentTriggers(ILog log)
        {
            try
            {
                using (var clientBl = new ClientBll(new SessionIdentity(), log))
                {
                    clientBl.FairSegmentTriggers();
                }
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
                using (var bonusService = new BonusService(new SessionIdentity(), log))
                {
                    bonusService.GiveJackpotWin();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public static void BroadcastRemoveCache(string key)
        {
            BaseHub._connectedClients.Group("BaseHub").onRemoveKeyFromCache(key);
        }

        public static void BroadcastRemoveClientFromCache(string key)
        {
            BaseHub._connectedClients.Group("BaseHub").onRemoveClient(key);
        }
    }
}
