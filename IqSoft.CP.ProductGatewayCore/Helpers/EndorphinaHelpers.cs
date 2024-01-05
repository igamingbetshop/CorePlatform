using IqSoft.CP.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public class EndorphinaHelpers
    {
        private static class ErrorCodes
        {
            public const string WrongAuthentication = "ACCESS_DENIED";
            public const string InsufficientFunds = "INSUFFICIENT_FUNDS";
            public const string TokenExpired = "TOKEN_EXPIRED";
            public const string LimitReached = "LIMIT_REACHED";
            public const string TokenNotFound = "TOKEN_NOT_FOUND";
            public const string InternalError = "INTERNAL_ERROR";
        }

        public readonly static Dictionary<string, HttpStatusCode> Statuses = new Dictionary<string, HttpStatusCode>
        {
            {ErrorCodes.WrongAuthentication, HttpStatusCode.Unauthorized},
            {ErrorCodes.InsufficientFunds, HttpStatusCode.PaymentRequired},
            {ErrorCodes.TokenExpired, HttpStatusCode.Forbidden},
            {ErrorCodes.LimitReached, HttpStatusCode.Forbidden},
            {ErrorCodes.TokenNotFound, HttpStatusCode.NotFound},
            {ErrorCodes.InternalError, HttpStatusCode.InternalServerError}
        };

        private readonly static Dictionary<int, string> Errors = new Dictionary<int, string>
        {
            {Constants.Errors.WrongToken, ErrorCodes.TokenNotFound},
            {Constants.Errors.WrongApiCredentials, ErrorCodes.WrongAuthentication},
            {Constants.Errors.LowBalance, ErrorCodes.InsufficientFunds},
            {Constants.Errors.ClientMaxLimitExceeded, ErrorCodes.LimitReached},
            {Constants.Errors.SessionExpired, ErrorCodes.TokenExpired},
            {Constants.Errors.GeneralException, ErrorCodes.InternalError}
        };

        public static string GetErrorCode(int errorId)
        {
            if (Errors.ContainsKey(errorId))
                return Errors[errorId];
            return ErrorCodes.InternalError;
        }

        public static HttpStatusCode GetStatusCode(string error)
        {
            if (Statuses.ContainsKey(error))
                return Statuses[error];
            return HttpStatusCode.BadRequest;
        }
    }
}