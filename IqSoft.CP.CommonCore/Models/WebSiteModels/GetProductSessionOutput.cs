namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetProductSessionOutput : ApiResponseBase
    {
        public int? ProductId { get; set; }
        public string ProductToken { get; set; }
        public decimal? Rating { get; set; }
    }
}