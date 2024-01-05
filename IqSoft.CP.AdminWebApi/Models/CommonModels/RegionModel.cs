namespace IqSoft.CP.AdminWebApi.Models.CommonModels
{
    public class RegionModel
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public int TypeId { get; set; }

        public long TranslationId { get; set; }

        public string Path { get; set; }

        public string IsoCode { get; set; }
        public string IsoCode3 { get; set; }
    }
}