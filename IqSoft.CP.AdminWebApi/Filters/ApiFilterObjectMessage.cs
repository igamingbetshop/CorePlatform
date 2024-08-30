using IqSoft.CP.Common.Models.Filters;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterObjectMessage : ApiFilterBase
    {
        public int ObjectTypeId { get; set; }     
        public DateTime? FromDate { get; set; }     
        public DateTime? ToDate { get; set; }
        public ApiFiltersOperation PartnerIds { get; set; }
        public ApiFiltersOperation MessageIds { get; set; }
        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation UserNames { get; set; }
        public ApiFiltersOperation MobileOrEmails { get; set; }
        public ApiFiltersOperation Subjects { get; set; }
        public ApiFiltersOperation Messages { get; set; }
        public ApiFiltersOperation MessageTypes { get; set; }
        public ApiFiltersOperation Statuses { get; set; }
        public ApiFiltersOperation CreationTimes { get; set; }
    }
}