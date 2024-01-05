using System.Collections.Generic;
using IqSoft.CP.Common;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class TwoWinPowerHelpers
    {
        public static class Action
        {
            public const string Sessions = "Sessions";
            public const string Bet = "bet";
            public const string Refund = "refund";
            public const string Win = "win";
            public const string Balance = "balance";
        }

        public static class ActionTypes
        {
            public const string Bet = "bet";
            public const string Win = "win";
            public const string Freespin = "freespin";
            public const string Jackpot = "jackpot";
        }

        public static class ErrorCodes
        {
            public const string AccessDenied = "ACCESS_DENIED";
            public const string InsufficientFunds = "INSUFFICIENT_FUNDS";
            public const string SessionExpired = "SESSION_EXPIRED";
            public const string SessionNotFound = "SESSION_NOT_FOUND";
            public const string InternalError = "INTERNAL_ERROR";
        }

        private readonly static Dictionary<int,  string> Errors = new Dictionary<int, string>
        {
            {Constants.Errors.WrongHash, ErrorCodes.InternalError },
            {Constants.Errors.LowBalance, ErrorCodes.InsufficientFunds},
            {Constants.Errors.GeneralException,ErrorCodes.InternalError}
        };

        public static string GetError(int error)
        {
            if (Errors.ContainsKey(error))
                return Errors[error];
            return ErrorCodes.InternalError;
        }


    }
}