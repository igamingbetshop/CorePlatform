using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public class ISoftBetHelpers
    {
        public static class Statuses
        {
            public const string Success = "success";
            public const string Error = "error";
        }

        public static class ErrorCodes
        {
            public const string InvalidRequest = "R_02";//??
            public const string InvalidHMAC = "R_03";
            public const string PlayerNotFound = "R_09";
            public const string InvalidLicensee = "R_10";
            public const string InvalidSessionId = "R_11";
            public const string InvalidCurrency = "R_13";
            public const string InvalidToken = "I_03";
            public const string TokenExist = "I_04";
            public const string InsufficientFunds = "B_03";
            public const string TransactionIdExist = "B_04";
            public const string TransactionCancelled = "B_05";
            public const string RoundClosed = "B_06";
            public const string OpenRoundByToken = "B_07";

            public const string InvalidRound = "W_03";
            public const string WinTransactionIdExist = "W_06";
            public const string InvalidCancel = "C_03";
            public const string RemoveBetInClosedRound = "C_04";

            public const string GeneralException = "ERnn";
        }
        private readonly static Dictionary<int, string> Errors = new Dictionary<int, string>
        {
            {Constants.Errors.WrongHash, ErrorCodes.InvalidHMAC},
            {Constants.Errors.ClientNotFound, ErrorCodes.PlayerNotFound},
            {Constants.Errors.PartnerProductSettingNotFound, ErrorCodes.InvalidLicensee},
            {Constants.Errors.SessionExpired, ErrorCodes.InvalidSessionId},
            {Constants.Errors.SessionNotFound, ErrorCodes.InvalidSessionId},
            {Constants.Errors.WrongToken, ErrorCodes.InvalidToken},
            {Constants.Errors.TokenExpired, ErrorCodes.InvalidToken},
            {Constants.Errors.LowBalance,  ErrorCodes.InsufficientFunds},
            {Constants.Errors.TransactionAlreadyExists,  ErrorCodes.TransactionIdExist},
            {Constants.Errors.DocumentNotFound,  ErrorCodes.InvalidRound},
            {Constants.Errors.GeneralException, ErrorCodes.GeneralException}
        };

        public static string GetErrorCode(int errorId)
        {
            if (Errors.ContainsKey(errorId))
                return Errors[errorId];
            return ErrorCodes.GeneralException;
        }
    }
}