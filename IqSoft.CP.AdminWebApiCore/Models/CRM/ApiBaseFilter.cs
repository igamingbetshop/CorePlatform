using IqSoft.CP.AdminWebApi.Filters;
using System;

namespace IqSoft.CP.AdminWebApi.Models.CRM
{
    public class ApiBaseFilter : ApiFilterBase
    {
        public int? Id { get; set; }
        public int? PartnerId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}