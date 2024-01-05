
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using System.Collections.Generic;
using System.Net;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public class SolidGamingHelpers
    {
        private static class Platforms
        {
            public const string Web = "DESKTOP";
            public const string Mobile = "MOBILE";
        }

        public static DeviceTypes MapDeviceType(string platform)
        {
            if (platform == Platforms.Mobile)
                return DeviceTypes.Mobile;
            if (platform == Platforms.Web)
                return DeviceTypes.Desktop;
            return DeviceTypes.Desktop;
        }

        private static class ErrorCodes
        {

            public const string InvalidToken = "TOKEN_NOT_FOUND";
            public const string WrongAuthentication = "AUTHENTICATION_FAILED";
            public const string ProductNotFount = "GAME_NOT_FOUND";
            public const string ClientNotFound = "PLAYER_NOT_FOUND";
            public const string RoundEnded = "ROUND_ENDED";
            public const string LowBalance = "OUT_OF_MONEY";
			public const string GamingLimitReached = "GAMING_LIMIT_REACHED";
            public const string SessionLimitReached = "TOKEN_NOT_VALID";
            public const string SessionNotFound = "TOKEN_NOT_FOUND";
            public const string InvalidCurrency = "INVALID_CURRENCY";
            public const string RoundNotFound = "ROUND_NOT_FOUND";
            public const string TransactionNotFound = "TRANSACTION_NOT_FOUND";
            public const string GeneralException = "ERROR";
		}

        public readonly static Dictionary<string, HttpStatusCode> Statuses = new Dictionary<string, HttpStatusCode>
        {
            {ErrorCodes.LowBalance, HttpStatusCode.OK},
            {ErrorCodes.GamingLimitReached, HttpStatusCode.OK},
			{ErrorCodes.WrongAuthentication, HttpStatusCode.Unauthorized}
		};

        private readonly static Dictionary<int, string> Errors = new Dictionary<int, string>
        {
            {Constants.Errors.WrongToken, ErrorCodes.InvalidToken},
            {Constants.Errors.WrongApiCredentials, ErrorCodes.WrongAuthentication},
            {Constants.Errors.ProductNotFound, ErrorCodes.ProductNotFount},
            {Constants.Errors.ClientNotFound, ErrorCodes.ClientNotFound},
            {Constants.Errors.LowBalance, ErrorCodes.LowBalance},
            {Constants.Errors.ClientMaxLimitExceeded, ErrorCodes.GamingLimitReached},
            {Constants.Errors.SessionExpired, ErrorCodes.SessionLimitReached},
            {Constants.Errors.SessionNotFound, ErrorCodes.SessionNotFound},
            {Constants.Errors.WrongCurrencyId, ErrorCodes.InvalidCurrency},
            {Constants.Errors.DocumentNotFound, ErrorCodes.TransactionNotFound},
            {Constants.Errors.GeneralException, ErrorCodes.GeneralException},
			{Constants.Errors.RoundNotFound, ErrorCodes.RoundNotFound}
		};

        public static string GetErrorCode(int errorId)
        {
            if (Errors.ContainsKey(errorId))
                return Errors[errorId];
            return ErrorCodes.GeneralException;
        }

		public static HttpStatusCode GetStatusCode(string error)
		{
			if (Statuses.ContainsKey(error))
				return Statuses[error];
			return HttpStatusCode.BadRequest;
		}
	}
}