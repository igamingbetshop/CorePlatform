using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Helpers
{
    public class WoopayHelpers
    {
        public class Methods
        {
            public const string Login = "core_login";
            public const string CreateInvoice = "cash_createInvoice";
            public const string CreateInvoiceExtended = "cash_createInvoiceExtended";
            public const string GetBalance = "cash_getBalance";
            public const string GetOperationData = "cash_getOperationData";
        }

        public static Dictionary<int, int> Errors;
        public static Dictionary<int, int> PaymentRequestStatus;
        public static Dictionary<int, int> PaymentRequestType;

        static WoopayHelpers()
        {
            PaymentRequestStatus = new Dictionary<int, int>
            {
                { (int)PaymentRequestStates.Pending, (int) StatusCodes.New},
                { (int)PaymentRequestStates.PayPanding, (int) StatusCodes.Pending},
                { (int)PaymentRequestStates.CanceledByUser, (int) StatusCodes.Rejected},
                { (int)PaymentRequestStates.Confirmed, (int) StatusCodes.Conducted}
            };

            PaymentRequestType = new Dictionary<int, int>
            {
                { (int)PaymentRequestTypes.Deposit, (int) TypeCodes.Transfer},
                { (int)PaymentRequestTypes.Withdraw, (int) TypeCodes.Withdraw}
            };

            Errors = new Dictionary<int, int>
            {
                {Constants.SuccessResponseCode, (int) ResponseCodes.Success},
                //{Constants.Errors.WrongParameters, (int) ResponseCodes.TemporaryError},
                //{Constants.Errors.WrongConvertion, (int) ResponseCodes.FormatError},
                //{Constants.Errors.ClientNotFound, (int) ResponseCodes.ClientNotFound},
                //{Constants.Errors.ClientBlocked, (int) ResponseCodes.InvalidAccount},
                //{Constants.Errors.WrongOperationAmount, (int) ResponseCodes.LessAmount},
                {Constants.Errors.GeneralException, (int) ResponseCodes.Other}
            };
        }

        public static int GetErrorCode(int errorId)
        {
            if (Errors.ContainsKey(errorId))
                return Errors[errorId];
            return (int)ResponseCodes.Other;
        }

        public static int GetPaymentRequestStatus(int status)
        {
            if (PaymentRequestStatus.ContainsKey(status))
                return PaymentRequestStatus[status];
            return (int)StatusCodes.Default;
        }

        public static int GetPaymentRequestType(int type)
        {
            if (PaymentRequestType.ContainsKey(type))
                return PaymentRequestType[type];
            return (int)TypeCodes.Default;
        }

        public enum ResponseCodes
        {
            Success = 0,
            FormatError = 1,
            MethodNotFound = 2,
            WrongFromServerSide = 3,
            NotAccess = 4,
            NecessaryLogin = 5,
            OperationFailed = 6,
            NotAccessProvider = 7,
            TechnicalProblem = 8,
            InvalidAccount = 79,
            LessAmount = 241,
            BigAmount = 242,
            Other = 300
        }

        public enum StatusCodes
        {
            Default = 0,
            New = 1,
            Pending = 2,
            Rejected = 3,
            Conducted = 4,
            Reversed = 5
        }

        public enum TypeCodes
        {
            Default = 0,
            Transfer = 1,
            Withdraw = 2
        }
    }
}