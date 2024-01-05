using IqSoft.CP.Common;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class EvenBetHelpers
    {

        public static class ErrorCodes
        {
            public const int Success = 0;
            public const int SignatureIsWrong = 1;
            public const int UserIdIsWrong = 2;
            public const int InsufficientFunds = 3;
            public const int InvalidParams = 4;
            public const int TransactionNotFount = 5;
            public const int TransactionIncompatible = 6;
        }
        private readonly static Dictionary<int, Tuple<int, string>> Errors = new Dictionary<int, Tuple<int, string>>
        {
            {Constants.SuccessResponseCode, Tuple.Create(ErrorCodes.Success, "Completed successfully") },
            {Constants.Errors.WrongHash,Tuple.Create(ErrorCodes.SignatureIsWrong, "Invalid signature") },
            {Constants.Errors.ClientNotFound, Tuple.Create(ErrorCodes.UserIdIsWrong, "Player not found") },
            {Constants.Errors.LowBalance, Tuple.Create(ErrorCodes.InsufficientFunds, "Insufficient funds") },
            {Constants.Errors.WrongInputParameters, Tuple.Create(ErrorCodes.InvalidParams, "Insufficient funds") },
            {Constants.Errors.DocumentNotFound, Tuple.Create(ErrorCodes.TransactionNotFount, "Referencetransaction does not exist") },
            {Constants.Errors.DocumentAlreadyWinned, Tuple.Create(ErrorCodes.TransactionNotFount, "Referencetransaction has incompatible data") },
            {Constants.Errors.GeneralException, Tuple.Create(Constants.Errors.GeneralException, "General Exception") }
        };

        public static Tuple<int, string> GetError(int error)
        {
            if (Errors.ContainsKey(error))
                return Errors[error];
            return Errors[Constants.Errors.GeneralException];
        }
    }
}