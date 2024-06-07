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
using System.Threading.Tasks;

namespace IqSoft.CP.WindowsServices.JobService
{
    public partial class JobService : ServiceBase
    {
        private IDisposable _server;

        private readonly Timer _closeAccountPeriodTimer; //1
        private readonly Timer _addMoneyToPartnerAccountTimer; //2
        private readonly Timer _calculateCompPoints; //3
        private readonly Timer _calculateJackpots; //4
        private readonly Timer _expireUserSessionsTimer; //5
        private readonly Timer _broadcastBets; //6
        private readonly Timer _resetBetShopDailyTicketNumberTimer; //7
        private readonly Timer _expireClientSessionsTimer; //9 
        private readonly Timer _expireClientVerificationKeysTimer; //10

        private readonly Timer _calculateCashBackBonusesTimer; //11
        private readonly Timer _closeClientPeriodTimer; //12 
        private readonly Timer _giveAffiliateBonusTimer; //13
        private readonly Timer _deletePaymentExpiredActiveRequestsTimer; //14
        private readonly Timer _updateCurrenciesRateTimer; //15
        private readonly Timer _sendActiveEmailsTimer; //16
        private readonly Timer _updateClientWageringBonusTimer; //17
        private readonly Timer _finalizeWageringBonusTimer; //18
        private readonly Timer _sendActiveMerchantRequestsTimer; //19
        private readonly Timer _approveIqWalletConfirmedRequestsTimer; //20

        private readonly Timer _sendAffiliateReportTimer; //21
        private readonly Timer _calculateAgentsGGRProfitTimer; //22
        private readonly Timer _calculateAgentsTurnoverProfitTimer; //23
        private readonly Timer _triggerCRMTimer; //24
        private readonly Timer _checkClientBlockedSessionsTimer; //25
        private readonly Timer _checkWithdrawRequestsStatusesTimer; //26
        private readonly Timer _deactivateExiredKYCTimer; //27
        private readonly Timer _triggerBonusTimer; //28
        private readonly Timer _checkUserBlockedSessionsTimer; //29
        private readonly Timer _inactiveClientsTimer; //30

        private readonly Timer _notifyIdentityExpirationTimer; //31
        private readonly Timer _inactivateImpossiblBonuses; //32
        private readonly Timer _updateJackpotFeed; //34
        private readonly Timer _reconsiderDynamicSegments; //35
        private readonly Timer _inactiveUsersTimer; //36
        private readonly Timer _sendPartnerDailyReport; //37
        private readonly Timer _sendPartnerWeeklyReport; //38
        private readonly Timer _sendPartnerMonthlyReport; //39
        private readonly Timer _awardCashBackBonusesTimer; //40

        private readonly Timer _fairSegmentTriggers; //41
        private readonly Timer _giveFreeSpinTimer; //42
        private readonly Timer _giveJackpotWinTimer; //43
        private readonly Timer _giveCompPoints; //44
        private readonly Timer _checkDepositRequestsStatusesTimer; //45
        private readonly Timer _checkForceBlockedClientsTimer; //47
        private readonly Timer _giveFixedFeeCommissionTimer; //48
        private readonly Timer _fulfillDepositAction; //49
        private readonly Timer _applyClientRestriction; //50

        private readonly Timer _settleBets; //51
        private readonly Timer _restrictUnverifiedClients; //52
        private readonly Timer _giveAffiliateCommissionTimer; //53
        private readonly Timer _expireClientVerificationStatus; //54
        private readonly Timer _checkDuplicateClients; //55
        private readonly Timer _closeTournamentsTimer; //56

        public JobService()
        {
            InitializeComponent();

            _closeAccountPeriodTimer = new Timer(CallJob, Constants.Jobs.CloseAccountPeriod, Timeout.Infinite, Timeout.Infinite);//1
            _addMoneyToPartnerAccountTimer = new Timer(CallJob, Constants.Jobs.AddMoneyToPartnerAccount, Timeout.Infinite, Timeout.Infinite);//2
            _calculateCompPoints = new Timer(CallJob, Constants.Jobs.CalculateCompPoints, Timeout.Infinite, Timeout.Infinite);//3
            _calculateJackpots = new Timer(CallJob, Constants.Jobs.CalculateJackpots, Timeout.Infinite, Timeout.Infinite);//4
            _expireUserSessionsTimer = new Timer(CallJob, Constants.Jobs.ExpireUserSessions, Timeout.Infinite, Timeout.Infinite);//5
            _broadcastBets = new Timer(CallJob, Constants.Jobs.BroadcastBets, Timeout.Infinite, Timeout.Infinite);//6
            _resetBetShopDailyTicketNumberTimer = new Timer(CallJob, Constants.Jobs.ResetBetShopDailyTicketNumber, Timeout.Infinite, Timeout.Infinite);//7
            _expireClientSessionsTimer = new Timer(CallJob, Constants.Jobs.ExpireClientSessions, Timeout.Infinite, Timeout.Infinite);//9
            _expireClientVerificationKeysTimer = new Timer(CallJob, Constants.Jobs.ExpireClientVerificationKeys, Timeout.Infinite, Timeout.Infinite);//10

            _calculateCashBackBonusesTimer = new Timer(CallJob, Constants.Jobs.CalculateCashBackBonuses, Timeout.Infinite, Timeout.Infinite);//11
            _closeClientPeriodTimer = new Timer(CallJob, Constants.Jobs.CloseClientPeriod, Timeout.Infinite, Timeout.Infinite);//12
            _giveAffiliateBonusTimer = new Timer(CallJob, Constants.Jobs.GiveAffiliateBonus, Timeout.Infinite, Timeout.Infinite);//13
            _deletePaymentExpiredActiveRequestsTimer = new Timer(CallJob, Constants.Jobs.DeletePaymentExpiredActiveRequests, Timeout.Infinite, Timeout.Infinite);//14
            _updateCurrenciesRateTimer = new Timer(CallJob, Constants.Jobs.UpdateCurrenciesRate, Timeout.Infinite, Timeout.Infinite);//15
            _sendActiveEmailsTimer = new Timer(CallJob, Constants.Jobs.SendActiveMails, Timeout.Infinite, Timeout.Infinite);//16
            _updateClientWageringBonusTimer = new Timer(CallJob, Constants.Jobs.UpdateClientWageringBonus, Timeout.Infinite, Timeout.Infinite);//17
            _finalizeWageringBonusTimer = new Timer(CallJob, Constants.Jobs.FinalizeWageringBonus, Timeout.Infinite, Timeout.Infinite);//18
            _sendActiveMerchantRequestsTimer = new Timer(CallJob, Constants.Jobs.SendActiveMerchantRequests, Timeout.Infinite, Timeout.Infinite);//19
            _approveIqWalletConfirmedRequestsTimer = new Timer(CallJob, Constants.Jobs.ApproveIqWalletConfirmedRequests, Timeout.Infinite, Timeout.Infinite);//20

            _sendAffiliateReportTimer = new Timer(CallJob, Constants.Jobs.SendAffiliateReport, Timeout.Infinite, Timeout.Infinite);//21
            _calculateAgentsGGRProfitTimer = new Timer(CallJob, Constants.Jobs.CalculateAgentsGGRProfit, Timeout.Infinite, Timeout.Infinite);//22
            _calculateAgentsTurnoverProfitTimer = new Timer(CallJob, Constants.Jobs.CalculateAgentsTurnoverProfit, Timeout.Infinite, Timeout.Infinite);//23
            _triggerCRMTimer = new Timer(CallJob, Constants.Jobs.TriggerCRM, Timeout.Infinite, Timeout.Infinite);//24
            _checkClientBlockedSessionsTimer = new Timer(CallJob, Constants.Jobs.CheckClientBlockedSessions, Timeout.Infinite, Timeout.Infinite);//25
            _checkWithdrawRequestsStatusesTimer = new Timer(CallJob, Constants.Jobs.CheckWithdrawRequestsStatuses, Timeout.Infinite, Timeout.Infinite);//26
            _deactivateExiredKYCTimer = new Timer(CallJob, Constants.Jobs.DeactivateExiredKYC, Timeout.Infinite, Timeout.Infinite);//27
            _triggerBonusTimer = new Timer(CallJob, Constants.Jobs.TriggerBonus, Timeout.Infinite, Timeout.Infinite);//28
            _checkUserBlockedSessionsTimer = new Timer(CallJob, Constants.Jobs.CheckUserBlockedSessions, Timeout.Infinite, Timeout.Infinite);//29
            _inactiveClientsTimer = new Timer(CallJob, Constants.Jobs.CheckInactiveClients, Timeout.Infinite, Timeout.Infinite);//30

            _notifyIdentityExpirationTimer = new Timer(CallJob, Constants.Jobs.NotifyIdentityExpiration, Timeout.Infinite, Timeout.Infinite);//31
            _inactivateImpossiblBonuses = new Timer(CallJob, Constants.Jobs.InactivateImpossibleBonuses, Timeout.Infinite, Timeout.Infinite);//32
            _updateJackpotFeed = new Timer(CallJob, Constants.Jobs.UpdateJackpotFeed, Timeout.Infinite, Timeout.Infinite);//34
            _reconsiderDynamicSegments = new Timer(CallJob, Constants.Jobs.ReconsiderDynamicSegments, Timeout.Infinite, Timeout.Infinite);//35
            _inactiveUsersTimer = new Timer(CallJob, Constants.Jobs.CheckInactiveUsers, Timeout.Infinite, Timeout.Infinite);//36
            _sendPartnerDailyReport = new Timer(CallJob, Constants.Jobs.SendPartnerDailyReport, Timeout.Infinite, Timeout.Infinite);//37
            _sendPartnerWeeklyReport = new Timer(CallJob, Constants.Jobs.SendPartnerWeeklyReport, Timeout.Infinite, Timeout.Infinite);//38
            _sendPartnerMonthlyReport = new Timer(CallJob, Constants.Jobs.SendPartnerMonthlyReport, Timeout.Infinite, Timeout.Infinite);//39
            _awardCashBackBonusesTimer = new Timer(CallJob, Constants.Jobs.AwardCashBackBonuses, Timeout.Infinite, Timeout.Infinite);//40

            _fairSegmentTriggers = new Timer(CallJob, Constants.Jobs.FairSegmentTriggers, Timeout.Infinite, Timeout.Infinite);//41
            _giveFreeSpinTimer = new Timer(CallJob, Constants.Jobs.GiveFreeSpin, Timeout.Infinite, Timeout.Infinite);//42
            _giveJackpotWinTimer = new Timer(CallJob, Constants.Jobs.GiveJackpotWin, Timeout.Infinite, Timeout.Infinite);//43
            _giveCompPoints = new Timer(CallJob, Constants.Jobs.GiveCompPoints, Timeout.Infinite, Timeout.Infinite);//44
            _checkDepositRequestsStatusesTimer = new Timer(CallJob, Constants.Jobs.CheckDepositRequestsStatuses, Timeout.Infinite, Timeout.Infinite);//45
            _checkForceBlockedClientsTimer = new Timer(CallJob, Constants.Jobs.CheckForceBlockedClients, Timeout.Infinite, Timeout.Infinite);//47
            _giveFixedFeeCommissionTimer = new Timer(CallJob, Constants.Jobs.GiveFixedFeeCommission, Timeout.Infinite, Timeout.Infinite);//48
            _fulfillDepositAction = new Timer(CallJob, Constants.Jobs.FulfillDepositAction, Timeout.Infinite, Timeout.Infinite);//49
            _applyClientRestriction = new Timer(CallJob, Constants.Jobs.ApplyClientRestriction, Timeout.Infinite, Timeout.Infinite);//50

            _settleBets = new Timer(CallJob, Constants.Jobs.SettleBets, Timeout.Infinite, Timeout.Infinite);//51
            _restrictUnverifiedClients = new Timer(CallJob, Constants.Jobs.RestrictUnverifiedClients, Timeout.Infinite, Timeout.Infinite);//52
            _giveAffiliateCommissionTimer = new Timer(CallJob, Constants.Jobs.GiveAffiliateCommission, Timeout.Infinite, Timeout.Infinite);//53
            _expireClientVerificationStatus = new Timer(CallJob, Constants.Jobs.ExpireClientVerificationStatus, Timeout.Infinite, Timeout.Infinite);//54
            _checkDuplicateClients = new Timer(CallJob, Constants.Jobs.CheckDuplicateClients, Timeout.Infinite, Timeout.Infinite);//55
            _closeTournamentsTimer = new Timer(CallJob, Constants.Jobs.CloseTournaments, Timeout.Infinite, Timeout.Infinite);//56
        }

        protected override void OnStart(string[] args)
        {
            _closeAccountPeriodTimer.Change(20000, 20000);//1
            _addMoneyToPartnerAccountTimer.Change(20000, 20000);//2
            _calculateCompPoints.Change(10000, 10000);//3
            _calculateJackpots.Change(10000, 10000);//4
            _expireUserSessionsTimer.Change(5000, 5000);//5
            _broadcastBets.Change(10000, 10000);//6
            _resetBetShopDailyTicketNumberTimer.Change(20000, 20000);//7
            _expireClientSessionsTimer.Change(5000, 5000);//9
            _expireClientVerificationKeysTimer.Change(5000, 5000);//10

            _calculateCashBackBonusesTimer.Change(60000, 60000);//11
            _closeClientPeriodTimer.Change(20000, 20000);//12
            _giveAffiliateBonusTimer.Change(60000, 60000);//13
            _deletePaymentExpiredActiveRequestsTimer.Change(60000, 60000);//14
            _updateCurrenciesRateTimer.Change(60000, 600000);//15
            _sendActiveEmailsTimer.Change(1000, 1000);//16
            _updateClientWageringBonusTimer.Change(1000, 1000);//17
            _finalizeWageringBonusTimer.Change(1000, 1000);//18
            _sendActiveMerchantRequestsTimer.Change(5000, 5000);//19
            _approveIqWalletConfirmedRequestsTimer.Change(5000, 5000);//20

            _sendAffiliateReportTimer.Change(60000, 60000);//21
            _calculateAgentsGGRProfitTimer.Change(60000, 3600000);//22
            _calculateAgentsTurnoverProfitTimer.Change(60000, 600000);//23
            _triggerCRMTimer.Change(60000, 60000);//24
            _checkClientBlockedSessionsTimer.Change(60000, 60000);//25
            _checkWithdrawRequestsStatusesTimer.Change(60000, 60000);//26
            _deactivateExiredKYCTimer.Change(600000, 600000);//27
            _triggerBonusTimer.Change(60000, 60000);//28
            _checkUserBlockedSessionsTimer.Change(60000, 60000);//29
            _inactiveClientsTimer.Change(300000, 300000);//30

            _notifyIdentityExpirationTimer.Change(300000, 300000);//31
            _inactivateImpossiblBonuses.Change(300000, 300000);//32
            _updateJackpotFeed.Change(300000, 300000);//34
            _reconsiderDynamicSegments.Change(20000, 20000);//35
            _inactiveUsersTimer.Change(300000, 300000);//36
            _sendPartnerDailyReport.Change(120000, 600000);//37
            _sendPartnerWeeklyReport.Change(120000, 600000);//38
            _sendPartnerMonthlyReport.Change(120000, 600000);//39
            _awardCashBackBonusesTimer.Change(60000, 60000);//40

            _fairSegmentTriggers.Change(20000, 20000);//41
            _giveFreeSpinTimer.Change(5000, 5000);//42
            _giveJackpotWinTimer.Change(60000, 60000);//43
            _giveCompPoints.Change(30000, 30000);//44
            _checkDepositRequestsStatusesTimer.Change(60000, 60000);//45
            _checkForceBlockedClientsTimer.Change(300000, 300000);//47
            _giveFixedFeeCommissionTimer.Change(60000, 60000);//48
            _fulfillDepositAction.Change(20000, 20000);//49
            _applyClientRestriction.Change(300000, 3600000);//50

            _settleBets.Change(60000, 60000);//51
            _restrictUnverifiedClients.Change(60000, 60000);//52
            _giveAffiliateCommissionTimer.Change(60000, 60000);//53
            _expireClientVerificationStatus.Change(60000, 3600000);//54
            _checkDuplicateClients.Change(60000, 3600000);//55
            _closeTournamentsTimer.Change(60000, 60000);//56

            var startOptions = new StartOptions("http://*:9010/");
            _server = WebApp.Start<Startup>(startOptions);
        }

        protected override void OnStop()
        {
            _closeAccountPeriodTimer.Change(Timeout.Infinite, Timeout.Infinite);//1
            _addMoneyToPartnerAccountTimer.Change(Timeout.Infinite, Timeout.Infinite);//2
            _calculateCompPoints.Change(Timeout.Infinite, Timeout.Infinite);//3
            _calculateJackpots.Change(Timeout.Infinite, Timeout.Infinite);//4
            _expireUserSessionsTimer.Change(Timeout.Infinite, Timeout.Infinite);//5
            _broadcastBets.Change(Timeout.Infinite, Timeout.Infinite);//6
            _resetBetShopDailyTicketNumberTimer.Change(Timeout.Infinite, Timeout.Infinite);//7
            _expireClientSessionsTimer.Change(Timeout.Infinite, Timeout.Infinite);//9
            _expireClientVerificationKeysTimer.Change(Timeout.Infinite, Timeout.Infinite);//10

            _calculateCashBackBonusesTimer.Change(Timeout.Infinite, Timeout.Infinite);//11
            _closeClientPeriodTimer.Change(Timeout.Infinite, Timeout.Infinite);//12
            _giveAffiliateBonusTimer.Change(Timeout.Infinite, Timeout.Infinite);//13
            _deletePaymentExpiredActiveRequestsTimer.Change(Timeout.Infinite, Timeout.Infinite);//14
            _updateCurrenciesRateTimer.Change(Timeout.Infinite, Timeout.Infinite);//15
            _sendActiveEmailsTimer.Change(Timeout.Infinite, Timeout.Infinite);//16
            _updateClientWageringBonusTimer.Change(Timeout.Infinite, Timeout.Infinite);//17
            _finalizeWageringBonusTimer.Change(Timeout.Infinite, Timeout.Infinite);//18
            _sendActiveMerchantRequestsTimer.Change(Timeout.Infinite, Timeout.Infinite);//19
            _approveIqWalletConfirmedRequestsTimer.Change(Timeout.Infinite, Timeout.Infinite);//20

            _sendAffiliateReportTimer.Change(Timeout.Infinite, Timeout.Infinite);//21
            _calculateAgentsGGRProfitTimer.Change(Timeout.Infinite, Timeout.Infinite);//22
            _calculateAgentsTurnoverProfitTimer.Change(Timeout.Infinite, Timeout.Infinite);//23
            _triggerCRMTimer.Change(Timeout.Infinite, Timeout.Infinite);//24
            _checkClientBlockedSessionsTimer.Change(Timeout.Infinite, Timeout.Infinite);//25
            _checkWithdrawRequestsStatusesTimer.Change(Timeout.Infinite, Timeout.Infinite);//26
            _deactivateExiredKYCTimer.Change(Timeout.Infinite, Timeout.Infinite);//27
            _triggerBonusTimer.Change(Timeout.Infinite, Timeout.Infinite);//28
            _checkUserBlockedSessionsTimer.Change(Timeout.Infinite, Timeout.Infinite);//29
            _inactiveClientsTimer.Change(Timeout.Infinite, Timeout.Infinite);//30

            _notifyIdentityExpirationTimer.Change(Timeout.Infinite, Timeout.Infinite);//31
            _inactivateImpossiblBonuses.Change(Timeout.Infinite, Timeout.Infinite);//32
            _updateJackpotFeed.Change(Timeout.Infinite, Timeout.Infinite);//34
            _reconsiderDynamicSegments.Change(Timeout.Infinite, Timeout.Infinite);//35
            _inactiveUsersTimer.Change(Timeout.Infinite, Timeout.Infinite);//36
            _sendPartnerDailyReport.Change(Timeout.Infinite, Timeout.Infinite);//37
            _sendPartnerWeeklyReport.Change(Timeout.Infinite, Timeout.Infinite);//38
            _sendPartnerMonthlyReport.Change(Timeout.Infinite, Timeout.Infinite);//39
            _awardCashBackBonusesTimer.Change(Timeout.Infinite, Timeout.Infinite);//40

            _fairSegmentTriggers.Change(Timeout.Infinite, Timeout.Infinite);//41
            _giveFreeSpinTimer.Change(Timeout.Infinite, Timeout.Infinite);//42
            _giveJackpotWinTimer.Change(Timeout.Infinite, Timeout.Infinite);//43
            _giveCompPoints.Change(Timeout.Infinite, Timeout.Infinite);//44
            _checkDepositRequestsStatusesTimer.Change(Timeout.Infinite, Timeout.Infinite);//45
            _checkForceBlockedClientsTimer.Change(Timeout.Infinite, Timeout.Infinite);//47
            _giveFixedFeeCommissionTimer.Change(Timeout.Infinite, Timeout.Infinite);//48
            _fulfillDepositAction.Change(Timeout.Infinite, Timeout.Infinite);//49
            _applyClientRestriction.Change(Timeout.Infinite, Timeout.Infinite);//50

            _settleBets.Change(Timeout.Infinite, Timeout.Infinite);//51
            _restrictUnverifiedClients.Change(Timeout.Infinite, Timeout.Infinite);//52
            _giveAffiliateCommissionTimer.Change(Timeout.Infinite, Timeout.Infinite);//53
            _expireClientVerificationStatus.Change(Timeout.Infinite, Timeout.Infinite);//54
            _checkDuplicateClients.Change(Timeout.Infinite, Timeout.Infinite);//55
            _closeTournamentsTimer.Change(Timeout.Infinite, Timeout.Infinite);//56

            if (_server != null)
            {
                _server.Dispose();
            }
        }

        public void CallJob(Object sender)
        {
            var jobId = Convert.ToInt32(sender);
            Timer timer = null;
            bool usePeriodInSeconds = true;
            int duration = 1000;

            switch (jobId)
            {
                case Constants.Jobs.CloseAccountPeriod://1
                    timer = _closeAccountPeriodTimer;
                    duration = 20000;
                    break;
                case Constants.Jobs.AddMoneyToPartnerAccount://2
                    timer = _addMoneyToPartnerAccountTimer;
                    duration = 20000;
                    break;
                case Constants.Jobs.CalculateCompPoints://3
                    timer = _calculateCompPoints;
                    duration = 10000;
                    break;
                case Constants.Jobs.CalculateJackpots://4
                    timer = _calculateJackpots;
                    duration = 10000;
                    break;
                case Constants.Jobs.ExpireUserSessions://5
                    timer = _expireUserSessionsTimer;
                    duration = 5000;
                    break;
                case Constants.Jobs.BroadcastBets://6
                    timer = _broadcastBets;
                    duration = 10000;
                    break;
                case Constants.Jobs.ResetBetShopDailyTicketNumber://7
                    timer = _resetBetShopDailyTicketNumberTimer;
                    duration = 20000;
                    break;
                case Constants.Jobs.ExpireClientSessions://9
                    timer = _expireClientSessionsTimer;
                    duration = 5000;
                    break;
                case Constants.Jobs.ExpireClientVerificationKeys://10
                    timer = _expireClientVerificationKeysTimer;
                    duration = 5000;
                    break;

                case Constants.Jobs.CalculateCashBackBonuses://11
                    timer = _calculateCashBackBonusesTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.CloseClientPeriod://12
                    timer = _closeClientPeriodTimer;
                    duration = 20000;
                    break;
                case Constants.Jobs.GiveAffiliateBonus://13
                    timer = _giveAffiliateBonusTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.DeletePaymentExpiredActiveRequests://14
                    timer = _deletePaymentExpiredActiveRequestsTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.UpdateCurrenciesRate://15
                    timer = _updateCurrenciesRateTimer;
                    duration = 600000;
                    break;
                case Constants.Jobs.SendActiveMails://16
                    timer = _sendActiveEmailsTimer;
                    duration = 1000;
                    break;
                case Constants.Jobs.UpdateClientWageringBonus://17
                    timer = _updateClientWageringBonusTimer;
                    duration = 1000;
                    break;
                case Constants.Jobs.FinalizeWageringBonus://18
                    timer = _finalizeWageringBonusTimer;
                    duration = 1000;
                    break;
                case Constants.Jobs.SendActiveMerchantRequests://19
                    timer = _sendActiveMerchantRequestsTimer;
                    duration = 5000;
                    break;
                case Constants.Jobs.ApproveIqWalletConfirmedRequests://20
                    timer = _approveIqWalletConfirmedRequestsTimer;
                    duration = 5000;
                    break;

                case Constants.Jobs.SendAffiliateReport://21
                    timer = _sendAffiliateReportTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.CalculateAgentsGGRProfit://22
                    timer = _calculateAgentsGGRProfitTimer;
                    duration = 3600000;
                    break;
                case Constants.Jobs.CalculateAgentsTurnoverProfit://23
                    timer = _calculateAgentsTurnoverProfitTimer;
                    duration = 600000;
                    break;
                case Constants.Jobs.TriggerCRM://24
                    timer = _triggerCRMTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.CheckClientBlockedSessions://25
                    timer = _checkClientBlockedSessionsTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.CheckWithdrawRequestsStatuses://26
                    timer = _checkWithdrawRequestsStatusesTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.DeactivateExiredKYC://27
                    timer = _deactivateExiredKYCTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.TriggerBonus://28
                    timer = _triggerBonusTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.CheckUserBlockedSessions://29
                    timer = _checkUserBlockedSessionsTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.CheckInactiveClients://30
                    timer = _inactiveClientsTimer;
                    duration = 300000;
                    break;

                case Constants.Jobs.NotifyIdentityExpiration://31
                    timer = _notifyIdentityExpirationTimer;
                    duration = 300000;
                    break;
                case Constants.Jobs.InactivateImpossibleBonuses://32
                    timer = _inactivateImpossiblBonuses;
                    duration = 300000;
                    break;
                case Constants.Jobs.UpdateJackpotFeed://34
                    timer = _updateJackpotFeed;
                    duration = 300000;
                    break;
                case Constants.Jobs.ReconsiderDynamicSegments://35
                    timer = _reconsiderDynamicSegments;
                    duration = 20000;
                    break;
                case Constants.Jobs.CheckInactiveUsers://36
                    timer = _inactiveUsersTimer;
                    duration = 300000;
                    break;
                case Constants.Jobs.SendPartnerDailyReport://37
                    timer = _sendPartnerDailyReport;
                    duration = 600000;
                    break;
                case Constants.Jobs.SendPartnerWeeklyReport://38
                    timer = _sendPartnerWeeklyReport;
                    duration = 600000;
                    break;
                case Constants.Jobs.SendPartnerMonthlyReport://39
                    timer = _sendPartnerMonthlyReport;
                    duration = 600000;
                    break;
                case Constants.Jobs.AwardCashBackBonuses://40
                    timer = _awardCashBackBonusesTimer;
                    duration = 60000;
                    break;

                case Constants.Jobs.FairSegmentTriggers://41
                    timer = _fairSegmentTriggers;
                    duration = 20000;
                    break;
                case Constants.Jobs.GiveFreeSpin://42
                    timer = _giveFreeSpinTimer;
                    duration = 5000;
                    usePeriodInSeconds = false;
                    break;
                case Constants.Jobs.GiveJackpotWin://43
                    timer = _giveJackpotWinTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.GiveCompPoints://44
                    timer = _giveCompPoints;
                    duration = 30000;
                    break;
                case Constants.Jobs.CheckDepositRequestsStatuses://45
                    timer = _checkDepositRequestsStatusesTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.CheckForceBlockedClients://47
                    timer = _checkForceBlockedClientsTimer;
                    duration = 300000;
                    break;
                case Constants.Jobs.GiveFixedFeeCommission://48
                    timer = _giveFixedFeeCommissionTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.FulfillDepositAction://49
                    timer = _fulfillDepositAction;
                    duration = 20000;
                    break;
                case Constants.Jobs.ApplyClientRestriction://50
                    timer = _applyClientRestriction;
                    duration = 300000;
                    break;

                case Constants.Jobs.SettleBets://51
                    timer = _settleBets;
                    duration = 10000;
                    break;
                case Constants.Jobs.RestrictUnverifiedClients://52
                    timer = _restrictUnverifiedClients;
                    duration = 60000;
                    break;
                case Constants.Jobs.GiveAffiliateCommission://53
                    timer = _giveAffiliateCommissionTimer;
                    duration = 60000;
                    break;
                case Constants.Jobs.ExpireClientVerificationStatus://54
                    timer = _expireClientVerificationStatus;
                    duration = 60000;
                    break;
                case Constants.Jobs.CheckDuplicateClients://55
                    timer = _checkDuplicateClients;
                    duration = 3600000;
                    break;
                case Constants.Jobs.CloseTournaments://56
                    timer = _closeTournamentsTimer;
                    duration = 60000;
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
                    job.NextExecutionTime = (usePeriodInSeconds ? job.NextExecutionTime.AddSeconds(job.PeriodInSeconds) : DateTime.UtcNow);
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
                case Constants.Jobs.CloseAccountPeriod://1
                    {
                        var closePeriodInput = JsonConvert.DeserializeObject<ClosePeriodInput>(job.Parameters);
                        var result = JobBll.CloseAccountPeriod(closePeriodInput);
                        if (result)
                            closePeriodInput.EndTime = closePeriodInput.EndTime.AddHours(Constants.ClosePeriodPeriodicy);
                        return JsonConvert.SerializeObject(closePeriodInput);
                    }
                case Constants.Jobs.AddMoneyToPartnerAccount://2
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
                case Constants.Jobs.CalculateCompPoints://3
                    if (job.NextExecutionTime <= jobStartTime)
                        JobBll.CalculateCompPoints(job.NextExecutionTime.AddSeconds(-job.PeriodInSeconds), Program.DbLogger);
                    break;
                case Constants.Jobs.CalculateJackpots://4
                    if (job.NextExecutionTime <= jobStartTime)
                        JobBll.CalculateJackpots(job.NextExecutionTime.AddSeconds(-job.PeriodInSeconds), Program.DbLogger);
                    break;
                case Constants.Jobs.ExpireUserSessions://5
                    JobBll.ExpireUserSessions();
                    break;
                case Constants.Jobs.BroadcastBets://6
                    if (job.NextExecutionTime <= jobStartTime)
                        JobBll.BroadcastBets(job.NextExecutionTime.AddSeconds(-job.PeriodInSeconds), Program.DbLogger);
                    break;
                case Constants.Jobs.ResetBetShopDailyTicketNumber://7
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
                case Constants.Jobs.ExpireClientSessions://9
                    JobBll.ExpireClientSessions();
                    break;
                case Constants.Jobs.ExpireClientVerificationKeys://10
                    JobBll.ExpireClientVerificationKeys();
                    break;

                case Constants.Jobs.CalculateCashBackBonuses://11
                    JobBll.CalculateCashBackBonuses(Program.DbLogger);
                    break;
                case Constants.Jobs.CloseClientPeriod://12
                    {
                        var closePeriodInput = JsonConvert.DeserializeObject<ClosePeriodInput>(job.Parameters);
                        var result = JobBll.CloseClientPeriod(closePeriodInput);
                        if (result)
                            closePeriodInput.EndTime = closePeriodInput.EndTime.AddHours(Constants.ClosePeriodPeriodicy);
                        return JsonConvert.SerializeObject(closePeriodInput);
                    }
                case Constants.Jobs.GiveAffiliateBonus://13
                    JobBll.GiveAffiliateBonus(Program.DbLogger);
                    break;
                case Constants.Jobs.DeletePaymentExpiredActiveRequests://14
                    JobBll.DeletePaymentExpiredActiveRequests(Program.DbLogger);
                    break;
                case Constants.Jobs.UpdateCurrenciesRate://15
                    if (job.NextExecutionTime <= jobStartTime)
                        JobBll.UpdateCurrentRate(Program.DbLogger);
                    break;
                case Constants.Jobs.SendActiveMails://16
                    JobBll.SendActiveMails(Program.DbLogger);
                    break;
                case Constants.Jobs.UpdateClientWageringBonus://17
                    JobBll.UpdateClientWageringBonus();
                    break;
                case Constants.Jobs.FinalizeWageringBonus://18
                    JobBll.FinalizeWageringBonus(Program.DbLogger);
                    break;
                case Constants.Jobs.SendActiveMerchantRequests://19
                    JobBll.SendActiveMerchantRequests(Program.DbLogger);
                    break;
                case Constants.Jobs.ApproveIqWalletConfirmedRequests://20
                    ApproveIqWalletConfirmedRequests();
                    break;

                case Constants.Jobs.SendAffiliateReport://21
                    if (job.NextExecutionTime <= jobStartTime)
                        JobBll.SendAffiliateReport(Program.DbLogger);
                    break;
                case Constants.Jobs.CalculateAgentsGGRProfit://22
                    if (job.NextExecutionTime <= jobStartTime)
                        success = JobBll.CalculateAgentsGGRProfit(job.NextExecutionTime, Program.DbLogger);
                    break;
                case Constants.Jobs.CalculateAgentsTurnoverProfit://23
                    if (job.NextExecutionTime <= jobStartTime)
                        success = JobBll.CalculateAgentsTurnoverProfit(job.NextExecutionTime, Program.DbLogger);
                    break;
                case Constants.Jobs.TriggerCRM://24
                    JobBll.TriggerMissedDepositCRM(job, Program.DbLogger);
                    break;
                case Constants.Jobs.CheckClientBlockedSessions://25
                    JobBll.CheckClientBlockedSessions(Program.DbLogger);
                    break;
                case Constants.Jobs.CheckWithdrawRequestsStatuses://26
                    CheckWithdrawRequestsStatuses(Program.DbLogger);
                    break;
                case Constants.Jobs.DeactivateExiredKYC://27
                    JobBll.DeactivateExiredKYC(Program.DbLogger);
                    break;
                case Constants.Jobs.TriggerBonus://28
                    JobBll.AwardClientCampaignBonus(Program.DbLogger);
                    break;
                case Constants.Jobs.CheckUserBlockedSessions://29
                    JobBll.CheckUserBlockedSessions();
                    break;
                case Constants.Jobs.CheckInactiveClients://30
                    JobBll.CheckInactiveClients(Program.DbLogger);
                    break;

                case Constants.Jobs.NotifyIdentityExpiration://31
                    JobBll.NotifyIdentityExpiration(Program.DbLogger);
                    break;
                case Constants.Jobs.InactivateImpossibleBonuses://32
                    JobBll.InactivateImpossiblBonuses();
                    break;
                case Constants.Jobs.UpdateJackpotFeed://34
                    JobBll.UpdateJackpotFeed(Program.DbLogger);
                    break;
                case Constants.Jobs.ReconsiderDynamicSegments://35
                    JobBll.ReconsiderDynamicSegments(Program.DbLogger);
                    break;
                case Constants.Jobs.CheckInactiveUsers://36
                    JobBll.CheckInactiveUsers();
                    break;
                case Constants.Jobs.SendPartnerDailyReport://37
                    JobBll.SendPartnerDailyReport(Program.DbLogger);
                    break;
                case Constants.Jobs.SendPartnerWeeklyReport://38
                    JobBll.SendPartnerWeeklyReport(Program.DbLogger);
                    break;
                case Constants.Jobs.SendPartnerMonthlyReport://39
                    JobBll.SendPartnerMonthlyReport(Program.DbLogger);
                    break;
                case Constants.Jobs.AwardCashBackBonuses://40
                    JobBll.AwardCashBackBonuses(job.NextExecutionTime, Program.DbLogger);
                    break;

                case Constants.Jobs.FairSegmentTriggers://41
                    JobBll.FairSegmentTriggers(Program.DbLogger);
                    break;
                case Constants.Jobs.GiveFreeSpin://42
                    if (job.NextExecutionTime <= jobStartTime)
                        JobBll.GiveFreeSpin(Program.DbLogger);
                    break;
                case Constants.Jobs.GiveJackpotWin://43
                    JobBll.GiveJackpotWin(Program.DbLogger);
                    break;
                case Constants.Jobs.GiveCompPoints://44
                    JobBll.GiveCompPoints(Program.DbLogger);
                    break;
                case Constants.Jobs.CheckDepositRequestsStatuses://45
                    CheckDepositRequestsStatuses(Program.DbLogger);
                    break;
                case Constants.Jobs.CheckForceBlockedClients://47
                    JobBll.CheckForceBlockedClients();
                    break;
                case Constants.Jobs.GiveFixedFeeCommission://48
                    JobBll.GiveFixedFeeCommission(Program.DbLogger);
                    break;
                case Constants.Jobs.FulfillDepositAction://49
                    JobBll.FulfillDepositAction(Program.DbLogger);
                    break;
                case Constants.Jobs.ApplyClientRestriction://50
                    JobBll.ApplyClientRestriction(Program.DbLogger);
                    break;

                case Constants.Jobs.SettleBets://51
                    JobBll.SettleBets(Program.DbLogger);
                    break;
                case Constants.Jobs.RestrictUnverifiedClients://52
                    JobBll.RestrictUnverifiedClients(Program.DbLogger);
                    break;
                case Constants.Jobs.GiveAffiliateCommission://53
                    JobBll.GiveAffiliateCommission(job.NextExecutionTime, Program.DbLogger);
                    break;
                case Constants.Jobs.ExpireClientVerificationStatus://54
                    JobBll.ExpireClientVerificationStatus();
                    break;
                case Constants.Jobs.CheckDuplicateClients://55
                    JobBll.CheckDuplicateClients(Program.DbLogger);
                    break;
                case Constants.Jobs.CloseTournaments://56
                    JobBll.CloseTournaments(Program.DbLogger);
                    break;
            }
            return null;
        }

        public void ApproveIqWalletConfirmedRequests()
        {
            var ps = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.IqWallet);
            if (ps != null && ps.Id > 0)
            {
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var paymentSystemBl = new PaymentSystemBll(clientBl))
                    {
                        using (var documentBl = new DocumentBll(clientBl))
                        {
                            using (var notificationBl = new NotificationBll(documentBl))
                            {
                                var confirmedRequests = paymentSystemBl.GetPaymentRequests(ps.Id, (int)PaymentRequestStates.Confirmed);
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
    }
}