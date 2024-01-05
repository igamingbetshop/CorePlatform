using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public class GanapatiHelpers
    {
        public static class ErrorCodes
        {
            public const int InternalServerError = 101;
            public const int InvalidInput = 102;
            public const int SessionExpired = 103;
            public const int TransactionDeclined = 200;
            public const int InsufficientFunds = 201;
            public const int LimitExceeded = 202;
        }

        private readonly static Dictionary<int, int> Error = new Dictionary<int, int>
        {
            {Constants.Errors.SessionExpired,  ErrorCodes.SessionExpired},
            {Constants.Errors.SessionNotFound, ErrorCodes.InvalidInput},
            {Constants.Errors.ClientNotFound, ErrorCodes.InvalidInput},
            {Constants.Errors.LowBalance,  ErrorCodes.InsufficientFunds},
            {Constants.Errors.DocumentNotFound, ErrorCodes.TransactionDeclined},
            {Constants.Errors.CanNotConnectCreditAndDebit, ErrorCodes.TransactionDeclined}
        };

        public static int GetErrorCode(int errorId)
        {
            if (Error.ContainsKey(errorId))
                return Error[errorId];
            return ErrorCodes.InternalServerError;
        }
    }
}