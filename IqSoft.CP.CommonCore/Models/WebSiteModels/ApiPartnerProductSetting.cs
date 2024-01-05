namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ApiPartnerProductSetting
    {
        public int Id { get; set; }

        public int PartnerId { get; set; }

        public int ProductId { get; set; }

        public decimal Percent { get; set; }

        public int State { get; set; }

        public decimal? Rating { get; set; }
        public bool? HasDemo { get; set; }
    }
}
