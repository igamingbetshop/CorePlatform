using System.Text;
using System;
using static IqSoft.CP.ProductGateway.Helpers.TimelessTechHelpers;
using System.Collections.Generic;
using IqSoft.CP.Common;
using static IqSoft.CP.ProductGateway.Helpers.LuckyStreakHelpers;

namespace IqSoft.CP.ProductGateway.Helpers
{
	public static class StringBuilderExt
	{
		public static StringBuilder AppendNewLine(this StringBuilder sb, string s)
		{
			sb.Append(s ?? String.Empty).Append("\n");
			return sb;
		}
	}
	public class LuckyStreakHelpers
	{
		
		public static class Method
		{
			public const string Validate = "validate";
			public const string GetBalance = "getBalance";
			public const string MoveFunds = "moveFunds";
			public const string AbortMoveFunds = "abortMoveFunds";
		}

		public static class TransactionType
		{
			public const string Bet = "Bet";
			public const string Tip = "Tip";
			public const string Win = "Win";
			public const string Loss = "Loss";
			public const string Refund = "Refund";
		}

		public static class Messages
		{
			public static readonly string InsufficientFunds = "Insufficient funds";
			public static readonly string AuthenticationCodeValidationFailure = "Authentication Code Validation Failure";
			public static readonly string TransactionAlreadyProcessed = "Transaction already processed";
			public static readonly string AccountNotFound = "Account not found";
			public static readonly string TransactionToAbortNotFound = "Transaction to abort not found";
			public static readonly string GeneralError = "General error";
		}

		public static class Codes
		{
			public static readonly string InsufficientFunds = "ERR-FUND-003";
			public static readonly string AuthenticationCodeValidationFailure = "ERR-AUTH-001";
			public static readonly string TransactionAlreadyProcessed = "ERR-FUND-001";
			public static readonly string AccountNotFound = "ERR-FUND-004";
			public static readonly string TransactionToAbortNotFound = "ERR-FUND-011";
			public static readonly string GeneralError = "ERR-GNRL-999";
		}

		private readonly static Dictionary<int, string> ErrorCodes = new Dictionary<int, string>
		{
			{Constants.Errors.LowBalance, Codes.InsufficientFunds},
			{Constants.Errors.WrongHash, Codes.AuthenticationCodeValidationFailure},
			{Constants.Errors.TransactionAlreadyExists, Codes.TransactionAlreadyProcessed},
			{Constants.Errors.ClientNotFound, Codes.AccountNotFound},
			{Constants.Errors.DocumentNotFound, Codes.TransactionToAbortNotFound},
			{Constants.Errors.GeneralException, Codes.GeneralError},
		};

		private readonly static Dictionary<int, string> ErrorMessages = new Dictionary<int, string>
		{
			{Constants.Errors.LowBalance, Messages.InsufficientFunds},
			{Constants.Errors.WrongHash, Messages.AuthenticationCodeValidationFailure},
			{Constants.Errors.TransactionAlreadyExists, Codes.TransactionAlreadyProcessed},
			{Constants.Errors.ClientNotFound, Messages.AccountNotFound},
			{Constants.Errors.DocumentNotFound, Messages.TransactionToAbortNotFound},
			{Constants.Errors.GeneralException, Messages.GeneralError}
		};

		public static string GetErrorMessage(int errorId)
		{
			if (ErrorMessages.ContainsKey(errorId))
				return ErrorMessages[errorId];
			return Messages.GeneralError;
		}

		public static string GetErrorCode(int errorId)
		{
			if (ErrorCodes.ContainsKey(errorId))
				return ErrorCodes[errorId];
			return Codes.GeneralError;
		}
	}
}