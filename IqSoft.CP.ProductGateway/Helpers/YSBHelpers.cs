using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public class YSBHelpers
    {
        public enum BetStatus
        {
            Pending = 0,
            Win = 1,
            Lose = 2,
            Void = 3,
            Undo = 5,
            WinHalf = 5,
            LoseHalf = 6,
            Cashout = 10
        }

        public enum BetType
        {
            Single=0,
            Lotto =1,
            Combo=3
        }
       
        public enum BetMode
        {
            Web=0,
            MobileWeb=1,
            NA=2,
            IOS=3,
            Android=4,
            Operator=5,
            Win32App=6,
            XAndroid = 7,
            XIOS =8
        }

        public static class Actions
        {
            public const string GetBalance = "ACCOUNTBALANCE";
            public const string Bet = "BET";
            public const string BetConfirmation = "BETCONFIRM";
            public const string Payout = "PAYOUT";
            public const string Refund = "REFUND";
        }


        public static class ErrorCodes
        {
            public const int ERR_INVALID_LOGIN = 1012;
            public const int ERR_FAILED_GET_BALANCE = 1014;
            public const int ERR_INSUFFICIENT_ACCOUNT_BALANCE = 1016;
            public const int ERR_ACCOUNT_NOTEXIST = 1021;
            public const int GENERAL = -1;
        }
        private readonly static Dictionary<int, int> Errors = new Dictionary<int, int>
        {
            {Constants.Errors.LowBalance, ErrorCodes.ERR_INSUFFICIENT_ACCOUNT_BALANCE},
            {Constants.Errors.ClientNotFound, ErrorCodes.ERR_ACCOUNT_NOTEXIST }
        };

        public static int GetErrorCode(int errorId)
        {
            if (Errors.ContainsKey(errorId))
                return Errors[errorId];
            return ErrorCodes.GENERAL;
        }
    }
}