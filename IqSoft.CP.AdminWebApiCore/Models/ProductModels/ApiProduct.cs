using IqSoft.CP.Common.Models.AdminModels;

namespace IqSoft.CP.AdminWebApi.Models.ProductModels
{
    public class ApiProduct
    {
        public int Id { get; set; }
        public int NewId { get; set; }
        public int? GameProviderId { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public int? CategoryId { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string LanguageId { get; set; }
        public int State { get; set; }
        public bool IsForDesktop { get; set; }
        public bool IsForMobile { get; set; }
        public bool HasDemo { get; set; }
        public string Jackpot { get; set; }
        public bool? FreeSpinSupport { get; set; }
        public int? SubproviderId { get; set; }
        public string WebImageUrl { get; set; }
        public string MobileImageUrl { get; set; }
        public string BackgroundImageUrl { get; set; }
        public string Comment { get; set; }
        public decimal? RTP { get; set; }
        public ApiSetting Countries { get; set; }
    }
}