using IqSoft.CP.Common;
using System;
using System.Collections.Generic;
namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class AleaPartnersHelpers
    {
        public enum StatusCodes
        {
            Success = 200,
            Error = 400,
            Unauthorized = 401,
            Forbiden = 403,
            NotFound = 404,
            WrongParameters = 501,
            NoTransaction = 510,
            InsufficientFunds = 511,
            ActionNotFound = 5002,
            ActionClosed = 5003,
            TicketAlreadyExist = 5005,
            PayoutTransactionAlreadyExist = 5011
        }

        private readonly static Dictionary<int, int> Error = new Dictionary<int, int>
        {
            {Constants.Errors.ClientNotFound,  (int)StatusCodes.WrongParameters},
            {Constants.Errors.SessionNotFound,  (int)StatusCodes.WrongParameters},
            {Constants.Errors.SessionExpired,  (int)StatusCodes.WrongParameters},
            {Constants.Errors.WrongOperationAmount,  (int)StatusCodes.WrongParameters},
            {Constants.Errors.WrongInputParameters ,  (int)StatusCodes.WrongParameters},
            {Constants.Errors.LowBalance,  (int)StatusCodes.InsufficientFunds},
            {Constants.Errors.ClientDocumentAlreadyExists,  (int)StatusCodes.TicketAlreadyExist},
            {Constants.Errors.TransactionAlreadyExists,  (int)StatusCodes.PayoutTransactionAlreadyExist}
        };

        public static int GetErrorCode(int errorId)
        {
            if (Error.ContainsKey(errorId))
                return Error[errorId];
            return (int)StatusCodes.Error;
        }
    }
}