using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public class PragmaticPlayHelpers
    {
        public static class ErrorCodes
        {
            public const int Success = 0;
            public const int InsufficientFunds = 1;
            public const int PlayerNotFound = 2;
            public const int BetIsNotAllowed = 3;
            public const int InvalidToken = 4;
            public const int WrongHash = 5;
            public const int PlayerBlocked = 6;
            public const int WrongInputParameters = 7;
            public const int GameBlocked = 8;
            public const int MaxLimitExceeded = 50;
            public const int InternalServerError = 120;
        }

        private readonly static Dictionary<int, int> Error = new Dictionary<int, int>
        {
            {Constants.Errors.LowBalance,  ErrorCodes.InsufficientFunds},
            {Constants.Errors.ClientNotFound,  ErrorCodes.PlayerNotFound},
            {Constants.Errors.TokenExpired,  ErrorCodes.InvalidToken},
            {Constants.Errors.SessionExpired,  ErrorCodes.InvalidToken},
            {Constants.Errors.SessionNotFound,  ErrorCodes.InvalidToken},
            {Constants.Errors.WrongHash,  ErrorCodes.WrongHash},
            {Constants.Errors.ClientBlocked,  ErrorCodes.PlayerBlocked},
            {Constants.Errors.WrongProductId,  ErrorCodes.WrongInputParameters},
            {Constants.Errors.WrongInputParameters,  ErrorCodes.WrongInputParameters},
            {Constants.Errors.ClientMaxLimitExceeded,  ErrorCodes.MaxLimitExceeded},
            {Constants.Errors.ProductNotAllowedForThisPartner,  ErrorCodes.GameBlocked},
            {Constants.Errors.ProductBlockedForThisPartner,  ErrorCodes.GameBlocked},
            {Constants.Errors.PartnerProductSettingNotFound,  ErrorCodes.GameBlocked}
        };

        public static int GetErrorCode(int errorId)
        {
            if (Error.ContainsKey(errorId))
                return Error[errorId];
            return ErrorCodes.InternalServerError;
        }
    }
}