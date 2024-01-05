using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class BetSolutionsHelpers
    {
        public enum StatusCodes
        {
            Success = 200,
            TransactionAlreadyProcessed = 201, // win, CancelBet
            InactiveToken = 401,
            InsufficientBalance = 402,
            InvalidHash = 403,
            InvalidToken = 404,
            TransferLimit = 405,
            UserNotFound = 406,
            InvalidAmount = 407,
            DuplicatedTransactionId = 408, // bet
            InvalidCurrency = 409,
            InvalidRequest = 411,
            InvalidIp = 412,
            GeneralError = 500
        }

        private readonly static Dictionary<int, int> Error = new Dictionary<int, int>
        {
            {Constants.Errors.SessionExpired,  (int)StatusCodes.InactiveToken},
            {Constants.Errors.SessionNotFound,  (int)StatusCodes.InvalidToken},
            {Constants.Errors.WrongCurrencyId,  (int)StatusCodes.InvalidCurrency},
            {Constants.Errors.LowBalance,  (int)StatusCodes.InsufficientBalance},
            {Constants.Errors.MaxLimitExceeded,  (int)StatusCodes.TransferLimit},
            {Constants.Errors.ClientDocumentAlreadyExists,  (int)StatusCodes.DuplicatedTransactionId},
            {Constants.Errors.DocumentAlreadyRollbacked,  (int)StatusCodes.TransactionAlreadyProcessed},
            {Constants.Errors.DocumentAlreadyWinned,  (int)StatusCodes.TransactionAlreadyProcessed},
            {Constants.Errors. ClientNotFound,  (int)StatusCodes.UserNotFound},
            {Constants.Errors. WrongHash,  (int)StatusCodes.InvalidHash},
            {Constants.Errors.WrongOperationAmount,  (int)StatusCodes.InvalidAmount},
            {Constants.Errors.WrongInputParameters ,  (int)StatusCodes.InvalidRequest}
        };

        public static int GetErrorCode(int errorId)
        {
            if (Error.ContainsKey(errorId))
                return Error[errorId];
            return (int)StatusCodes.GeneralError;
        }

        public enum TransactionsTypes
        {
            Normal = 1,
            FreeSpin = 2,
            ReSpin = 3,
            Bonus = 7,
            LockBalance = 16,
            UnlockBalance = 17,
            TournamentBuyIn = 18,
            CancelTournamentBuyIn = 19,
            TournamentWin = 20,
            TournamentBounty = 21,
            AchievementClaim = 26,
            CancelBet = 41,
            Jackpot = 42,
            Drop = 46,
            DropWin = 47,
        }


    }
}