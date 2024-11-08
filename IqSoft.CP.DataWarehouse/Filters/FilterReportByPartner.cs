﻿using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DataWarehouse.Filters
{
    public class FilterReportByPartner : FilterBase<fnReportByPartner>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public FiltersOperation PartnerIds { get; set; }
        public FiltersOperation PartnerNames { get; set; }
        public FiltersOperation TotalBetAmounts { get; set; }
        public FiltersOperation TotalBetsCounts { get; set; }
        public FiltersOperation TotalWinAmounts { get; set; }
        public FiltersOperation TotalGGRs { get; set; }

        public override void CreateQuery(ref IQueryable<fnReportByPartner> objects, bool orderBy, bool orderByDate = false)
        {

            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, PartnerNames, "PartnerName");
            FilterByValue(ref objects, TotalBetAmounts, "TotalBetAmount");
            FilterByValue(ref objects, TotalWinAmounts, "TotalWinAmount");
            FilterByValue(ref objects, TotalBetsCounts, "TotalBetsCount");
            FilterByValue(ref objects, TotalGGRs, "TotalGGR");
            base.FilteredObjects(ref objects, orderBy, orderByDate, null);
        }

        public IQueryable<fnReportByPartner> FilterObjects(IQueryable<fnReportByPartner> objects, bool ordering)
        {
            CreateQuery(ref objects, ordering);
            return objects;
        }
    }
}
