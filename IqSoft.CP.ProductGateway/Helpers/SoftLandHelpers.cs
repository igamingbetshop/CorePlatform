using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
	public class SoftLandHelpers
	{
		public enum ErrorCodes
		{
			InternalError = 100,
			TransactionNotFound = 101,
			WrongRequest = 102,
			AlreadyExist = 105,
			Unauthorized = 106,
			SessionExpired = 109,
			SessionNotFound = 110,
			PlayerBlocked = 111,
			PlayerNotFound = 112,
			GameNotFound = 113,
			InvalidCurrency = 114,
			InsufficientFunds = 117,
		}

		private readonly static Dictionary<int, int> Error = new Dictionary<int, int>
		{
			{Constants.Errors.TranslationNotFound, (int)ErrorCodes.TransactionNotFound},
			{Constants.Errors.WrongProductId, (int) ErrorCodes.WrongRequest},
			{Constants.Errors.WrongInputParameters, (int) ErrorCodes.WrongRequest},
			{Constants.Errors.TransactionAlreadyExists, (int) ErrorCodes.AlreadyExist},
			{Constants.Errors.WrongHash, (int)ErrorCodes.Unauthorized},
			{Constants.Errors.SessionExpired, (int)ErrorCodes.SessionExpired},
			{Constants.Errors.ClientNotFound, (int)ErrorCodes.PlayerNotFound},
			{Constants.Errors.ClientBlocked, (int)ErrorCodes.PlayerBlocked},
			{Constants.Errors.RoundNotFound, (int)ErrorCodes.GameNotFound },
			{Constants.Errors.LowBalance, (int)ErrorCodes.InsufficientFunds},
			{Constants.Errors.WrongCurrencyId, (int)ErrorCodes.InvalidCurrency}
		};

		public static int GetErrorCode(int errorId)
		{
			if (Error.ContainsKey(errorId))
				return Error[errorId];
			return (int)ErrorCodes.InternalError;
		}
	}
}