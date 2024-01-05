using System.Collections.Generic;
using IqSoft.CP.Common;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class EvolutionHelpers
    {
        public static List<string> RestrictedCurrencies = new List<string> { Constants.Currencies.IranianTuman, Constants.Currencies.USDT };
        public static class Statuses
        {
            public const string Ok = "OK";
            public const string TemporaryError = "TEMPORARY_ERROR";
            public const string InvalidTokenId = "INVALID_TOKEN_ID";
            public const string InvalidSid = "INVALID_SID";
            public const string AccountLocked = "ACCOUNT_LOCKED";
            public const string FatalErrorCloseUserSession = "FATAL_ERROR_CLOSE_USER_SESSION";
            public const string UnknownError = "UNKNOWN_ERROR";
            public const string InvalidParameter = "INVALID_PARAMETER";
            public const string BetAlreadySettled = "BET_ALREADY_SETTLED";
            public const string BetDoesNotExist = "BET_DOES_NOT_EXIST";
            public const string BetAlreadyExist = "BET_ALREADY_EXIST";
            public const string InsufficientFunds = "INSUFFICIENT_FUNDS";
            public const string FinalErrorActionFailed = "FINAL_ERROR_ACTION_FAILED ";
            public const string SessionAlreadyClosed = "SESSION_ALREADY_CLOSED";
            public const string SessionDoesNotExist = "SESSION_DOES_NOT_EXIST";
            public const string CasinoLimitExceededTurnover = "CASINO_LIMIT_EXCEEDED_TURNOVER";
            public const string CasinoLimitExceededSessionTime = "CASINO_LIMIT_EXCEEDED_SESSION_TIME";
            public const string CasinoLimitExceededStake = "CASINO_LIMIT_EXCEEDED_STAKE";
            public const string CasinoLimitExceededLoss = "CASINO_LIMIT_EXCEEDED_LOSS";
        }

        public static class Methods
        {
            public const string Check = "Check";
            public const string Sid = "Sid";
            public const string Balance = "Balance";
            public const string Debit = "Debit";
            public const string Credit = "Credit";
            public const string Cancel = "Cancel";
        }

        private readonly static Dictionary<int, string> StatusMapping = new Dictionary<int, string>
        {
            {Constants.Errors.SessionNotFound, Statuses.InvalidSid},
            {Constants.Errors.SessionExpired, Statuses.InvalidSid},
            {Constants.Errors.WrongLoginParameters, Statuses.InvalidTokenId},
            {Constants.Errors.WrongParameters, Statuses.InvalidParameter},
            {Constants.Errors.TransactionAlreadyExists, Statuses.BetAlreadyExist},
            {Constants.Errors.ClientNotFound, Statuses.InvalidParameter},
            {Constants.Errors.ProductNotFound, Statuses.FinalErrorActionFailed},
            {Constants.Errors.ClientBlocked, Statuses.AccountLocked},
            {Constants.Errors.ProductNotAllowedForThisPartner, Statuses.FinalErrorActionFailed},
            {Constants.Errors.AccountNotFound, Statuses.FinalErrorActionFailed},
            {Constants.Errors.LowBalance, Statuses.InsufficientFunds},
            {Constants.Errors.ClientMaxLimitExceeded, Statuses.CasinoLimitExceededStake},
            {Constants.Errors.PartnerProductLimitExceeded, Statuses.CasinoLimitExceededStake},
            {Constants.Errors.DocumentNotFound, Statuses.BetDoesNotExist},
            {Constants.Errors.DocumentAlreadyRollbacked, Statuses.BetAlreadySettled},
            {Constants.Errors.CanNotDeleteRollbackDocument, Statuses.BetAlreadySettled},
            {Constants.Errors.CanNotConnectCreditAndDebit, Statuses.BetDoesNotExist},
            {Constants.Errors.DocumentAlreadyWinned, Statuses.BetAlreadySettled}
        };

        public static string GetResponseStatus(int error)
        {
            if (StatusMapping.ContainsKey(error))
                return StatusMapping[error];
            return Statuses.FinalErrorActionFailed;
        }
    }
}