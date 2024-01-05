using System;
namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class AuthorizationBase
    {
        public string CashDeskData { get; set; }
        public int CashDeskId { get; set; }
        public CashDeskData Data { get; set; }
    }

    public class CashDeskData
    {
        public string MacAddress { get; set; }
        public DateTime Date { get; set; }
        public string Password { get; set; }
    }
}