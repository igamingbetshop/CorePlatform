using IqSoft.CP.Common;
using System.Collections.Generic;
using System.Net;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class RelaxGamingHelpers
    {
        public static class ErrorHttpCodes
        {
            public static readonly HttpStatusCode InvalidToken = HttpStatusCode.Unauthorized;
            public static readonly HttpStatusCode Blocked= HttpStatusCode.Forbidden;
            public static readonly HttpStatusCode Unhandled = HttpStatusCode.InternalServerError;
        }

        private readonly static Dictionary<int, HttpStatusCode> Error = new Dictionary<int, HttpStatusCode>
        {
            {Constants.Errors.SessionExpired,  ErrorHttpCodes.InvalidToken},
            {Constants.Errors.SessionNotFound, ErrorHttpCodes.InvalidToken},
            {Constants.Errors.ClientBlocked, ErrorHttpCodes.Blocked},
            {Constants.Errors.ClientNotFound, ErrorHttpCodes.Blocked},
            {Constants.Errors.WrongClientId, ErrorHttpCodes.Blocked},
            {Constants.Errors.WrongCurrencyId, ErrorHttpCodes.Blocked},
            {Constants.Errors.ProductBlockedForThisPartner, ErrorHttpCodes.Blocked},
            {Constants.Errors.ProductNotFound, ErrorHttpCodes.Blocked},
        };
        public static HttpStatusCode GetErrorCode(int errorId)
        {
            if (Error.ContainsKey(errorId))
                return Error[errorId];
            return ErrorHttpCodes.Unhandled;
        }

        public static class ErrorCodes
        {
            public static readonly string INSUFFICIENT_FUNDS = "INSUFFICIENT_FUNDS";
            public static readonly string TRANSACTION_DECLINED = "TRANSACTION_DECLINED";
            public static readonly string SPENDING_BUDGET_EXCEEDED = "SPENDING_BUDGET_EXCEEDED";
            public static readonly string BLOCKED_FROM_PRODUCT = "BLOCKED_FROM_PRODUCT";
            public static readonly string MAXIMUM_BETLIMIT = "MAXIMUM_BETLIMIT";
            public static readonly string DAILY_TIME_LIMIT = "DAILY_TIME_LIMIT";
            public static readonly string WEEKLY_TIME_LIMIT = "WEEKLY_TIME_LIMIT";
            public static readonly string MONTHLY_TIME_LIMIT = "MONTHLY_TIME_LIMIT";
            public static readonly string CUSTOM_ERROR = "CUSTOM_ERROR";
        }

        private readonly static Dictionary<int, string> ErrorMessages = new Dictionary<int, string>
        {
            {Constants.Errors.SessionExpired,  ErrorCodes.TRANSACTION_DECLINED},
            {Constants.Errors.SessionNotFound, ErrorCodes.TRANSACTION_DECLINED},
            {Constants.Errors.ClientBlocked, ErrorCodes.BLOCKED_FROM_PRODUCT},
            {Constants.Errors.ClientNotFound, ErrorCodes.CUSTOM_ERROR},
            {Constants.Errors.WrongClientId, ErrorCodes.CUSTOM_ERROR},
            {Constants.Errors.WrongCurrencyId, ErrorCodes.CUSTOM_ERROR},
            {Constants.Errors.ProductBlockedForThisPartner, ErrorCodes.BLOCKED_FROM_PRODUCT},
            {Constants.Errors.ProductNotFound, ErrorCodes.BLOCKED_FROM_PRODUCT},
            {Constants.Errors.LowBalance, ErrorCodes.INSUFFICIENT_FUNDS},
            {Constants.Errors.MaxLimitExceeded, ErrorCodes.MAXIMUM_BETLIMIT},
            {Constants.Errors.DocumentAlreadyWinned, ErrorCodes.TRANSACTION_DECLINED},
            {Constants.Errors.DocumentAlreadyRollbacked, ErrorCodes.TRANSACTION_DECLINED},
            {Constants.Errors.ClientDocumentAlreadyExists, ErrorCodes.TRANSACTION_DECLINED},
            {Constants.Errors.CanNotDeleteRollbackDocument, ErrorCodes.TRANSACTION_DECLINED},
            {Constants.Errors.CanNotConnectCreditAndDebit, ErrorCodes.TRANSACTION_DECLINED},
            {Constants.Errors.DocumentNotFound, ErrorCodes.TRANSACTION_DECLINED},
            {Constants.Errors.GeneralException, ErrorCodes.CUSTOM_ERROR}
        };

        public static string GetErrorMsg(int errorId)
        {
            if (ErrorMessages.ContainsKey(errorId))
                return ErrorMessages[errorId];
            return ErrorCodes.CUSTOM_ERROR;
        }

    }
}