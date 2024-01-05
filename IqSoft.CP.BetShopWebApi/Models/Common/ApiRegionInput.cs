namespace IqSoft.CP.BetShopWebApi.Models.Common
{
    public class ApiRegionInput : PlatformRequestBase
    {
        public int? ParentId { get; set; }

        public int TypeId { get; set; }
    }
}