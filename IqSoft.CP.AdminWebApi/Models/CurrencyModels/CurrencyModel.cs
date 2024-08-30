namespace IqSoft.CP.AdminWebApi.Models.CurrencyModels
{
    public class CurrencyModel
    {
        public string Id { get; set; }
        public decimal CurrentRate { get; set; }
        public string Symbol { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
    }
}