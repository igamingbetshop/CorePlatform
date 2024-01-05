namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetProductSessionOutput : ApiResponseBase
    {
        public int ProductId { get; set; }

        public string ProductToken { get; set; }
    }
}