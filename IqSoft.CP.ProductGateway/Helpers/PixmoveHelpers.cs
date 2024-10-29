using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class PixmoveHelpers
    {
        public static class ErrorCodes
        {
            public static readonly string RequiredParamsNotFound = "RequiredParamsNotFound";
            public static readonly string InvalidParamType = "InvalidParamType";
            public static readonly string CurrencyNotFound = "CurrencyNotFound";
            public static readonly string IncorrectHash = "IncorrectHash";
            public static readonly string ServerError = "ServerError";
        }

        private readonly static Dictionary<int, string> ErrorMessages = new Dictionary<int, string>
        {
            {Constants.Errors.SessionExpired,  ErrorCodes.InvalidParamType},
            {Constants.Errors.SessionNotFound, ErrorCodes.RequiredParamsNotFound},
            {Constants.Errors.WrongHash, ErrorCodes.IncorrectHash},
            {Constants.Errors.ClientBlocked, ErrorCodes.InvalidParamType},
            {Constants.Errors.ClientNotFound, ErrorCodes.RequiredParamsNotFound},
            {Constants.Errors.WrongClientId, ErrorCodes.RequiredParamsNotFound},
            {Constants.Errors.WrongCurrencyId, ErrorCodes.CurrencyNotFound},
            {Constants.Errors.ProductBlockedForThisPartner, ErrorCodes.InvalidParamType},
            {Constants.Errors.ProductNotFound, ErrorCodes.RequiredParamsNotFound},
            {Constants.Errors.LowBalance, ErrorCodes.InvalidParamType},
            {Constants.Errors.MaxLimitExceeded, ErrorCodes.InvalidParamType},
            {Constants.Errors.DocumentAlreadyWinned, ErrorCodes.InvalidParamType},
            {Constants.Errors.DocumentAlreadyRollbacked, ErrorCodes.InvalidParamType},
            {Constants.Errors.ClientDocumentAlreadyExists, ErrorCodes.InvalidParamType},
            {Constants.Errors.CanNotDeleteRollbackDocument, ErrorCodes.InvalidParamType},
            {Constants.Errors.CanNotConnectCreditAndDebit, ErrorCodes.InvalidParamType},
            {Constants.Errors.DocumentNotFound, ErrorCodes.InvalidParamType},
            {Constants.Errors.GeneralException, ErrorCodes.ServerError}
        };

        public static string GetErrorMsg(int errorId)
        {
            if (ErrorMessages.ContainsKey(errorId))
                return ErrorMessages[errorId];
            return ErrorCodes.ServerError;
        }
    }
}