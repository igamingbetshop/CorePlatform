namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetPartnerProductsInput : ApiRequestBase
    {
        public int RootProductId { get; set; }

		public bool IsForDesktop { get; set; }
    }
}