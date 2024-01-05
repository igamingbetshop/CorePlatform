using System;
namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
    public class ApiFilterReportByUserTransaction : ApiFilterBase
    {
        public int? PartnerId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public ApiFiltersOperation PartnerIds { get; set; }
        public ApiFiltersOperation UserIds { get; set; }
        public ApiFiltersOperation Usernames { get; set; }
        public ApiFiltersOperation NickNames { get; set; }
        public ApiFiltersOperation UserFirstNames { get; set; }
        public ApiFiltersOperation UserLastNames { get; set; }
        public ApiFiltersOperation FromUserIds { get; set; }
        public ApiFiltersOperation FromUsernames { get; set; }
        public ApiFiltersOperation ClientIds { get; set; }
        public ApiFiltersOperation ClientUsernames { get; set; }
        public ApiFiltersOperation OperationTypeIds { get; set; }
        public ApiFiltersOperation Amounts { get; set; }
        public ApiFiltersOperation CurrencyIds { get; set; }
    }
}