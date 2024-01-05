using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public class OutcomeBetHelpers
    {
        public static class ErrorCodes
        {
            public const int InternalServerError = 16383;
            public const int PlayerNotFound = 8201;
            public const int InsufficientFunds = 8209;
        }

        private readonly static Dictionary<int, int> Error = new Dictionary<int, int>
        {
            {Constants.Errors.ClientNotFound,  ErrorCodes.PlayerNotFound},
            {Constants.Errors.LowBalance,  ErrorCodes.InsufficientFunds}
        };

        public static int GetErrorCode(int errorId)
        {
            if (Error.ContainsKey(errorId))
                return Error[errorId];
            return ErrorCodes.InternalServerError;
        }
    }
}