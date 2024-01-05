using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class DriveMediaHelpers
    {
        public static class Methods
        {
            public const string ListGames = "ListGames";
            public const string OpenGame = "OpenGame";
            public const string GetBalance = "GetBalance";
            public const string WriteBet = "WriteBet";
        }

        public static class BetInfo
        {
            public const string Bet = "bet";
            public const string Win = "win";
            public const string Rollback = "CancelTransaction";
            public const string SpinomenalFreeSpin = "Free_InstantWin";
            public const string Jackpot = "jackpot";
        }

        public static class Statuses
        {
            public const string Success = "success";
            public const string Fail = "fail";
        }

        private static class Error
        {
            public const string MethodNotFound = "Error connection to method";
            public const string UserNotFound = "user_not_found";
            public const string ErrorSign = "error_sign";
            public const string InternalError = "internal_error";
            public const string GameNotFound = "game_not_found";
            public const string RepeatOperation = "repeat_operation";
            public const string BetNotFound = "bet_not_found";
            public const string ErrorBalance = "error_balance";
        }

        private readonly static Dictionary<int, string> Errors = new Dictionary<int, string>
        {
            {Constants.Errors.MethodNotFound, Error.MethodNotFound },
            {Constants.Errors.ClientNotFound, Error.UserNotFound },
            {Constants.Errors.WrongHash, Error.ErrorSign },
            {Constants.Errors.GeneralException, Error.InternalError },
            {Constants.Errors.ProductNotFound, Error.GameNotFound },
            {Constants.Errors.TransactionAlreadyExists, Error.RepeatOperation },
            {Constants.Errors.DocumentNotFound, Error.BetNotFound },
            {Constants.Errors.LowBalance, Error.ErrorBalance }
        };

        public static string GetError(int error)
        {
            if (Errors.ContainsKey(error))
                return Errors[error];
            return Errors[Constants.Errors.GeneralException];
        }
    }
}