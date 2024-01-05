using IqSoft.CP.Common;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class DragonGamingHelpers
    {
        public enum StatusCodes
        {
            INVALID_LOGIN_CREDENTIALS = 2014,
            INVALID_API_KEY = 2015,
            INVALID_SESSION_ID = 2016,
            INVALID_PROVIDER = 2017,
            INVALID_GAME_TYPE = 2018,
            INVALID_PLAYER_ID = 2021,
            INVALID_GAME_ID = 2022,
            INVALID_CURRENCY = 2026,
            NEGATIVE_AMOUNT_VALUE = 2027,
            INSUFFICINT_BALANCE = 2028,
            INVALID_AMOUNT_VALUE = 2030,
            OPERATOR_AUTHENTICATION_FAIL = 2032,
            CURRENCY_MISMATCH = 2033,
            ZERO_AMOUNT_VALUE = 2034,
            FALIED_TRANS = 2035
        }

        private readonly static Dictionary<int, StatusCodes> Error = new Dictionary<int, StatusCodes>
        {
            {Constants.Errors.WrongApiCredentials,  StatusCodes.INVALID_API_KEY},
            {Constants.Errors.WrongToken, StatusCodes.INVALID_SESSION_ID},
            {Constants.Errors.SessionNotFound, StatusCodes.INVALID_SESSION_ID},
            {Constants.Errors.SessionExpired, StatusCodes.INVALID_SESSION_ID},
            {Constants.Errors.WrongOperationAmount, StatusCodes.INVALID_AMOUNT_VALUE},
            {Constants.Errors.LowBalance, StatusCodes.INSUFFICINT_BALANCE},
            {Constants.Errors.WrongCurrencyId, StatusCodes.CURRENCY_MISMATCH},
            {Constants.Errors.ClientDocumentAlreadyExists, StatusCodes.FALIED_TRANS},
            {Constants.Errors.TransactionAlreadyExists, StatusCodes.FALIED_TRANS}
        };

        public static StatusCodes GetErrorCode(int errorId)
        {
            if (Error.ContainsKey(errorId))
                return Error[errorId];
            return StatusCodes.OPERATOR_AUTHENTICATION_FAIL;
        }
    }
}