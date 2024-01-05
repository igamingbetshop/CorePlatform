using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.PaymentGateway.Helpers
{
    public static class PayBoxHelpers
    {
        public static class Methods
        {
            public const string Result = "result";
            public const string Check = "check";
            public const string Refund = "refund";
            public const string Capture = "capture";
        }

        public static class Statuses
        {
            public const string Ok = "ok";
            public const string Error = "error";
            public const string Rejected = "rejected";
        }

        private readonly static Dictionary<int, int> Errors = new Dictionary<int, int>
        {
            {Constants.SuccessResponseCode, (int) ResponseCodes.Success},
            {Constants.Errors.WrongParameters, (int) ResponseCodes.WrongParameters},
            {Constants.Errors.GeneralException, (int) ResponseCodes.GeneralException}
        };

        public static int GetErrorCode(int errorId)
        {
            if (Errors.ContainsKey(errorId))
                return Errors[errorId];
            return (int)ResponseCodes.GeneralException;
        }

        private enum ResponseCodes
        {
            Success = 0,
            UnknownError = 1,
            GeneralException = 2,
            PaymentSystemError = 3,
            PaymentCancel = 50,
            WrongParameters = 100,
            InvalidCard = 304
        }
    }
}