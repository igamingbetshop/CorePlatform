using IqSoft.CP.DAL.Models.Integration.ProductsIntegration;

namespace IqSoft.CP.ProductGateway.Models.IqSoft
{
    public class ApiFinOperationInput : InputBase
    {
        public string CurrencyId { get; set; }

        public string RoundId { get; set; }

        public string GameId { get; set; }

        public int? UnitId { get; set; }

        public string Info { get; set; }

        public int? OperationTypeId { get; set; }

        public string TransactionId { get; set; }
        public string RollbackTransactionId { get; set; }

        public string CreditTransactionId { get; set; }

        public int? DeviceTypeId { get; set; }

        public string BetId { get; set; }

        public int? TypeId { get; set; }

        public int? BetState { get; set; }

        public decimal? PossibleWin { get; set; }

        public int ClientId { get; set; }

		public string UserName { get; set; }

		public int? CashDeskId { get; set; }

        public string Token { get; set; }

        public decimal Amount { get; set; }

        public int Type { get; set; }

        public int BonusId { get; set; }

        public int? SelectionsCount { get; set; }

        public bool? IsFreeBet { get; set; }
    }
}