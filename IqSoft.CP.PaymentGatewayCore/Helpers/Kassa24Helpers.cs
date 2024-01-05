using System.Collections.Generic;
using IqSoft.CP.Common;

namespace IqSoft.CP.PaymentGateway.Helpers
{
    public static class Kassa24Helpers
    {
        public static class Statuses
        {
            public const string Success = "Ok";
            public const string Fail = "Fail";
        }

        public static class Methods
        {
            public const string Check = "check";
            public const string Payment = "payment";
            public const string Cancel = "cancel";
            public const string Status = "status";  
        }

        public enum ResponseCodes
        {
            Success = 0,
            WrongAction = 1,
            ClientNotFound = 2,
            WrongAmount = 3,
            WrongTransactionId = 4,
            WrongDate = 5,
            TransactionNotFound = 6,
            TransactionCancelled = 7,
            UnknownTransactionState = 8,
            CanNotCancelTransfer = 9,
            OtherError = 10
        }

        private readonly static Dictionary<int, int> ResponseCodesMapping = new Dictionary<int, int>
        {
            {Constants.Errors.ClientNotFound, (int) ResponseCodes.ClientNotFound},
            {Constants.Errors.WrongOperationAmount, (int) ResponseCodes.WrongAmount},
            {Constants.Errors.TransactionAlreadyExists, (int) ResponseCodes.WrongTransactionId},
            {Constants.Errors.CanNotDeleteRollbackDocument, (int) ResponseCodes.CanNotCancelTransfer},
            {Constants.Errors.DocumentAlreadyRollbacked, (int) ResponseCodes.CanNotCancelTransfer},
            {Constants.Errors.PaymentRequestNotFound, (int) ResponseCodes.TransactionNotFound},
        };       

        public static int GetErrorCode(int ourErrorCode)
        {
            if (ResponseCodesMapping.ContainsKey(ourErrorCode))
                return ResponseCodesMapping[ourErrorCode];
            return (int)ResponseCodes.OtherError;
        }
    }
}