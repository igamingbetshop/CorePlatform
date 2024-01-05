using System;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class SettingModel
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string Name { get; set; }
        public Nullable<decimal> NumericValue { get; set; }
        public string StringValue { get; set; }
        public Nullable<System.DateTime> DateValue { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<System.DateTime> CreationTime { get; set; }
        public Nullable<System.DateTime> LastUpdateTime { get; set; }
    }
}
