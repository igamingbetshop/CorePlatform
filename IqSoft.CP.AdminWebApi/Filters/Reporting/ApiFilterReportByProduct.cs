﻿using IqSoft.CP.Common.Models.Filters;
using System;

namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
    public class ApiFilterReportByProduct : ApiFilterBase
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public ApiFiltersOperation ClientIds { get; set; }

        public ApiFiltersOperation ClientNames { get; set; }

        public ApiFiltersOperation Currencies { get; set; }

        public ApiFiltersOperation ProductIds { get; set; }

        public ApiFiltersOperation ProductNames { get; set; }

        public ApiFiltersOperation DeviceTypeIds { get; set; }

        public ApiFiltersOperation ProviderNames { get; set; }

        public ApiFiltersOperation TotalBetsAmounts { get; set; }

        public ApiFiltersOperation TotalWinsAmounts { get; set; }

        public ApiFiltersOperation TotalBetsCounts { get; set; }

        public ApiFiltersOperation TotalUncalculatedBetsCounts { get; set; }

        public ApiFiltersOperation TotalUncalculatedBetsAmounts { get; set; }

        public ApiFiltersOperation GGRs { get; set; }
    }
}