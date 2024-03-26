using IqSoft.CP.DataManager.Services;
using System;
using System.ServiceProcess;
using System.Threading;

namespace IqSoft.CP.DataManager
{
    public partial class DataManagerService : ServiceBase
    {
        private readonly Timer _betGroupingTimer;
        private readonly Timer _betCleaningTimer;

        private readonly Timer _documentMigrationTimer;
        private readonly Timer _clientMigrationTimer;
        private readonly Timer _userMigrationTimer;
        private readonly Timer _productMigrationTimer;
        private readonly Timer _gameProviderMigrationTimer;
        private readonly Timer _currencyMigrationTimer;
        private readonly Timer _clientSessionMigrationTimer;
        private readonly Timer _clientBonusMigrationTimer;
        private readonly Timer _bonusMigrationTimer;
        private readonly Timer _paymentRequestMigrationTimer;
        private readonly Timer _partnerMigrationTimer;
        private readonly Timer _affiliatePlatformMigrationTimer;
        private readonly Timer _affiliateReferralMigrationTimer;

        private readonly Timer _dashboardInfoTimer;
        private readonly Timer _providerBetsTimer;
        private readonly Timer _paymentRequestsTimer;
        private readonly Timer _clientInfoTimer;
        private readonly Timer _executeInfoTimer;

        public DataManagerService()
        {
            InitializeComponent();

            _betGroupingTimer = new Timer(GroupBets, null, Timeout.Infinite, Timeout.Infinite);
            _betCleaningTimer = new Timer(CleanBets, null, Timeout.Infinite, Timeout.Infinite);
            
            _documentMigrationTimer = new Timer(MigrateDocuments, null, Timeout.Infinite, Timeout.Infinite);
            _clientMigrationTimer = new Timer(MigrateClients, null, Timeout.Infinite, Timeout.Infinite);
            _userMigrationTimer = new Timer(MigrateUsers, null, Timeout.Infinite, Timeout.Infinite);
            _productMigrationTimer = new Timer(MigrateProducts, null, Timeout.Infinite, Timeout.Infinite);
            _gameProviderMigrationTimer = new Timer(MigrateGameProviders, null, Timeout.Infinite, Timeout.Infinite);
            _currencyMigrationTimer = new Timer(MigrateCurrencies, null, Timeout.Infinite, Timeout.Infinite);
            _clientSessionMigrationTimer = new Timer(MigrateClientSessions, null, Timeout.Infinite, Timeout.Infinite);
            _clientBonusMigrationTimer = new Timer(MigrateClientBonuses, null, Timeout.Infinite, Timeout.Infinite);
            _bonusMigrationTimer = new Timer(MigrateBonuses, null, Timeout.Infinite, Timeout.Infinite);
            _paymentRequestMigrationTimer = new Timer(MigratePaymentRequests, null, Timeout.Infinite, Timeout.Infinite);
            _partnerMigrationTimer = new Timer(MigratePartners, null, Timeout.Infinite, Timeout.Infinite);
            _affiliatePlatformMigrationTimer = new Timer(MigrateAffiliatePlatform, null, Timeout.Infinite, Timeout.Infinite);
            _affiliateReferralMigrationTimer = new Timer(MigrateAffiliateReferral, null, Timeout.Infinite, Timeout.Infinite);

            _dashboardInfoTimer = new Timer(CalculateDashboardInfo, null, Timeout.Infinite, Timeout.Infinite);
            _providerBetsTimer = new Timer(CalculateProviderBets, null, Timeout.Infinite, Timeout.Infinite);
            _paymentRequestsTimer = new Timer(CalculatePaymentInfo, null, Timeout.Infinite, Timeout.Infinite);
            _clientInfoTimer = new Timer(CalculateClientInfo, null, Timeout.Infinite, Timeout.Infinite);
            _executeInfoTimer = new Timer(ExecuteInfoFunctions, null, Timeout.Infinite, Timeout.Infinite);
        }

        protected override void OnStart(string[] args)
        {
            _betGroupingTimer.Change(1000, 1000);
            _betCleaningTimer.Change(10000, 10000);
            _documentMigrationTimer.Change(1000, 1000);
            _clientMigrationTimer.Change(1000, 1000);
            _userMigrationTimer.Change(60000, 60000);
            _productMigrationTimer.Change(60000, 60000);
            _gameProviderMigrationTimer.Change(60000, 60000);
            _currencyMigrationTimer.Change(60000, 60000);
            _clientSessionMigrationTimer.Change(60000, 60000);
            _clientBonusMigrationTimer.Change(60000, 60000);
            _bonusMigrationTimer.Change(60000, 60000);
            _paymentRequestMigrationTimer.Change(60000, 60000);
            _partnerMigrationTimer.Change(60000, 60000);
            _affiliatePlatformMigrationTimer.Change(60000, 60000);
            _affiliateReferralMigrationTimer.Change(60000, 60000);

            _dashboardInfoTimer.Change(60000, 60000);
            _providerBetsTimer.Change(60000, 60000);
            _paymentRequestsTimer.Change(60000, 60000);
            _clientInfoTimer.Change(60000, 60000);
            _executeInfoTimer.Change(60000, 60000);
        }

        protected override void OnStop()
        {
            _betGroupingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _betCleaningTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _documentMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _clientMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _userMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _productMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _gameProviderMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _currencyMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _clientSessionMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _clientBonusMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _bonusMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _paymentRequestMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _partnerMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _affiliatePlatformMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _affiliateReferralMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);

            _dashboardInfoTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _providerBetsTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _paymentRequestsTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _clientInfoTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _executeInfoTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void GroupBets(Object sender)
        {
            _betGroupingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            var documentId = DataCombiner.LastProcessedBetDocumentId;
            if (documentId >= 0)
            {
                DataCombiner.GroupNewBets(documentId, Program.DbLogger);
            }
            _betGroupingTimer.Change(0, 1000);
        }

        public void CleanBets(Object sender)
        {
            _betCleaningTimer.Change(Timeout.Infinite, Timeout.Infinite);
            var count = DataCombiner.CleanBets(Program.DbLogger);
            if(count > 0)
                _betCleaningTimer.Change(0, 60000);
            else
                _betCleaningTimer.Change(300000, 300000);
        }

        public void MigrateDocuments(Object sender)
        {
            _documentMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            DataCollector.MigrateDocuments();
            _documentMigrationTimer.Change(100, 100);
        }
        public void MigrateClients(Object sender)
        {
            _clientMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            DataCollector.MigrateClients();
            _clientMigrationTimer.Change(1000, 1000);
        }
        public void MigrateUsers(Object sender)
        {
            _userMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            DataCollector.MigrateUsers();
            _userMigrationTimer.Change(60000, 60000);
        }
        public void MigrateProducts(Object sender)
        {
            _productMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            DataCollector.MigrateProducts();
            _productMigrationTimer.Change(300000, 300000);
        }
        public void MigrateGameProviders(Object sender)
        {
            _gameProviderMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            DataCollector.MigrateGameProviders();
            _gameProviderMigrationTimer.Change(300000, 300000);
        }
        public void MigrateCurrencies(Object sender)
        {
            _currencyMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            DataCollector.MigrateCurrencies();
            _currencyMigrationTimer.Change(300000, 300000);
        }
        public void MigrateClientSessions(Object sender)
        {
            _clientSessionMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            DataCollector.MigrateClientSessions();
            _clientSessionMigrationTimer.Change(60000, 60000);
        }
        public void MigrateClientBonuses(Object sender)
        {
            _clientBonusMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            DataCollector.MigrateClientBonuses();
            _clientBonusMigrationTimer.Change(60000, 60000);
        }

        public void MigrateBonuses(Object sender)
        {
            _bonusMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            DataCollector.MigrateBonuses();
            _bonusMigrationTimer.Change(120000, 120000);
        }

        public void MigratePaymentRequests(Object sender)
        {
            _paymentRequestMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            DataCollector.MigratePaymentRequests();
            _paymentRequestMigrationTimer.Change(60000, 60000);
        }
        public void MigratePartners(Object sender)
        {
            _partnerMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            DataCollector.MigratePartners();
            _partnerMigrationTimer.Change(300000, 300000);
        }
        public void MigrateAffiliatePlatform(Object sender)
        {
            _affiliatePlatformMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            DataCollector.MigrateAffiliatePlatform();
            _affiliatePlatformMigrationTimer.Change(300000, 300000);
        }
        public void MigrateAffiliateReferral(Object sender)
        {
            _affiliateReferralMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            DataCollector.MigrateAffiliateReferral();
            _affiliateReferralMigrationTimer.Change(60000, 60000);
        }

        public void CalculateDashboardInfo(Object sender)
        {
            _dashboardInfoTimer.Change(Timeout.Infinite, Timeout.Infinite);
            DataCombiner.CalculateDashboardInfo(Program.DbLogger, 0);
            _dashboardInfoTimer.Change(60000, 60000);
        }
        public void CalculateProviderBets(Object sender)
        {
            _providerBetsTimer.Change(Timeout.Infinite, Timeout.Infinite);
            DataCombiner.CalculateProviderBets(Program.DbLogger, 0);
            _providerBetsTimer.Change(60000, 60000);
        }
        public void CalculatePaymentInfo(Object sender)
        {
            _paymentRequestsTimer.Change(Timeout.Infinite, Timeout.Infinite);
            DataCombiner.CalculatePaymentInfo(Program.DbLogger, 0);
            _paymentRequestsTimer.Change(60000, 60000);
        }
        public void CalculateClientInfo(Object sender)
        {
            _clientInfoTimer.Change(Timeout.Infinite, Timeout.Infinite);
            DataCombiner.CalculateClientInfo(Program.DbLogger, 0, 0);
            _clientInfoTimer.Change(60000, 60000);
        }

        public void ExecuteInfoFunctions(Object sender)
        {
            _executeInfoTimer.Change(Timeout.Infinite, Timeout.Infinite);
            DataCombiner.ExecuteInfoFunctions(Program.DbLogger);
            _executeInfoTimer.Change(60000, 60000);
        }
    }
}