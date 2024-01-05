namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetProductSessionInput
    {
        public string Token { get; set; }

        public int ProductId { get; set; }

        public int? ClientId { get; set; }

        public int? DeviceType { get; set; }
    }
}