using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class BetsyHelpers
    {
        public enum TransactionTypes
        {
            Bet = 1,
            Settle = 2,
            Rollback = 3
        }

        public enum StatusCodes
        {
            WrongHash = 1,
            ClientNotFound = 2,
            SessionNotFound = 3,
            SessionExpired = 4,
            WongAmount = 6,
            WrongInputParameters = 5,
            WrongBetAmount = 7,
            InsufficientFunds = 8,
            BetAlreadyProccessed = 9,
            ClientBlocked = 10,
            LimitExceeded = 11,
            GeneralError = 100
        }

        private readonly static Dictionary<int, int> Error = new Dictionary<int, int>
        {
            {Constants.Errors.WrongHash,  (int)StatusCodes.WrongHash},
            {Constants.Errors.WrongToken,  (int)StatusCodes.SessionNotFound},
            {Constants.Errors.ClientNotFound,  (int)StatusCodes.ClientNotFound},
            {Constants.Errors.SessionNotFound,  (int)StatusCodes.SessionNotFound},
            {Constants.Errors.SessionExpired,  (int)StatusCodes.SessionExpired},
            {Constants.Errors.WrongOperationAmount,  (int)StatusCodes.WongAmount},
            {Constants.Errors.BetAfterRollback,  (int)StatusCodes.BetAlreadyProccessed},
            {Constants.Errors.WrongInputParameters ,  (int)StatusCodes.WrongInputParameters},
            {Constants.Errors.LowBalance,  (int)StatusCodes.WrongBetAmount},
            {Constants.Errors.ClientDocumentAlreadyExists,  (int)StatusCodes.BetAlreadyProccessed},
            {Constants.Errors.TransactionAlreadyExists,  (int)StatusCodes.BetAlreadyProccessed},
            {Constants.Errors.ClientBlocked,  (int)StatusCodes.ClientBlocked},
            {Constants.Errors.MaxLimitExceeded,  (int)StatusCodes.LimitExceeded}
        };

        public static int GetErrorCode(int errorId)
        {
            if (Error.ContainsKey(errorId))
                return Error[errorId];
            return (int)StatusCodes.GeneralError;
        }
    }
}