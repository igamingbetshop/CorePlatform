using System;
using System.Collections.Generic;
using IqSoft.CP.Common;
using System.Security.Cryptography;
using System.Text;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class TomHornHelpers
    {
        public static class Methods
        {
            public const string Deposit = "Deposit";
            public const string Withdraw = "Withdraw";
            public const string GetBalance = "GetBalance";
            public const string RollbackTransaction = "RollbackTransaction";
            public const string GetSession = "GetSession";
        }

        public enum State { Open = 1, Closed = 0 }

        public static class ErrorCodes
        {
            public const int Success = 0;
            public const int GeneralError = 1;
            public const int WrongInputParameters = 2;
            public const int InvalidSign = 3;
            public const int InvalidPartner = 4;
            public const int IdentityNotFound = 5;
            public const int InsufficientFunds = 6;
            public const int InvalidCurrency = 8;
            public const int TransactionIsAlreadyRolledBack = 9;
            public const int PlayersLimitReached = 10;
            public const int DuplicateReference = 11;
            public const int UnknownTransaction = 12;
            //for WebSiteWebApi
            public const int SessionAlreadyOpen = 1005;
            public const int IdentityAlreadyExists = 1014;
        }

        private readonly static Dictionary<int, int> Errors = new Dictionary<int, int>
        {
            { Constants.SuccessResponseCode, ErrorCodes.Success },
            {Constants.Errors.GeneralException, ErrorCodes.GeneralError},
            {Constants.Errors.WrongPartnerId, ErrorCodes.InvalidPartner},
            {Constants.Errors.WrongHash, ErrorCodes.InvalidSign},
            {Constants.Errors.TokenExpired, ErrorCodes.IdentityNotFound},
            {Constants.Errors.WrongToken, ErrorCodes.IdentityNotFound},
            {Constants.Errors.SessionExpired, ErrorCodes.IdentityNotFound},
            {Constants.Errors.SessionNotFound, ErrorCodes.IdentityNotFound},
            {Constants.Errors.LowBalance, ErrorCodes.InsufficientFunds},
            {Constants.Errors.WrongCurrencyId, ErrorCodes.InvalidCurrency},
            {Constants.Errors.DocumentAlreadyRollbacked, ErrorCodes.TransactionIsAlreadyRolledBack},
            {Constants.Errors.TransactionAlreadyExists, ErrorCodes.DuplicateReference},
            {Constants.Errors.DocumentNotFound, ErrorCodes.UnknownTransaction},
            {Constants.Errors.CanNotConnectCreditAndDebit, ErrorCodes.UnknownTransaction}
        };
        public static int GetError(int error)
        {
            if (Errors.ContainsKey(error))
                return Errors[error];
            return Errors[Constants.Errors.GeneralException];
        }
    }
}