using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class EveryMatrixHelpers
    {
        public static class TransactionTypes
        {
            public const string Bet = "wager";
            public const string Win = "result";
            public const string Rollback = "rollback";
        }

        public static class Methods
        {
            public const string GetAccount = "GetAccount";
            public const string GetBalance = "GetBalance";
            public const string WalletDebit = "WalletDebit";
            public const string WalletCredit = "WalletCredit";
            //public const string GetTransactionStatus = "GetTransactionStatus";
            //public const string GetTransactions = "GetTransactions";
        }
        public enum ReturnCodes
        {
            Success = 0,
            UnknownError = 101,
            ClientBlocked = 102,
            ClientNotFound = 103,
            InsufficientFunds = 104,
            IpIsNotAllowed = 105,
            CurrencyNotSupported = 106,
            TransactionProcedding = 107,
            TransactionNotFound = 108,
            CasinoLossLimitExceeded = 109,
            CasinoStakeLimitExceeded = 110,
            CasinoSessionLimitExceeded = 111
        }

        private readonly static Dictionary<int, int> Error = new Dictionary<int, int>
        {
             {Constants.Errors.ClientBlocked,  (int)ReturnCodes.ClientBlocked},
             {Constants.Errors.ClientNotFound,  (int)ReturnCodes.ClientNotFound},
             {Constants.Errors.LowBalance,  (int)ReturnCodes.InsufficientFunds},
             {Constants.Errors.DontHavePermission,  (int)ReturnCodes. IpIsNotAllowed},
             {Constants.Errors.WrongCurrencyId,  (int)ReturnCodes. CurrencyNotSupported},
             {Constants.Errors.DocumentAlreadyRollbacked,  (int)ReturnCodes. TransactionProcedding},
             {Constants.Errors.ClientDocumentAlreadyExists,  (int)ReturnCodes. TransactionProcedding},
             {Constants.Errors.CanNotConnectCreditAndDebit,  (int)ReturnCodes. TransactionNotFound},
             {Constants.Errors.DocumentNotFound,  (int)ReturnCodes. TransactionNotFound},
             {Constants.Errors.LevelLimitExceeded,  (int)ReturnCodes. CasinoLossLimitExceeded},
             {Constants.Errors.SessionExpired,  (int)ReturnCodes. CasinoSessionLimitExceeded}
        };

        public static int GetErrorCode(int errorId)
        {
            if (Error.ContainsKey(errorId))
                return Error[errorId];
            return (int)ReturnCodes.UnknownError;
        }
    }
}