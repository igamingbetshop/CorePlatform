using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public class AWCHelpers
    {
        public static class Methods
        {
            public const string GetBalance = "getBalance";
            public const string DoBet = "bet";
            public const string AdjustBet = "adjustBet";
            public const string DoWin = "settle";
            public const string BetNSettle = "betNSettle";
            public const string DoLost = "unsettle";
            public const string Rollback = "cancelBet";
            public const string RollbackBetNSettle = "cancelBetNSettle";
            public const string VoidBet = "voidBet";
            public const string VoidSettle = "voidSettle";
            public const string Resettle = "resettle";
            public const string BonusWin = "give";
            public const string DonateTip = "tip";
            public const string CancelTip = "cancelTip";
        }

        public static class ErrorCodes
        {
            public static readonly string Success = "0000";
            public static readonly string GeneralError = "9999";
            public static readonly string WrongInputParameters = "9999";
            public static readonly string ClientNotFound = "1000";
            public static readonly string WrongCurrency = "1004";
            public static readonly string SessionNotFound = "1008";
            public static readonly string UnknownAgent = "1035";
        }

        private readonly static Dictionary<int, string> Error = new Dictionary<int, string>
        {
            {Constants.Errors.SessionExpired,  ErrorCodes.SessionNotFound},
            {Constants.Errors.SessionNotFound, ErrorCodes.SessionNotFound},
            {Constants.Errors.ClientNotFound, ErrorCodes.ClientNotFound},
            {Constants.Errors.WrongClientId, ErrorCodes.ClientNotFound},
            {Constants.Errors.PartnerKeyNotFound, ErrorCodes.ClientNotFound},
            {Constants.Errors.GeneralException, ErrorCodes.GeneralError}
        };

        public static string GetErrorCode(int errorId)
        {
            if (Error.ContainsKey(errorId))
                return Error[errorId];
            return ErrorCodes.GeneralError;
        }
    }
}