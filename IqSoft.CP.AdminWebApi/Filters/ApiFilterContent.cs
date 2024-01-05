namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterContent : ApiFilterBase
    {
        public int? Id { get; set; }
        public int? PartnerId { get; set; }
        public int? ParentId { get; set; }
        public int? DeviceType { get; set; }
    }
}