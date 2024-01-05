namespace IqSoft.CP.ProductGateway.Models.EveryMatrix
{
    public class BaseInput
    {
        public string ApiVersion { get; set; }
        public string Request { get; set; }
        public string OperatorId { get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }

        public bool ValidateSession { get; set; }
        public string SessionId { get; set; }
        public string ExternalUserId { get; set; }


        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }
        public string TransactionTimestamp { get; set; }
        public string EMGameId { get; set; }
        public string RoundId { get; set; }
        public string RoundStatus { get; set; }
        public bool IsReversal { get; set; }
        public string ReversalTransactionId { get; set; }
        public string RollbackTransactionId { get; set; }
        public AdditionalDataModel AdditionalData { get; set; }
    }

    public class AdditionalDataModel
    {
        public string GameSlug { get; set; }
        public string Language { get; set; }
        public string CasinoGameId { get; set; }
        public string GameName { get; set; }
        public string GameCode { get; set; }
        public string ReportCategory { get; set; }
        public string GameModel { get; set; }
        public string Vendor { get; set; }
        public string PlayerUserName { get; set; }
        public string PlayerCountry { get; set; }
        public string PlayerCurrency { get; set; }
        public string TransactionInsertTime { get; set; }
    }
}