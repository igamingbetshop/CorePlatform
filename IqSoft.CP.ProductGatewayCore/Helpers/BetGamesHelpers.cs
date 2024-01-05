using System.Collections.Generic;
using IqSoft.CP.Common;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class BetGamesHelpers
    {
        public static  class Methods
        {
            public const string Ping = "ping";
            public const string GetAccountDetails = "get_account_details";
            public const string RefreshToken = "refresh_token";
            public const string RequestNewToken = "request_new_token";
            public const string GetBalance = "get_balance";
            public const string DoBet = "transaction_bet_payin";
            public const string DoWin = "transaction_bet_payout";


            public const string DoMultipleBets = "transaction_bet_subscription_payin";
            public const string CombinationBet = "transaction_bet_combination_payin";
            public const string CombinationWin = "transaction_bet_combination_payout";
            public const string DoPromoWin = "transaction_promo_payout";

        }

        public static class Statuses
        {
            public const int Success = 1;
            public const int Fail = 0;
        }

        public static class ErrorCodes
        {
            public const int WrongSignatiure = 1;
            public const int RequestIsExpired = 2;
            public const int ThereIsNoPayInDocument = 700;
            public const int InsufficientFunds= 703;
            public const int InternalServerError = 500;
        }

        public static Dictionary<int, int> ErrorCodesMapping { get; private set; } = new Dictionary<int, int>
        {
            {Constants.Errors.WrongHash, ErrorCodes.WrongSignatiure},
            {Constants.Errors.RequestExpired, ErrorCodes.RequestIsExpired},
            {Constants.Errors.LowBalance, ErrorCodes.InsufficientFunds},
            {Constants.Errors.CanNotConnectCreditAndDebit, ErrorCodes.ThereIsNoPayInDocument}
        };

        public static int GetErrorCode(int errorId)
        {
            if (ErrorCodesMapping.ContainsKey(errorId))
                return ErrorCodesMapping[errorId];
            return ErrorCodes.InternalServerError;
        }
    }
}