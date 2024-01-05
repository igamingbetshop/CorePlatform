using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public class PlaynGoHelpers
    {
        private static class ErrorCodes
        {
            public const int NOUSER = 1;
            public const int INTERNAL = 2;
            public const int INVALIDCURRENCY = 3;
            public const int WRONGUSERNAMEPASSWORD  = 4;
            public const int ACCOUNTLOCKED = 5;
            public const int ACCOUNTDISABLED = 6;
            public const int NOTENOUGHMONEY = 7;
            public const int MAXCONCURRENTCALLS = 8;
            public const int SESSIONEXPIRED = 10;
        }

        private readonly static Dictionary<int, int> Error = new Dictionary<int, int>
        {
            {Constants.Errors.SessionExpired,  ErrorCodes.SESSIONEXPIRED},
            {Constants.Errors.SessionNotFound,  ErrorCodes.WRONGUSERNAMEPASSWORD},
            {Constants.Errors.WrongCurrencyId,  ErrorCodes.INVALIDCURRENCY},
            {Constants.Errors.LowBalance,  ErrorCodes.NOTENOUGHMONEY},
            {Constants.Errors.ClientBlocked,  ErrorCodes.ACCOUNTLOCKED},
            {Constants.Errors.ClientNotFound,  ErrorCodes.NOUSER},
        };

        public static int GetErrorCode(int errorId)
        {
            if (Error.ContainsKey(errorId))
                return Error[errorId];
            return ErrorCodes.INTERNAL;
        }
    }
}