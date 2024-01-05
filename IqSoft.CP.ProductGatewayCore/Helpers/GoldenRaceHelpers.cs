using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class GoldenRaceHelpers
    {
        public enum ReturnCodes
        {
            InternalError = 101,
            InvalidPlatformIdentifier = 102,
            UnknownRequest = 104,
            IncompleteOrMalformedRequest = 103,
            InvalidSecureToken = 106,
            InsufficentBalance = 107,
            PlayerAccountLocked = 108,
            TransactionFailed = 110,
            UnsupportedGameid = 111,
            GameCycleNotExists = 112,
            IncorrectParametersForSession = 113,
            IncorrectPlayerIdentifie = 114,
            GameCycleExists = 115,
            TransactionAlreadyExists = 116,
            TransactionNotExists = 117,
            GameCycleAlreadyClosed = 118
        }
        private readonly static Dictionary<int, int> Error = new Dictionary<int, int>
        {
             {Constants.Errors.WrongInputParameters,  (int)ReturnCodes.UnknownRequest},
             {Constants.Errors.WrongApiCredentials,  (int)ReturnCodes.InvalidPlatformIdentifier},
             {Constants.Errors.WrongOperatorId,  (int)ReturnCodes.IncompleteOrMalformedRequest},
             {Constants.Errors.SessionNotFound,  (int)ReturnCodes.InvalidSecureToken},
             {Constants.Errors.SessionExpired,  (int)ReturnCodes.IncorrectParametersForSession},
             {Constants.Errors.ClientBlocked,  (int)ReturnCodes.PlayerAccountLocked},
             {Constants.Errors.ProductBlockedForThisPartner,  (int)ReturnCodes.UnsupportedGameid},
             {Constants.Errors.ProductNotFound,  (int)ReturnCodes.UnsupportedGameid},
             {Constants.Errors.WrongProductId,  (int)ReturnCodes.UnsupportedGameid},
             {Constants.Errors.ClientNotFound,  (int)ReturnCodes.IncorrectPlayerIdentifie},
             {Constants.Errors.WrongClientId,  (int)ReturnCodes.IncorrectPlayerIdentifie},
             {Constants.Errors.LowBalance,  (int)ReturnCodes.InsufficentBalance},
             {Constants.Errors.ClientDocumentAlreadyExists,  (int)ReturnCodes.TransactionAlreadyExists},
             {Constants.Errors.CanNotConnectCreditAndDebit,  (int)ReturnCodes.TransactionAlreadyExists},
             {Constants.Errors.DocumentNotFound,  (int)ReturnCodes.TransactionNotExists},
             {Constants.Errors.WrongDocumentId,  (int)ReturnCodes.TransactionAlreadyExists},
             {Constants.Errors.WrongDocumentNumber,  (int)ReturnCodes.GameCycleExists},
             {Constants.Errors.RoundNotFound,  (int)ReturnCodes.GameCycleNotExists},
             {Constants.Errors.DocumentAlreadyWinned,  (int)ReturnCodes.GameCycleAlreadyClosed},
             {Constants.Errors.WinAlreadyPayed,  (int)ReturnCodes.GameCycleAlreadyClosed},
             {Constants.Errors.DocumentAlreadyRollbacked,  (int)ReturnCodes.GameCycleAlreadyClosed}
        };

        public static int GetErrorCode(int errorId)
        {
            if (Error.ContainsKey(errorId))
                return Error[errorId];
            return (int)ReturnCodes.InternalError;
        }

    }
}