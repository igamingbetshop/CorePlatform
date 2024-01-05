namespace IqSoft.CP.Common.Models.WebSiteModels.Products
{
    public class GetProductUrlInput : ApiRequestBase
    {
        public int ClientId { get; set; }
        public string Token { get; set; }
        public string Position { get; set; }
        public int? DeviceType { get; set; }
        public bool IsForDemo { get; set; }
        public bool? IsForMobile { get; set; }
        public int ProductId { get; set; }
        public int ProviderId { get; set; }
        public string MethodName { get; set; }
    }
}