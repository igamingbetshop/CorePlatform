using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public class EliteHelpers
    {
        public static class Methods
        {
            public const string GetBalance = "getBalance";
            public const string Debit = "debit";
            public const string CancelDebit = "cancelDebit";
            public const string Credit = "credit";
            public const string CancelCredit = "cancelCredit";
            public const string CreditDebit = "creditDebit";
            public const string CancelCreditDebit = "cancelCreditDebit";
            public const string CloseGameRound = "closeGameRound";
            public const string PromoWin = "promoWin";
            public const string Recon = "recon";
        }
        public static class ErrorCodes
        {
            public const int GeneralError = -1;
            public const int InvalidMethod = 2;
            public const int MissingOrInvalidParameter = 3;
            public const int AccessDenied = 5;
            public const int InternalErrorOccurred = 0;
            public const int SessionNotFound = 100;
            public const int SessionInvalid = 101;
            public const int SessionExpired = 102;
            public const int InvalidUserId = 103;
            public const int InvalidGameId = 106;
            public const int BalanceTooLow = 116;
            public const int InvalidSecureToken = 118;
        }

        public static Error ErrorMapping(int errorCode, string message)
        {
            var error = new Error() { Code = errorCode , Message = message };   
            switch (errorCode)
            {
                case Constants.Errors.MethodNotFound:
                    error.Code = ErrorCodes.InvalidMethod;
                    error.Message = ErrorMessages.Invalid_Method;
                    break;
                case Constants.Errors.LowBalance:
                    error.Code = ErrorCodes.BalanceTooLow;
                    error.Message = ErrorMessages.Balance_Too_Low;
                    break;
                case Constants.Errors.WrongHash:
                    error.Code = ErrorCodes.InvalidSecureToken;
                    error.Message = ErrorMessages.Invalid_Secure_Token;
                    break;
                case Constants.Errors.GeneralException:
                    error.Code = ErrorCodes.GeneralError;
                    error.Message = ErrorMessages.General_Error;
                    break;
                case Constants.Errors.SessionNotFound:
                    error.Code = ErrorCodes.SessionNotFound;
                    error.Message = ErrorMessages.Session_Not_Found;
                    break;
                case Constants.Errors.SessionExpired:
                    error.Code = ErrorCodes.SessionExpired;
                    error.Message = ErrorMessages.Session_Expired;
                    break;
            }
            return error;
        }

        public static class ErrorMessages
        {
            public static readonly string General_Error = "General_Error";
            public static readonly string Invalid_Method = "Invalid_Method";
            public static readonly string Balance_Too_Low = "Balance_Too_Low";
            public static readonly string Session_Not_Found = "Session_Not_Found";
            public static readonly string Session_Expired = "Session_Expired";
            public static readonly string Invalid_Secure_Token = "Invalid_Secure_Token";
        }

        public class Error
        {
           public int Code { get; set; }
           public string Message { get; set; }
        }
    }
}