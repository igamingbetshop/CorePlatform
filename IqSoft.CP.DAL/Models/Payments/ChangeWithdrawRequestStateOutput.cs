namespace IqSoft.CP.DAL.Models.Payments
{
    public class ChangeWithdrawRequestStateOutput
    {
        public int ClientId { get; set; }

        public int PartnerId { get; set; }

        public decimal ObjectLimit { get; set; }

        public decimal CashierBalance { get; set; }

        public decimal ClientBalance { get; set; }

        public string CurrencyId { get; set; }

        public long RequestId { get; set; }

        public decimal RequestAmount { get; set; }

        public decimal CommissionAmount { get; set; }

        public string ClientDocumentNumber { get; set; }
        public string ClientUserName { get; set; }

        public int? PartnerPaymentSettingId { get; set; }

		public int PaymentSystemId { get; set; }

		public string Info { get; set; }

		public string ExternalTransactionId { get; set; }

		public int Status { get; set; }

        public string BetShop { get; set; }

        public string CashCode { get; set; }

        public string BetShopAddress { get; set; }
    }
}
