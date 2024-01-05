using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Report
{
    public class ObjectChangeHistoryItem
    {
        public long Id { get; set; }
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public string Object { get; set; }
        public Nullable<long> SessionId { get; set; }
        public DateTime ChangeDate { get; set; }
        public string Comment { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> ObjectChangedItems { get; set; }
    }
}