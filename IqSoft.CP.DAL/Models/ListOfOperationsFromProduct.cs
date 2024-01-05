using IqSoft.CP.DAL.Models.Cache;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models
{
    public class ListOfOperationsFromApi
    {
        public string CurrencyId { get; set; }
        public string RoundId { get; set; }
        public decimal CurrencyRate { get; set; }
        public int GameProviderId { get; set; }
        public int? OperationTypeId { get; set; }
        public int? State { get; set; }
        public string ExternalProductId { get; set; }
        public int? ProductId { get; set; }
        public string Info { get; set; }
        public string TicketInfo { get; set; }
        public string TransactionId { get; set; }
        public long? CreditTransactionId { get; set; }
        public long? ExternalOperationId { get; set; }
        public int? TypeId { get; set; }
        public int? SelectionsCount { get; set; }        
        public long? SessionId { get; set; }     
        public int BonusId { get; set; }
        public bool? IsFreeBet { get; set; }
        public List<OperationItemFromProduct> OperationItems { get; set; }
    }

    public class OperationItemFromProduct
    {
        public BllClient Client { get; set; }
        public int CashierId { get; set; }
        public int CashDeskId { get; set; }
        public decimal Amount { get; set; }
        public int Type { get; set; }
        public int? DeviceTypeId { get; set; }
        public decimal? PossibleWin { get; set; }
    }
}
