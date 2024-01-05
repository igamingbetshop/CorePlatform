using IqSoft.CP.Common.Enums;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static  class RocabeeHelpers
    {
        public static Dictionary<string, int> OperationType { get; private set; } = new Dictionary<string, int>
        {
            { "Bet", (int)OperationTypes.Bet },
            { "Win", (int)OperationTypes.Win }
        };

        public static class ErrorCodes
        {
            public const int Success = 200;
        }
    }
}