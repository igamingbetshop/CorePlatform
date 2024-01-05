namespace IqSoft.CP.Common.Models.WebSiteModels.Products
{
    public class ApiProductInfoInput : ApiRequestBase
    {
        public int ProductId { get; set; }
        public int? ClientId { get; set; }
        public string Token { get; set; }
    }
}