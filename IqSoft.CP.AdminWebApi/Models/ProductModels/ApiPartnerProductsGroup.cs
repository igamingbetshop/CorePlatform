namespace IqSoft.CP.AdminWebApi.Models.ProductModels
{
    public class ApiPartnerProductsGroup
    {
        public int Id { get; set; }

        public int PartnerId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public int GameProviderId { get; set; }

        public int Type { get; set; }
    }
}