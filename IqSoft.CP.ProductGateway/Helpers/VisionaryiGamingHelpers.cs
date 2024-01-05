using IqSoft.CP.Common;
using IqSoft.CP.ProductGateway.Models.VisionaryiGaming;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public class VisionaryiGamingHelpers
    {
        public static class Methods
        {
            public const string Authenticate = "Authenticate";
            public const string BatchGetBalance = "BatchGetBalance";
            public const string BatchDebitFunds = "BatchDebitFunds";
            public const string BatchCreditFunds = "BatchCreditFunds";
            public const string LobbyStatus = "LobbyStatus";
            public const string RNGLobbyStatus = "RNGLobbyStatus";
        }
        public static string ErrorMapping(int errorMessaged, string message)
        {
            switch (errorMessaged)
            {
                case Constants.Errors.WrongCurrencyId:
                    message = "Invalid currency";
                    break;
                case Constants.Errors.ClientNotFound:
                    message = "No such user";
                    break;
                case Constants.Errors.WrongProviderId:
                    message = "Invalid siteID";
                    break;
                case Constants.Errors.WrongHash:
                    message = "Not authorized";
                    break;
                case Constants.Errors.LowBalance:
                    message = "Insufficient funds";
                    break;
            }
            return message;
        }

        public static object ErrorResponce(string method, AuthOutput output)
        {
            switch (method)
            {
                case VisionaryiGamingHelpers.Methods.Authenticate:
                    return new AuthenticateOutput() { AuthenticateResponse = new List<AuthOutput> { output } };
                case VisionaryiGamingHelpers.Methods.BatchGetBalance:
                    return new BatchGetBalanceOutput() { BatchGetBalanceResponse = new List<BalanceOutput> { output } };
                case VisionaryiGamingHelpers.Methods.BatchCreditFunds:
                    return new BatchCreditFundsOutput() { BatchCreditFundsResponse = new List<BalanceOutput> { output } };
                case VisionaryiGamingHelpers.Methods.BatchDebitFunds:
                    return new BatchDebitFundsOutput() { BatchDebitFundsResponse = new List<BalanceOutput> { output } };
            }
            return null;
        }

    }
}