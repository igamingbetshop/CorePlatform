using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public class SkyCityHelpers
    {
        public static class ErrorCode
        {
            public const int Normal = 0;
            public const int LowBalance = 1;
            public const int Error = 2;
        }

        private readonly static Dictionary<int, int> Error = new Dictionary<int, int>
        {
            {Constants.Errors.LowBalance,  ErrorCode.LowBalance},
            {Constants.Errors.GeneralException, ErrorCode.Error}
        };

        public static int GetErrorCode(int errorId)
        {
            if (Error.ContainsKey(errorId))
                return Error[errorId];
            return ErrorCode.Error;
        }
    }
}