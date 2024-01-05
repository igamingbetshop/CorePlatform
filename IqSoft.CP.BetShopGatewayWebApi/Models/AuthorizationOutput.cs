using System.Collections.Generic;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class AuthorizationOutput : ApiResponseBase
    {
        public int CashierId { get; set; }

        public string CashierFirstName { get; set; }

        public string CashierLastName { get; set; }

        public int CashDeskId { get; set; }

        public string CashDeskName { get; set; }

        public int PartnerId { get; set; }

        public string BetShopCurrencyId { get; set; }

        public int BetShopId { get; set; }

        public string BetShopName { get; set; }

        public string BetShopAddress { get; set; }

        public string Token { get; set; }

        public decimal Balance { get; set; }

        public decimal CurrentLimit { get; set; }

		public bool PrintLogo { get; set; }

        public bool? AnonymousBet { get; set; }

        public bool? AllowCashout { get; set; }
        
        public bool? AllowLive { get; set; }
        
        public bool? UsePin { get; set; }

        public int? Restrictions { get; set; }

        public List<ApiPaymentSystem> PaymentSystems { get; set; }
    }
}