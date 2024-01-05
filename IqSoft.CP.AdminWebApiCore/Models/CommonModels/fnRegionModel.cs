namespace IqSoft.CP.AdminWebApi.Models.CommonModels
{
    public class FnRegionModel
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int TypeId { get; set; }
        public string Name { get; set; }
        public string NickName { get; set; }
        public string IsoCode { get; set; }
        public string IsoCode3 { get; set; }
        public int State { get; set; }
        public string CurrencyId { get; set; }
        public string LanguageId { get; set; }
        public string Info { get; set; }
    }
}