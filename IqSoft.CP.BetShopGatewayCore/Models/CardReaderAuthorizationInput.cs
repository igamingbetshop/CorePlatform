namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class CardReaderAuthorizationInput
    {
        public string CashDeskData { get; set; }
        public int CashDeskId { get; set; }
        public CardReaderData Data { get; set; }
    }

    public class CardReaderData
    {
        public string MacAddress { get; set; }
        public long Date { get; set; }
        public string Password { get; set; }
    }
}