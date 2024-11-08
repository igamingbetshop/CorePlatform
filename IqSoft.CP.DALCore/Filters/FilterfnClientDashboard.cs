﻿using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterfnClientDashboard : FilterBase<fnClientReport>
    {
        public int? PartnerId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public FiltersOperation ClientIds { get; set; }
        public FiltersOperation UserNames { get; set; }
        public FiltersOperation PartnerIds { get; set; }
        public FiltersOperation CurrencyIds { get; set; }
        public FiltersOperation FirstNames { get; set; }
        public FiltersOperation LastNames { get; set; }
        public FiltersOperation AffiliatePlatformIds { get; set; }
        public FiltersOperation AffiliateIds { get; set; }
        public FiltersOperation AffiliateReferralIds { get; set; }
        public FiltersOperation TotalWithdrawalAmounts { get; set; }
        public FiltersOperation WithdrawalsCounts { get; set; }
        public FiltersOperation TotalDepositAmounts { get; set; }
        public FiltersOperation DepositsCounts { get; set; }
        public FiltersOperation TotalBetAmounts { get; set; }
        public FiltersOperation BetsCounts { get; set; }
        public FiltersOperation TotalWinAmounts { get; set; }
        public FiltersOperation WinsCounts { get; set; }
        public FiltersOperation GGRs { get; set; }
        public FiltersOperation TotalDebitCorrections { get; set; }
        public FiltersOperation DebitCorrectionsCounts { get; set; }
        public FiltersOperation TotalCreditCorrections { get; set; }
        public FiltersOperation CreditCorrectionsCounts { get; set; }

        protected override IQueryable<fnClientReport> CreateQuery(IQueryable<fnClientReport> objects, Func<IQueryable<fnClientReport>, IOrderedQueryable<fnClientReport>> orderBy = null)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);


            FilterByValue(ref objects, ClientIds, "ClientId");
            FilterByValue(ref objects, UserNames, "UserName");
            FilterByValue(ref objects, CurrencyIds, "CurrencyId");
            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, FirstNames, "FirstName");
            FilterByValue(ref objects, LastNames, "LastName");
            FilterByValue(ref objects, AffiliatePlatformIds, "AffiliatePlatformId");
            FilterByValue(ref objects, AffiliateIds, "AffiliateId");
            FilterByValue(ref objects, AffiliateReferralIds, "AffiliateReferralId");
            FilterByValue(ref objects, TotalWithdrawalAmounts, "TotalWithdrawalAmount");
            FilterByValue(ref objects, WithdrawalsCounts, "WithdrawalsCount");
            FilterByValue(ref objects, TotalDepositAmounts, "TotalDepositAmount");
            FilterByValue(ref objects, DepositsCounts, "DepositsCount");
            FilterByValue(ref objects, TotalBetAmounts, "TotalBetAmount");
            FilterByValue(ref objects, BetsCounts, "BetsCount");
            FilterByValue(ref objects, TotalWinAmounts, "TotalWinAmount");
            FilterByValue(ref objects, WinsCounts, "WinsCount");
            FilterByValue(ref objects, GGRs, "GGR");
            FilterByValue(ref objects, TotalDebitCorrections, "TotalDebitCorrection");
            FilterByValue(ref objects, CreditCorrectionsCounts, "CreditCorrectionsCount");
            FilterByValue(ref objects, TotalCreditCorrections, "TotalCreditCorrection");
            FilterByValue(ref objects, DebitCorrectionsCounts, "DebitCorrectionsCount");
            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnClientReport> FilterObjects(IQueryable<fnClientReport> clients, Func<IQueryable<fnClientReport>,
            IOrderedQueryable<fnClientReport>> orderBy = null)
        {
            clients = CreateQuery(clients, orderBy);
            return clients;
        }
        public long SelectedObjectsCount(IQueryable<fnClientReport> objects)
        {
            objects = CreateQuery(objects);
            return objects.Count();
        }     
    }
}