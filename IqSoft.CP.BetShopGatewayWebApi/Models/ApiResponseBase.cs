namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class ApiResponseBase
    {
        public int ResponseCode { get; set; }
        public object ResponseObject { get; set; }
        public string Description { get; set; }
    }
}