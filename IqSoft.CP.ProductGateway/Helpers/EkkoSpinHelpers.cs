using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class EkkoSpinHelpers
    {
        public const int OperatorId = 29;
        public const int Success = 1;
        public const int LiveRacingId = 10000;
        public enum TransactionType { Bet = 1, Win = 2 }

        public static Dictionary<int, string> TransactionInfo { get; private set; } = new Dictionary<int, string>
        {
            {1, "spin" },
            {2, "gamble" },
            {3, "free spin" }
        };
    }
}