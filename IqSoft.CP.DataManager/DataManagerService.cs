using IqSoft.CP.DataManager.Services;
using System;
using System.ServiceProcess;
using System.Threading;

namespace IqSoft.CP.DataManager
{
    public partial class DataManagerService : ServiceBase
    {
        private readonly Timer _betGroupingTimer;
        private readonly Timer _documentMigrationTimer;
        private readonly Timer _clientMigrationTimer;
        private readonly Timer _userMigrationTimer;
        private readonly Timer _productMigrationTimer;
        private readonly Timer _gameProviderMigrationTimer;
        public DataManagerService()
        {
            InitializeComponent();

            _betGroupingTimer = new Timer(GroupBets, null, Timeout.Infinite, Timeout.Infinite);
            _documentMigrationTimer = new Timer(MigrateDocuments, null, Timeout.Infinite, Timeout.Infinite);
            _clientMigrationTimer = new Timer(MigrateClients, null, Timeout.Infinite, Timeout.Infinite);
            _userMigrationTimer = new Timer(MigrateUsers, null, Timeout.Infinite, Timeout.Infinite);
            _productMigrationTimer = new Timer(MigrateProducts, null, Timeout.Infinite, Timeout.Infinite);
            _gameProviderMigrationTimer = new Timer(MigrateGameProviders, null, Timeout.Infinite, Timeout.Infinite);
        }

        protected override void OnStart(string[] args)
        {
            _betGroupingTimer.Change(1000, 1000);
            _documentMigrationTimer.Change(1000, 1000);
            _clientMigrationTimer.Change(1000, 1000);
            _userMigrationTimer.Change(60000, 60000);
            _productMigrationTimer.Change(60000, 60000);
            _gameProviderMigrationTimer.Change(60000, 60000);
        }

        protected override void OnStop()
        {
            _betGroupingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _documentMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _clientMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _userMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
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
            _productMigrationTimer.Change(60000, 60000);
        }
        public void MigrateGameProviders(Object sender)
        {
            _gameProviderMigrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            DataCollector.MigrateGameProviders();
            _gameProviderMigrationTimer.Change(300000, 300000);
        }
    }
}