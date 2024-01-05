using IqSoft.CP.Common;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace IqSoft.CP.ProductGateway.Helpers
{
	public static class SingularHelpers
	{
		public static class Methods
		{
			public const string AuthenticateUserByToken = "authenticateUserByToken";
			public const string GetBalance = "getBalance";
			public const string DepositMoney = "depositMoney";
			public const string WithdrawMoney = "withdrawMoney";
			public const string CheckTransactionStatus = "checkTransactionStatus";
			public const string RollbackTransaction = "rollbackTransaction";
			public const string GetExchangeRates = "getExchangeRates";
			public const string Exchange = "exchange";
		}


		public static Dictionary<string, int> OperatorIds { get; private set; } = new Dictionary<string, int>
            {
                {"7c5dbea0-c490-11e3-894d-005056a8fc2a", 3},
                {"1284c72c-29c2-4744-b5f8-cba9eab42655", 4}
            };

        public enum ExternalProductId
        {
            BackgammonP2P = 1
        }

        public static class ErrorCodes
        {
            public const int Success = 10;
            public const int GenericFailedError = 111;
            public const int MissingParameters = 112;
            public const int AccountNotFound = 125;
            public const int AccountIsBlocked = 131;
            public const int ProviderNotFound = 137;
            public const int AccessDenied = 138;
            public const int WrongHash = 139;
            public const int UserIsSelfExcluded = 141;
            public const int MustBeMoreThanZero = 144;
            public const int CurrencyNotFound = 145;
            public const int UsersAccountsNotFound = 146;
            public const int TokenNotFound = 147;
            public const int TokenIsExpired = 148;
            public const int DublicatedProvidersTransactionId = 151;
            public const int TransactionAmountAndLimitDontMatch = 153;
            public const int InsufficientFunds = 154;
            public const int IncorrectTransactionIdFormat = 155;
            public const int TransactionNotFound = 156;
            public const int TransactionStatusSuccess = 157;
            public const int TransactionStatusRollbacked = 158;
            public const int TransactionRollbackTimeExpired = 172;
            public const int UnableToRollbackTransaction = 173;
            public const int UnidentifiedTransactionStatuse = 178;
            public const int TransactionStatusApproved = 181;
            public const int TransactionStatusPending = 182;
            public const int TransactionStatusRejected = 183;
            public const int TransactionStatusFrozen = 184;
            public const int TransactionStatusCanceled = 185;
        }

        private readonly static Dictionary<int, int> Errors = new Dictionary<int, int>
        {
            {Constants.SuccessResponseCode, ErrorCodes.Success },
            {Constants.Errors.GeneralException, ErrorCodes.GenericFailedError },
            {Constants.Errors.WrongHash, ErrorCodes.WrongHash },
            {Constants.Errors.WrongOperatorId, ErrorCodes.ProviderNotFound },
            {Constants.Errors.SessionNotFound, ErrorCodes.TokenNotFound },
            {Constants.Errors.SessionExpired, ErrorCodes.TokenIsExpired },
            {Constants.Errors.WrongCurrencyId, ErrorCodes.CurrencyNotFound },
            {Constants.Errors.WrongOperationAmount, ErrorCodes.MustBeMoreThanZero },
            {Constants.Errors.ClientBlocked, ErrorCodes.AccessDenied },
            {Constants.Errors.ProductNotAllowedForThisPartner, ErrorCodes.AccessDenied },
            {Constants.Errors.ProductBlockedForThisPartner, ErrorCodes.AccessDenied },
            {Constants.Errors.ClientNotFound, ErrorCodes.AccountNotFound },
            {Constants.Errors.CurrencyNotExists, ErrorCodes.CurrencyNotFound },
            {Constants.Errors.TransactionAlreadyExists, ErrorCodes.DublicatedProvidersTransactionId },
            {Constants.Errors.LowBalance, ErrorCodes.InsufficientFunds },
            {Constants.Errors.DocumentAlreadyRollbacked, ErrorCodes.TransactionStatusRollbacked },
            {Constants.Errors.DocumentNotFound, ErrorCodes.TransactionNotFound },
            {Constants.Errors.CanNotDeleteRollbackDocument, ErrorCodes.UnableToRollbackTransaction }
        };

        public static Dictionary<int, string> Currencies { get; private set; } = new Dictionary<int, string>
        {
            { 2, "GEL" }, { 3, "USD" }, { 4, "EUR" }, { 5, "GBP" },
            { 6, "RUB" }, { 7, "UAH" }, { 8, "AMD" }, { 9, "TRY" },
			{ 10, "IRR" }
		};

        public static int GetError(int error)
        {
            if (Errors.ContainsKey(error))
                return Errors[error];
            return Errors[Constants.Errors.GeneralException];
        }

        public static string GetSign(string key, string strParams)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(strParams + key));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();
            }
        }
    }
}