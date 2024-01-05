using System;

namespace IqSoft.CP.AdminWebApi.Models.ReportModels
{
    public class ApiReportByObjectChangeHistory
    {
        public long Id { get; set; }
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public string Object { get; set; }
        public string Comment { get; set; }
        public DateTime ChangeDate { get; set; }
        public int? PartnerId { get; set; }
        public int? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

    }
}