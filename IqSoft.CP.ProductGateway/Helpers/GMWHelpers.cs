using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class GMWHelpers
    {
        public static class TransactionTypes
        {
            public readonly static string Bet = "DEBIT";
            public readonly static string Win = "CREDIT";
        }
        public static class ErrorCodes
        {
            public const int GeneralError = 1;
            public const int WrongRequest = 2;
            public const int AuthenticationFailed = 100;
            public const int InsufficientFunds = 101;
            public const int ClientDocumentAlreadyExists = 301;
        }

        private readonly static Dictionary<int, int> Error = new Dictionary<int, int>
        {
            {Constants.Errors.SessionExpired,  ErrorCodes.AuthenticationFailed},
            {Constants.Errors.SessionNotFound,  ErrorCodes.AuthenticationFailed},
            {Constants.Errors.WrongCurrencyId,  ErrorCodes.WrongRequest},
            {Constants.Errors.LowBalance,  ErrorCodes.InsufficientFunds},
            {Constants.Errors.ClientDocumentAlreadyExists,  ErrorCodes.ClientDocumentAlreadyExists},
            {Constants.Errors.DocumentAlreadyRollbacked,  ErrorCodes.ClientDocumentAlreadyExists},
            {Constants.Errors.DocumentAlreadyWinned,  ErrorCodes.ClientDocumentAlreadyExists},
            {Constants.Errors.CanNotConnectCreditAndDebit, ErrorCodes.WrongRequest}
        };

        public static int GetErrorCode(int errorId)
        {
            if (Error.ContainsKey(errorId))
                return Error[errorId];
            return ErrorCodes.GeneralError;
        }
    }
}