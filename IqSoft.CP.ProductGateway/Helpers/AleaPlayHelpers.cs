using IqSoft.CP.Common;
using System.Collections.Generic;
using System.Net;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public class AleaPlayHelpers
    {
        public enum TransactionTypes
        {
            BET,
            WIN,
            BET_WIN,
            ROLLBACK
        }
        public static class ErrorCodes
        {
            public static readonly string INVALID_SIGNATURE = "INVALID_SIGNATURE";
            public static readonly string SESSION_EXPIRED = "SESSION_EXPIRED";
            public static readonly string PLAYER_BLOCKED = "PLAYER_BLOCKED";
            public static readonly string PLAYER_NOT_FOUND = "PLAYER_NOT_FOUND";
            public static readonly string INVALID_CURRENCY = "INVALID_CURRENCY";
            public static readonly string GAME_NOT_ALLOWED = "GAME_NOT_ALLOWED";
            public static readonly string INSUFFICIENT_FUNDS = "INSUFFICIENT_FUNDS";
            public static readonly string BET_MAX = "BET_MAX";
            public static readonly string BET_DENIED = "BET_DENIED";
            public static readonly string TRANSACTION_ALREADY_PROCESSED = "TRANSACTION_ALREADY_PROCESSED";
            public static readonly string TRANSACTION_NOT_FOUND = "TRANSACTION_NOT_FOUND";
            public static readonly string GAME_NOT_FOUND = "GAME_NOT_FOUND";
            public static readonly string DUPLICATE_TRANSACTION_DATA_MISMATCH = "DUPLICATE_TRANSACTION_DATA_MISMATCH";
            public static readonly string UNKNOWN_ERROR = "UNKNOWN_ERROR";
            public static readonly string INTERNAL_ERROR = "INTERNAL_ERROR";
            public static readonly string MAINTENANCE = "MAINTENANCE";
        }


        public static class ErrorHttpCodes
        {
            public static readonly HttpStatusCode INVALID_SIGNATURE = HttpStatusCode.Unauthorized;
            public static readonly HttpStatusCode SESSION_EXPIRED = HttpStatusCode.Forbidden;
            public static readonly HttpStatusCode PLAYER_BLOCKED = HttpStatusCode.Forbidden;
            public static readonly HttpStatusCode PLAYER_NOT_FOUND = HttpStatusCode.NotFound;
            public static readonly HttpStatusCode INVALID_CURRENCY = HttpStatusCode.BadRequest;

            public static readonly HttpStatusCode GAME_NOT_ALLOWED = HttpStatusCode.Forbidden;
            public static readonly HttpStatusCode INSUFFICIENT_FUNDS = HttpStatusCode.Forbidden;
            public static readonly HttpStatusCode BET_MAX = HttpStatusCode.Forbidden;
            public static readonly HttpStatusCode BET_DENIED = HttpStatusCode.Forbidden;
            public static readonly HttpStatusCode TRANSACTION_ALREADY_PROCESSED = HttpStatusCode.Forbidden;
            public static readonly HttpStatusCode TRANSACTION_NOT_FOUND = HttpStatusCode.Forbidden;
            public static readonly HttpStatusCode GAME_NOT_FOUND = HttpStatusCode.Forbidden;
            public static readonly HttpStatusCode DUPLICATE_TRANSACTION_DATA_MISMATCH = HttpStatusCode.BadRequest;

            public static readonly HttpStatusCode UNKNOWN_ERROR = HttpStatusCode.BadRequest;
            public static readonly HttpStatusCode INTERNAL_ERROR = HttpStatusCode.InternalServerError;
            public static readonly HttpStatusCode MAINTENANCE = HttpStatusCode.ServiceUnavailable;
        }

        private readonly static Dictionary<int, HttpStatusCode> Error = new Dictionary<int, HttpStatusCode>
        {
            {Constants.Errors.SessionExpired,  ErrorHttpCodes.SESSION_EXPIRED},
            {Constants.Errors.SessionNotFound, ErrorHttpCodes.SESSION_EXPIRED},
            {Constants.Errors.ClientBlocked, ErrorHttpCodes.PLAYER_BLOCKED},
            {Constants.Errors.ClientNotFound, ErrorHttpCodes.PLAYER_NOT_FOUND},
            {Constants.Errors.WrongClientId, ErrorHttpCodes.PLAYER_NOT_FOUND},
            {Constants.Errors.WrongHash, ErrorHttpCodes.INVALID_SIGNATURE},
            {Constants.Errors.WrongCurrencyId, ErrorHttpCodes.INVALID_CURRENCY},
            {Constants.Errors.ProductBlockedForThisPartner, ErrorHttpCodes.GAME_NOT_ALLOWED},
            {Constants.Errors.ProductNotFound, ErrorHttpCodes.GAME_NOT_FOUND},
            {Constants.Errors.LowBalance, ErrorHttpCodes.INSUFFICIENT_FUNDS},
            {Constants.Errors.MaxLimitExceeded, ErrorHttpCodes.BET_MAX},
            {Constants.Errors.DocumentExpired, ErrorHttpCodes.TRANSACTION_ALREADY_PROCESSED},
            {Constants.Errors.DocumentNotFound, ErrorHttpCodes.TRANSACTION_NOT_FOUND},
            {Constants.Errors.ClientDocumentAlreadyExists, ErrorHttpCodes.TRANSACTION_ALREADY_PROCESSED},
            {Constants.Errors.DocumentAlreadyWinned, ErrorHttpCodes.TRANSACTION_ALREADY_PROCESSED},
            {Constants.Errors.DocumentAlreadyRollbacked, ErrorHttpCodes.TRANSACTION_ALREADY_PROCESSED},
            {Constants.Errors.CanNotDeleteRollbackDocument, ErrorHttpCodes.TRANSACTION_ALREADY_PROCESSED},
            {Constants.Errors.CanNotConnectCreditAndDebit, ErrorHttpCodes.DUPLICATE_TRANSACTION_DATA_MISMATCH},
            {Constants.Errors.WrongDocumentNumber, ErrorHttpCodes.BET_DENIED},
            {Constants.Errors.GeneralException, ErrorHttpCodes.UNKNOWN_ERROR}
        };

        private readonly static Dictionary<int, string> ErrorMessages = new Dictionary<int, string>
        {
            {Constants.Errors.SessionExpired,  ErrorCodes.SESSION_EXPIRED},
            {Constants.Errors.SessionNotFound, ErrorCodes.SESSION_EXPIRED},
            {Constants.Errors.ClientBlocked, ErrorCodes.PLAYER_BLOCKED},
            {Constants.Errors.ClientNotFound, ErrorCodes.PLAYER_NOT_FOUND},
            {Constants.Errors.WrongClientId, ErrorCodes.PLAYER_NOT_FOUND},
            {Constants.Errors.WrongHash, ErrorCodes.INVALID_SIGNATURE},
            {Constants.Errors.WrongCurrencyId, ErrorCodes.INVALID_CURRENCY},
            {Constants.Errors.ProductBlockedForThisPartner, ErrorCodes.GAME_NOT_ALLOWED},
            {Constants.Errors.ProductNotFound, ErrorCodes.GAME_NOT_FOUND},
            {Constants.Errors.LowBalance, ErrorCodes.INSUFFICIENT_FUNDS},
            {Constants.Errors.MaxLimitExceeded, ErrorCodes.BET_MAX},
            {Constants.Errors.DocumentExpired, ErrorCodes.TRANSACTION_ALREADY_PROCESSED},
            {Constants.Errors.DocumentNotFound, ErrorCodes.TRANSACTION_NOT_FOUND},
            {Constants.Errors.DocumentAlreadyWinned, ErrorCodes.TRANSACTION_ALREADY_PROCESSED},
            {Constants.Errors.DocumentAlreadyRollbacked, ErrorCodes.TRANSACTION_ALREADY_PROCESSED},
            {Constants.Errors.ClientDocumentAlreadyExists, ErrorCodes.TRANSACTION_ALREADY_PROCESSED},
            {Constants.Errors.CanNotDeleteRollbackDocument, ErrorCodes.TRANSACTION_ALREADY_PROCESSED},
            {Constants.Errors.CanNotConnectCreditAndDebit, ErrorCodes.DUPLICATE_TRANSACTION_DATA_MISMATCH},
            {Constants.Errors.WrongDocumentNumber, ErrorCodes.BET_DENIED},
            {Constants.Errors.GeneralException, ErrorCodes.UNKNOWN_ERROR}
        };

        public static string GetErrorMsg(int errorId)
        {
            if (ErrorMessages.ContainsKey(errorId))
                return ErrorMessages[errorId];
            return ErrorCodes.INTERNAL_ERROR;
        }

        public static HttpStatusCode GetErrorCode(int errorId)
        {
            if (Error.ContainsKey(errorId))
                return Error[errorId];
            return ErrorHttpCodes.INTERNAL_ERROR;
        }
    }
}