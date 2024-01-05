using System.Collections.Generic;
using IqSoft.CP.Common.Enums;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class IqSoftHelpers
    {
        public static class Methods
        {
            public readonly static string CheckPermission = "CheckPermission";
        }

        public static class SiteGamesMethods
        {
            public const string Authorization = "Authorization";
            public const string GetBalance = "GetBalance";
            public const string RefreshToken = "RefreshToken";
            public const string ExtendToken = "ExtendToken";
            public const string Debit = "Debit";
            public const string Credit = "Credit";
            public const string RollBack = "RollBack";
        }

        public static class BetShopGamesMethods
        {
            public const string Authorization = "Authorization";
            public const string RefreshToken = "RefreshToken";
            public const string Debit = "Debit";
            public const string Credit = "Credit";
            public const string RollBack = "RollBack";
        }

        public static class BetShopGatewayMethods
        {
            public const string Authorization = "Authorization";
            public const string GetCashDeskInfo = "GetCashDeskInfo";
            public const string GetProductSession = "GetProductSession";
            public const string CloseSession = "CloseSession";
            public const string GetClient = "GetClient";
            public const string DepositToInternetClient = "DepositToInternetClient";
            public const string GetPaymentRequests = "GetPaymentRequests";
            public const string PayPaymentRequest = "PayPaymentRequest";
            public const string GetBetShopBets = "GetBetShopBets";
            public const string GetBetByBarcode = "GetBetByBarcode";
            public const string PayWin = "PayWin";
            public const string GetTicketInfoForPrint = "GetTicketInfoForPrint";
            public const string GetShiftReport = "GetShiftReport";
            public const string GetCashDeskOperations = "GetCashDeskOperations";
            public const string CreateDebitCorrectionOnCashDesk = "CreateDebitCorrectionOnCashDesk";
            public const string CreateCreditCorrectionOnCashDesk = "CreateCreditCorrectionOnCashDesk";
            public const string GetCashDesksBalance = "GetCashDesksBalance";
            public const string GetCashDeskCurrentBalance = "GetCashDeskCurrentBalance";
            public const string GetBetShopOperations = "GetBetShopOperations";
            public const string CloseShift = "CloseShift";
            public const string GetBetShops = "GetBetShops";
            public const string GetCashDesks = "GetCashDesks";
            public const string GetCashierSessionByToken = "GetCashierSessionByToken";
            public const string GetCashierSessionByProductId = "GetCashierSessionByProductId";
            public const string GetBetByDocumentId = "GetBetByDocumentId";
            public const string GetUserProductSession = "GetUserProductSession";
        }

        public static class BetTypes
        {
            public const int Single = 1;
            public const int Multiple = 2;
            public const int System = 3;
            public const int Chain = 4;
        }

        public enum BetStatus
        {
            Uncalculated = 1,
            Won = 2,
            Lost = 3,
            Canceled = 4,
            Cashouted = 5,
            Returned = 6
        };

        public static Dictionary<int, int> BetTypesMapping { get; private set; } = new Dictionary<int, int>
        {
            {BetTypes.Single, (int)CreditDocumentTypes.Single},
            {BetTypes.Multiple, (int)CreditDocumentTypes.Multiple},
            {BetTypes.System, (int)CreditDocumentTypes.System},
            {BetTypes.Chain, (int)CreditDocumentTypes.Chain}
        };

        public static Dictionary<int, int> BetStatesMapping { get; private set; } = new Dictionary<int, int>
        {
            {(int)BetStatus.Uncalculated, (int)BetDocumentStates.Uncalculated},
            {(int)BetStatus.Won, (int)BetDocumentStates.Won},
            {(int)BetStatus.Lost, (int)BetDocumentStates.Lost},
            {(int)BetStatus.Canceled, (int)BetDocumentStates.Deleted},
            {(int)BetStatus.Cashouted, (int)BetDocumentStates.Cashouted},
            {(int)BetStatus.Returned, (int)BetDocumentStates.Returned}
        };
    }
}