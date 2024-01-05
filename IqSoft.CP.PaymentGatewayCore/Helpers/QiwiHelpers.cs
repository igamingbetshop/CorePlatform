using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.PaymentGateway.Helpers
{
    public static class QiwiHelpers
    {
        public static class Methods
        {
            public const string ApiRequest = "ApiRequest";
            public const string Pay = "pay";
            public const string Check = "check";
        }

        public static class Statuses
        {
            public const string Success = "Ok";
            public const string Fail = "Fail";
        }

        private readonly static Dictionary<int, int> Errors = new Dictionary<int, int>
        {
            {Constants.SuccessResponseCode, (int) ResponseCodes.Success},
            {Constants.Errors.WrongParameters, (int) ResponseCodes.TemporaryError},
            {Constants.Errors.WrongConvertion, (int) ResponseCodes.FormatError},
            {Constants.Errors.ClientNotFound, (int) ResponseCodes.ClientNotFound},
            {Constants.Errors.ClientBlocked, (int) ResponseCodes.InvalidAccount},
            {Constants.Errors.WrongOperationAmount, (int) ResponseCodes.LessAmount},
            {Constants.Errors.GeneralException, (int) ResponseCodes.Other}
        };

        public static int GetErrorCode(int errorId)
        {
            if (Errors.ContainsKey(errorId))
                return Errors[errorId];
            return (int)ResponseCodes.Other;
        }

        public enum ResponseCodes
        {
            Success = 0,
            TemporaryError = 1,
            FormatError = 4,
            ClientNotFound = 5,
            NotAccessProvider = 7,
            TechnicalProblem = 8,
            InvalidAccount = 79,
            LessAmount = 241,
            BigAmount = 242,
            Other = 300
        }
    }
}