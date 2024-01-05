using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class MancalaHelpers
    {
        public enum ReturnCodes
        {
            InternalError = 100,
            InsufficentBalance = 213
        }
        private readonly static Dictionary<int, int> Error = new Dictionary<int, int>
        {
             {Constants.Errors.LowBalance,  (int)ReturnCodes. InsufficentBalance}
        };

        public static int GetErrorCode(int errorId)
        {
            if (Error.ContainsKey(errorId))
                return Error[errorId];
            return (int)ReturnCodes.InternalError;
        }
    }
}