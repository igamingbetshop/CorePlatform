using System;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterNote
    {
        public long? Id { get; set; }
        public long? ObjectId { get; set; }
        public int? ObjectTypeId { get; set; }
        public string Message { get; set; }
        public int Type { get; set; }
        public int? State { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}