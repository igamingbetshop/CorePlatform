using System;
using System.Collections.Generic;
using IqSoft.CP.Common;
using static IqSoft.CP.Common.Constants;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class EzugiHelpers
    {
        public static List<string> UnsuppordedCurrenies = new List<string>
        {
            Currencies.KoreanWon,
            Currencies.UzbekistanSom,
            Currencies.MexicanPeso,
            Currencies.IranianTuman,
            Currencies.USDT
        };

        public static class Methods
        {
            public const string Authentication = "Authentication";
            public const string Debit = "Debit";
            public const string Rollback = "Rollback";
            public const string Credit = "Credit";
        }

        public enum ReturnReasonCodes
        {
            SuccessfulBet = 0,
            CancelBet = 1,
            CanceledRound = 2,
            ManualCredit = 7
        }

        public static class ErrorCodes
        {
            public const int Success = 0;
            public const int GeneralError = 1;
            public const int SavedForFutureUse = 2;
            public const int InsufficientFunds = 3;
            public const int InsufficientBehaviorPlayer1 = 4;
            public const int InsufficientBehaviorPlayer2 = 5;
            public const int TokenNotFound = 6;
            public const int UserNotFound = 7;
            public const int UserBlocked = 8;
            public const int TransactionNotFound = 9;
            public const int TransactionTimedOut = 10;
            public const int ReservedForFutureUse = 11;
            public const int BetAfterRollback = 12;
        }

        public static class BetTypeCodes
        {
            //General Bet
            public const string TableBet = "101";
            public const string Insurance = "104";
            public const string Double = "105";
            public const string Split = "106";
            public const string Ante = "107";
            public const string TableBetBB = "116";    //Bet Behind
            public const string SplitBB = "117";
            public const string DoubleBB = "118";
            public const string InsuranceBB = "119";
            //Casino Hold'em
            public const string Call = "124";
        }

        private readonly static Dictionary<int, Tuple<int, string>> Errors = new Dictionary<int, Tuple<int, string>>
        {
            {Constants.SuccessResponseCode, Tuple.Create(ErrorCodes.Success,"Completed successfully") },
            {Constants.Errors.TransactionAlreadyExists, Tuple.Create(ErrorCodes.Success, "Transaction has already processed") },
            {Constants.Errors.DocumentAlreadyRollbacked, Tuple.Create(ErrorCodes.Success, "Transaction has already processed") },
            {Constants.Errors.DocumentAlreadyWinned, Tuple.Create(ErrorCodes.Success, "Transaction has already processed") },
            {Constants.Errors.ClientDocumentAlreadyExists, Tuple.Create(ErrorCodes.GeneralError, "Transaction has already processed") },
            {Constants.Errors.GeneralException, Tuple.Create(ErrorCodes.GeneralError, "General error") },
            {Constants.Errors.BetAfterRollback, Tuple.Create(ErrorCodes.GeneralError, "Debit after rollback") },
            {Constants.Errors.WrongOperationAmount, Tuple.Create(ErrorCodes.GeneralError, "Negative amount") },
            {Constants.Errors.PaymentRequestInValidAmount, Tuple.Create(ErrorCodes.GeneralError, "Wrong amount") },
            {Constants.Errors.WrongProductId, Tuple.Create(ErrorCodes.GeneralError, "Unknown Game") },
            {Constants.Errors.WrongHash, Tuple.Create(ErrorCodes.GeneralError, "OperatorId or Signature Not Match") },
            {Constants.Errors.WrongDocumentNumber, Tuple.Create(ErrorCodes.GeneralError, "Invalid Bet Type") },
            //Saved for future use 
            {Constants.Errors.LowBalance, Tuple.Create(ErrorCodes.InsufficientFunds, "Insufficient funds") },
            {Constants.Errors.TokenExpired, Tuple.Create(ErrorCodes.TokenNotFound, "Token not found")},
            {Constants.Errors.WrongToken, Tuple.Create(ErrorCodes.TokenNotFound, "Wrong hash")},
            {Constants.Errors.SessionExpired, Tuple.Create(ErrorCodes.TokenNotFound, "Token not found")},
            {Constants.Errors.SessionNotFound, Tuple.Create(ErrorCodes.TokenNotFound, "Token not found")},
            {Constants.Errors.UserNotFound, Tuple.Create(ErrorCodes.UserNotFound, "User not found")},
            {Constants.Errors.ClientNotFound, Tuple.Create(ErrorCodes.UserNotFound, "User not found")},
            {Constants.Errors.ClientBlocked, Tuple.Create(ErrorCodes.UserBlocked, "User blocked")},
            {Constants.Errors.DocumentNotFound, Tuple.Create(ErrorCodes.TransactionNotFound, "Transaction not found")},
            {Constants.Errors.CanNotConnectCreditAndDebit, Tuple.Create(ErrorCodes.TransactionNotFound, "Transaction not found")}
             //Transaction timed out
            //Reserved for the future use
        };

        public static Dictionary<string, int> BetTypes { get; private set; } = new Dictionary<string, int>
        {
            {BetTypeCodes.TableBet, 1},
            {BetTypeCodes.Insurance, 4 },
            {BetTypeCodes.Double, 5 },
            {BetTypeCodes.Split, 6},
            {BetTypeCodes.Ante, 7},
            {BetTypeCodes.TableBetBB, 16},
            {BetTypeCodes.SplitBB, 17},
            {BetTypeCodes.DoubleBB, 18},
            {BetTypeCodes.InsuranceBB, 19},
            {BetTypeCodes.Call, 24}
        };
       
        public static Tuple<int, string> GetError(int error)
        {
            if (Errors.ContainsKey(error))
                return Errors[error];
            return Errors[Constants.Errors.GeneralException];
        }
    }
}