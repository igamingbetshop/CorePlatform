using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public class KironHelpers
    {
        public static class ErrorCodes
        {
            public const int InvalidToken = 100;
            public const int PlayerBlocked = 101;
            public const int InvalidCurrency = 120;
            public const int InsufficientFunds = 121;
            public const int ExceedLimit = 122;
            public const int TransactionAlreadyProcessed = 123;
            public const int OriginalTransactionNotFound = 131;
            public const int InternalServerError = 500;
        }

        private readonly static Dictionary<int, int> Error = new Dictionary<int, int>
        {
            {Constants.Errors.SessionExpired,  ErrorCodes.InvalidToken},
            {Constants.Errors.SessionNotFound,  ErrorCodes.InvalidToken},
            {Constants.Errors.WrongCurrencyId,  ErrorCodes.InvalidCurrency},
            {Constants.Errors.LowBalance,  ErrorCodes.InsufficientFunds},
            {Constants.Errors.MaxLimitExceeded,  ErrorCodes.InvalidToken},
            {Constants.Errors.ClientDocumentAlreadyExists,  ErrorCodes.TransactionAlreadyProcessed},
            {Constants.Errors.DocumentAlreadyRollbacked,  ErrorCodes.TransactionAlreadyProcessed},
            {Constants.Errors.DocumentAlreadyWinned,  ErrorCodes.TransactionAlreadyProcessed},
            {Constants.Errors.CanNotConnectCreditAndDebit, ErrorCodes.OriginalTransactionNotFound},
            {Constants.Errors.ClientBlocked,  ErrorCodes.PlayerBlocked}
        };

        public static int GetErrorCode(int errorId)
        {
            if (Error.ContainsKey(errorId))
                return Error[errorId];
            return ErrorCodes.InternalServerError;
        }
    }
}