using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
	public class TimelessTechHelpers
	{
		public static class Methods
		{
			public const string Authenticate = "authenticate";
			public const string Balance = "balance";
			public const string Changebalance = "changebalance";
			public const string Status = "status";
			public const string Cancel = "cancel";
			public const string FinishRound = "finishround";
		}

		public static class TransactionType
		{
			public const string BET = "BET";
			public const string WIN = "WIN";
			public const string REFUND = "REFUND";
		}

		public static class TransactionStatus
		{
			public const string OK = "OK";
			public const string ERROR = "ERROR";
			public const string CANCELED = "CANCELED";
		}

		public static class Messages
		{
			public static readonly string InvalidHash = "Invalid hash";
			public static readonly string InsufficientFunds = "Insufficient funds";
			public static readonly string PlayerNotFound = "Player is not found";
			public static readonly string TransactionNotFound = "Transaction not found";
			public static readonly string InternalError = "Internal error";
		}

		public static class Codes
		{
			public static readonly string InvalidHash = "OP_20";
			public static readonly string InsufficientFunds = "OP_31";
			public static readonly string PlayerNotFound = "OP_34";
			public static readonly string TransactionNotFound = "OP_41";
			public static readonly string InternalError = "OP_50";
		}

			private readonly static Dictionary<int, string> ErrorCodes = new Dictionary<int, string>
		{
			{Constants.Errors.WrongHash, Codes.InvalidHash},
			{Constants.Errors.LowBalance, Codes.InsufficientFunds},
			{Constants.Errors.ClientNotFound, Codes.PlayerNotFound},
			{Constants.Errors.DocumentNotFound, Codes.TransactionNotFound},
		};

		private readonly static Dictionary<int, string> ErrorMessages = new Dictionary<int, string>
		{
			{Constants.Errors.WrongHash, Messages.InvalidHash},
			{Constants.Errors.LowBalance, Messages.InsufficientFunds},
			{Constants.Errors.ClientNotFound, Messages.PlayerNotFound},
			{Constants.Errors.DocumentNotFound, Messages.TransactionNotFound},
			{Constants.Errors.GeneralException, Messages.InternalError}
		};

		public static string GetErrorMessage(int errorId)
		{
			if (ErrorMessages.ContainsKey(errorId))
				return ErrorMessages[errorId];
			return Messages.InternalError;
		}

		public static string GetErrorCode(int errorId)
		{
			if (ErrorCodes.ContainsKey(errorId))
				return ErrorCodes[errorId];
			return Codes.InternalError;
		}
	}
}