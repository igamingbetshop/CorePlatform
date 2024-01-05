using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public class GrooveHelpers
    {
        public static class Actions
        {
            public const string Authenticate = "getaccount";
            public const string GetBalance = "getbalance";
            public const string Bet = "wager";
            public const string Win = "result";
            public const string BetWin = "wagerandresult";
            public const string BetRollback = "rollback";
            public const string WinRollback = "reversewin";
            public const string Jackpot = "jackpot";
        }

        public static class StatusCodes
        {
            public const int Success = 200;
            public const int Technical = 1;
            public const int WagerNotFound = 102;
            public const int OperationNotAllowed = 110;
            public const int TransactionMismatch= 400;
            public const int TransactionExists = 409;
            public const int NotLogedOn = 1000;
            public const int AuthenticationFailed = 1003;
            public const int LowBalance = 1006;
            public const int UnknownCurrency = 1007;
            public const int GamingLimit = 1019;
            public const int AccountBlocked = 1035;
        }
        private readonly static Dictionary<int, int> StatusPairs = new Dictionary<int, int>
        {
            {Constants.Errors.WinAlreadyPayed,  StatusCodes.Success},
            {Constants.Errors.LowBalance,  StatusCodes.LowBalance},
            {Constants.Errors.ClientNotFound,  StatusCodes.OperationNotAllowed},
            {Constants.Errors.SessionExpired,  StatusCodes.AuthenticationFailed},
            {Constants.Errors.SessionNotFound,  StatusCodes.OperationNotAllowed},
            {Constants.Errors.WrongHash,  StatusCodes.AuthenticationFailed},
            {Constants.Errors.ClientBlocked,  StatusCodes.AccountBlocked},
            {Constants.Errors.WrongProductId,  StatusCodes.OperationNotAllowed},
            {Constants.Errors.WrongInputParameters,  StatusCodes.OperationNotAllowed},
            {Constants.Errors.ClientMaxLimitExceeded,  StatusCodes.GamingLimit},
            {Constants.Errors.WrongOperationAmount,  StatusCodes.OperationNotAllowed},
            {Constants.Errors.ProductNotAllowedForThisPartner,  StatusCodes.OperationNotAllowed},
            {Constants.Errors.ProductBlockedForThisPartner,  StatusCodes.OperationNotAllowed},
            {Constants.Errors.PartnerProductSettingNotFound,  StatusCodes.OperationNotAllowed},
            {Constants.Errors.DocumentAlreadyWinned,  StatusCodes.TransactionExists},
            {Constants.Errors.DocumentAlreadyRollbacked,  StatusCodes.Success},
            {Constants.Errors.ClientDocumentAlreadyExists,  StatusCodes.Success},
            {Constants.Errors.DocumentNotFound,  StatusCodes.WagerNotFound},
            {Constants.Errors.CanNotConnectCreditAndDebit,  StatusCodes.WagerNotFound},
            {Constants.Errors.WrongCurrencyId,  StatusCodes.UnknownCurrency}
        };
        private readonly static Dictionary<int, string> StatusDescriptionsPairs = new Dictionary<int, string>
        {
            { StatusCodes.Success, "Success - duplicate request"},
            { StatusCodes.Technical, "Technical Error"},
            { StatusCodes.WagerNotFound, "Wager not found"},
            { StatusCodes.OperationNotAllowed, "Operation not allowed"},
            { StatusCodes.TransactionMismatch, "Transaction operator mismatch"},
            { StatusCodes.TransactionExists, "Round closed or transaction ID exists"},
            { StatusCodes.NotLogedOn, "Not logged on"},
            { StatusCodes.AuthenticationFailed, "Authentication Failed"},
            { StatusCodes.LowBalance, "Out of money"},
            { StatusCodes.UnknownCurrency, "Unknown currency"},
            { StatusCodes.GamingLimit, "Gaming limit"},
            { StatusCodes.AccountBlocked, "Account blocked"}
        };


        public static int GetErrorCode(int errorId)
        {
            if (StatusPairs.ContainsKey(errorId))
                return StatusPairs[errorId];
            return StatusCodes.Technical;
        }

        public static string GetErrorMessage(int errorId)
        {
            if (StatusDescriptionsPairs.ContainsKey(errorId))
                return StatusDescriptionsPairs[errorId];
            return StatusDescriptionsPairs[StatusCodes.Technical];
        }
    }
}