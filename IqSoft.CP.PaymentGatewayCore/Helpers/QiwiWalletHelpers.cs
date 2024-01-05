using System.Collections.Generic;
using IqSoft.CP.Common;

namespace IqSoft.CP.PaymentGateway.Helpers
{
    public static class QiwiWalletHelpers
    {
        public static class ResponseCodes
        {
            public const int Success = 0;
            public const int WrongParameterFormat = 5;
            public const int ServerBusy = 13;
            public const int InvalidOperation = 78;
            public const int AuthorisationError = 150;
            public const int ProtocolIsNotConnected = 152;
            public const int UserNotFound = 210;
            public const int PaymentRequestAlreadyExist = 215;
            public const int AmountIsLess = 241;
            public const int AmountIsHigh = 242;
            public const int WalletNotFound = 298;
            public const int TechnicalError = 300;
            public const int WrongMobileNumber = 303;
            public const int BlockedProvider = 316;
            public const int NotAllowedAction = 319;
            public const int BlockedIp = 339;
            public const int ExceededLimits = 700;
            public const int WalletIsBlocked = 774;
            public const int ForbiddenCurrency = 1001;
        }

        public static class Statuses
        {
            public const string Waiting = "waiting";
            public const string Paid = "paid";
            public const string Rejected = "rejected";
            public const string Unpaid = "unpaid";
            public const string Expired = "expired";
            public const string Processing = "processing";
            public const string Success = "success";
            public const string Fail = "fail";
        }

        private readonly static Dictionary<int, int> ResponseCodesMapping= new Dictionary<int, int>
        {
            {Constants.Errors.PaymentRequestNotFound, ResponseCodes.WrongParameterFormat},
            {Constants.Errors.WrongOperationAmount, ResponseCodes.AmountIsLess},
            {Constants.Errors.TransactionAlreadyExists, ResponseCodes.PaymentRequestAlreadyExist}
        };        

        public static int GetErrorCode(int ourErrorCode)
        {
            if (ResponseCodesMapping.ContainsKey(ourErrorCode))
                return ResponseCodesMapping[ourErrorCode];
            return ResponseCodes.TechnicalError;
        }
    }
}