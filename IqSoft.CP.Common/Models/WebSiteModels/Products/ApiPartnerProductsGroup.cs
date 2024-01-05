namespace IqSoft.CP.Common.Models.WebSiteModels.Products
{
    public class ApiPartnerProductsGroup
    {
        int Id { get; set; }

        public int PartnerId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public int GameProviderId { get; set; }

        public int Type { get; set; }
    }
}