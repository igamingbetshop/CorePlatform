namespace IqSoft.CP.ProductGateway.Models.Kiron
{
    public class GetBalanceInput :BaseInput
    {
        public string PlayerID { get; set; }
        public string CurrencyCode { get; set; }

    }
}