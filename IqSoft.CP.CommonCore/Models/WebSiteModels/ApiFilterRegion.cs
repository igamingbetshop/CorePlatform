namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ApiFilterRegion : ApiRequestBase
    {
        public int? ParentId { get; set; }

        public int TypeId { get; set; }
    }
}