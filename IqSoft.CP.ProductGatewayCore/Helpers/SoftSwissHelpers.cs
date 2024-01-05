using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public class SoftSwissHelpers
    {
        public static class ErrorCodes
        {
            public const int InsufficientFunds = 100;
            public const int PlayerNotFound = 101;
            public const int ClientMaxLimitExceeded = 105;
            public const int PartnerProductLimitExceeded = 106;
            public const int GameBlocked = 107;
            public const int PlayerBlocked = 10;
            public const int GameBlockedFor = 153;
            public const int WrongCurrency = 154;
            public const int WrongHash = 403;
            public const int InternalServerError = 500;
        }

        private readonly static Dictionary<int, int> Error = new Dictionary<int, int>
        {
            {Constants.Errors.LowBalance,  ErrorCodes.InsufficientFunds},
            {Constants.Errors.ClientNotFound,  ErrorCodes.PlayerNotFound},
            {Constants.Errors.ClientMaxLimitExceeded,  ErrorCodes.ClientMaxLimitExceeded},
            {Constants.Errors.ProductNotAllowedForThisPartner,  ErrorCodes.PartnerProductLimitExceeded},
            {Constants.Errors.ProductNotFound,  ErrorCodes.GameBlocked},
            {Constants.Errors.ClientBlocked,  ErrorCodes.PlayerBlocked},
            {Constants.Errors.WrongCurrencyId,  ErrorCodes.WrongCurrency},
            {Constants.Errors.WrongHash,  ErrorCodes.WrongHash}
        };

        public static int GetErrorCode(int errorId)
        {
            if (Error.ContainsKey(errorId))
                return Error[errorId];
            return ErrorCodes.InternalServerError;
        }
    }
}