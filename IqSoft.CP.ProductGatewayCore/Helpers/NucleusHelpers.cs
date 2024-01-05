using IqSoft.CP.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class NucleusHelpers
    {
        public enum ReturnCodes
        {
            InternalError = 399,
            InvalidToken = 400,
            InvalidHash = 500
        }
        private readonly static Dictionary<int, int> Error = new Dictionary<int, int>
        {
             {Constants.Errors.SessionExpired,  (int)ReturnCodes. InvalidToken},
             {Constants.Errors.SessionNotFound,  (int)ReturnCodes. InvalidToken},
             {Constants.Errors.WrongHash,  (int)ReturnCodes. InvalidHash}
        };

        public static int GetErrorCode(int errorId)
        {
            if (Error.ContainsKey(errorId))
                return Error[errorId];
            return (int)ReturnCodes.InternalError;
        }

    }
}