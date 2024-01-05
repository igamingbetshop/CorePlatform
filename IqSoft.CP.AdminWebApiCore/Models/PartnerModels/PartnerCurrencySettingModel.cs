namespace IqSoft.CP.AdminWebApi.Models.PartnerModels
{
    public class PartnerCurrencySettingModel
    {
        public int Id { get; set; }
        
        public int PartnerId { get; set; }
        
        public string CurrencyId { get; set; }
        
        public int State { get; set; }
        public decimal? UserMinLimit { get; set; }
        public decimal? UserMaxLimit { get; set; }   
        public decimal? ClientMinBet { get; set; }
        public System.DateTime CreationTime { get; set; }
        
        public System.DateTime LastUpdateTime { get; set; }

        public int? Priority { get; set; }
    }
}