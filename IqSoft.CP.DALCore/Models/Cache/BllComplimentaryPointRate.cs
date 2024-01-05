namespace IqSoft.CP.DAL.Models.Cache
{
    public class BllComplimentaryPointRate
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public int ProductId { get; set; }
        public string CurrencyId { get; set; }
        public decimal Rate { get; set; }
        public System.DateTime CreationDate { get; set; }
        public System.DateTime LastUpdateDate { get; set; }
    }
}