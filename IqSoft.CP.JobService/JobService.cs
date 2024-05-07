using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using IqSoft.CP.DAL;
using IqSoft.CP.Common;
using Newtonsoft.Json;
using System.ServiceModel;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Services;
using System.Collections.Generic;
using IqSoft.CP.Integration.Payments.Helpers;
using IqSoft.CP.Common.Enums;
using System.Data.Entity;
using log4net;
using Microsoft.Owin.Hosting;
using IqSoft.CP.JobService;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Models.Bonus;

namespace IqSoft.CP.WindowsServices.JobService
{
    public partial class JobService : ServiceBase
    {
        private IDisposable _server;

        private readonly Timer _closeAccountPeriodTimer;
        private readonly Timer _closeClientPeriodTimer;
        private readonly Timer _addMoneyToPartnerAccountTimer;
        private readonly Timer _expireUserSessionsTimer;
        private readonly Timer _expireClientSessionsTimer;
        private readonly Timer _resetBetShopDailyTicketNumberTimer;
        private readonly Timer _expireClientVerificationKeysTimer;
        private readonly Timer _calculateCashBackBonusesTimer;
        private readonly Timer _awardCashBackBonusesTimer;
        private readonly Timer _giveAffiliateBonusTimer;
        private readonly Timer _giveFixedFeeCommissionTimer;
        private readonly Timer _giveAffiliateCommissionTimer;
        private readonly Timer _deletePaymentExpiredActiveRequestsTimer;
        private readonly Timer _updateCurrenciesRateTimer;
        private readonly Timer _sendActiveMailsTimer;
        private readonly Timer _updateClientWageringBonusTimer;
        private readonly Timer _finalizeWageringBonusTimer;
        private readonly Timer _sendActiveMerchantRequestsTimer;
        private readonly Timer _approveIqWalletConfirmedRequestsTimer;
        private readonly Timer _sendAffiliateReportTimer;
        private readonly Timer _calculateAgentsTurnoverProfitTimer;
        private readonly Timer _calculateAgentsGGRProfitTimer;
        private readonly Timer _triggerCRMTimer;
        private readonly Timer _checkClientBlockedSessionsTimer;
        private readonly Timer _checkUserBlockedSessionsTimer;
        private readonly Timer _checkWithdrawRequestsStatusesTimer;
        private readonly Timer _checkDepositRequestsStatusesTimer;
        private readonly Timer _giveFreeSpinTimer;
        private readonly Timer _giveJackpotWinTimer;
        private readonly Timer _deactivateExiredKYCTimer;
        private readonly Timer _triggerBonusTimer;
        private readonly Timer _inactiveClientsTimer;
        private readonly Timer _checkForceBlockedClientsTimer;
        private readonly Timer _inactiveUsersTimer;
        private readonly Timer _applyClientRestriction;
        private readonly Timer _notifyIdentityExpirationTimer;
        private readonly Timer _inactivateImpossiblBonuses;
        private readonly Timer _updateJackpotFeed;
        private readonly Timer _sendPartnerActivityReport;
        private readonly Timer _reconsiderDynamicSegments;
        private readonly Timer _fulfillDepositAction;
        private readonly Timer _fairSegmentTriggers;
        private readonly Timer _sendPartnerDailyReport;
        private readonly Timer _sendPartnerWeeklyReport;
        private readonly Timer _sendPartnerMonthlyReport;
        private readonly Timer _calculateCompPoints;
        private readonly Timer _settleBets;
        private readonly Timer _restrictUnverifiedClients;
        private readonly Timer _expireClientVerificationStatus;
        private readonly Timer _checkDuplicateClients;


        public JobService()
        {
            InitializeComponent();

            _closeAccountPeriodTimer = new Timer(CallJob, Constants.Jobs.CloseAccountPeriod, Timeout.Infinite, Timeout.Infinite);
            _closeClientPeriodTimer = new Timer(CallJob, Constants.Jobs.CloseClientPeriod, Timeout.Infinite, Timeout.Infinite);
            _addMoneyToPartnerAccountTimer = new Timer(CallJob, Constants.Jobs.AddMoneyToPartnerAccount, Timeout.Infinite, Timeout.Infinite);
            _expireUserSessionsTimer = new Timer(CallJob, Constants.Jobs.ExpireUserSessions, Timeout.Infinite, Timeout.Infinite);
            _expireClientSessionsTimer = new Timer(CallJob, Constants.Jobs.ExpireClientSessions, Timeout.Infinite, Timeout.Infinite);
            _resetBetShopDailyTicketNumberTimer = new Timer(CallJob, Constants.Jobs.ResetBetShopDailyTicketNumber, Timeout.Infinite, Timeout.Infinite);
            _expireClientVerificationKeysTimer = new Timer(CallJob, Constants.Jobs.ExpireClientVerificationKeys, Timeout.Infinite, Timeout.Infinite);
            _calculateCashBackBonusesTimer = new Timer(CallJob, Constants.Jobs.CalculateCashBackBonuses, Timeout.Infinite, Timeout.Infinite);
            _awardCashBackBonusesTimer = new Timer(CallJob, Constants.Jobs.AwardCashBackBonuses, Timeout.Infinite, Timeout.Infinite);
            _giveAffiliateBonusTimer = new Timer(CallJob, Constants.Jobs.GiveAffiliateBonus, Timeout.Infinite, Timeout.Infinite);
            _giveFixedFeeCommissionTimer = new Timer(CallJob, Constants.Jobs.GiveFixedFeeCommission, Timeout.Infinite, Timeout.Infinite);
            _giveAffiliateCommissionTimer = new Timer(CallJob, Constants.Jobs.GiveAffiliateCommission, Timeout.Infinite, Timeout.Infinite);
            _deletePaymentExpiredActiveRequestsTimer = new Timer(CallJob, Constants.Jobs.DeletePaymentExpiredActiveRequests, Timeout.Infinite, Timeout.Infinite);
            _updateCurrenciesRateTimer = new Timer(CallJob, Constants.Jobs.UpdateCurrenciesRate, Timeout.Infinite, Timeout.Infinite);
            _sendActiveMailsTimer = new Timer(CallJob, Constants.Jobs.SendActiveMails, Timeout.Infinite, Timeout.Infinite);
            _updateClientWageringBonusTimer = new Timer(CallJob, Constants.Jobs.UpdateClientWageringBonus, Timeout.Infinite, Timeout.Infinite);
            _finalizeWageringBonusTimer = new Timer(CallJob, Constants.Jobs.FinalizeWageringBonus, Timeout.Infinite, Timeout.Infinite);
            _sendActiveMerchantRequestsTimer = new Timer(CallJob, Constants.Jobs.SendActiveMerchantRequests, Timeout.Infinite, Timeout.Infinite);
            _approveIqWalletConfirmedRequestsTimer = new Timer(CallJob, Constants.Jobs.ApproveIqWalletConfirmedRequests, Timeout.Infinite, Timeout.Infinite);
            _sendAffiliateReportTimer = new Timer(CallJob, Constants.Jobs.SendAffiliateReport, Timeout.Infinite, Timeout.Infinite);
            _calculateAgentsTurnoverProfitTimer = new Timer(CallJob, Constants.Jobs.CalculateAgentsTurnoverProfit, Timeout.Infinite, Timeout.Infinite);
            _calculateAgentsGGRProfitTimer = new Timer(CallJob, Constants.Jobs.CalculateAgentsGGRProfit, Timeout.Infinite, Timeout.Infinite);
            _triggerCRMTimer = new Timer(CallJob, Constants.Jobs.TriggerCRM, Timeout.Infinite, Timeout.Infinite);
            _checkClientBlockedSessionsTimer = new Timer(CallJob, Constants.Jobs.CheckClientBlockedSessions, Timeout.Infinite, Timeout.Infinite);
            _checkUserBlockedSessionsTimer = new Timer(CallJob, Constants.Jobs.CheckUserBlockedSessions, Timeout.Infinite, Timeout.Infinite);
            _checkWithdrawRequestsStatusesTimer = new Timer(CallJob, Constants.Jobs.CheckWithdrawRequestsStatuses, Timeout.Infinite, Timeout.Infinite);
            _checkDepositRequestsStatusesTimer = new Timer(CallJob, Constants.Jobs.CheckDepositRequestsStatuses, Timeout.Infinite, Timeout.Infinite);
            _giveFreeSpinTimer = new Timer(CallJob, Constants.Jobs.GiveFreeSpin, Timeout.Infinite, Timeout.Infinite);
            _giveJackpotWinTimer = new Timer(CallJob, Constants.Jobs.GiveJackpotWin, Timeout.Infinite, Timeout.Infinite);
            _deactivateExiredKYCTimer = new Timer(CallJob, Constants.Jobs.DeactivateExiredKYC, Timeout.Infinite, Timeout.Infinite);
            _triggerBonusTimer = new Timer(CallJob, Constants.Jobs.TriggerBonus, Timeout.Infinite, Timeout.Infinite);
            _inactiveClientsTimer = new Timer(CallJob, Constants.Jobs.CheckInactiveClients, Timeout.Infinite, Timeout.Infinite);
            _checkForceBlockedClientsTimer = new Timer(CallJob, Constants.Jobs.CheckForceBlockedClients, Timeout.Infinite, Timeout.Infinite);
            _inactiveUsersTimer = new Timer(CallJob, Constants.Jobs.CheckInactiveUsers, Timeout.Infinite, Timeout.Infinite);
            _applyClientRestriction = new Timer(CallJob, Constants.Jobs.ApplyClientRestriction, Timeout.Infinite, Timeout.Infinite);
            _notifyIdentityExpirationTimer = new Timer(CallJob, Constants.Jobs.NotifyIdentityExpiration, Timeout.Infinite, Timeout.Infinite);
            _inactivateImpossiblBonuses = new Timer(CallJob, Constants.Jobs.InactivateImpossibleBonuses, Timeout.Infinite, Timeout.Infinite);
            _updateJackpotFeed = new Timer(CallJob, Constants.Jobs.UpdateJackpotFeed, Timeout.Infinite, Timeout.Infinite);
            _reconsiderDynamicSegments = new Timer(CallJob, Constants.Jobs.ReconsiderDynamicSegments, Timeout.Infinite, Timeout.Infinite);
            _fulfillDepositAction = new Timer(CallJob, Constants.Jobs.FulfillDepositAction, Timeout.Infinite, Timeout.Infinite);
            _fairSegmentTriggers = new Timer(CallJob, Constants.Jobs.FairSegmentTriggers, Timeout.Infinite, Timeout.Infinite);
            _sendPartnerDailyReport = new Timer(CallJob, Constants.Jobs.SendPartnerDailyReport, Timeout.Infinite, Timeout.Infinite);
            _sendPartnerWeeklyReport = new Timer(CallJob, Constants.Jobs.SendPartnerWeeklyReport, Timeout.Infinite, Timeout.Infinite);
            _sendPartnerMonthlyReport = new Timer(CallJob, Constants.Jobs.SendPartnerMonthlyReport, Timeout.Infinite, Timeout.Infinite);
            _calculateCompPoints = new Timer(CallJob, Constants.Jobs.CalculateCompPoints, Timeout.Infinite, Timeout.Infinite);
            _sendPartnerActivityReport = new Timer(CallJob, Constants.Jobs.SendPartnerActivityReport, Timeout.Infinite, Timeout.Infinite);
            _settleBets = new Timer(CallJob, Constants.Jobs.SettleBets, Timeout.Infinite, Timeout.Infinite);
            _restrictUnverifiedClients = new Timer(CallJob, Constants.Jobs.RestrictUnverifiedClients, Timeout.Infinite, Timeout.Infinite);
            _expireClientVerificationStatus = new Timer(CallJob, Constants.Jobs.ExpireClientVerificationStatus, Timeout.Infinite, Timeout.Infinite);
            _checkDuplicateClients = new Timer(CallJob, Constants.Jobs.CheckDuplicateClients, Timeout.Infinite, Timeout.Infinite);
        }

        protected override void OnStart(string[] args)
        {
            _closeAccountPeriodTimer.Change(20000, 20000);
            _closeClientPeriodTimer.Change(20000, 20000);
            _addMoneyToPartnerAccountTimer.Change(20000, 20000);
            _expireUserSessionsTimer.Change(5000, 5000);
            _expireClientSessionsTimer.Change(5000, 5000);
            _resetBetShopDailyTicketNumberTimer.Change(20000, 20000);
            _expireClientVerificationKeysTimer.Change(5000, 5000);
            _calculateCashBackBonusesTimer.Change(60000, 60000);
            _awardCashBackBonusesTimer.Change(60000, 60000);
            _giveAffiliateBonusTimer.Change(60000, 60000);
            _giveFixedFeeCommissionTimer.Change(60000, 60000);
            _giveAffiliateCommissionTimer.Change(60000, 60000);
            _deletePaymentExpiredActiveRequestsTimer.Change(60000, 60000);
            _updateCurrenciesRateTimer.Change(60000, 600000);
            _sendActiveMailsTimer.Change(1000, 1000);
            _updateClientWageringBonusTimer.Change(1000, 1000);
            _finalizeWageringBonusTimer.Change(1000, 1000);
            _sendActiveMerchantRequestsTimer.Change(5000, 5000);
            _approveIqWalletConfirmedRequestsTimer.Change(5000, 5000);
            _sendAffiliateReportTimer.Change(60000, 60000);
            _calculateAgentsTurnoverProfitTimer.Change(60000, 600000);
            _calculateAgentsGGRProfitTimer.Change(60000, 3600000);
            _triggerCRMTimer.Change(60000, 60000);
            _checkClientBlockedSessionsTimer.Change(60000, 60000);
            _checkUserBlockedSessionsTimer.Change(60000, 60000);
            _checkWithdrawRequestsStatusesTimer.Change(60000, 60000);
            _checkDepositRequestsStatusesTimer.Change(60000, 60000); //45
            _giveFreeSpinTimer.Change(60000, 60000);
            _giveJackpotWinTimer.Change(60000, 60000);
            _deactivateExiredKYCTimer.Change(600000, 600000);
            _triggerBonusTimer.Change(60000, 60000);
            _inactiveClientsTimer.Change(300000, 300000);
            _checkForceBlockedClientsTimer.Change(300000, 300000);
            _inactiveUsersTimer.Change(300000, 300000);
            _applyClientRestriction.Change(300000, 3600000);
            _notifyIdentityExpirationTimer.Change(300000, 300000);
            _inactivateImpossiblBonuses.Change(300000, 300000);
            _updateJackpotFeed.Change(300000, 300000);
            _reconsiderDynamicSegments.Change(20000, 20000); //35
            _fulfillDepositAction.Change(20000, 20000);
            _fairSegmentTriggers.Change(20000, 20000);
            _sendPartnerDailyReport.Change(120000, 600000);
            _sendPartnerWeeklyReport.Change(120000, 600000);
            _sendPartnerMonthlyReport.Change(120000, 600000);
            _calculateCompPoints.Change(30000, 30000);
            _sendPartnerActivityReport.Change(60000, 600000);
            _settleBets.Change(60000, 60000);
            _restrictUnverifiedClients.Change(60000, 60000);
            _expireClientVerificationStatus.Change(60000, 3600000);
            _checkDuplicateClients.Change(60000, 3600000);

            var startOptions = new StartOptions("http://*:9010/");
            _server = WebApp.Start<Startup>(startOptions);
        }

        protected override void OnStop()
        {
            _closeAccountPeriodTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _closeClientPeriodTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _addMoneyToPartnerAccountTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _expireUserSessionsTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _expireClientSessionsTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _resetBetShopDailyTicketNumberTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _expireClientVerificationKeysTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _calculateCashBackBonusesTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _awardCashBackBonusesTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _giveAffiliateBonusTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _giveFixedFeeCommissionTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _giveAffiliateCommissionTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _deletePaymentExpiredActiveRequestsTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _updateCurrenciesRateTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _sendActiveMailsTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _updateClientWageringBonusTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _finalizeWageringBonusTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _sendActiveMerchantRequestsTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _approveIqWalletConfirmedRequestsTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _sendAffiliateReportTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _calculateAgentsTurnoverProfitTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _calculateAgentsGGRProfitTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _triggerCRMTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _checkClientBlockedSessionsTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _checkUserBlockedSessionsTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _checkWithdrawRequestsStatusesTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _checkDepositRequestsStatusesTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _giveFreeSpinTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _giveJackpotWinTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _deactivateExiredKYCTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _triggerBonusTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _inactiveClientsTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _applyClientRestriction.Change(Timeout.Infinite, Timeout.Infinite);
            _checkForceBlockedClientsTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _inactiveUsersTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _notifyIdentityExpirationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _inactivateImpossiblBonuses.Change(Timeout.Infinite, Timeout.Infinite);
            _updateJackpotFeed.Change(Timeout.Infinite, Timeout.Infinite);
            _sendPartnerActivityReport.Change(Timeout.Infinite, Timeout.Infinite);
            _reconsiderDynamicSegments.Change(Timeout.Infinite, Timeout.Infinite);
            _fulfillDepositAction.Change(Timeout.Infinite, Timeout.Infinite);
            _fairSegmentTriggers.Change(Timeout.Infinite, Timeout.Infinite);
            _sendPartnerDailyReport.Change(Timeout.Infinite, Timeout.Infinite);
            _sendPartnerWeeklyReport.Change(Timeout.Infinite, Timeout.Infinite);
            _sendPartnerMonthlyReport.Change(Timeout.Infinite, Timeout.Infinite);
            _calculateCompPoints.Change(Timeout.Infinite, Timeout.Infinite);
            _settleBets.Change(Timeout.Infinite, Timeout.Infinite);
            _restrictUnverifiedClients.Change(Timeout.Infinite, Timeout.Infinite);
            _expireClientVerificationStatus.Change(Timeout.Infinite, Timeout.Infinite);
            _checkDuplicateClients.Change(Timeout.Infinite, Timeout.Infinite);

            if (_server != null)
            {
                _server.Dispose();
            }
        }

        public void CallJob(Object sender)
        {
            var jobId = Convert.ToInt32(sender);
            Timer timer = null;
            int duration = 1000;

            switch (jobId)
            {
                case Constants.Jobs.CloseAccountPeriod:
                    timer = _closeAccountPeriodTimer;
                    duration = 20000;
                    break;
                case Constants.Jobs.CloseClientPeriod:
                    timer = _closeClientPeriodTimer;
                    duration = 20000;
                    break;
                case Constants.Jobs.AddMoneyToPartnerAccount:
                    timer = _addMoneyToPartnerAccountTimer;
                    duration = 20000;
                    break;
                case Constants.Jobs.ExpireUserSessions:
                    timer = _expireUserSessionsTimer;
                    duration = 5000;
                    break;
                case Constants.Jobs.ExpireClientSessions:
                    timer = _expireClientSessionsTimer;
                    duration = 5000;
                    break;
                case Constants.Jobs.ResetBetShopDailyTicketNumber:
                    timer = _resetBetShopDailyTicketNumberTimer;
                    duration = 20000;
                    break;
                case Constants.Jobs.ExpireClientVerificationKeys:
                    timer = _expireClientVerificationKeysTimer;
                    duration = 5000;
                    break;
                case Constants.Jobs.CalculateCashBackBonuses:
                    timer = _calculateCashBackBonusesTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.AwardCashBackBonuses:
                    timer = _awardCashBackBonusesTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.GiveAffiliateBonus:
                    timer = _giveAffiliateBonusTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.GiveFixedFeeCommission:
                    timer = _giveFixedFeeCommissionTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.GiveAffiliateCommission:
                    timer = _giveAffiliateCommissionTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.DeletePaymentExpiredActiveRequests:
                    timer = _deletePaymentExpiredActiveRequestsTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.UpdateCurrenciesRate:
                    timer = _updateCurrenciesRateTimer;
                    duration = 600000;
                    break;
                case Constants.Jobs.SendActiveMails:
                    timer = _sendActiveMailsTimer;
                    duration = 1000;
                    break;
                case Constants.Jobs.UpdateClientWageringBonus:
                    timer = _updateClientWageringBonusTimer;
                    duration = 1000;
                    break;
                case Constants.Jobs.FinalizeWageringBonus:
                    timer = _finalizeWageringBonusTimer;
                    duration = 1000;
                    break;
                case Constants.Jobs.SendActiveMerchantRequests:
                    timer = _sendActiveMerchantRequestsTimer;
                    duration = 5000;
                    break;
                case Constants.Jobs.ApproveIqWalletConfirmedRequests:
                    timer = _approveIqWalletConfirmedRequestsTimer;
                    duration = 5000;
                    break;
                case Constants.Jobs.SendAffiliateReport:
                    timer = _sendAffiliateReportTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.CalculateAgentsTurnoverProfit:
                    timer = _calculateAgentsTurnoverProfitTimer;
                    duration = 600000;
                    break;
                case Constants.Jobs.CalculateAgentsGGRProfit:
                    timer = _calculateAgentsGGRProfitTimer;
                    duration = 3600000;
                    break;
                case Constants.Jobs.TriggerCRM:
                    timer = _triggerCRMTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.CheckClientBlockedSessions:
                    timer = _checkClientBlockedSessionsTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.CheckUserBlockedSessions:
                    timer = _checkUserBlockedSessionsTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.CheckWithdrawRequestsStatuses:
                    timer = _checkWithdrawRequestsStatusesTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.CheckDepositRequestsStatuses:
                    timer = _checkDepositRequestsStatusesTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.GiveFreeSpin:
                    timer = _giveFreeSpinTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.GiveJackpotWin:
                    timer = _giveJackpotWinTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.DeactivateExiredKYC:
                    timer = _deactivateExiredKYCTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.TriggerBonus:
                    timer = _triggerBonusTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.CheckInactiveClients:
                    timer = _inactiveClientsTimer;
                    duration = 300000;
                    break;
                case Constants.Jobs.CheckForceBlockedClients:
                    timer = _checkForceBlockedClientsTimer;
                    duration = 300000;
                    break;
                case Constants.Jobs.CheckInactiveUsers:
                    timer = _inactiveUsersTimer;
                    duration = 300000;
                    break;
                case Constants.Jobs.ApplyClientRestriction:
                    timer = _applyClientRestriction;
                    duration = 300000;
                    break;
                case Constants.Jobs.NotifyIdentityExpiration:
                    timer = _notifyIdentityExpirationTimer;
                    duration = 300000;
                    break;
                case Constants.Jobs.InactivateImpossibleBonuses:
                    timer = _inactivateImpossiblBonuses;
                    duration = 300000;
                    break;
                case Constants.Jobs.UpdateJackpotFeed:
                    timer = _updateJackpotFeed;
                    duration = 300000;
                    break;
                case Constants.Jobs.ReconsiderDynamicSegments:
                    timer = _reconsiderDynamicSegments;
                    duration = 20000;
                    break;
                case Constants.Jobs.FulfillDepositAction:
                    timer = _fulfillDepositAction;
                    duration = 20000;
                    break;
                case Constants.Jobs.FairSegmentTriggers:
                    timer = _fairSegmentTriggers;
                    duration = 20000;
                    break;
                case Constants.Jobs.SendPartnerDailyReport:
                    timer = _sendPartnerDailyReport;
                    duration = 600000;
                    break;
                case Constants.Jobs.SendPartnerWeeklyReport:
                    timer = _sendPartnerWeeklyReport;
                    duration = 600000;
                    break;
                case Constants.Jobs.SendPartnerMonthlyReport:
                    timer = _sendPartnerMonthlyReport;
                    duration = 600000;
                    break;
                case Constants.Jobs.CalculateCompPoints:
                    timer = _calculateCompPoints;
                    duration = 30000;
                    break;
                case Constants.Jobs.SendPartnerActivityReport:
                    timer = _sendPartnerActivityReport;
                    duration = 600000;
                    break;
                case Constants.Jobs.SettleBets:
                    timer = _settleBets;
                    duration = 60000;
                    break;
                case Constants.Jobs.RestrictUnverifiedClients:
                    timer = _restrictUnverifiedClients;
                    duration = 60000;
                    break;
                case Constants.Jobs.ExpireClientVerificationStatus:
                    timer = _expireClientVerificationStatus;
                    duration = 60000;
                    break;
                case Constants.Jobs.CheckDuplicateClients:
                    timer = _checkDuplicateClients;
                    duration = 3600000;
                    break;
            }

            timer.Change(Timeout.Infinite, Timeout.Infinite);
            var jobStartTime = DateTime.Now;
            string jobMessage = "Job completed successfully";
            bool success = true;
            string parameters;
            var job = new Job();
            try
            {
                job = JobBll.GetJobById(jobId);
                if (job == null)
                {
                    timer.Change(duration, duration);
                    return;
                }
                parameters = CallJobFunction(job, jobStartTime, out success);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                jobMessage = JsonConvert.SerializeObject(ex);
                parameters = job.Parameters;
                success = false;
                Program.DbLogger.Error("JobId_" + jobId + "_" + ex.Detail?.Id + "_" + ex.Detail?.Message);
            }
            catch (Exception ex)
            {
                jobMessage = JsonConvert.SerializeObject(ex);
                parameters = job.Parameters;
                success = false;
                Program.DbLogger.Error("JobId_" + jobId + "_" + ex.Message + "_" + ex.InnerException);
            }
            var jobResult = new JobResult
            {
                JobId = job.Id,
                Duration = (int)((DateTime.Now - jobStartTime).TotalSeconds),
                Message = jobMessage
            };

            try
            {
                JobBll.SaveJobResult(jobResult);
                if (job.NextExecutionTime <= jobStartTime && success)
                    job.NextExecutionTime = job.NextExecutionTime.AddSeconds(job.PeriodInSeconds);
                job.Parameters = parameters;
                JobBll.SaveJob(job);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
            timer.Change(duration, duration);
        }

        public string CallJobFunction(Job job, DateTime jobStartTime, out bool success)
        {
            success = true;
            switch (job.Id)
            {
                case Constants.Jobs.CloseAccountPeriod:
                    {
                        var closePeriodInput = JsonConvert.DeserializeObject<ClosePeriodInput>(job.Parameters);
                        var result = JobBll.CloseAccountPeriod(closePeriodInput);
                        if (result)
                            closePeriodInput.EndTime = closePeriodInput.EndTime.AddHours(Constants.ClosePeriodPeriodicy);
                        return JsonConvert.SerializeObject(closePeriodInput);
                    }
                case Constants.Jobs.CloseClientPeriod:
                    {
                        var closePeriodInput = JsonConvert.DeserializeObject<ClosePeriodInput>(job.Parameters);
                        var result = JobBll.CloseClientPeriod(closePeriodInput);
                        if (result)
                            closePeriodInput.EndTime = closePeriodInput.EndTime.AddHours(Constants.ClosePeriodPeriodicy);
                        return JsonConvert.SerializeObject(closePeriodInput);
                    }
                case Constants.Jobs.AddMoneyToPartnerAccount:
                    {
                        var addMoneyToPartnerAccountInput =
                            JsonConvert.DeserializeObject<AddMoneyToPartnerAccountInput>(job.Parameters);
                        var result = JobBll.AddMoneyToPartnerAccount(Program.DbLogger, addMoneyToPartnerAccountInput);
                        if (result)
                            addMoneyToPartnerAccountInput.EndTime =
                                addMoneyToPartnerAccountInput.EndTime.AddHours(
                                    Constants.AddMoneyToPartnerAccountPeriodicy);
                        return JsonConvert.SerializeObject(addMoneyToPartnerAccountInput);
                    }
                case Constants.Jobs.ExpireUserSessions:
                    JobBll.ExpireUserSessions();
                    break;
                case Constants.Jobs.ExpireClientSessions:
                    JobBll.ExpireClientSessions();
                    break;
                case Constants.Jobs.ExpireClientVerificationStatus:
                    JobBll.ExpireClientVerificationStatus();
                    break;
                case Constants.Jobs.ResetBetShopDailyTicketNumber:
                    {
                        var resetBetShopDailyTicketNumberInput =
                            JsonConvert.DeserializeObject<ResetBetShopDailyTicketNumberInput>(job.Parameters);
                        var result = JobBll.ResetBetShopDailyTicketNumber(resetBetShopDailyTicketNumberInput);
                        foreach (var dailyTicketNumberResetSetting in resetBetShopDailyTicketNumberInput.Settings)
                        {
                            if (
                                result.Results.FirstOrDefault(
                                    x => x.PartnerId == dailyTicketNumberResetSetting.PartnerId && x.ResetResult) !=
                                null)
                                dailyTicketNumberResetSetting.ResetTime =
                                    dailyTicketNumberResetSetting.ResetTime.AddHours(
                                        Constants.BetShopDailyTicketNumberResetPeriodicy);
                        }
                        return JsonConvert.SerializeObject(resetBetShopDailyTicketNumberInput);
                    }
                case Constants.Jobs.ExpireClientVerificationKeys:
                    JobBll.ExpireClientVerificationKeys();
                    break;
                case Constants.Jobs.CalculateCashBackBonuses:
                    JobBll.CalculateCashBackBonuses(Program.DbLogger);
                    break;
                case Constants.Jobs.AwardCashBackBonuses:
                    JobBll.AwardCashBackBonuses(job.NextExecutionTime, Program.DbLogger);
                    break;
                case Constants.Jobs.GiveAffiliateBonus:
                    JobBll.GiveAffiliateBonus(Program.DbLogger);
                    break;
                case Constants.Jobs.GiveAffiliateCommission:
                    JobBll.GiveAffiliateCommission(job.NextExecutionTime, Program.DbLogger);
                    break;
                case Constants.Jobs.GiveFixedFeeCommission:
                    JobBll.GiveFixedFeeCommission(Program.DbLogger);
                    break;
                case Constants.Jobs.DeletePaymentExpiredActiveRequests:
                    JobBll.DeletePaymentExpiredActiveRequests(Program.DbLogger);
                    break;
                case Constants.Jobs.UpdateCurrenciesRate:
                    if (job.NextExecutionTime <= jobStartTime)
                        JobBll.UpdateCurrentRate(Program.DbLogger);
                    break;
                case Constants.Jobs.SendActiveMails:
                    JobBll.SendActiveMails(Program.DbLogger);
                    break;
                case Constants.Jobs.UpdateClientWageringBonus:
                    JobBll.UpdateClientWageringBonus();
                    break;
                case Constants.Jobs.FinalizeWageringBonus:
                    JobBll.FinalizeWageringBonus(Program.DbLogger);
                    break;
                case Constants.Jobs.SendActiveMerchantRequests:
                    JobBll.SendActiveMerchantRequests(Program.DbLogger);
                    break;
                case Constants.Jobs.ApproveIqWalletConfirmedRequests:
                    ApproveIqWalletConfirmedRequests();
                    break;
                case Constants.Jobs.SendAffiliateReport:
                    if (job.NextExecutionTime <= jobStartTime)
                        JobBll.SendAffiliateReport(Program.DbLogger);
                    break;
                case Constants.Jobs.CalculateAgentsTurnoverProfit:
                    if (job.NextExecutionTime <= jobStartTime)
                        success = JobBll.CalculateAgentsTurnoverProfit(job.NextExecutionTime, Program.DbLogger);
                    break;
                case Constants.Jobs.CalculateAgentsGGRProfit:
                    if (job.NextExecutionTime <= jobStartTime)
                        success = JobBll.CalculateAgentsGGRProfit(job.NextExecutionTime, Program.DbLogger);
                    break;
                case Constants.Jobs.TriggerCRM:
                    JobBll.TriggerMissedDepositCRM(job, Program.DbLogger);
                    break;
                case Constants.Jobs.CheckClientBlockedSessions:
                    JobBll.CheckClientBlockedSessions(Program.DbLogger);
                    break;
                case Constants.Jobs.CheckUserBlockedSessions:
                    JobBll.CheckUserBlockedSessions();
                    break;
                case Constants.Jobs.CheckWithdrawRequestsStatuses:
                    CheckWithdrawRequestsStatuses(Program.DbLogger);
                    break;
                case Constants.Jobs.CheckDepositRequestsStatuses:
                    CheckDepositRequestsStatuses(Program.DbLogger);
                    break;
                case Constants.Jobs.GiveFreeSpin:
                    GiveFreeSpin(Program.DbLogger);
                    break;
                case Constants.Jobs.GiveJackpotWin:
                    JobBll.GiveJackpotWin(Program.DbLogger);
                    break;
                case Constants.Jobs.DeactivateExiredKYC:
                    JobBll.DeactivateExiredKYC(Program.DbLogger);
                    break;
                case Constants.Jobs.TriggerBonus:
                    JobBll.AwardClientCampaignBonus(Program.DbLogger);
                    break;
                case Constants.Jobs.CheckForceBlockedClients:
                    JobBll.CheckForceBlockedClients();
                    break;
                case Constants.Jobs.CheckInactiveClients:
                    JobBll.CheckInactiveClients(Program.DbLogger);
                    break;
                case Constants.Jobs.CheckInactiveUsers:
                    JobBll.CheckInactiveUsers();
                    break;
                case Constants.Jobs.ApplyClientRestriction:
                    JobBll.ApplyClientRestriction(Program.DbLogger);
                    break;
                case Constants.Jobs.NotifyIdentityExpiration:
                    JobBll.NotifyIdentityExpiration(Program.DbLogger);
                    break;
                case Constants.Jobs.InactivateImpossibleBonuses:
                    JobBll.InactivateImpossiblBonuses();
                    break;
                case Constants.Jobs.UpdateJackpotFeed:
                    JobBll.UpdateJackpotFeed(Program.DbLogger);
                    break;
                case Constants.Jobs.ReconsiderDynamicSegments:
                    JobBll.ReconsiderDynamicSegments(Program.DbLogger);
                    break;
                case Constants.Jobs.FulfillDepositAction:
                    JobBll.FulfillDepositAction(Program.DbLogger);
                    break;
                case Constants.Jobs.FairSegmentTriggers:
                    JobBll.FairSegmentTriggers(Program.DbLogger);
                    break;
                case Constants.Jobs.SendPartnerDailyReport:
                    JobBll.SendPartnerDailyReport(Program.DbLogger);
                    break;
                case Constants.Jobs.SendPartnerWeeklyReport:
                    JobBll.SendPartnerWeeklyReport(Program.DbLogger);
                    break;
                case Constants.Jobs.SendPartnerMonthlyReport:
                    JobBll.SendPartnerMonthlyReport(Program.DbLogger);
                    break;
                case Constants.Jobs.CalculateCompPoints:
                    JobBll.CalculateCompPoints(Program.DbLogger);
                    break;
                case Constants.Jobs.SettleBets:
                    JobBll.SettleBets(Program.DbLogger);
                    break;
                case Constants.Jobs.RestrictUnverifiedClients:
                    JobBll.RestrictUnverifiedClients(Program.DbLogger);
                    break;
                case Constants.Jobs.CheckDuplicateClients:
                    JobBll.CheckDuplicateClients(Program.DbLogger);
                    break;
            }
            return null;
        }

        public void ApproveIqWalletConfirmedRequests()
        {
            if (JobBll.IqWalletId > 0)
            {
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var paymentSystemBl = new PaymentSystemBll(clientBl))
                    {
                        using (var documentBl = new DocumentBll(clientBl))
                        {
                            using (var notificationBl = new NotificationBll(documentBl))
                            {
                                var confirmedRequests = paymentSystemBl.GetPaymentRequests(JobBll.IqWalletId, (int)PaymentRequestStates.Confirmed);
                                foreach (var r in confirmedRequests)
                                {
                                    try
                                    {
                                        var response = PaymentHelpers.SendPaymentWithdrawalsRequest(r, clientBl.Identity, Program.DbLogger);

                                        if (response.Status == PaymentRequestStates.PayPanding)
                                        {
                                            clientBl.ChangeWithdrawRequestState(r.Id, PaymentRequestStates.PayPanding,
                                                "AutoApprove", null, null, true, r.Parameters, documentBl, notificationBl);
                                        }
                                    }
                                    catch (FaultException<BllFnErrorType> ex)
                                    {
                                        Program.DbLogger.Error(new { ResponseCode = ex.Detail == null ? Constants.Errors.GeneralException : ex.Detail.Id, Description = ex.Detail == null ? ex.Message : ex.Detail.Message });
                                    }
                                    catch (Exception e)
                                    {
                                        Program.DbLogger.Error(e);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void CheckWithdrawRequestsStatuses(ILog log)
        {
            var session = new SessionIdentity { LanguageId = Constants.DefaultLanguageId };

            List<PaymentRequest> payPandingPaymentRequests;
            Dictionary<int, List<PaymentRequest>> confirmedPaymentRequests;

            var datePoint = DateTime.UtcNow.AddDays(-30);
            var date = (long)datePoint.Year * 100000000 + (long)datePoint.Month * 1000000 + (long)datePoint.Day * 10000 + (long)datePoint.Hour*100 + (long)datePoint.Minute;
            using (var db = new IqSoftCorePlatformEntities())
            {
                payPandingPaymentRequests = db.PaymentRequests.Include(x => x.PaymentSystem)
                                                              .Where(x => x.Type == (int)PaymentRequestTypes.Withdraw && x.Status == (int)PaymentRequestStates.PayPanding &&  
                                                                          x.Date >= date).AsNoTracking().ToList();

                confirmedPaymentRequests = db.PaymentRequests.
                    Where(x => x.Type == (int)PaymentRequestTypes.Withdraw && x.Status == (int)PaymentRequestStates.Confirmed && 
                    x.Date >= date && x.BetShopId == null).GroupBy(x => x.Client.PartnerId).Select(x => new
                    {
                        PartnerId = x.Key,
                        Requests = x.ToList()
                    }).ToDictionary(x => x.PartnerId, x => x.Requests);
            }

            foreach (var paymentRequest in payPandingPaymentRequests)
            {
                try
                {
                    switch (paymentRequest.PaymentSystem.Name)
                    {
                        case Constants.PaymentSystems.WalletOne:
                        case Constants.PaymentSystems.CreditCards:
                            WalletOneHelpers.GetPayoutRequestStatus(paymentRequest, session, log);
                            break;
                        case Constants.PaymentSystems.PayBoxATM:
                            PayBoxHelpers.GetPayoutRequestStatus(paymentRequest, session, log);
                            break;
                        case Constants.PaymentSystems.PiastrixVisaMaster:
                        case Constants.PaymentSystems.PiastrixAlfaclick:
                        case Constants.PaymentSystems.PiastrixQiwi:
                        case Constants.PaymentSystems.PiastrixYandex:
                        case Constants.PaymentSystems.PiastrixPayeer:
                        case Constants.PaymentSystems.PiastrixBeeline:
                        case Constants.PaymentSystems.PiastrixMTS:
                        case Constants.PaymentSystems.PiastrixMegafon:
                        case Constants.PaymentSystems.PiastrixTele2:
                        case Constants.PaymentSystems.PiastrixWallet:
                            PiastrixHelpers.GetPayoutRequestStatus(paymentRequest, session, log);
                            break;
                        case Constants.PaymentSystems.CardToCard:
                            CardToCardHelpers.GetPayoutRequestStatus(paymentRequest, session, log);
                            break;
                        case Constants.PaymentSystems.FreeKassaWallet:
                        case Constants.PaymentSystems.FreeKassaPayeer:
                        case Constants.PaymentSystems.FreeKassaAdnvCash:
                        case Constants.PaymentSystems.FreeKassaExmo:
                        case Constants.PaymentSystems.FreeKassaQIWI:
                        case Constants.PaymentSystems.FreeKassaMTS:
                        case Constants.PaymentSystems.FreeKassaBeeline:
                        case Constants.PaymentSystems.FreeKassaTele2:
                        case Constants.PaymentSystems.FreeKassaAlfaBank:
                        case Constants.PaymentSystems.FreeKassaSberBank:
                        case Constants.PaymentSystems.FreeKassaCard:
                            FreeKassaHelpers.GetPayoutRequestStatus(paymentRequest, session, log);
                            break;
                        case Constants.PaymentSystems.Freelanceme:
                            FreeKassaHelpers.GetPayoutRequestStatus(paymentRequest, session, log);
                            break;
                        case Constants.PaymentSystems.Neteller:
                            NetellerHelpers.GetPayoutRequestStatus(paymentRequest, session, log);
                            break;
                        case Constants.PaymentSystems.Pay4Fun:
                            Pay4FunHelpers.GetPayoutRequestStatus(paymentRequest, session, log);
                            break;
                        case Constants.PaymentSystems.Paylado:
                            PayladoHelpers.GetTransactionDetails(paymentRequest, session, log);
                            break;
						case Constants.PaymentSystems.InterkassaVisa:
						case Constants.PaymentSystems.InterkassaMC:
						case Constants.PaymentSystems.InterkassaDeVisa:
						case Constants.PaymentSystems.InterkassaDeMC:
							InterkassaHelpers.GetTransactionDetails(paymentRequest, session, log);
							break;
                        case Constants.PaymentSystems.BankTransferSwift:
                        case Constants.PaymentSystems.BankTransferSepa:
                        case Constants.PaymentSystems.BankWire:
                        case Constants.PaymentSystems.CepBank:
                        case Constants.PaymentSystems.ShebaTransfer:
                        case Constants.PaymentSystems.CryptoTransfer:
                            BankTransferHelpers.ApprovePayoutRequest(paymentRequest, session, log);
                            break;
                        case Constants.PaymentSystems.Praxis:
                        case Constants.PaymentSystems.PraxisFiat:
                            PraxisHelpers.GetPayoutRequestStatus(paymentRequest, session, log);
                            break;
                        case Constants.PaymentSystems.OktoPay:
                            OktoPayHelpers.CancelPaymentRequest(paymentRequest, session, log);
                            break;
                        case Constants.PaymentSystems.Chapa:
                            ChapaHelpers.CheckTransactionStatus(paymentRequest, session, log);
                            break;
                        case Constants.PaymentSystems.IqWallet:
                            IqWalletHelpers.ApprovePayoutRequest(paymentRequest, session, log, out int toClientId);
                            JobBll.BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, toClientId));
                            break;
                        default:
                            break;
                    }
                    JobBll.BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, paymentRequest.ClientId));
                    // should be added broadcast to WebSiteWebApi
                }
                catch (FaultException<BllFnErrorType> ex)
                {
                    log.Error(ex);
                }
                catch (Exception e)
                {
                    log.Error(e);
                }
            }

            using (var clientBl = new ClientBll(session, log))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    using (var notificationBl = new NotificationBll(clientBl))
                    {
                        foreach (var p in confirmedPaymentRequests)
                        {
                            try
                            {
                                var partner = CacheManager.GetPartnerById(p.Key);
                                if (partner.AutoApproveWithdrawMaxAmount > 0)
                                {
                                    foreach (var pr in p.Value)
                                    {
                                        try
                                        {
                                            var partnerAutoApproveWithdrawMaxAmount = BaseBll.ConvertCurrency(partner.CurrencyId, pr.CurrencyId, partner.AutoApproveWithdrawMaxAmount);
                                            if (partnerAutoApproveWithdrawMaxAmount > pr.Amount)
											{
												var response = PaymentHelpers.SendPaymentWithdrawalsRequest(pr, session, log);
                                                var changeFromPaymentSystem = response.Status == PaymentRequestStates.Approved;

												if (response.Status == PaymentRequestStates.Approved || 
                                                    response.Status == PaymentRequestStates.ApprovedManually || 
                                                    response.Status == PaymentRequestStates.PayPanding)
                                                {
                                                    var resp = clientBl.ChangeWithdrawRequestState(pr.Id, response.Status,
                                                        String.Empty, null, null, false, pr.Parameters, documentBl, notificationBl, false, changeFromPaymentSystem);
                                                    if (response.Status != PaymentRequestStates.PayPanding)
                                                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                                                    JobBll.BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, pr.ClientId));
                                                }
                                            }
                                        }
                                        catch (FaultException<BllFnErrorType> ex)
                                        {
                                            log.Error(ex.Detail);
                                        }
                                        catch (Exception e)
                                        {
                                            log.Error(e);
                                        }
                                    }
                                }
                            }
                            catch (FaultException<BllFnErrorType> ex)
                            {
                                log.Error(ex);
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
        public void CheckDepositRequestsStatuses(ILog log)
        {
            var session = new SessionIdentity();
            session.LanguageId = Constants.DefaultLanguageId;
            List<PaymentRequest> paymentRequests;
            var datePoint = DateTime.UtcNow.AddDays(-30);
            var date = (long)datePoint.Year * 100000000 + (long)datePoint.Month * 1000000 + (long)datePoint.Day * 10000 + (long)datePoint.Hour*100 + (long)datePoint.Minute;
            using (var db = new IqSoftCorePlatformEntities())
            {
                paymentRequests = db.PaymentRequests.Include(x => x.PaymentSystem)
                                                    .Where(x => x.Type == (int)PaymentRequestTypes.Deposit && 
                                                               (x.Status == (int)PaymentRequestStates.PayPanding || x.Status == (int)PaymentRequestStates.Pending) &&
                                                                x.Date >= date).AsNoTracking().ToList();
            }
            foreach (var paymentRequest in paymentRequests)
            {
                try
                {
                    switch (paymentRequest.PaymentSystem.Name)
                    {
                        case Constants.PaymentSystems.MoneyPayVisaMaster:
                        case Constants.PaymentSystems.MoneyPayAmericanExpress:
                            MoneyPayHelpers.GetPayinRequestStatus(paymentRequest, session, log);
                            break;
                        case Constants.PaymentSystems.DPOPay:
                            DPOPayHelpers.GetPaymentRequestStatus(paymentRequest, session, log);
                            break;
                        case Constants.PaymentSystems.BankTransferSwift:
                        case Constants.PaymentSystems.BankTransferSepa:
                        case Constants.PaymentSystems.BankWire:
                        case Constants.PaymentSystems.CepBank:
                        case Constants.PaymentSystems.ShebaTransfer:
                            BankTransferHelpers.PayPaymentRequest(paymentRequest, session, log);
                            break;
						case Constants.PaymentSystems.OktoPay:
							OktoPayHelpers.CancelPaymentRequest(paymentRequest, session, log);
							break;
                        case Constants.PaymentSystems.Chapa:
                            ChapaHelpers.CheckTransactionStatus(paymentRequest, session, log);
                            break;
                        case Constants.PaymentSystems.InternationalPSP:
                            InternationalPSPHelpers.CheckPaymentRequestStatus(paymentRequest, session, log);
                            break;
                        default:
                            break;
                    }
                    JobBll.BroadcastRemoveCache(string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, paymentRequest.ClientId));
                }
                catch (Exception e)
                {
                    log.Error(e);
                }
            }
        }

        public void GiveFreeSpin(ILog log) 
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                var activeFreeSpinBonuses = db.ClientBonus.Include(x => x.Bonu).Where(x => 
                    (x.Bonu.Type == (int)BonusTypes.CampaignFreeSpin || 
                    x.Bonu.Type == (int)BonusTypes.CampaignWagerCasino) &&
                    x.Status == (int)ClientBonusStatuses.Active && x.Considered != true).GroupBy(x => x.BonusId).ToList();

                var currentDate = DateTime.UtcNow;
                foreach (var bonus in activeFreeSpinBonuses)
                {
                    var bonusProducts = db.BonusProducts.Where(x => x.Product.GameProvider != null &&
                                                                    x.Product.FreeSpinSupport.HasValue && x.BonusId == bonus.Key &&
                                                                    x.Product.FreeSpinSupport.Value && x.Count > 0)
                                                        .GroupBy(x => x.Product.GameProvider.Name)
                                                        .Select(x => new
                                                        {
                                                            GameProviderName = x.Key,
                                                            Products = x.Select(y => new
                                                            {
                                                                y.Product.ExternalId,
                                                                SpinCount = y.Count.Value,
                                                                y.Lines,
                                                                y.Coins,
                                                                y.CoinValue,
                                                                y.BetValueLevel
                                                            }).ToList()
                                                        }).ToList();

                    if (!bonusProducts.Any())
                    {
                        bonus.All(x => 
                            { 
                                x.Status = (x.Bonu.Type == (int)BonusTypes.CampaignFreeSpin ? (int)ClientBonusStatuses.Finished : x.Status);
                                x.Considered = true;
                                return true; 
                            });
                        db.SaveChanges();
                        continue;
                    }
                    foreach (var clientBonus in bonus)
                    {
                        if (clientBonus.Bonu.Type == (int)BonusTypes.CampaignFreeSpin)
                        {
                            if ((clientBonus.Bonu.MaxGranted.HasValue && clientBonus.Bonu.TotalGranted > clientBonus.Bonu.MaxGranted) ||
                               (clientBonus.Bonu.MaxReceiversCount.HasValue && clientBonus.Bonu.TotalReceiversCount > clientBonus.Bonu.MaxReceiversCount))
                            {
                                bonus.All(x => { x.Status = (int)ClientBonusStatuses.Expired; return true; });
                                clientBonus.Bonu.Status = (int)BonusStatuses.Inactive;
                                db.SaveChanges();
                                continue;
                            }
                        }
                        bonusProducts.ForEach(x =>
                        {
                            var freespinModel = new FreeSpinModel
                            {
                                ClientId = clientBonus.ClientId,
                                BonusId = bonus.Key,
                                StartTime = currentDate,
                                FinishTime = currentDate.AddHours(clientBonus.Bonu.ValidForSpending.Value),
                            };
                            try
                            {
                                switch (x.GameProviderName)
                                {
                                    case Constants.GameProviders.IqSoft:
                                        x.Products.ForEach(y =>
                                        {
                                            freespinModel.ProductExternalId = y.ExternalId;
                                            freespinModel.SpinCount = Convert.ToInt32(y.SpinCount);
                                            freespinModel.Lines = y.Lines;
                                            freespinModel.Coins = y.Coins;
                                            freespinModel.CoinValue = y.CoinValue;
                                            freespinModel.BetValueLevel = y.BetValueLevel;
                                            Integration.Products.Helpers.IqSoftHelpers.AddFreeRound(freespinModel);
                                        });
                                        break;
                                    case Constants.GameProviders.TwoWinPower:
                                        Integration.Products.Helpers.TwoWinPowerHelpers.SetFreespin(clientBonus.ClientId, clientBonus.Id, bonus.Key, Program.DbLogger);
                                        break;
                                    case Constants.GameProviders.OutcomeBet:
                                    case Constants.GameProviders.Mascot:
                                        break;
                                    case Constants.GameProviders.BlueOcean:
                                        var productsBySpin = x.Products.GroupBy(s => s.SpinCount)
                                            .Select(s => new { SpinCount = s.Key, ExternalIds = s.Select(e => e.ExternalId).ToList() });
                                        foreach (var p in productsBySpin)
                                        {
                                            Integration.Products.Helpers.BlueOceanHelpers.AddFreeRound(clientBonus.ClientId, p.ExternalIds, Convert.ToInt32(p.SpinCount),
                                                currentDate, currentDate.AddHours(clientBonus.Bonu.ValidForSpending.Value));
                                        }
                                        break;
                                    case Constants.GameProviders.SoftGaming:
                                        x.Products.ForEach(y =>
                                        {
                                            freespinModel.ProductExternalId = y.ExternalId;
                                            freespinModel.SpinCount = Convert.ToInt32(y.SpinCount);
                                            freespinModel.BetValueLevel = y.BetValueLevel;
                                            Integration.Products.Helpers.SoftGamingHelpers.AddFreeRound(freespinModel, Program.DbLogger);
                                        });
                                        break;
                                    case Constants.GameProviders.PragmaticPlay:
                                        var pragmaticPlayBySpin = x.Products.GroupBy(s => s.SpinCount)
                                        .Select(s => new { SpinCount = s.Key, ExternalIds = s.Select(e => e.ExternalId).ToList() });
                                        foreach (var p in pragmaticPlayBySpin)
                                        {
                                            Integration.Products.Helpers.PragmaticPlayHelpers.AddFreeRound(clientBonus.ClientId, p.ExternalIds, Convert.ToInt32(p.SpinCount),
                                              clientBonus.Id, currentDate, currentDate.AddHours(clientBonus.Bonu.ValidForSpending.Value));
                                        }
                                        break;
                                    case Constants.GameProviders.Habanero:
                                        var habaneroBySpin = x.Products.GroupBy(s => s.SpinCount)
                                       .Select(s => new { SpinCount = s.Key, ExternalIds = s.Select(e => e.ExternalId).ToList() });
                                        foreach (var p in habaneroBySpin)
                                        {
                                            Integration.Products.Helpers.HabaneroHelpers.AddFreeRound(clientBonus.ClientId, p.ExternalIds, Convert.ToInt32(p.SpinCount),
                                            currentDate, currentDate.AddHours(clientBonus.Bonu.ValidForSpending.Value));
                                        }
                                        break;
                                    case Constants.GameProviders.BetSoft:
                                        var betSoftBySpin = x.Products.GroupBy(s => s.SpinCount)
                                           .Select(s => new { SpinCount = s.Key, ExternalIds = s.Select(e => e.ExternalId).ToList() });
                                        foreach (var p in betSoftBySpin)
                                        {
                                            Integration.Products.Helpers.BetSoftHelpers.AddFreeRound(clientBonus.ClientId, p.ExternalIds, Convert.ToInt32(p.SpinCount), bonus.Key,
                                            currentDate, currentDate.AddHours(clientBonus.Bonu.ValidForSpending.Value));
                                        }
                                        break;
                                    case Constants.GameProviders.SoftSwiss:
                                        var softSwissBySpin = x.Products.GroupBy(s => s.SpinCount)
                                           .Select(s => new { SpinCount = s.Key, ExternalIds = s.Select(e => e.ExternalId).ToList() });
                                        foreach (var p in softSwissBySpin)
                                        {
                                            Integration.Products.Helpers.SoftSwissHelpers.AddFreeRound(clientBonus.ClientId, clientBonus.Id, p.ExternalIds, Convert.ToInt32(p.SpinCount),
                                                                                                       currentDate.AddHours(clientBonus.Bonu.ValidForSpending.Value), log);
                                        }
                                        break;
                                    case Constants.GameProviders.EveryMatrix:
                                        x.Products.ForEach(y =>
                                        {
                                            freespinModel.ProductExternalId = y.ExternalId;
                                            freespinModel.SpinCount = Convert.ToInt32(y.SpinCount);
                                            freespinModel.Lines = y.Lines;
                                            freespinModel.Coins = y.Coins;
                                            freespinModel.CoinValue = y.CoinValue;
                                            freespinModel.BetValueLevel = y.BetValueLevel;
                                            freespinModel.BonusId = clientBonus.Id;
                                            Integration.Products.Helpers.EveryMatrixHelpers.AwardFreeSpin(freespinModel, log);
                                        });
                                        break;
                                    case Constants.GameProviders.PlaynGo:
                                        x.Products.ForEach(y =>
                                        {
                                            freespinModel.ProductExternalIds = new List<string> { y.ExternalId };
                                            freespinModel.SpinCount = Convert.ToInt32(y.SpinCount);
                                            freespinModel.Lines = y.Lines;
                                            freespinModel.Coins = y.Coins;
                                            freespinModel.CoinValue = y.CoinValue;
                                            freespinModel.BetValueLevel = y.BetValueLevel;
                                            freespinModel.BonusId = clientBonus.Id;
                                            Integration.Products.Helpers.PlaynGoHelpers.AddFreeRound(freespinModel, log);
                                        });
                                        break;
                                    case Constants.GameProviders.AleaPlay:
                                        x.Products.ForEach(y =>
                                        {
                                            freespinModel.ProductExternalId = y.ExternalId;
                                            freespinModel.SpinCount = Convert.ToInt32(y.SpinCount);
                                            freespinModel.BetValueLevel = y.BetValueLevel;
                                            freespinModel.BonusId = clientBonus.Id;
                                            Integration.Products.Helpers.AleaPlayHelpers.AddFreeRound(freespinModel, log);
                                        });
                                        break;
                                    case Constants.GameProviders.TimelessTech:
                                        x.Products.ForEach(y =>
                                        {
                                            freespinModel.ProductExternalId = y.ExternalId;
                                            freespinModel.SpinCount = Convert.ToInt32(y.SpinCount);
                                            freespinModel.BonusId = clientBonus.Id;
											freespinModel.BetValueLevel = y.BetValueLevel;
											Integration.Products.Helpers.TimelessTechHelpers.CreateCampaign(freespinModel, Constants.GameProviders.TimelessTech, log);
                                        });
                                        break;
                                    case Constants.GameProviders.BCWGames:
                                        x.Products.ForEach(y =>
                                        {
                                            freespinModel.ProductExternalId = y.ExternalId;
                                            freespinModel.SpinCount = Convert.ToInt32(y.SpinCount);
                                            freespinModel.BonusId = clientBonus.Id;
											freespinModel.BetValueLevel = y.BetValueLevel;
											Integration.Products.Helpers.TimelessTechHelpers.CreateCampaign(freespinModel, Constants.GameProviders.BCWGames, log);
                                        });
                                        break;
                                    case Constants.GameProviders.Endorphina:
                                        x.Products.ForEach(y =>
                                        {
                                            freespinModel.ProductExternalId = y.ExternalId;
                                            freespinModel.SpinCount = Convert.ToInt32(y.SpinCount);
                                            freespinModel.BetValueLevel = y.BetValueLevel;
                                            freespinModel.CoinValue = y.CoinValue;
                                            freespinModel.BonusId = clientBonus.Id;
                                            Integration.Products.Helpers.EndorphinaHelpers.AddFreeRound(freespinModel, log);
                                        });
                                        break;

                                    default:
                                        break;
                                }

                                var spc = x.Products.Sum(b => b.SpinCount);

                                if (clientBonus.Bonu.Type == (int)BonusTypes.CampaignFreeSpin)
                                {
                                    clientBonus.Status = (int)ClientBonusStatuses.Finished;
                                    clientBonus.Bonu.TotalGranted += spc;
                                    ++clientBonus.Bonu.TotalReceiversCount;
                                    clientBonus.BonusPrize = spc;
                                }
                                clientBonus.Considered = true;
                                db.SaveChanges();
                            }
                            catch (Exception e)
                            {
                                Program.DbLogger.Error(e);
                            }
                        });
                    }
                }
            }
        }
    }
}