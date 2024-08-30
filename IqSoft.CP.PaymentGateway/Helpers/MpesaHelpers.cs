using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.PaymentGateway.Helpers
{
    public static class MpesaHelpers
    {
        public static class ResultCodes
        {
            public const string InvalidMSISDN = "C2B00011";
            public const string InvalidAccountNumber = "C2B00012";
            public const string InvalidAmount = "C2B00013";
            public const string InvalidKYCDetails = "C2B00014";
            public const string InvalidShortcode = "C2B00015";
            public const string OtherError = "C2B00016";
        }

        public static Dictionary<int, string> ResponseCodesMapping { get; private set; } = new Dictionary<int, string>
        {
            {Constants.Errors.ClientNotFound, ResultCodes.InvalidMSISDN},
            {Constants.Errors.WrongOperationAmount, ResultCodes.InvalidAmount},
            {Constants.Errors.PaymentRequestInValidAmount, ResultCodes.InvalidAmount},
            {Constants.Errors.WrongApiCredentials, ResultCodes.InvalidShortcode}
        };

        public static string GetErrorCode(int ourErrorCode)
        {
            if (ResponseCodesMapping.ContainsKey(ourErrorCode))
                return ResponseCodesMapping[ourErrorCode];
            return ResultCodes.OtherError;
        }
    }
}