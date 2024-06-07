﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IqSoft.CP.DataWarehouse
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class IqSoftDataWarehouseEntities : DbContext
    {
        public IqSoftDataWarehouseEntities()
            : base("name=IqSoftDataWarehouseEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Document> Documents { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<GameProvider> GameProviders { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<ClientBonu> ClientBonus { get; set; }
        public DbSet<Gtd_Dashboard_Info> Gtd_Dashboard_Info { get; set; }
        public DbSet<Gtd_Provider_Bets> Gtd_Provider_Bets { get; set; }
        public DbSet<PaymentRequest> PaymentRequests { get; set; }
        public DbSet<Gtd_Deposit_Info> Gtd_Deposit_Info { get; set; }
        public DbSet<Gtd_Withdraw_Info> Gtd_Withdraw_Info { get; set; }
        public DbSet<JobTrigger> JobTriggers { get; set; }
        public DbSet<AffiliatePlatform> AffiliatePlatforms { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<AffiliateReferral> AffiliateReferrals { get; set; }
        public DbSet<Bonu> Bonus { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Bet> Bets { get; set; }
        public DbSet<Gtd_Client_Info> Gtd_Client_Info { get; set; }
        public DbSet<AgentCommission> AgentCommissions { get; set; }
        public DbSet<ClientSession> ClientSessions { get; set; }
        public DbSet<AccountBalance> AccountBalances { get; set; }
        public DbSet<DuplicatedClient> DuplicatedClients { get; set; }
        public DbSet<DuplicatedClientHistory> DuplicatedClientHistories { get; set; }
        public DbSet<Opt_Document_Considered> Opt_Document_Considered { get; set; }
    
        public virtual int sp_InsertDocuments(Nullable<long> minId)
        {
            var minIdParameter = minId.HasValue ?
                new ObjectParameter("minId", minId) :
                new ObjectParameter("minId", typeof(long));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_InsertDocuments", minIdParameter);
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_AffiliateClient")]
        public virtual IQueryable<fnAffiliateClient> fn_AffiliateClient(Nullable<long> fromDate, Nullable<long> toDate, Nullable<int> partnerId)
        {
            var fromDateParameter = fromDate.HasValue ?
                new ObjectParameter("FromDate", fromDate) :
                new ObjectParameter("FromDate", typeof(long));
    
            var toDateParameter = toDate.HasValue ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(long));
    
            var partnerIdParameter = partnerId.HasValue ?
                new ObjectParameter("PartnerId", partnerId) :
                new ObjectParameter("PartnerId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnAffiliateClient>("[IqSoftDataWarehouseEntities].[fn_AffiliateClient](@FromDate, @ToDate, @PartnerId)", fromDateParameter, toDateParameter, partnerIdParameter);
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_BetShopBet")]
        public virtual IQueryable<fnBetShopBet> fn_BetShopBet()
        {
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnBetShopBet>("[IqSoftDataWarehouseEntities].[fn_BetShopBet]()");
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_InternetGame")]
        public virtual IQueryable<fnInternetGame> fn_InternetGame(Nullable<long> fromDate, Nullable<long> toDate)
        {
            var fromDateParameter = fromDate.HasValue ?
                new ObjectParameter("FromDate", fromDate) :
                new ObjectParameter("FromDate", typeof(long));
    
            var toDateParameter = toDate.HasValue ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(long));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnInternetGame>("[IqSoftDataWarehouseEntities].[fn_InternetGame](@FromDate, @ToDate)", fromDateParameter, toDateParameter);
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_ProfitByAgent")]
        public virtual IQueryable<fnProfitByAgent> fn_ProfitByAgent(Nullable<long> fromDate, Nullable<long> toDate)
        {
            var fromDateParameter = fromDate.HasValue ?
                new ObjectParameter("FromDate", fromDate) :
                new ObjectParameter("FromDate", typeof(long));
    
            var toDateParameter = toDate.HasValue ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(long));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnProfitByAgent>("[IqSoftDataWarehouseEntities].[fn_ProfitByAgent](@FromDate, @ToDate)", fromDateParameter, toDateParameter);
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_ReportByPartner")]
        public virtual IQueryable<fnReportByPartner> fn_ReportByPartner(Nullable<long> fromDate, Nullable<long> toDate)
        {
            var fromDateParameter = fromDate.HasValue ?
                new ObjectParameter("FromDate", fromDate) :
                new ObjectParameter("FromDate", typeof(long));
    
            var toDateParameter = toDate.HasValue ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(long));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnReportByPartner>("[IqSoftDataWarehouseEntities].[fn_ReportByPartner](@FromDate, @ToDate)", fromDateParameter, toDateParameter);
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_ReportByProvider")]
        public virtual IQueryable<fnReportByProvider> fn_ReportByProvider(Nullable<long> fromDate, Nullable<long> toDate)
        {
            var fromDateParameter = fromDate.HasValue ?
                new ObjectParameter("FromDate", fromDate) :
                new ObjectParameter("FromDate", typeof(long));
    
            var toDateParameter = toDate.HasValue ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(long));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnReportByProvider>("[IqSoftDataWarehouseEntities].[fn_ReportByProvider](@FromDate, @ToDate)", fromDateParameter, toDateParameter);
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_Document")]
        public virtual IQueryable<fnDocument> fn_Document()
        {
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnDocument>("[IqSoftDataWarehouseEntities].[fn_Document]()");
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_ProviderClientsCount")]
        public virtual IQueryable<fnProviderClientsCount> fn_ProviderClientsCount(Nullable<long> fromDate, Nullable<long> toDate)
        {
            var fromDateParameter = fromDate.HasValue ?
                new ObjectParameter("FromDate", fromDate) :
                new ObjectParameter("FromDate", typeof(long));
    
            var toDateParameter = toDate.HasValue ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(long));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnProviderClientsCount>("[IqSoftDataWarehouseEntities].[fn_ProviderClientsCount](@FromDate, @ToDate)", fromDateParameter, toDateParameter);
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_DeviceClientsCount")]
        public virtual IQueryable<fnDeviceClientsCount> fn_DeviceClientsCount(Nullable<long> fromDate, Nullable<long> toDate)
        {
            var fromDateParameter = fromDate.HasValue ?
                new ObjectParameter("FromDate", fromDate) :
                new ObjectParameter("FromDate", typeof(long));
    
            var toDateParameter = toDate.HasValue ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(long));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnDeviceClientsCount>("[IqSoftDataWarehouseEntities].[fn_DeviceClientsCount](@FromDate, @ToDate)", fromDateParameter, toDateParameter);
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_PartnerClientsCount")]
        public virtual IQueryable<fnPartnerClientsCount> fn_PartnerClientsCount(Nullable<long> fromDate, Nullable<long> toDate)
        {
            var fromDateParameter = fromDate.HasValue ?
                new ObjectParameter("FromDate", fromDate) :
                new ObjectParameter("FromDate", typeof(long));
    
            var toDateParameter = toDate.HasValue ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(long));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnPartnerClientsCount>("[IqSoftDataWarehouseEntities].[fn_PartnerClientsCount](@FromDate, @ToDate)", fromDateParameter, toDateParameter);
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_PartnerDeviceBets")]
        public virtual IQueryable<fnPartnerDeviceBets> fn_PartnerDeviceBets(Nullable<long> fromDate, Nullable<long> toDate)
        {
            var fromDateParameter = fromDate.HasValue ?
                new ObjectParameter("FromDate", fromDate) :
                new ObjectParameter("FromDate", typeof(long));
    
            var toDateParameter = toDate.HasValue ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(long));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnPartnerDeviceBets>("[IqSoftDataWarehouseEntities].[fn_PartnerDeviceBets](@FromDate, @ToDate)", fromDateParameter, toDateParameter);
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_PartnerProviderBets")]
        public virtual IQueryable<fnPartnerProviderBets> fn_PartnerProviderBets(Nullable<long> fromDate, Nullable<long> toDate)
        {
            var fromDateParameter = fromDate.HasValue ?
                new ObjectParameter("FromDate", fromDate) :
                new ObjectParameter("FromDate", typeof(long));
    
            var toDateParameter = toDate.HasValue ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(long));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnPartnerProviderBets>("[IqSoftDataWarehouseEntities].[fn_PartnerProviderBets](@FromDate, @ToDate)", fromDateParameter, toDateParameter);
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_InternetBet")]
        public virtual IQueryable<fnInternetBet> fn_InternetBet()
        {
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnInternetBet>("[IqSoftDataWarehouseEntities].[fn_InternetBet]()");
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_ClientBets")]
        public virtual IQueryable<fnClientBets> fn_ClientBets(Nullable<long> fromDate, Nullable<long> toDate)
        {
            var fromDateParameter = fromDate.HasValue ?
                new ObjectParameter("FromDate", fromDate) :
                new ObjectParameter("FromDate", typeof(long));
    
            var toDateParameter = toDate.HasValue ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(long));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnClientBets>("[IqSoftDataWarehouseEntities].[fn_ClientBets](@FromDate, @ToDate)", fromDateParameter, toDateParameter);
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_AgentProfit")]
        public virtual IQueryable<fnAgentProfit> fn_AgentProfit(Nullable<long> fromDate, Nullable<long> toDate)
        {
            var fromDateParameter = fromDate.HasValue ?
                new ObjectParameter("FromDate", fromDate) :
                new ObjectParameter("FromDate", typeof(long));
    
            var toDateParameter = toDate.HasValue ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(long));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnAgentProfit>("[IqSoftDataWarehouseEntities].[fn_AgentProfit](@FromDate, @ToDate)", fromDateParameter, toDateParameter);
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_ProductCommission")]
        public virtual IQueryable<fnProductCommission> fn_ProductCommission(Nullable<int> productId, Nullable<int> agentId)
        {
            var productIdParameter = productId.HasValue ?
                new ObjectParameter("productId", productId) :
                new ObjectParameter("productId", typeof(int));
    
            var agentIdParameter = agentId.HasValue ?
                new ObjectParameter("agentId", agentId) :
                new ObjectParameter("agentId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnProductCommission>("[IqSoftDataWarehouseEntities].[fn_ProductCommission](@productId, @agentId)", productIdParameter, agentIdParameter);
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_ProductGroup")]
        public virtual IQueryable<fnProductGroup> fn_ProductGroup(Nullable<int> productId)
        {
            var productIdParameter = productId.HasValue ?
                new ObjectParameter("productId", productId) :
                new ObjectParameter("productId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnProductGroup>("[IqSoftDataWarehouseEntities].[fn_ProductGroup](@productId)", productIdParameter);
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_ProfitByClientProduct")]
        public virtual IQueryable<fnProfitByClientProduct> fn_ProfitByClientProduct(Nullable<long> fromDate, Nullable<long> toDate)
        {
            var fromDateParameter = fromDate.HasValue ?
                new ObjectParameter("FromDate", fromDate) :
                new ObjectParameter("FromDate", typeof(long));
    
            var toDateParameter = toDate.HasValue ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(long));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnProfitByClientProduct>("[IqSoftDataWarehouseEntities].[fn_ProfitByClientProduct](@FromDate, @ToDate)", fromDateParameter, toDateParameter);
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_ClientSession")]
        public virtual IQueryable<fnClientSession> fn_ClientSession()
        {
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnClientSession>("[IqSoftDataWarehouseEntities].[fn_ClientSession]()");
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_DuplicateClient")]
        public virtual IQueryable<fnDuplicateClient> fn_DuplicateClient(Nullable<long> fromDate, Nullable<long> toDate)
        {
            var fromDateParameter = fromDate.HasValue ?
                new ObjectParameter("FromDate", fromDate) :
                new ObjectParameter("FromDate", typeof(long));
    
            var toDateParameter = toDate.HasValue ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(long));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnDuplicateClient>("[IqSoftDataWarehouseEntities].[fn_DuplicateClient](@FromDate, @ToDate)", fromDateParameter, toDateParameter);
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_AffiliateCorrection")]
        public virtual IQueryable<fnAffiliateCorrection> fn_AffiliateCorrection()
        {
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnAffiliateCorrection>("[IqSoftDataWarehouseEntities].[fn_AffiliateCorrection]()");
        }
    
        [DbFunction("IqSoftDataWarehouseEntities", "fn_ClientReport")]
        public virtual IQueryable<fnClientReport> fn_ClientReport(Nullable<long> fromDate, Nullable<long> toDate)
        {
            var fromDateParameter = fromDate.HasValue ?
                new ObjectParameter("FromDate", fromDate) :
                new ObjectParameter("FromDate", typeof(long));
    
            var toDateParameter = toDate.HasValue ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(long));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnClientReport>("[IqSoftDataWarehouseEntities].[fn_ClientReport](@FromDate, @ToDate)", fromDateParameter, toDateParameter);
        }
    }
}
