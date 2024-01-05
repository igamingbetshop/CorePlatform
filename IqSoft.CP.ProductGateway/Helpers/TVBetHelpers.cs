using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public class TVBetHelpers
    {
        public enum TransactionTypes
        {
            Bet = -1,
            Win = 1,
            Refund = 2,
            RefundWin = -2,
            JackpotPayout = 4
        }

        public static class ErrorCodes
        {
            public const int Success = 0;
            public const int InvalidSignature = 1;
            public const int TimeOut = 2;
            public const int ClientNotFound = 3;
            public const int SessionNotFound = 5;
            public const int SessionExpired = 6;
            public const int WrongAmount = 7;
            public const int LowBalance = 8;
            public const int ClientBlockeed = 11;
            public const int TransactionAlreadyExists = 12;
            public const int TransactionRollbacked = 13;
            public const int InvalidTypeOfTransaction = 14;
            public const int InternalError = 1000;
        }

        private readonly static Dictionary<int, int> Errors = new Dictionary<int, int>
        {
            {Constants.Errors.WrongHash, ErrorCodes.InvalidSignature },
            {Constants.Errors.ClientNotFound, ErrorCodes.ClientNotFound},
            {Constants.Errors.SessionExpired, ErrorCodes.SessionExpired },
            {Constants.Errors.SessionNotFound, ErrorCodes.SessionNotFound},
            {Constants.Errors.TransactionAlreadyExists, ErrorCodes.TransactionAlreadyExists},
            {Constants.Errors.DocumentAlreadyRollbacked, ErrorCodes.TransactionRollbacked},
            {Constants.Errors.GeneralException,ErrorCodes.InternalError},
        };

        public static int GetError(int error)
        {
            if (Errors.ContainsKey(error))
                return Errors[error];
            return ErrorCodes.InternalError;
        }
    }
}