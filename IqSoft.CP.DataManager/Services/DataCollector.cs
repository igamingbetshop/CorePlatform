﻿using IqSoft.CP.DataWarehouse;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace IqSoft.CP.DataManager.Services
{
    public class DataCollector
    {
        private static string CorePlatformDbConnectionString = ConfigurationManager.AppSettings["IqSoftCorePlatformEntities"];

        static DataCollector()
        {
        }

        public static void MigrateDocuments()
        {
            try
            {
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    db.Database.CommandTimeout = 600;
                    var lastDocumentDate = db.Documents.OrderByDescending(x => x.CreationTime).Select(x => x.CreationTime).FirstOrDefault();
                    var ids = db.Documents.Where(x => x.CreationTime == lastDocumentDate).Select(x => x.Id).ToList();
                    var existings = string.Empty;
                    if (ids.Any())
                        existings = String.Join(",", ids);
                    else
                        existings = "0";
                    db.sp_InsertDocuments(lastDocumentDate, existings);
                }
                //Program.DbLogger.Info("MigrateDocuments_Finished");
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
        }

        public static void MigrateClients()
        {
            try
            {
                var updateTime = DateTime.UtcNow.AddHours(-1);
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var lastClientId = db.Clients.OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefault();
                    var sqlString = "SELECT TOP(5000) * FROM Client WHERE Id > @clientId or LastUpdateTime > @updateTime"; 
                    var context = new DbContext(CorePlatformDbConnectionString);
                    var paramId = new SqlParameter("@clientId", lastClientId);
                    var paramUpdateDate = new SqlParameter("@updateTime", updateTime);
                    var result = context.Database.SqlQuery<Client>(sqlString, paramId, paramUpdateDate).ToList();
                    db.Clients.AddOrUpdateExtension(result);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
        }

        public static void MigrateUsers()
        {
            try
            {
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var sqlString = "SELECT * FROM [User]";
                    var context = new DbContext(CorePlatformDbConnectionString);
                    var result = context.Database.SqlQuery<User>(sqlString).ToList();
                    if (result.Count > 0)
                    {
                        foreach (var r in result)
                        {
                            var old = db.Users.FirstOrDefault(x => x.Id == r.Id);
                            if (old == null)
                                db.Users.Add(r);
                            else
                            {
                                old.State = r.State;
                                old.LastUpdateTime = r.LastUpdateTime;
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
        }

        public static void MigrateProducts()
        {
            try
            {
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var sqlString = "SELECT * FROM [Product]";
                    var context = new DbContext(CorePlatformDbConnectionString);
                    var result = context.Database.SqlQuery<Product>(sqlString).ToList();
                    if (result.Count > 0)
                    {
                        foreach (var r in result)
                        {
                            var old = db.Products.FirstOrDefault(x => x.Id == r.Id);
                            if (old == null)
                                db.Products.Add(r);
                            else
                            {
                                old.State = r.State;
                                old.GameProviderId = r.GameProviderId;
                                old.SubproviderId = r.SubproviderId;
                                old.CategoryId = r.CategoryId;
                                old.WebImageUrl = r.WebImageUrl;
                                old.MobileImageUrl = r.MobileImageUrl;
                                old.LastUpdateTime = r.LastUpdateTime;
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
        }

        public static void MigrateGameProviders()
        {
            try
            {
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var sqlString = "SELECT * FROM GameProvider";
                    var context = new DbContext(CorePlatformDbConnectionString);
                    var result = context.Database.SqlQuery<GameProvider>(sqlString).ToList();
                    if (result.Count > 0)
                    {
                        foreach (var gp in result)
                        {
                            var old = db.GameProviders.FirstOrDefault(x => x.Id == gp.Id);
                            if (old == null)
                                db.GameProviders.Add(gp);
                            else
                            {
                                old.Name = gp.Name;
                                old.Type = gp.Type;
                                old.SessionExpireTime = gp.SessionExpireTime;
                                old.GameLaunchUrl = gp.GameLaunchUrl;
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
        }

        public static void MigrateCurrencies()
        {
            try
            {
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var sqlString = "SELECT * FROM Currency";
                    var context = new DbContext(CorePlatformDbConnectionString);
                    var result = context.Database.SqlQuery<Currency>(sqlString).ToList();
                    if (result.Count > 0)
                    {
                        foreach (var c in result)
                        {
                            var old = db.Currencies.FirstOrDefault(x => x.Id == c.Id);
                            if (old == null)
                                db.Currencies.Add(c);
                            else
                            {
                                old.CurrentRate = c.CurrentRate;
                                old.Symbol = c.Symbol;
                                old.LastUpdateTime = c.LastUpdateTime;
                                old.Code = c.Code;
                                old.Name = c.Name;
                                old.Type = c.Type;
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
        }

        public static void MigratePartners()
        {
            try
            {
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var sqlString = "SELECT * FROM Partner";
                    var context = new DbContext(CorePlatformDbConnectionString);
                    var result = context.Database.SqlQuery<Partner>(sqlString).ToList();
                    if (result.Count > 0)
                    {
                        foreach (var p in result)
                        {
                            var old = db.Partners.FirstOrDefault(x => x.Id == p.Id);
                            if (old == null)
                                db.Partners.Add(p);
                            else
                            {
                                old.Name = p.Name;
                                old.CurrencyId = p.CurrencyId;
                                old.SiteUrl = p.SiteUrl;
                                old.AdminSiteUrl = p.AdminSiteUrl;
                                old.State = p.State;
                                old.LastUpdateTime = p.LastUpdateTime;
                                old.ClientMinAge = p.ClientMinAge;
                                old.PasswordRegExp = p.PasswordRegExp;
                                old.VerificationType = p.VerificationType;
                                old.EmailVerificationCodeLength = p.EmailVerificationCodeLength;
                                old.MobileVerificationCodeLength = p.MobileVerificationCodeLength;
                                old.UnusedAmountWithdrawPercent = p.UnusedAmountWithdrawPercent;
                                old.UserSessionExpireTime = p.UserSessionExpireTime;
                                old.UnpaidWinValidPeriod = p.UnpaidWinValidPeriod;
                                old.VerificationKeyActiveMinutes = p.VerificationKeyActiveMinutes;
                                old.AutoApproveBetShopDepositMaxAmount = p.AutoApproveBetShopDepositMaxAmount;
                                old.AutoApproveWithdrawMaxAmount = p.AutoApproveWithdrawMaxAmount;
                                old.ClientSessionExpireTime = p.ClientSessionExpireTime;
                                old.AutoConfirmWithdrawMaxAmount = p.AutoConfirmWithdrawMaxAmount;
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
        }

        public static void MigrateAffiliatePlatform()
        {
            try
            {
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var sqlString = "SELECT * FROM AffiliatePlatform";
                    var context = new DbContext(CorePlatformDbConnectionString);
                    var result = context.Database.SqlQuery<AffiliatePlatform>(sqlString).ToList();
                    if (result.Count > 0)
                    {
                        foreach (var p in result)
                        {
                            var old = db.AffiliatePlatforms.FirstOrDefault(x => x.Id == p.Id);
                            if (old == null)
                                db.AffiliatePlatforms.Add(p);
                            else
                            {
                                old.PartnerId = p.PartnerId;
                                old.Name = p.Name;
                                old.Status = p.Status;
                                old.KickOffTime = p.KickOffTime;
                                old.LastExecutionTime = p.LastExecutionTime;
                                old.StepInHours = p.StepInHours;
                                old.PeriodInHours = p.PeriodInHours;
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
        }

        public static void MigrateClientSessions()
        {
            try
            {
                var startTime = DateTime.UtcNow.AddHours(-1);
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var sqlString = "SELECT * FROM [ClientSession] WHERE ProductId = 1 AND (StartTime > @startTime or LastUpdateTime > @startTime or EndTime > @startTime)";
                    var context = new DbContext(CorePlatformDbConnectionString);
                    var result = context.Database.SqlQuery<ClientSession>(sqlString, new SqlParameter("@startTime", startTime)).ToList();
                    if (result.Count > 0)
                    {
                        foreach (var r in result)
                        {
                            var old = db.ClientSessions.FirstOrDefault(x => x.Id == r.Id);
                            if (old == null)
                                db.ClientSessions.Add(r);
                            else
                            {
                                old.LastUpdateTime = r.LastUpdateTime;
                                old.EndTime = r.EndTime;
                                old.State = r.State;
                                old.LogoutType = r.LogoutType;
                                old.AccountId = r.AccountId;
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
        }

        public static void MigrateClientBonuses()
        {
            try
            {
                var startTime = DateTime.UtcNow.AddDays(-28);
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var sqlString = "SELECT * FROM [ClientBonus] WHERE CreationTime > @startTime";
                    var context = new DbContext(CorePlatformDbConnectionString);
                    var result = context.Database.SqlQuery<ClientBonu>(sqlString, new SqlParameter("@startTime", startTime)).ToList();
                    if (result.Count > 0)
                    {
                        foreach (var r in result)
                        {
                            var old = db.ClientBonus.FirstOrDefault(x => x.Id == r.Id);
                            if (old == null)
                                db.ClientBonus.Add(r);
                            else
                            {
                                old.Status = r.Status;
                                old.BonusPrize = r.BonusPrize;
                                old.AwardingTime = r.AwardingTime;
                                old.FinalAmount = r.FinalAmount;
                                old.CalculationTime = r.CalculationTime;
                                old.ValidUntil = r.ValidUntil;
                                old.Considered = r.Considered;
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
        }

        public static void MigrateBonuses()
        {
            try
            {
                var startTime = DateTime.UtcNow.AddDays(-28);
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var sqlString = "SELECT * FROM [Bonus] WHERE CreationTime > @startTime";
                    var context = new DbContext(CorePlatformDbConnectionString);
                    var result = context.Database.SqlQuery<Bonu>(sqlString, new SqlParameter("@startTime", startTime)).ToList();
                    if (result.Count > 0)
                    {
                        foreach (var r in result)
                        {
                            var old = db.Bonus.FirstOrDefault(x => x.Id == r.Id);
                            if (old == null)
                                db.Bonus.Add(r);
                            else
                            {
                                old.Status = r.Status;
                                old.StartTime = r.StartTime;
                                old.FinishTime = r.FinishTime;
                                old.TotalGranted = r.TotalGranted;
                                old.TotalReceiversCount = r.TotalReceiversCount;
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
        }

        public static void MigratePaymentRequests()
        {
            try
            {
                var startTime = DateTime.UtcNow.AddDays(-15);
                var startDate = (long)startTime.Year * 100000000 + (long)startTime.Month * 1000000 + (long)startTime.Day * 10000 + 
                    (long)startTime.Hour * 100 + +(long)startTime.Minute;
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var sqlString = "SELECT * FROM [PaymentRequest] WHERE Date >= @startDate";
                    var context = new DbContext(CorePlatformDbConnectionString);
                    var result = context.Database.SqlQuery<PaymentRequest>(sqlString, new SqlParameter("@startDate", startDate)).ToList();
                    if (result.Count > 0)
                    {
                        foreach (var r in result)
                        {
                            var old = db.PaymentRequests.FirstOrDefault(x => x.Id == r.Id);
                            if (old == null)
                                db.PaymentRequests.Add(r);
                            else
                            {
                                old.Status = r.Status;
                                old.Amount = r.Amount;
                                old.UserId = r.UserId;
                                old.SessionId = r.SessionId;
                                old.LastUpdateTime = r.LastUpdateTime;
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
        }

        public static void MigrateAffiliateReferral()
        {
            try
            {
                var startTime = DateTime.UtcNow.AddHours(-24);
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var sqlString = "SELECT * FROM [AffiliateReferral] WHERE CreationTime > @startTime OR LastProcessedBonusTime > @startTime";
                    var context = new DbContext(CorePlatformDbConnectionString);
                    var result = context.Database.SqlQuery<AffiliateReferral>(sqlString, new SqlParameter("@startTime", startTime)).ToList();
                    if (result.Count > 0)
                    {
                        foreach (var p in result)
                        {
                            var old = db.AffiliateReferrals.FirstOrDefault(x => x.Id == p.Id);
                            if (old == null)
                                db.AffiliateReferrals.Add(p);
                            else
                            {
                                old.Status = p.Status;
                                old.LastProcessedBonusTime = p.LastProcessedBonusTime;
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
        }

        public static void MigrateAgentCommission()
        {
            try
            {
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var sqlString = "SELECT * FROM AgentCommission";
                    var context = new DbContext(CorePlatformDbConnectionString);
                    var result = context.Database.SqlQuery<AgentCommission>(sqlString).ToList();
                    if (result.Count > 0)
                    {
                        foreach (var p in result)
                        {
                            var old = db.AgentCommissions.FirstOrDefault(x => x.Id == p.Id);
                            if (old == null)
                                db.AgentCommissions.Add(p);
                            else
                            {
                                old.Percent = p.Percent;
                                old.TurnoverPercent = p.TurnoverPercent;
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
        }

        public static int MigrateAccountBalances()
        {
            try
            {
                using (var db = new IqSoftDataWarehouseEntities())
                {
                    var lastId = db.AccountBalances.OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefault();
                    var sqlString = "SELECT TOP(5000) ab.Id, ab.AccountId, acc.ObjectId, acc.ObjectTypeId, acc.TypeId, " +
                        "acc.CurrencyId, ab.Balance, ab.Date FROM AccountBalance ab JOIN Account acc ON ab.AccountId = acc.Id WHERE ab.Id > @lastId";
                    var context = new DbContext(CorePlatformDbConnectionString);
                    var paramId = new SqlParameter("@lastId", lastId);
                    var result = context.Database.SqlQuery<AccountBalance>(sqlString, paramId).ToList();
                    if (result.Count > 0)
                    {
                        db.AccountBalances.AddRange(result);
                        db.SaveChanges();
                    }
                    return result.Count;
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                return 0;
            }
        }
    }
}
