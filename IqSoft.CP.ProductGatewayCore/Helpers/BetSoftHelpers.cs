using System.Collections.Generic;
using IqSoft.CP.Common;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class BetSoftHelpers
    {
        public static class Error
        {
            public const string InsufficientFunds = "300";//betwin
            public const string OperationFailed = "301";//betwin
            public const string UnknownTransactionId = "302";//Refund
            public const string UnknownUserId = "310";//betwin //Refund //GetAccountInfo //BonusRelease
            public const string InternalError = "399";//authenticate //betwin //Refund //GetAccountInfo //BonusRelease
            public const string InvalidToken = "400";//authenticate
            public const string InvalidHash = "500";//authenticate //betwin //Refund //GetAccountInfo //BonusRelease
        }

        public static class ResponseResults
        {
            public const string SuccessResponse = "OK";
            public const string ErrorResponse = "FAILED";
        }

        private readonly static Dictionary<int, string> ErrorCodesMapping = new Dictionary<int, string>
        {
            {Constants.Errors.LowBalance, Error.InsufficientFunds},
            {Constants.Errors.DocumentNotFound, Error.UnknownTransactionId},
            {Constants.Errors.ClientNotFound, Error.UnknownUserId},
            {Constants.Errors.WrongParameters, Error.OperationFailed},
            {Constants.Errors.WrongClientId, Error.UnknownUserId},
            {Constants.Errors.GeneralException, Error.InternalError},
            {Constants.Errors.SessionNotFound, Error.InvalidToken},
            {Constants.Errors.SessionExpired, Error.InvalidToken},
            {Constants.Errors.TokenExpired, Error.InvalidToken},
            {Constants.Errors.WrongToken, Error.InvalidToken},
            {Constants.Errors.WrongHash, Error.InvalidHash}
        };

        public static string GetResponseStatus(int error)
        {
            if (ErrorCodesMapping.ContainsKey(error))
                return ErrorCodesMapping[error];
            return Error.InternalError;
        }
    }
}